/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Azos.Data;
using Azos.Data.Idgen;
using Azos.Serialization.JSON;

namespace Azos.Sky.Fabric
{
  /// <summary>
  /// Identifies a job instance
  /// </summary>
  public struct FiberId : IEquatable<FiberId>, IDistributedStableHashProvider, IJsonWritable, IJsonReadable, IRequiredCheck, IValidatable
  {
    public const string UNASSIGNED = "<?>";

    public static readonly FiberId EMPTY = new FiberId(Atom.ZERO, Atom.ZERO, GDID.ZERO);

    public FiberId(Atom rs, Atom shard, GDID gdid)
    {
      Runspace = rs;
      MemoryShard = shard;
      Gdid = gdid;
    }

    public readonly Atom Runspace;
    public readonly Atom MemoryShard;
    public readonly GDID Gdid;

    public bool Assigned => !Runspace.IsZero && !MemoryShard.IsZero && !Gdid.IsZero;

    public bool CheckRequired(string targetName) => Assigned;

    public ValidState Validate(ValidState state, string scope = null)
      => MemoryShard.Validate(Runspace.Validate(state, scope.Default("<JobId.Runspace>")), scope.Default("<JobId.MemoryShard>"));

    public override bool Equals(object obj) => obj is FiberId other ? this.Equals(other) : false;

    public bool Equals(FiberId other) => this.Runspace == other.Runspace &&
                                         this.MemoryShard == other.MemoryShard &&
                                         this.Gdid == other.Gdid;

    public override int GetHashCode() => Runspace.GetHashCode() ^
                                         MemoryShard.GetHashCode() ^
                                         Gdid.GetHashCode();

    public ulong GetDistributedStableHash() => Runspace.GetDistributedStableHash() ^
                                               MemoryShard.GetDistributedStableHash() ^
                                               Gdid.GetDistributedStableHash();

    public override string ToString() => Assigned ? $"{Runspace.Value}::{MemoryShard.Value}->{Gdid.ToString()}" : UNASSIGNED;// biz::s1->0:1:2

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
     => JsonWriter.EncodeString(wri, ToString(), options);

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data == null) return (true, EMPTY);

      if (data is string str && TryParse(str, out var got)) return (true, got);

      return (false, null);
    }

    public static bool TryParse(string value, out FiberId id)
    {
      id = EMPTY;
      if (value.IsNullOrWhiteSpace()) return true;
      if (value.EqualsOrdSenseCase(UNASSIGNED)) return true;

      // biz::s1->0:1:2
      var irs = value.IndexOf("::");
      if (irs < 1 || irs == value.Length - 2) return false;

      var ipt = value.IndexOf("->", irs + 2);
      if (ipt < 1 || ipt == value.Length - 2) return false;

      if (!Atom.TryEncode(value.Substring(0, irs), out var runspace)) return false;
      if (!Atom.TryEncode(value.Substring(irs+2, ipt - irs - 2), out var shard)) return false;
      GDID gdid;
      if (!GDID.TryParse(value.Substring(ipt + 2), out gdid)) return false;

      id = new FiberId(runspace, shard, gdid);
      return true;
    }


    public static bool operator ==(FiberId a, FiberId b) => a.Equals(b);
    public static bool operator !=(FiberId a, FiberId b) => !a.Equals(b);

  }
}
