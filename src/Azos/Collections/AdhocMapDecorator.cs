/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Azos.Serialization.JSON;

namespace Azos.Collections
{
  /// <summary>
  /// Implements a decorator pattern over a IDictionary&lt;string, object&gt; ensuring a controlled access to key/value pairs
  /// </summary>
  public class AdhocMapDecorator : IDictionary<string, object>, IJsonWritable, IJsonReadable
  {
    #region .ctor

    public AdhocMapDecorator(IDictionary<string, object> data)
      => m_Data = data.NonNull(nameof(data));

    #endregion

    #region Fields

    private IDictionary<string, object> m_Data;

    #endregion

    #region Properties

    public int Count => m_Data.Count;

    public bool IsReadOnly => m_Data.IsReadOnly;

    public ICollection<string> Keys => m_Data.Keys;

    public ICollection<object> Values => m_Data.Values;

    public object this[string key]
    {
      get => GetValue(key);
      set => SetValue(CheckKey(key), CheckValue(value));
    }

    #endregion

    #region Public

    IEnumerator IEnumerable.GetEnumerator() => m_Data.GetEnumerator();

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => m_Data.GetEnumerator();

    public void Clear() => m_Data.Clear();

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => m_Data.CopyTo(array, arrayIndex);

    public bool Remove(string key) => m_Data.Remove(key);

    public bool Remove(KeyValuePair<string, object> item) => m_Data.Remove(item);

    public bool TryGetValue(string key, out object value) => m_Data.TryGetValue(key, out value);

    public void Add(string key, object value) => m_Data.Add(CheckKey(key), CheckValue(value));

    public void Add(KeyValuePair<string, object> item) => m_Data.Add(Check(item));

    public bool Contains(KeyValuePair<string, object> item) => m_Data.Contains(Check(item));

    public bool ContainsKey(string key) => m_Data.ContainsKey(CheckKey(key));

    #endregion

    #region Protected

    protected KeyValuePair<string, object> Check(KeyValuePair<string, object> item)
      => new KeyValuePair<string, object>(CheckKey(item.Key), CheckValue(item.Value));

    protected virtual string CheckKey(string key) => key.NonBlank(nameof(key));

    protected virtual object CheckValue(object value) => value;

    protected virtual object GetValue(string key) => m_Data[key];

    protected virtual void SetValue(string key, object value) => m_Data[key] = value;

    #endregion

    #region IJsonReadable / IJsonWritable Members

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
      => JsonWriter.WriteMap(wri, m_Data.Select(kvp => new DictionaryEntry(kvp.Key, kvp.Value)), nestingLevel, options);

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        map.ForEach(kvp => Add(kvp));
        return (true, this);
      }

      return (false, null);
    }

    #endregion
  }
}
