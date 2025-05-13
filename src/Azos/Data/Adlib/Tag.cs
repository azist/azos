/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.IO;

using Azos.Conf;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Data.Adlib
{
  /// <summary>
  /// A tag represents a (key: Atom, value: string|long) tuple having a purposely restricted key and value domains for
  /// efficiency and control. A tag can have either a string or long numeric value.
  /// You can store decimal/float values as scaled longs. This is done on purpose to limit
  /// the value domain - to ensure an efficient storage and indexing of tags.
  /// In Adlib data library, tags get added to items for indexing.
  /// Azos uses tags in places where ad hoc properties are required.
  /// The tags are typically used for indexing and searches
  /// </summary>
  public struct Tag : IEquatable<Tag>, IJsonReadable, IJsonWritable, IRequiredCheck, IValidatable, IConfigurationPersistent
  {
    public Tag(Atom prop, string value)
    {
      Prop = prop.IsValidNonZero(nameof(prop));
      SValue = value.NonBlankMax(Constraints.MAX_TAG_SVAL_LENGTH, nameof(value));
      NValue = 0;
    }

    public Tag(Atom prop, long value)
    {
      Prop = prop.IsValidNonZero(nameof(prop));
      SValue = null;
      NValue = value;
    }

    public Tag(Atom prop, TagVal value)
    {
      Prop = prop.IsValidNonZero(nameof(prop));
      SValue = value.SValue;
      NValue = value.NValue;
    }

    public Tag(BixReader reader, int formatVersion = 0)
    {
      Prop = reader.ReadAtom();
      SValue = reader.ReadString();
      NValue = reader.ReadLong();
    }

    public Tag(IConfigSectionNode cfg)
    {
      if (cfg == null)
      {
        Prop   = default;
        SValue = default;
        NValue = default;
        return;
      }

      Prop = cfg.Of("p").ValueAsAtom(Atom.ZERO);
      SValue = cfg.Of("s").ValueAsString(null);
      NValue = cfg.Of("n").ValueAsLong(0);
    }


    public void Write(BixWriter wri, int formatVersion = 0)
    {
      wri.Write(Prop);
      wri.Write(SValue);
      wri.Write(NValue);
    }


    private Tag(Atom prop, string svalue, long lvalue)
    {
      Prop = prop;
      SValue = svalue;
      NValue = lvalue;
    }


    public readonly Atom    Prop;
    public readonly string  SValue;
    public readonly long    NValue;

    public bool IsText => SValue != null;
    public bool IsAssigned => !Prop.IsZero;

    public bool CheckRequired(string targetName) => IsAssigned;


    public ValidState Validate(ValidState state, string scope = null)
    {
      if (Prop.IsZero || !Prop.IsValid)
      {
        state = new ValidState(state, new FieldValidationException(nameof(Tag), nameof(Prop), "Valid prop atom", scope));
      }

      if (SValue != null && SValue.Length > Constraints.MAX_TAG_SVAL_LENGTH)
      {
        state = new ValidState(state,
                      new FieldValidationException(nameof(Tag),
                        nameof(SValue),
                        "SValue len exceeds max of {0}".Args(Azos.IOUtils.FormatByteSizeWithPrefix(Constraints.MAX_TAG_SVAL_LENGTH)),
                        scope
                      )
                    );
      }
      return state;
    }

    public override string ToString() => IsAssigned ? (IsText ? $"Tag({Prop}=`{SValue}`)" : $"Tag({Prop}={NValue}") : "Tag(<unassigned>)";
    public bool Equals(Tag other) => this.Prop == other.Prop && this.SValue.EqualsOrdSenseCase(other.SValue) && this.NValue == other.NValue;
    public override bool Equals(object obj) => obj is Tag tag ? this.Equals(tag) : false;
    public override int GetHashCode() => Prop.GetHashCode() ^ ((SValue != null) ? SValue.GetHashCode() : NValue.GetHashCode());

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
    {
      JsonWriter.WriteMap(wri, nestingLevel, options,
             new DictionaryEntry("p", Prop),
             SValue != null ? new DictionaryEntry("s", SValue) : new DictionaryEntry("n", NValue));
    }

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        if (!Atom.TryEncode(map["p"].AsString(), out var prop)) return (false, null);

        var sv = map["s"].AsString();
        if (sv.IsNotNullOrWhiteSpace()) return (true, new Tag(prop, sv, 0));

        var nv = map["n"];
        if (nv is int iv) return (true, new Tag(prop, null, iv));
        if (nv is long lv) return (true, new Tag(prop, null, lv));
      }

      return (false, null);
    }

    public ConfigSectionNode PersistConfiguration(ConfigSectionNode parentNode, string name)
    {
      var result = parentNode.AddChildNode(name);

      result.AddAttributeNode("p", this.Prop);

      if (this.SValue != null)
        result.AddAttributeNode("s", this.SValue);
      else
        result.AddAttributeNode("n", this.NValue);

      return result;
    }

    public static bool operator ==(Tag left, Tag right) => left.Equals(right);
    public static bool operator !=(Tag left, Tag right) => !left.Equals(right);
  }


  /// <summary>
  /// This struct represents a value variant which is (value: string|long) tuple having a purposely restricted value domains for
  /// efficiency and control. A tag value can either be a string or a long numeric scalar.
  /// You can store decimal/float values as scaled longs. This is done on purpose to limit
  /// the value domain - to ensure an efficient storage and indexing of tags.
  /// In Adlib data library, tags with their values get added to items for indexing.
  /// Azos uses tags in places where ad hoc properties are required.
  /// The tags are typically used for indexing and searches
  /// </summary>
  public struct TagVal : IEquatable<TagVal>, IJsonReadable, IJsonWritable, IValidatable
  {
    public TagVal(string value)
    {
      SValue = value.NonBlankMax(Constraints.MAX_TAG_SVAL_LENGTH, nameof(value));
      NValue = 0;
    }

    public TagVal(long value)
    {
      SValue = null;
      NValue = value;
    }

    public TagVal(BixReader reader, int formatVersion = 0)
    {
      SValue = reader.ReadString();
      NValue = reader.ReadLong();
    }

    public void Write(BixWriter wri, int formatVersion = 0)
    {
      wri.Write(SValue);
      wri.Write(NValue);
    }

    private TagVal(string svalue, long lvalue)
    {
      SValue = svalue;
      NValue = lvalue;
    }


    public readonly string SValue;
    public readonly long NValue;

    public bool IsText => SValue != null;

    public ValidState Validate(ValidState state, string scope = null)
    {
      if (SValue != null && SValue.Length > Constraints.MAX_TAG_SVAL_LENGTH)
      {
        state = new ValidState(state,
                                new FieldValidationException(nameof(Tag),
                                                             nameof(SValue),
                                                             "SValue len exceeds max of {0}".Args(Azos.IOUtils.FormatByteSizeWithPrefix(Constraints.MAX_TAG_SVAL_LENGTH))
                                                            )
                              );
      }
      return state;
    }

    public override string ToString() => IsText ? $"`{SValue}`" : $"{NValue}";
    public bool Equals(TagVal other) => this.SValue.EqualsOrdSenseCase(other.SValue) && this.NValue == other.NValue;
    public override bool Equals(object obj) => obj is TagVal tval ? this.Equals(tval) : false;
    public override int GetHashCode() => (SValue != null) ? SValue.GetHashCode() : NValue.GetHashCode();

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
    {
      if (SValue != null)
        JsonWriter.EncodeString(wri, SValue, options);
      else
        wri.Write(NValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is string sv) return (true, new TagVal(sv));
      if (data is long lv) return (true, new TagVal(lv));
      if (data is int iv) return (true, new TagVal(iv));
      return (false, null);
    }


    public static bool operator ==(TagVal left, TagVal right) => left.Equals(right);
    public static bool operator !=(TagVal left, TagVal right) => !left.Equals(right);
  }

}

