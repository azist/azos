/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

using Azos.Conf;
using Azos.Data;

namespace Azos.Serialization.JSON
{
  /// <summary>
  /// Represents a data transfer object (DTO) abstraction used to read/write JSON data
  /// </summary>
  public interface IJsonDataObject
  {
    object this[string key] { get; }
  }

  /// <summary>
  /// Represents a data transfer object (DTO) JSON map which associates string keys with object values
  /// implementing IDictionary&lt;string, object&gt; contract.
  /// This type is used in many parts of the framework acting as a schema-less DTO (data transfer object).
  /// Json serializer uses this type as CLR equivalent of JSON map: '{"key": "value", ...}'
  /// </summary>
  [Serializable]
  public class JsonDataMap : Dictionary<string, object>, IJsonDataObject
  {
    /// <summary>
    /// Turns URL encoded content into JSONDataMap
    /// </summary>
    public static JsonDataMap FromURLEncodedStream(Stream stream, Encoding encoding = null, bool caseSensitive = false)
    {
      using(var reader = encoding==null ? new StreamReader(stream, Encoding.UTF8, true, 1024, true)
                                        : new StreamReader(stream, encoding, true, 1024, true))
      {
        return FromURLEncodedString(reader.ReadToEnd(), caseSensitive);
      }
    }

    /// <summary>
    /// Turns URL encoded content into JSONDataMap
    /// </summary>
    public static JsonDataMap FromURLEncodedString(string content, bool caseSensitive = false)
    {
      var result = new JsonDataMap(caseSensitive);

      if (content.IsNullOrWhiteSpace()) return result;

      int queryLen = content.Length;
      int idx = 0;

      while (idx < queryLen)
      {
        int ampIdx = content.IndexOf('&', idx);
        int kvLen = (ampIdx != -1) ? ampIdx - idx : queryLen - idx;

        if (kvLen < 1)
        {
          idx = ampIdx + 1;
          continue;
        }

        int eqIdx = content.IndexOf('=', idx, kvLen);
        if (eqIdx == -1)
        {
          var key = Uri.UnescapeDataString(content.Substring(idx, kvLen).Replace('+',' '));
          result.Add(key, null);
        }
        else
        {
          int keyLen = eqIdx - idx;
          if (keyLen > 0)
          {
            var key = Uri.UnescapeDataString(content.Substring(idx, keyLen).Replace('+',' '));
            var val = Uri.UnescapeDataString(content.Substring(eqIdx + 1, kvLen - keyLen - 1).Replace('+',' '));

            result.Add(key, val);
          }
        }

        idx += kvLen + 1;
      }

      return result;
    }


    public JsonDataMap(): base(StringComparer.Ordinal)
    {
      CaseSensitive = true;
    }

    public JsonDataMap(bool caseSensitive) : base(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
    {
      CaseSensitive = caseSensitive;
    }

    protected JsonDataMap(SerializationInfo info,
                          StreamingContext context) : base(info.NonNull(nameof(info)).GetBoolean("csense") ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
    {
      CaseSensitive = Comparer == StringComparer.Ordinal;
      var data = info.GetValue("d", typeof(KeyValuePair<string, object>[])) as KeyValuePair<string, object>[];
      for(var i=0; i<data.Length; i++)
      {
        var kvp = data[i];
        this.Add(kvp.Key, kvp.Value);
      }
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("csense", CaseSensitive);
      var data = new KeyValuePair<string, object> [this.Count];
      var i = 0;
      foreach(var kvp in this)
      {
        data[i++] = kvp;
      }
      info.AddValue("d", data);
    }


    public readonly bool CaseSensitive;

    public new object this[string key]
    {
      get
      {
        object result;
        if (base.TryGetValue(key, out result)) return result;
        return null;
      }
      set
      {
        base[key] = value;
      }
    }


    /// <summary>
    /// Appends contents of another JSONDataMap for keys that do not exist in this one or null.
    /// Only appends references, does not provide deep reference copy
    /// </summary>
    public JsonDataMap Append(JsonDataMap other, bool deep = false)
    {
      if (other==null) return this;

      foreach(var kvp in other)
      {
        var here = this[kvp.Key];
        if (here==null)
          this[kvp.Key] = kvp.Value;
        else
          if (deep && here is JsonDataMap) ((JsonDataMap)here).Append(kvp.Value as JsonDataMap, deep);
      }

      return this;
    }

    public override string ToString()
    {
      return this.ToJson(JsonWritingOptions.Compact);
    }

    /// <summary>
    /// Returns this object as a config tree
    /// </summary>
    public ConfigSectionNode ToConfigNode(string rootName = null)
    {
      var mc = new LaconicConfiguration();
      mc.Create(rootName ?? GetType().Name);

      buildNode(mc.Root, this);

      return mc.Root;
    }

    private void buildNode(ConfigSectionNode node, JsonDataMap map)
    {
      foreach(var kvp in map)
      {
        var cmap = kvp.Value as JsonDataMap;
        if (cmap!=null)
        buildNode( node.AddChildNode(kvp.Key), cmap);
        else
        node.AddAttributeNode(kvp.Key, kvp.Value);
      }
    }
  }

  /// <summary>
  /// Represents a data transfer object (DTO) JSON array, that holds a list of values
  /// </summary>
  public class JsonDataArray : List<object>, IJsonDataObject
  {
    public JsonDataArray() {}
    public JsonDataArray(IEnumerable<object> other) : base(other) {}
    public JsonDataArray(int capacity) : base(capacity) {}

    /// <summary>
    /// Supports indexing access, treating string keys as integer
    /// </summary>
    public object this[string key] => this[key.AsInt(-1)];

    public override string ToString()
    {
      return this.ToJson(JsonWritingOptions.Compact);
    }
  }
}
