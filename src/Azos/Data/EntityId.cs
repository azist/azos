/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data.Idgen;
using Azos.Serialization.JSON;

using System;
using System.IO;

namespace Azos.Data
{
  /// <summary>
  /// A tuple of (SYSTEM: Atom, TYPE: Atom, ADDRESS: string) used for identification of entities in business systems.
  /// The concept is somewhat similar to an "URI" in its intended purpose, as it identifies objects by an "Address"
  /// string which is interpreted in a scope of "Type", which in turn is in the scope of a "System".
  /// As a string, an EntityId is formatted like: `type@system::address`, for example: `vin@car::1A8987339HBz0909W874`
  /// vs `license@car::I9973OD`. The system qualifier is required, but type qualifier is optional, which denotes "default type"
  /// for example: `car::I9973OD` is a valid EntityId pointing to a "car" system "license" type by default
  /// </summary>
  public struct EntityId : IEquatable<EntityId>, IDistributedStableHashProvider, IJsonReadable, IJsonWritable, IRequiredCheck
  {
    public const string TP_PREFIX = "@";
    public const string SYS_PREFIX = "::";

    /// <summary>
    /// Initializes an instance
    /// </summary>
    /// <param name="sys">System is required</param>
    /// <param name="type">Type is optional, so you can pass Atom.ZERO</param>
    /// <param name="address">Required entity address</param>
    public EntityId(Atom sys, Atom type, string address)
    {
      if (sys.IsZero) throw new CallGuardException(nameof(EntityId), nameof(sys), "Required sys.isZero");
      System = sys;
      Type = type;
      Address = address.NonBlank(nameof(address));
    }

    /// <summary>
    /// System id, non zero for assigned values
    /// </summary>
    public readonly Atom System;

    /// <summary>
    /// Entity type. It may be Zero for a default type
    /// </summary>
    public readonly Atom Type;

    /// <summary>
    /// Address for entity per Type and System
    /// </summary>
    public readonly string Address;

    /// <summary>
    /// True if the EntityId value is assigned
    /// </summary>
    public bool IsAssigned => !System.IsZero;

    public bool CheckRequired(string targetName) => IsAssigned;

    /// <summary>
    /// Returns a string representation which can be used with Parse()/TryParse() calls
    /// </summary>
    public string AsString => IsAssigned ? (Type.IsZero ? System + SYS_PREFIX + Address : Type + TP_PREFIX + System + SYS_PREFIX + Address) : string.Empty;

    public override string ToString() => IsAssigned ? "{0}(`{1}`)".Args(GetType().Name, AsString) : string.Empty;

    public override int GetHashCode() => System.GetHashCode() ^ Type.GetHashCode() ^ (Address==null ? 0 :  Address.GetHashCode());

    public ulong GetDistributedStableHash() => (System.GetDistributedStableHash() << 32) ^
                                                Type.GetDistributedStableHash() ^
                                                ShardKey.ForString(Address);

    public override bool Equals(object obj) => obj is EntityId other ? Equals(other) : false;

    public bool Equals(EntityId other)
      => Type == other.Type &&
         System == other.System &&
         Address.EqualsOrdSenseCase(other.Address);

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is string str)
      {
        if (TryParse(str, out var got)) return (true, got);
      }

      return (false, null);
    }

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
     => JsonWriter.EncodeString(wri, AsString, options);

    public static EntityId Parse(string val)
    {
      if (TryParse(val, out var result)) return result;
      throw new DataException("Supplied value is not parable as EntityId: `{0}`".Args(val.TakeFirstChars(32)));
    }

    public static bool TryParse(string val, out EntityId result)
    {
      result = default(EntityId);
      if (val.IsNullOrWhiteSpace()) return true;

      var i = val.IndexOf(SYS_PREFIX);
      if (i < 1 || i == val.Length - SYS_PREFIX.Length) return false;
      var ssys = val.Substring(0, i);
      var sadr = val.Substring(i + SYS_PREFIX.Length);

      if (sadr.IsNullOrWhiteSpace()) return false;

      Atom type = Atom.ZERO;
      var j = ssys.IndexOf(TP_PREFIX);
      if (j >= 0)
      {
        if (j == ssys.Length - 1) return false;
        var stp = ssys.Substring(0, j);
        if (!Atom.TryEncode(stp, out type)) return false;
        ssys = ssys.Substring(j + TP_PREFIX.Length);
      }

      if (!Atom.TryEncode(ssys, out var sys)) return false;
      result = new EntityId(sys, type, sadr);
      return true;
    }

    public static bool operator ==(EntityId a, EntityId b) => a.Equals(b);
    public static bool operator !=(EntityId a, EntityId b) => !a.Equals(b);

    public static implicit operator string(EntityId v) => v.AsString;
    public static implicit operator EntityId(string v) => Parse(v);
  }
}
