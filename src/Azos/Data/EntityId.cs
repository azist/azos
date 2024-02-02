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
  /// A tuple of (SYSTEM: Atom, TYPE: Atom, SCHEMA: Atom, ADDRESS: string) used for identification of entities in business systems.
  /// The concept is somewhat similar to an "URI" in its intended purpose, as it identifies objects by an "Address"
  /// string which is interpreted in a scope of "Type/Schema", which in turn is in the scope of a "System".
  /// As a string, an EntityId is formatted like: `type.schema@system::address`, for example: `car.vin@dealer::1A8987339HBz0909W874`
  /// vs `boat.license@dealer::I9973OD`. The system qualifier is required, but type (and schema) qualifier is optional, which denotes "default type"
  /// for example: `dealer::I9973OD` is a valid EntityId pointing to a "dealer" system "car" type with "license" address schema by default.
  /// The optional schema sub-qualifier defines the "schema" of addressing used per type, this way you can identify the same entity types within a system with
  /// different addressing schemas
  /// </summary>
  public struct EntityId : IEquatable<EntityId>, IDistributedStableHashProvider, IJsonReadable, IJsonWritable, IRequiredCheck, IValidatable
  {
    //MUST be private and NOT derive from other JSON styles
    private static readonly JsonWritingOptions COMPOSITE_ADDRESS_JSON_FORMAT = new JsonWritingOptions
    {
      RowMapTargetName = null,
      MaxNestingLevel = 10,
      Purpose = JsonSerializationPurpose.Unspecified,
      EnableTypeHints = false,
      RowsAsMap = true,
      RowsetMetadata = false,
      ASCIITarget = false,
        IndentWidth = 0,
        MemberLineBreak = false,
        SpaceSymbols = false,
        ObjectLineBreak = false,
      NLSMapLanguageISO = CoreConsts.ISOA_LANG_ENGLISH,
    //=================================================
      MapSkipNulls = true,
      MapSortKeys = true,
      ISODates = true
    };

    public const string TP_PREFIX = "@";
    public const char   SCHEMA_DIV = '.';
    public const string SYS_PREFIX = "::";

    /// <summary> Empty/Unassigned instance </summary>
    public static readonly EntityId EMPTY = new EntityId();

    // Unsafe initializes an instance from deser, not for use from user code
    internal EntityId(string ____address, Atom sys, Atom type, Atom schema)
    {
      System = sys;
      Type = type;
      Schema = schema;
      Address = ____address;
    }

    /// <summary>
    /// Initializes an instance
    /// </summary>
    /// <param name="sys">System is required</param>
    /// <param name="type">Type is optional, so you can pass Atom.ZERO</param>
    /// <param name="schema">Optional address schema, or Atom.ZERO </param>
    /// <param name="address">Required entity address</param>
    public EntityId(Atom sys, Atom type, Atom schema, string address)
    {
      if (sys.IsZero) throw new CallGuardException(nameof(EntityId), nameof(sys), "Required sys.isZero");
      System = sys;
      Type = type;
      Schema = schema;
      Address = address.NonBlank(nameof(address));
    }

    /// <summary>
    /// Initializes an instance
    /// </summary>
    /// <param name="sys">System is required</param>
    /// <param name="type">Type is optional, so you can pass Atom.ZERO</param>
    /// <param name="compositeAddress">Required composite entity address - will be converted to ordered JSON string</param>
    /// <param name="schema">Optional address schema(or Atom.ZERO) </param>
    public EntityId(Atom sys, Atom type, object compositeAddress, Atom schema = new Atom())
    {
      if (sys.IsZero) throw new CallGuardException(nameof(EntityId), nameof(sys), "Required sys.isZero");
      System = sys;
      Type = type;
      Schema = schema;
      Address = compositeAddress.NonNull(nameof(compositeAddress)).ToJson(COMPOSITE_ADDRESS_JSON_FORMAT);
      IsCompositeAddress.IsTrue(nameof(IsCompositeAddress));
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
    /// Address schema. It may be Zero if not specified
    /// </summary>
    public readonly Atom Schema;

    /// <summary>
    /// Address for entity per Type and System
    /// </summary>
    public readonly string Address;

    /// <summary>
    /// Returns true if an address is assigned a composite json object - starts with '{' and ends with '}'
    /// without any leading/trailing spaces
    /// </summary>
    public bool IsCompositeAddress
    {
      get
      {
        if (Address.IsNullOrWhiteSpace()) return false;
        if (Address.StartsWith("{") && Address.EndsWith("}")) return true;
        return false;
      }
    }

    /// <summary>
    /// Returns a composite address as JsonDataMap. No value caching is performed.
    /// Null for empty/null address. Throws for non-composite address. <see cref="IsCompositeAddress"/>
    /// </summary>
    public JsonDataMap CompositeAddress //  type.sch@sys::{"a": 1,"b": "value"}
    {
      get
      {
        if (Address.IsNullOrWhiteSpace()) return null;
        IsCompositeAddress.IsTrue(nameof(IsCompositeAddress));
        var result = Address.JsonToDataObject()
                            .ExpectJsonDataMap();//may throw on invalid
        return result;
      }
    }

    /// <summary>
    /// True if the EntityId value is assigned
    /// </summary>
    public bool IsAssigned => !System.IsZero && Address.IsNotNullOrWhiteSpace();  //#794

    public bool CheckRequired(string targetName) => IsAssigned;

    public ValidState Validate(ValidState state, string scope = null)
    {
      if (IsCompositeAddress)
      {
        try
        {
          var map = CompositeAddress;
        }
        catch
        {
          state = new ValidState(state, new FieldValidationException(nameof(EntityId), scope.Default("<eid>"), "Invalid composite value", scope));
        }
      }
      return state;
    }

    /// <summary>
    /// Returns a string representation which can be used with Parse()/TryParse() calls
    /// </summary>
    public string AsString
    {
      get
      {
        if (!IsAssigned) return string.Empty;
        if (Type.IsZero) return System + SYS_PREFIX + Address;
        if (Schema.IsZero) return Type + TP_PREFIX + System + SYS_PREFIX + Address;
        return Type.ToString() + SCHEMA_DIV + Schema + TP_PREFIX + System + SYS_PREFIX + Address;
      }
    }

    public override string ToString() => AsString;//20230816 DKh JPK JGW // IsAssigned ? "{0}(`{1}`)".Args(GetType().Name, AsString) : string.Empty;

    public override int GetHashCode() => System.GetHashCode() ^
                                         Type.GetHashCode() ^
                                         Schema.GetHashCode() ^
                                         (Address==null ? 0 :  Address.GetHashCode());

    public ulong GetDistributedStableHash() => (System.GetDistributedStableHash() << 32) ^
                                                Type.GetDistributedStableHash() ^
                                                Schema.GetDistributedStableHash() ^
                                                ShardKey.ForString(Address);

    public override bool Equals(object obj) => obj is EntityId other ? Equals(other) : false;

    public bool Equals(EntityId other)
      => Type == other.Type &&
         Schema == other.Schema &&
         System == other.System &&
         Address.EqualsOrdSenseCase(other.Address);//addresses are CASE sensitive

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is string str)
      {
        if (TryParse(str, out var got)) return (true, got);
      }

      return (false, null);
    }

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
     => JsonWriter.EncodeString(wri, AsString, options, TypeHint.H_ENTITYID);

    public static EntityId Parse(string val)
    {
      if (TryParse(val, out var result)) return result;
      throw new DataException("Supplied value is not parsable as EntityId: `{0}`".Args(val.TakeFirstChars(48)));
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
      Atom schema = Atom.ZERO;
      var j = ssys.IndexOf(TP_PREFIX);
      if (j >= 0)
      {
        if (j == ssys.Length - 1) return false;
        var stp = ssys.Substring(0, j);
        ssys = ssys.Substring(j + TP_PREFIX.Length);

        var kvp = stp.SplitKVP(SCHEMA_DIV);

        if (!Atom.TryEncode(kvp.Key, out type)) return false;//unparsable type
        if (kvp.Value.IsNotNullOrEmpty())//there is Schema
        {
          if (!Atom.TryEncode(kvp.Value, out schema)) return false;//unparsable schema
        }
      }

      if (!Atom.TryEncode(ssys, out var sys)) return false;
      result = new EntityId(sys, type, schema, sadr);
      return true;
    }



    public static bool operator ==(EntityId a, EntityId b) => a.Equals(b);
    public static bool operator !=(EntityId a, EntityId b) => !a.Equals(b);

    public static implicit operator string(EntityId v) => v.AsString;
    public static implicit operator EntityId(string v) => Parse(v);
  }
}
