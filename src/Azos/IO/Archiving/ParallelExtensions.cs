/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Provides extensions which facilitate parallel archive processing akin to
  /// thread-based map:reduce
  /// </summary>
  public static class ParallelExtensions
  {
    /// <summary>
    /// Processes all entries in parallel batches of the specified size, page-by-page - each batch gets processed in parallel.
    /// Batches are split at the page boundaries. The method is useful for performing CPU-intensive analysis of entries.
    /// WARNING: NEVER rely on `Entry` buffer beyond the boundaries of `body` method. You can materialize valid entries by
    /// calling `Materialize(entry)` of the reader
    /// </summary>
    /// <param name="reader">Reader instance</param>
    /// <param name="start">Starting bookmark in the volume</param>
    /// <param name="batchBy">The size of entry batch. Batches get processed in-parallel</param>
    /// <param name="body">The body of the method. WARNING: do not retain any references to entry buffer past this method</param>
    /// <param name="options">Optional parallel loop options</param>
    /// <param name="skipCorruptPages">Skip corrupt data, if false then corrupt data throws an exception</param>
    public static void ParallelProcessRawEntryBatchesStartingAt(this ArchiveReader reader,
                                                         Bookmark start,
                                                         int batchBy,
                                                         Action<IEnumerable<Entry>, ParallelLoopState, long> body,
                                                         ParallelOptions options = null,
                                                         bool skipCorruptPages = false)
    {
      reader.NonNull(nameof(reader));
      body.NonNull(nameof(body));
      batchBy.IsTrue(v => v > 0, "batchBy < 1");

      var secondaryPage = false;
      foreach (var page in reader.GetPagesStartingAt(start.PageId, skipCorruptPages))
      {
        var batches = page.Entries
                          .Where(e => secondaryPage || e.Address >= start.Address)
                          .BatchBy(batchBy);

        bool wasCompleted;
        try
        {
          if (options == null)
            wasCompleted = Parallel.ForEach(batches, body).IsCompleted;
          else
            wasCompleted = Parallel.ForEach(batches, options, body).IsCompleted;
        }
        finally
        {
          reader.Recycle(page);
        }

        if (!wasCompleted) break;

        secondaryPage = true;
      }
    }

    /// <summary>
    /// Processes all pages in parallel batches of the specified size.
    /// The method is useful for performing CPU-intensive analysis of pages.
    /// WARNING: NEVER rely on page state, such as `Entry` buffer beyond the boundaries of `body` method. You can materialize valid entries by
    /// calling `Materialize(entry)` of the reader
    /// </summary>
    /// <param name="reader">Reader instance</param>
    /// <param name="startPageId">Starting pageId</param>
    /// <param name="batchBy">The size of entry batch. Batches get processed in-parallel</param>
    /// <param name="body">The body of the method. WARNING: do not retain any references to entry buffer past this method</param>
    /// <param name="options">Optional parallel loop options</param>
    /// <param name="skipCorruptPages">Skip corrupt data, if false then corrupt data throws an exception</param>
    public static void ParallelProcessPageBatchesStartingAt(this ArchiveReader reader,
                                                     long startPageId,
                                                     int batchBy,
                                                     Action<Page, ParallelLoopState, long> body,
                                                     ParallelOptions options = null,
                                                     bool skipCorruptPages = false)
    {
      reader.NonNull(nameof(reader));
      body.NonNull(nameof(body));
      batchBy = batchBy.KeepBetween(1, Environment.ProcessorCount);

      foreach (var pages in reader.GetPagesStartingAt(startPageId, skipCorruptPages).BatchBy(batchBy))
      {
        bool wasCompleted;
        try
        {
          if (options == null)
            wasCompleted = Parallel.ForEach(pages, body).IsCompleted;
          else
            wasCompleted = Parallel.ForEach(pages, options, body).IsCompleted;
        }
        finally
        {
          pages.ForEach(p => reader.Recycle(p));
        }

        if (!wasCompleted) break;
      }
    }


    /// <summary>
    /// Performs parallel processing of archive data sources supplied as enumerable of streams.
    /// The count of streams controls the degree of stream parallelism.
    /// The method delegates actual page-level work to `body` function which gets page-by-page stream as
    /// materialized from multiple volumes mounted for every input stream.
    /// </summary>
    /// <typeparam name="TReader">Type of stream reader</typeparam>
    /// <param name="dataSource">Enumerable of streams to process</param>
    /// <param name="crypto">Crypto provider used for volume mounting</param>
    /// <param name="startPageId">Starting page id - a position in the whole archive</param>
    /// <param name="readerFactory">Factory method making TReader instances</param>
    /// <param name="body">Worker body functor</param>
    /// <param name="cancel">Optional cancellation functor which returns null if the processing should stop</param>
    /// <param name="skipCorruptPages">False by default. When true skips archive page corruptions</param>
    public static void ParallelProcessVolumeBatchesStartingAt<TReader>(this IEnumerable<Stream> dataSource,
                                                              Security.ICryptoManager crypto,
                                                              long startPageId,
                                                              Func<IVolume, TReader> readerFactory,
                                                              Action<Page, TReader, Func<bool>> body,
                                                              Func<bool> cancel = null,
                                                              bool skipCorruptPages = false) where TReader : ArchiveReader
    {
      dataSource.NonNull(nameof(dataSource));
      crypto.NonNull(nameof(crypto));
      readerFactory.NonNull(nameof(readerFactory));
      body.NonNull(nameof(body));

      var readers = new List<TReader>();
      try
      {
        //mount all volumes
        foreach(var stream in dataSource)
        {
          if (stream==null || !stream.CanRead) continue;
          var volume = new DefaultVolume(crypto, stream, false);
          var reader = readerFactory(volume);
          readers.Add(reader);
        }

        readers.IsTrue( _ => readers.Count > 0, "dataSource non empty");

        var main = readers[0];

        foreach(var pageSet in main.Volume.ReadPageInfos(startPageId).BatchBy(readers.Count))
        {
          if (cancel != null && cancel()) break;

          var tasks = new List<Task>();

          foreach(var pair in pageSet.Select((pi, i) => new KeyValuePair<long, TReader>(pi.PageId, readers[i % readers.Count])))
          {
            if (cancel != null && cancel()) break;

            tasks.Add(Task.Factory.StartNew(objKvp =>
            {
              var kvp = (KeyValuePair<long, TReader>)objKvp;
              var reader = kvp.Value;
              Page page = null;
              try
              {
                page = reader.GetOnePageAt(kvp.Key, exactPageId: true);
              }
              catch
              {
                if (!skipCorruptPages) throw;
              }

              if (page != null)
              {
                try
                {
                  body(page, reader, cancel);
                }
                finally
                {
                  reader.Recycle(page);
                }
              }
            }, pair));//Task
          }

          Task.WaitAll(tasks.ToArray());
        }
      }
      finally
      {
        readers.ForEach(r => r.Volume.Dispose());
      }
    }



  }
}
