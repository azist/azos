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
  /// Points to a volume page by (guid volumeId, long pageId)
  /// </summary>
  public struct VolumePagePtr : IEquatable<VolumePagePtr>
  {
    public VolumePagePtr(Guid vid, long pageId)
    {
      VolumeId = vid;
      PageId = pageId;
    }

    public readonly Guid VolumeId;
    public readonly long PageId;

    public bool Equals(VolumePagePtr other) => this.VolumeId == other.VolumeId && this.PageId == other.PageId;
    public override bool Equals(object obj) => obj is VolumePagePtr vpp ? this.Equals(vpp) : false;
    public override int GetHashCode() => (int)PageId;
    public override string ToString() => "{0}::{1:x8}".Args(VolumeId, PageId);

    public bool Assigned => VolumeId != Guid.Empty;

    public static bool operator ==(VolumePagePtr lp, VolumePagePtr rp) => lp.Equals(rp);
    public static bool operator !=(VolumePagePtr lp, VolumePagePtr rp) => !lp.Equals(rp);
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
    int LifeTimeSec { get; set; }

    /// <summary>
    /// When set to greater than zero imposes a memory limit on the cache.
    /// Some caches measure limits in item count
    /// </summary>
    long MemoryLimit {  get; set; }

    /// <summary>
    /// Returns true if the cache contains the page without trying to fetch its data
    /// </summary>
    bool Contains(VolumePagePtr pPage);

    /// <summary>
    /// Tries to get a page content by pageId
    /// </summary>
    bool TryGet(VolumePagePtr pPage, MemoryStream pageData, out PageInfo info);

    /// <summary>
    /// Tries to get a page info only by pageId
    /// </summary>
    bool TryGet(VolumePagePtr pPage, out PageInfo info);

    /// <summary>
    /// Puts data in cache
    /// </summary>
    void Put(VolumePagePtr pPage, PageInfo info, ArraySegment<byte> content);

    /// <summary>
    /// Clears all data for the specified volume id.
    /// Pass Guid.Empty to clear all caches for all volumes
    /// </summary>
    void Clear(Guid idVolume);

    /// <summary>
    /// Gets ids of volumes which have cached data
    /// </summary>
    IEnumerable<Guid> VolumeIds{  get; }
  }

}
