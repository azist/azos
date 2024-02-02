/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Time
{
  /// <summary>
  /// Provides a UTC timestamp-ordered list of items of type T constrained in size by design.
  /// This class is designed for business cases which need to store a history of model changes, e.g. customer name change history,
  /// consequently this class is not built for storing many items as typically less than 10 items will be stored.
  /// </summary>
  public sealed class UtcHistory<T> : IEnumerable<UtcHistory<T>.Entry>, IRequiredCheck, ILengthCheck, IJsonReadable, IJsonWritable
                            where T : IJsonWritable, IJsonReadable, new()
  {
    /// <summary>
    /// Entry tuple stored by UtcHistory class. Equatable an comparable only on Utc date
    /// </summary>
    public struct Entry : IEquatable<Entry>, IComparable<Entry>
    {
      internal Entry(DateTime utc, T d){ Utc = utc; Data = d;}
      public readonly DateTime Utc;
      public readonly T Data;
      public bool Assigned => Utc != default;
      public override bool Equals(object obj) => obj is Entry other ? Equals(other) : false;
      public override int GetHashCode() => Utc.GetHashCode();
      public bool Equals(Entry other) => this.Utc == other.Utc;
      public int CompareTo(Entry other) => this.Utc < other.Utc ? -1 : this.Utc == other.Utc ? 0 : +1;
    }

    public UtcHistory()
    {
      m_Data = new List<Entry>();
    }

    private List<Entry> m_Data;


    /// <summary> Count of entries </summary>
    public int Count => m_Data.Count;

    /// <summary>
    /// Provides a way to obtain an entry as of arbitrary point in UTC time.
    /// The system scans the history and finds the most adjacent record time-wise which logically covers the requested point in time.
    /// An <see cref="Entry"/> is returned, you can check its <see cref="Entry.Assigned"/> property.
    /// If nothing was found then entry is not assigned meaning that you are trying to get data which is BEFORE any known historical entry.
    /// </summary>
    public Entry this[DateTime asOfUtc]
    {
      get
      {
        utc(asOfUtc);

        Entry result = default;
        for (var i = 0; i < m_Data.Count; i++)
        {
          var itm = m_Data[i];
          if (itm.Utc <= asOfUtc)
          {
            result = itm;
            continue;
          } else break;
        }

        return result;
      }
    }

    /// <summary>
    /// Adds an entry as of date
    /// </summary>
    /// <param name="asOfUtc">Must be UTC</param>
    /// <param name="data">Data to store as of the specified history entry</param>
    public void Add(DateTime asOfUtc, T data) => Add(new Entry(asOfUtc, data));

    /// <summary>
    /// Adds an entry
    /// </summary>
    /// <param name="entry">Must be UTC</param>
    public void Add(Entry entry)
    {
      utc(entry.Utc);

      for (var i = 0; i < m_Data.Count; i++)
      {
        var itm = m_Data[i];
        if (itm.Utc >= entry.Utc)
        {
          m_Data.Insert(i, entry);
          return;
        }
      }

      m_Data.Add(entry);
    }

    /// <summary>
    /// Removes the first occurrence of data at the specific time returning true, or false when not found
    /// </summary>
    public bool Remove(DateTime specific)
    {
      utc(specific);
      for(var i=0; i < m_Data.Count; i++)
      {
        var itm = m_Data[i];
        if (itm.Utc == specific)
        {
          m_Data.RemoveAt(i);
          return true;
        }
      }
      return false;
    }

    private void utc(DateTime asof) => (asof.Kind == DateTimeKind.Utc).IsTrue("Need UTC");

    public IEnumerator<Entry> GetEnumerator() => m_Data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_Data.GetEnumerator();

    public bool CheckRequired(string targetName) => m_Data.Count > 0;

    public bool CheckMinLength(string targetName, int minLength) => m_Data.Count >= minLength;
    public bool CheckMaxLength(string targetName, int maxLength) => m_Data.Count <= maxLength;

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataArray dataArray)
      {
        m_Data.Clear();
        foreach(var map in dataArray.OfType<JsonDataMap>())
        {
          var utc = map["utc"].AsDateTime(CoreConsts.UTC_TIMESTAMP_STYLES);
          var d = new T();
          var (ok, dd) = d.ReadAsJson(map["d"], fromUI, options);
          if (ok) Add(utc, (T)dd);
        }
        return (true, this);
      }
      else return (false, null);
    }

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
      => JsonWriter.WriteArray(wri, m_Data.Select(one => new {utc = one.Utc, d = one.Data}), nestingLevel+1, options);
  }
}
