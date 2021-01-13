/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Points to an archive entry on an archive page. A page is identified by a unique id (within archive volume)
  /// which is technically an offset of that page in an archive data stream.
  /// An entry is pointed-to using an "Address" offset relative to the page data stream start
  /// </summary>
  public struct Bookmark : IEquatable<Bookmark>
  {
    public Bookmark(long pageId, int address)
    {
      PageId = pageId;
      Address = address;
    }

    /// <summary>
    /// Page ID - an offset of the page from the Archive beginning
    /// </summary>
    public readonly long PageId;

    /// <summary>
    /// An offset of entry relative to page data stream
    /// </summary>
    public readonly int Address;

    /// <summary>
    /// True when the instance points to some data page
    /// </summary>
    public bool Assigned => PageId > 0;

    public bool Equals(Bookmark other) => this.PageId == other.PageId && this.Address == other.Address;
    public override bool Equals(object obj) => obj is Bookmark other ? this.Equals(other) : false;
    public override int GetHashCode() => (int)PageId ^ Address;

    public override string ToString() => PageId.ToString("X4") + ":" + Address.ToString("X4");

    public static bool operator ==(Bookmark l, Bookmark r) => l.Equals(r);
    public static bool operator !=(Bookmark l, Bookmark r) => !l.Equals(r);
  }
}
