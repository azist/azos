/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Time
{
  /// <summary>
  /// Efficiently represents a list of minute-aligned time spans within a day
  /// which is typically used for operation hours/schedules
  /// </summary>
  public struct HourList : IEquatable<HourList>, IJsonWritable, IJsonReadable, IValidatable, IRequiredCheck, IConfigurationPersistent
  {
    public const int MINUTES_PER_DAY = 24 * 60;
    public const int MINUTES_PER_HALFDAY = 12 * 60;

    public struct Span : IEquatable<Span>, IComparable<Span>
    {

      internal Span(int start, int duration)
      {
        StartMinute = start;
        DurationMinutes = duration;
      }

      public readonly int StartMinute;
      public readonly int DurationMinutes;

      public int FinishMinute => StartMinute + DurationMinutes;
      public bool IsAssigned => StartMinute!=0 || DurationMinutes!=0;

      public string Start
      {
        get
        {
          var hr = StartMinute / 60;
          var min = StartMinute % 60;
          return "{0:D}:{1:D2}".Args(hr, min);
        }
      }

      public string Finish
      {
        get
        {
          var to = StartMinute + DurationMinutes;
          if (to > MINUTES_PER_DAY) to = to % MINUTES_PER_DAY;
          var hr = to / 60;
          var min = to % 60;
          return "{0:D}:{1:D2}".Args(hr, min);
        }
      }

      public override string ToString() => Start + "-" + Finish;
      public override int GetHashCode() => StartMinute;
      public override bool Equals(object obj) => obj is Span span ? this.Equals(span) : false;
      public bool Equals(Span other) => this.Start == other.Start && this.DurationMinutes == other.DurationMinutes;
      public int CompareTo(Span other) => this.StartMinute < other.StartMinute ? -1 : this.StartMinute > other.StartMinute ? +1 : 0;

      public static bool operator ==(Span a, Span b) =>  a.Equals(b);
      public static bool operator !=(Span a, Span b) => !a.Equals(b);
    }

    public static readonly char[] DELIMS = new[] { ',', ';' };

    public static HourList Parse(string v)
    {
      var data = parse(v);
      if (data==null) throw new TimeoutException(StringConsts.ARGUMENT_ERROR + " Bad hour list syntax, out of order, or overlap in `{0}`".Args(v.TakeFirstChars(64)));;
      var result = new HourList(v);
      result.m_Spans = data;
      return result;
    }

    public static bool TryParse(string v, out HourList result)
    {
      var data = parse(v);
      if (data == null)
      {
        result = default(HourList);
        return false;
      }

      result = new HourList(v);
      result.m_Spans = data;
      return true;
    }

    public HourList(string data)
    {
      Data = data;
      m_Spans = null;
    }

    [ConfigCtor]
    public HourList(IConfigAttrNode node)
    {
      Data = node.Value;
      m_Spans = null;
    }

    public ConfigSectionNode PersistConfiguration(ConfigSectionNode parentNode, string name)
    {
      parentNode.NonNull(nameof(parentNode)).AddAttributeNode(name.NonBlank(nameof(name)), Data);
      return parentNode;
    }

    private Span[] m_Spans;

    public readonly string Data;

    public bool IsAssigned => Data.IsNotNullOrWhiteSpace();

    public bool CheckRequired(string targetName) => IsAssigned;

    /// <summary>
    /// Returns ordered set of hour spans. Throws if data is assigned but invalid
    /// </summary>
    public IEnumerable<Span> Spans
    {
      get
      {
        if (m_Spans != null) return m_Spans;
        if (!IsAssigned) return Enumerable.Empty<Span>();
        parseState();
        return m_Spans;
      }
    }


    /// <summary>
    /// Returns count of spans or 0
    /// </summary>
    public int Count
    {
      get
      {
        if (m_Spans!=null) return m_Spans.Length;
        if (!IsAssigned) return 0;
        parseState();
        return m_Spans.Length;
      }
    }

    /// <summary>
    /// Provides indexer access to span collection. Default span is returned if index is out of bounds, check for Span.IsAssigned
    /// </summary>
    public Span this[int i]
    {
      get
      {
        if (i<0) return default(Span);
        if (m_Spans != null) return i < m_Spans.Length ? m_Spans[i] : default(Span);
        if (!IsAssigned) return default(Span);
        parseState();
        return i < m_Spans.Length ? m_Spans[i] : default(Span);
      }
    }


    /// <summary>
    /// Returns true if the time component of the specified DateTime is covered(included)
    /// by one of the HourList spans
    /// </summary>
    public bool IsCovered(DateTime when)
    {
      var ts = (int)when.TimeOfDay.TotalMinutes;
      return Spans.Any(s => s.StartMinute <= ts && s.FinishMinute >= ts);
    }


    public override bool Equals(object obj) => obj is HourList other ? this.Equals(other) : false;
    public override int GetHashCode() => Data==null ? 0 : Data.GetHashCode();
    public override string ToString() => Data.Default("<unassigned>");
    public bool Equals(HourList other) => this.Data.EqualsOrdSenseCase(other.Data);

    public static bool operator ==(HourList a, HourList b) =>  a.Equals(b);
    public static bool operator !=(HourList a, HourList b) => !a.Equals(b);

    /// <summary>
    /// Returns true when two instances contain representations of identical spans or both are unassigned.
    /// Throws is one of instances contain invalid data
    /// </summary>
    public bool IsEquivalent(HourList other) => this.IsAssigned==other.IsAssigned && (!IsAssigned || this.Spans.SequenceEqual(other.Spans));


    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
     => JsonWriter.EncodeString(wri, Data, options);

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data != null) return (true, new HourList(data.ToString()));
      return (false, null);
    }

    public ValidState Validate(ValidState state, string scope)
    {
      if (Data.IsNotNullOrWhiteSpace() && m_Spans==null)
      {
        m_Spans = parse(Data);
        if (m_Spans==null)
        {
          state = new ValidState(state, new ValidationException(StringConsts.TIME_HOURLIST_BAD_SPEC_FOR.Args(Data.TakeFirstChars(64), scope)));
        }
      }
      return state;
    }

    private void parseState()
    {
      m_Spans = parse(Data);
      if (m_Spans == null)
        throw new TimeException(StringConsts.TIME_HOURLIST_BAD_SPEC.Args(Data.TakeFirstChars(64)));
    }

    // 8:00-12:00,12:30pm-4:45pm,21:00-22:15
    // 8-1:30pm;14-18.5;23-2
    private static Span[] parse(string content)
    {
      if (content.IsNullOrWhiteSpace()) return null;

      var kvps = content.Split(DELIMS)
                      .Where(p => p.IsNotNullOrWhiteSpace())
                      .Select(p => p.SplitKVP('-'));

      var data = new List<Span>();
      foreach (var kvp in kvps)
      {
        var rawStart = parseComponent(kvp.Key);
        if (rawStart < 0) return null;
        var rawFinish = parseComponent(kvp.Value);
        if (rawFinish < 0) return null;
        var duration = rawFinish >= rawStart ? rawFinish - rawStart : (MINUTES_PER_DAY - rawStart) + rawFinish;
        var span = new Span(rawStart, duration);
        data.Add(span);
      }

      //order it time
      var result = data.OrderBy(s => s.StartMinute).ToArray();

      //check for overlap
      var start = 0;
      foreach (var span in result)
        if (span.StartMinute < start)
          return null;
        else
          start = span.FinishMinute;

      return result;
    }

    // 8:00    12:00  12:30pm 4:45pm 22:15
    // 8       12pm 14   18.5  2
    private static int parseComponent(string str)
    {
      if (str.IsNullOrWhiteSpace()) return -1;
      str = str.Trim();
      var result = 0;
      if (str.EndsWith("am", StringComparison.InvariantCultureIgnoreCase))
      {
        str = str.Substring(0, str.Length - 2);
        result = parseMinutes(str);
      }
      else if(str.EndsWith("pm", StringComparison.InvariantCultureIgnoreCase))
      {
        str = str.Substring(0, str.Length - 2);
        result = parseMinutes(str);

        if (result>=MINUTES_PER_HALFDAY) result -= MINUTES_PER_HALFDAY;//#713 20220628 JPK

        if (result>=0) result += MINUTES_PER_HALFDAY;
      }
      else result = parseMinutes(str);

      return result;
    }

    private static readonly IFormatProvider INVARIANT = CultureInfo.InvariantCulture;

    private static int parseMinutes(string str)
    {
      if (str.IsNullOrWhiteSpace()) return -1;
      var i = str.IndexOf(':');
      if (i<0)//decimal
      {
        if (double.TryParse(str, out var result)) return (int)(result * 60);
        return -1;
      }
      if (i == 0 || i == str.Length) return -1;

      if (!int.TryParse(str.Substring(0, i), NumberStyles.None, INVARIANT, out var h)) return -1;
      if (!int.TryParse(str.Substring(i+1), NumberStyles.None, INVARIANT, out var m)) return -1;
      if (h < 0 || h > 23) return -1;
      if (m < 0 || m > 59) return -1;

      return (h * 60) + m;
    }


  }
}
