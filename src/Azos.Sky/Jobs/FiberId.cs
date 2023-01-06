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

namespace Azos.Sky.Jobs
{
  /// <summary>
  /// Identifies a job instance
  /// </summary>
  public struct FiberId : IEquatable<FiberId>, IDistributedStableHashProvider, IJsonWritable, IJsonReadable, IRequiredCheck, IValidatable
  {
    public const string UNASSIGNED = "<?>";

    public static readonly FiberId EMPTY = new FiberId(Atom.ZERO, GDID.ZERO);

    public FiberId(Atom rs, GDID gdid)
    {
      Runspace = rs;
      Gdid = gdid;
    }

    public readonly Atom Runspace;
    public readonly GDID Gdid;

    public bool Assigned => !Runspace.IsZero && !Gdid.IsZero;
    public bool CheckRequired(string targetName) => Assigned;
    public ValidState Validate(ValidState state, string scope = null) => Runspace.Validate(state, scope.Default("<JobId.Runspace>"));

    public override bool Equals(object obj) => obj is FiberId other ? this.Equals(other) : false;
    public bool Equals(FiberId other) => this.Runspace == other.Runspace && this.Gdid == other.Gdid;
    public override int GetHashCode() => Runspace.GetHashCode() ^ Gdid.GetHashCode();
    public ulong GetDistributedStableHash() => Runspace.GetDistributedStableHash() ^ Gdid.GetDistributedStableHash();

    public override string ToString() => Assigned ? $"{Runspace.Value}:{Gdid.ToString()}" : UNASSIGNED;// biz:0:3:27364

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

      // biz:0:3:27364
      var i = value.IndexOf(':');
      if (i < 1 || i == value.Length - 1) return false;

      var runspace = Atom.Encode(value.Substring(0, i));
      GDID gdid;
      if (!GDID.TryParse(value.Substring(i + 1), out gdid)) return false;

      id = new FiberId(runspace, gdid);
      return true;
    }


    public static bool operator ==(FiberId a, FiberId b) => a.Equals(b);
    public static bool operator !=(FiberId a, FiberId b) => !a.Equals(b);

  }
}
