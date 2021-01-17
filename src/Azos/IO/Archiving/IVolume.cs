/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
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

    /// <summary>
    /// Fills the page instance with archive data performing necessary decompression/decryption
    /// when needed. Returns a positive long value with the next adjacent `pageId` or a negative
    /// value to indicate the EOF condition. This method MAY be called by multiple threads at the same time
    /// (even over the same source stream which this class accesses). The implementor MAY perform internal
    /// cache/access coordination and tries to satisfy requests in lock-free manner
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the supplied `pageId` is not pointing to a correct volume memory space (e.g. corrupt file data),
    /// then the system scrolls to the fist subsequent readable page, so you can compare the `page.PageId` with
    /// the requested value to detect any volume corruptions (when there are not corruptions both values are the same).
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
  /// Abstracts storing page raw entry stream blocks in memory.
  /// The service provided is thread-safe by design
  /// </summary>
  public interface IPageCache
  {
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
    bool TryGet(long pageId, MemoryStream pageData);

    /// <summary>
    /// Puts data in cache
    /// </summary>
    void Put(long pageId, Subarray<byte> content);//or byte[]?
  }

}
