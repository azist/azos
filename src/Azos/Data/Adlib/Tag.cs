/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Azos.Data;
using Azos.Data.Idgen;
using Azos.Serialization.JSON;

namespace Azos.Data.Adlib
{
  /// <summary>
  /// Tags get added to items for indexing.
  /// A tag can have either a string or numeric value
  /// </summary>
  public struct Tag : IEquatable<Tag>, IJsonReadable, IJsonWritable
  {
    public Tag(Atom prop, string value)
    {
      Prop = prop;
      SValue = value.NonBlankMax(64, nameof(value));
      NValue = 0;
    }

    public Tag(Atom prop, long value)
    {
      Prop = prop;
      SValue = null;
      NValue = value;
    }

    public readonly Atom    Prop;
    public readonly string  SValue;
    public readonly long    NValue;

    public bool IsText => SValue != null;

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
        if (sv.IsNotNullOrWhiteSpace()) return (true, new Tag(prop, sv));
        if (map["n"] is long lv) return (true, new Tag(prop, lv));
      }

      return (false, null);
    }

    public static bool operator ==(Tag left, Tag right) => left.Equals(right);
    public static bool operator !=(Tag left, Tag right) => !left.Equals(right);
  }

}

