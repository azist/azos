/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;


namespace Azos.Apps
{
  /// <summary>
  /// Fast unique id based on GUID, but stored as two LONGS for speed.
  /// This simplifies bit conversion and transmission without making any copies.
  /// This struct is primarily used as internal implementation technique and regular users
  /// should use Guid instead
  /// </summary>
  public struct Fguid : IEquatable<Fguid>, IEquatable<Guid>
  {
    public Fguid(Guid guid) => (S1, S2) = IOUtils.CastGuidToLongs(guid);
    public Fguid(string guid) => (S1, S2) = IOUtils.CastGuidToLongs(Guid.Parse(guid.NonBlank(nameof(guid))));

    /// <summary> First 64 bit segment of Guid </summary>
    public readonly ulong S1;

    /// <summary> Second 64 bit segment of Guid </summary>
    public readonly ulong S2;

    public bool IsZero => S1 == 0 && S2 == 0;

    public Guid AsGuid => IOUtils.CastGuidFromLongs(S1, S2);

    public override string ToString() => AsGuid.ToString();

    public override bool Equals(object obj) => obj is Fguid other ? this.Equals(other) : false;

    public bool Equals(Fguid other) => this.S1 == other.S1 && this.S2 == other.S2;

    public override int GetHashCode() => (int)(S1 ^ S2);

    public bool Equals(Guid other) => this.AsGuid == other;

    public static bool operator == (Fguid a, Fguid b) => a.Equals(b);
    public static bool operator != (Fguid a, Fguid b) => !a.Equals(b);

    public static bool operator ==(Fguid a, Guid b) => a.Equals(b);
    public static bool operator !=(Fguid a, Guid b) => !a.Equals(b);

    public static bool operator ==(Guid a, Fguid b) => b.Equals(a);
    public static bool operator !=(Guid a, Fguid b) => !b.Equals(a);

    public static implicit operator Fguid(Guid guid) => new Fguid(guid);
    public static implicit operator Fguid(string guid) => new Fguid(guid);

    public static implicit operator Guid(Fguid tid) => tid.AsGuid;

  }
}
