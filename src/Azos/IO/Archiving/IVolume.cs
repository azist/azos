/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Provides an abstraction for accessing archive data. The data may be stored as a file,
  /// memory-mapped file, socket etc. The details are up to concrete implementations.
  /// By design the `Read` and `Append` operations are thread-safe as the reader fills a page
  /// copy which is private for every caller
  /// </summary>
  public interface IVolume : IDisposable
  {
    /// <summary>
    /// Controls page split on writing. Does not affect reading
    /// </summary>
    int PageSizeBytes { get; set; }


    PageInfo ReadPageInfo(long pageId);

    IEnumerable<PageInfo> PageInfos(long pageId);

    /// <summary>
    /// Fills an existing page instance with archive data performing necessary decompression/decryption
    /// when needed. Returns a positive long value with the next adjacent `pageId` or a negative
    /// value to indicate the EOF condition. This method MAY be called by multiple threads at the same time
    /// (even over the same source stream which this class accesses). The implementor MAY perform internal
    /// cache/access coordination and tries to satisfy requests in a lock-free manner
    /// </summary>
    /// <param name="pageId">
    /// Requested pageId. Note: `page.PageId` will contain an actual `pageId` which may be fast-forwarded
    /// to the next readable block relative to the requested `pageId`
    /// </param>
    /// <param name="page">Existing page instance to fill with data</param>
    /// <returns>
    /// Returns a positive long value with the next adjacent `pageId` or a negative
    /// value to indicate the EOF condition.
    /// </returns>
    /// <remarks>
    /// <para>
    /// If the supplied `pageId` is not pointing to a correct volume memory space (e.g. corrupt file data),
    /// then the system scrolls to the fist subsequent readable page, so you can compare the `page.PageId` with
    /// the requested value to detect any volume corruptions (when there are no corruptions both values are the same).
    /// </para>
    /// </remarks>
    long ReadPage(long pageId, Page page);

    /// <summary>
    /// Appends the page at the end of volume. Returns the pageId of the appended page.
    /// The implementor MAY perform internal cache/access coordination and tries to satisfy requests in lock-free manner
    /// </summary>
    long AppendPage(Page page);
  }


  /// <summary>
  /// Provides page information
  /// </summary>
  public struct PageInfo
  {
    public long     PageId;
    public long     NextPageId;
    public DateTime CreateUtc;
    public Atom     App;
    public string   Host;

    public bool Assigned => PageId > 0;
  }

  /// <summary>
  /// Abstracts storing page raw entry stream blocks in memory.
  /// The service provided is thread-safe by design
  /// </summary>
  public interface IPageCache
  {
    /// <summary>
    /// Enables the cache. Disabled cache does not store and does not find anything in it.
    /// Disabling cache does not lose items which are already stored, they just become "invisible" while cache is disabled
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// When set to greater than zero value imposes a time limit on buffer life in cache
    /// </summary>
    int LifeTimeSec{ get; set; }

    /// <summary>
    /// When set to greater than zero imposes a memory limit on the cache
    /// </summary>
    long MemoryLimit {  get; set; }


    /// <summary>
    /// Returns true if the cache contains the page without trying to fetch its data
    /// </summary>
    bool Contains(long pageId);

    /// <summary>
    /// Tries to get a page content by pageId
    /// </summary>
    bool TryGet(long pageId, MemoryStream pageData, out PageInfo info);

    /// <summary>
    /// Tries to get a page info only by pageId
    /// </summary>
    bool TryGet(long pageId, out PageInfo info);

    /// <summary>
    /// Puts data in cache
    /// </summary>
    void Put(long pageId, PageInfo info, ArraySegment<byte> content);
  }

}
