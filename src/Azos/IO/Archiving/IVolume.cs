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
    /// Controls page split on writing. Does not affect reading directly as readers try to infer
    /// the average page size while reading through archived data
    /// </summary>
    int PageSizeBytes { get; set; }

    /// <summary>
    /// Reads one `PageInfo` object at the specified pageId.
    /// If the pageId is not exact then scrolls to the first readable page.
    /// Returns `!PageInfo.Assigned` for the EOF.
    /// </summary>
    PageInfo ReadPageInfo(long pageId);

    /// <summary>
    /// Lazily reads sequential `PageInfo` enumeration starting from the specified pageId.
    /// </summary>
    IEnumerable<PageInfo> ReadPageInfos(long pageId);

    /// <summary>
    /// Fills an existing page instance with archive data performing necessary decompression/decryption
    /// when needed. Returns a positive long value with the next adjacent `pageId` or a negative
    /// value to indicate the EOF condition. This method MAY be called by multiple threads at the same time
    /// (even over the same source stream which this class accesses). The implementor MAY perform internal
    /// cache/access coordination and tries to satisfy requests in a lock-free manner
    /// </summary>
    /// <param name="pageId">
    /// Requested pageId. Note: `page.PageId` will contain an actual `pageId` which may be fast-forwarded
    /// to the next readable block relative to the requested `pageId` if `exactPageId` is not set to true.
    /// </param>
    /// <param name="page">Existing page instance to fill with data</param>
    /// <param name="exactPageId">If true then will read the page exactly from the specified address</param>
    /// <returns>
    /// Returns a positive long value with the next adjacent `pageId` or a negative
    /// value to indicate the EOF condition. Throws on decipher/decompression or if bad page id was supplied when `exactPageId=true`
    /// </returns>
    /// <remarks>
    /// <para>
    /// If the supplied `pageId` is not pointing to a correct volume memory space (e.g. corrupt file data), AND
    /// `exactPageId=true` then the system scrolls to the fist subsequent readable page header, so you can compare the `page.PageId` with
    /// the requested value to detect any volume corruptions (when there are no corruptions both values are the same).
    /// Throws compression/decipher error if the underlying volume data is corrupted
    /// </para>
    /// </remarks>
    long ReadPage(long pageId, Page page, bool exactPageId = false);

    /// <summary>
    /// Appends the page at the end of volume. Returns the pageId of the appended page.
    /// The implementor MAY perform internal cache/access coordination and tries to satisfy requests in lock-free manner
    /// </summary>
    long AppendPage(Page page);
  }


  /// <summary>
  /// Provides archive page header information: (pageId, nextPageId, createUtc, app, host)
  /// </summary>
  public struct PageInfo : IEquatable<PageInfo>
  {
    /// <summary>The Exact pageId of the page header</summary>
    public long  PageId;

    /// <summary>The Exact pageId of the next adjacent page header</summary>
    public long  NextPageId;

    /// <summary>UTC timestamp of page creation </summary>
    public DateTime CreateUtc;

    /// <summary>Id of application which created the page</summary>
    public Atom  App;

    /// <summary>User/Host which created the page. The convention is to use optional user name: 'user@host'</summary>
    public string  Host;

    /// <summary> True when the structure is assigned (vs having default value)</summary>
    public bool Assigned => PageId > 0;

    public bool Equals(PageInfo other) => this.PageId == other.PageId &&
                                          this.NextPageId == other.NextPageId &&
                                          this.CreateUtc == other.CreateUtc &&
                                          this.App == other.App &&
                                          this.Host.EqualsOrdSenseCase(other.Host);
    public override bool Equals(object obj) => obj is PageInfo pi ? this.Equals(pi) : false;
    public override int GetHashCode() => this.PageId.GetHashCode();
    public override string ToString() => "@{0:x8}::{1:x8}".Args(PageId, NextPageId);

    public static bool operator ==(PageInfo lh, PageInfo rh) => lh.Equals(rh);
    public static bool operator !=(PageInfo lh, PageInfo rh) => !lh.Equals(rh);
  }

}
