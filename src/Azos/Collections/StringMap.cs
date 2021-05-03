/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Collections
{
  /// <summary>
  /// Efficiently maps string -> string for serialization.
  /// Compared to Dictionary[string,string] this class yields 20%-50% better Slim serialization speed improvement and 5%-10% space improvement
  /// </summary>
  [Serializable]
  public sealed class StringMap : IDictionary<string, string>, IJsonWritable, IJsonReadable
  {
    internal static Dictionary<string, string> MakeDictionary(bool senseCase)
      => new Dictionary<string, string>(senseCase ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase);

    private StringMap(){ }

    internal StringMap(bool senseCase, Dictionary<string, string> data)
    {
      m_CaseSensitive = senseCase;
      m_Data = data;
    }

    public StringMap(bool senseCase = true)
    {
      m_CaseSensitive = senseCase;
      m_Data = MakeDictionary(senseCase);
    }

    private bool m_CaseSensitive;
    private Dictionary<string, string> m_Data;

    public bool CaseSensitive => m_CaseSensitive;

    public void Add(string key, string value) => m_Data.Add(key, value);

    public bool ContainsKey(string key) => m_Data.ContainsKey(key);

    public ICollection<string> Keys => m_Data.Keys;

    public bool Remove(string key) => m_Data.Remove(key);

    public bool TryGetValue(string key, out string value)
    => m_Data.TryGetValue(key, out value);

    public ICollection<string> Values => m_Data.Values;

    public string this[string key]
    {
      get
      {
        if (m_Data.TryGetValue(key, out string result)) return result;
        return null;
      }
      set { m_Data[key] = value; }
    }

    public void Add(KeyValuePair<string, string> item)
    => ((IDictionary<string, string>)m_Data).Add(item);

    public void Clear() => m_Data.Clear();

    public bool Contains(KeyValuePair<string, string> item) => ((IDictionary<string, string>)m_Data).Contains(item);

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    => ((IDictionary<string, string>)m_Data).CopyTo(array, arrayIndex);

    public int Count => m_Data.Count;

    public bool IsReadOnly => ((IDictionary<string, string>)m_Data).IsReadOnly;

    public bool Remove(KeyValuePair<string, string> item)
    => ((IDictionary<string, string>)m_Data).Remove(item);

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    => m_Data.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      => m_Data.GetEnumerator();

    void IJsonWritable.WriteAsJson(System.IO.TextWriter wri, int nestingLevel, JsonWritingOptions options)
      => JsonWriter.WriteMap(wri, m_Data, nestingLevel, options);

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data==null) return (true, null);
      if (data is JsonDataMap map)
      {
        if (m_Data==null)
          m_Data = MakeDictionary(m_CaseSensitive);
        else
          m_Data.Clear();

        foreach(var entry in map)
          m_Data[entry.Key] = entry.Value.AsString();

        return (true, this);
      }

      return (false, null);
    }
  }
}
