/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Points to an archive entry
  /// </summary>
  public struct ArchivePointer : IEquatable<ArchivePointer>
  {
    public ArchivePointer(long pageOffset, int address)
    {
      PageOffset = pageOffset;
      Address = address;
    }

    /// <summary>
    /// Page ID - an offset of the page from the Archive beginning
    /// </summary>
    public readonly long PageOffset;

    /// <summary>
    /// An offset of entry relative to PageOffset
    /// </summary>
    public readonly int Address;

    public bool Assigned => PageOffset > 0;

    public bool Equals(ArchivePointer other) => this.PageOffset == other.PageOffset && this.Address == other.Address;
    public override bool Equals(object obj) => obj is ArchivePointer other ? this.Equals(other) : false;
    public override int GetHashCode() => (int)PageOffset ^ Address;

    public override string ToString() => PageOffset.ToString("X4") + ":" + Address.ToString("X4");

    public static bool operator ==(ArchivePointer l, ArchivePointer r) => l.Equals(r);
    public static bool operator !=(ArchivePointer l, ArchivePointer r) => !l.Equals(r);
  }
}
