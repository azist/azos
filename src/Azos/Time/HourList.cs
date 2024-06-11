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
using System.Text;
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

    /// <summary>
    /// Represents a minute time span within a day.
    /// Note: start may not be beyond MINUTES_PER_DAY, however start+duration may extend into the next day
    /// </summary>
    public struct Span : IEquatable<Span>, IComparable<Span>
    {

      internal Span(int start, int duration)
      {
        (start <= MINUTES_PER_DAY).IsTrue("start <= MINUTES_PER_DAY");
        (duration >= 0 && duration < MINUTES_PER_DAY * 2).IsTrue("duration >= 0 && < MIN_PER-DAY*2");
        StartMinute = start;
        DurationMinutes = duration;
      }

      /// <summary>
      /// Start minute within a day. This value is always &lt;= to MINUTES_PER_DAY.
      /// This minute includes the first minute of duration, so a span with 1 minute duration has its `StartMinute` equal to its `FinishMinute`
      /// </summary>
      public readonly int StartMinute;

      /// <summary>
      /// Duration of the span within a day.
      /// This is always a positive number. A value of 0 signifies an empty span
      /// A one minute duration span has its `StartMinute` equal to its `FinishMinute`
      /// </summary>
      public readonly int DurationMinutes;

      /// <summary>
      /// Finish minute relative to the day of start.
      /// This minute includes the last minute of duration, so a 1min span has its `FinishMinute` equal to its `StartMinute`
      /// Note: this can extend beyond the original day.
      /// -1 is retuned for empty spans of zero duration
      /// </summary>
      public int FinishMinute => DurationMinutes > 0 ? StartMinute + DurationMinutes - 1 : -1;

      /// <summary>
      /// True if the span duration extends past original day 24 period.
      /// For example:  23pm-3am working hour window starts at the day end but finishes at the NEXT day start
      /// </summary>
      public bool EndsTheNextDay => FinishMinute > MINUTES_PER_DAY;

      /// <summary>
      /// True when this structure was assigned a value: either a start minute or duration is set
      /// </summary>
      public bool IsAssigned => StartMinute != 0 || DurationMinutes != 0;

      /// <summary>
      /// Start time string specifier, such as `14:02` using a 24 hr clock
      /// </summary>
      public string Start
      {
        get
        {
          var hr = StartMinute / 60;
          var min = StartMinute % 60;
          return "{0:D}:{1:D2}".Args(hr, min);
        }
      }

      /// <summary>
      /// Finish time string specifier, such as `23:09` using a 24 hr clock.
      /// Empty string for an unassigned instance
      /// </summary>
      public string Finish
      {
        get
        {
          var to = FinishMinute;
          if (to < 0) return string.Empty;
          if (to > MINUTES_PER_DAY) to = to % MINUTES_PER_DAY;
          var hr = to / 60;
          var min = to % 60;
          return "{0:D}:{1:D2}".Args(hr, min);
        }
      }

      /// <summary>
      /// Span string specifier using a 24 hr clock, such as `13:00-16:59`
      /// </summary>
      public override string ToString() => IsAssigned ? Start + "-" + Finish : string.Empty;

      public override int GetHashCode() => StartMinute;
      public override bool Equals(object obj) => obj is Span span ? this.Equals(span) : false;
      public bool Equals(Span other) => this.Start == other.Start && this.DurationMinutes == other.DurationMinutes;
      public int CompareTo(Span other) => this.StartMinute < other.StartMinute ? -1 : this.StartMinute > other.StartMinute ? +1 : 0;

      /// <summary>
      /// Returns true if both spans are assigned and intersect in time
      /// </summary>
      public bool IntersectsWith(Span other)
      {
        if (!IsAssigned || !other.IsAssigned) return false;
        return (this.DurationMinutes > 0) &&
               (other.DurationMinutes > 0) &&
               (other.StartMinute <= this.FinishMinute) &&
               (other.FinishMinute >= this.StartMinute);
      }

      /// <summary>
      /// Returns an intersection span of this span with another, or an unassigned span if they do not intersect
      /// </summary>
      public Span Intersect(Span other)
      {
        if (!IsAssigned || !other.IsAssigned) return default;
        if (other.FinishMinute < this.StartMinute) return default;
        if (other.StartMinute > this.FinishMinute) return default;

        if (other.StartMinute < this.StartMinute)
          return new Span(this.StartMinute, 1 + Math.Min(this.FinishMinute, other.FinishMinute) - this.StartMinute);
        else
          return new Span(other.StartMinute, 1 + Math.Min(other.FinishMinute, this.FinishMinute) - other.StartMinute);
      }

      /// <summary>
      /// Returns true if THIS span completely contains (covers) the other span, that is: the other span intersects with this one
      /// and it is smaller than or the same size as this one
      /// </summary>
      public bool CoversAnother(Span other)
      {
        if (!IsAssigned || !other.IsAssigned) return false;
        return (other.StartMinute >= this.StartMinute && other.FinishMinute <= this.FinishMinute);
      }


      /// <summary>
      /// Returns either a single span which is this span with subtracted other span, or two spans which
      /// come out of this span with the other span cut-out of the original span.
      /// Note that the 2nd span, when it is returned, may or may not start at the NEXT day, check for `bIsNextDay` flag
      /// </summary>
      public (Span a, Span b, bool bIsNextDay) Exclude(Span other)
      {
        if (!IsAssigned) return default;
        if (!other.IsAssigned) return (this, default, false);
        if (other.FinishMinute < this.StartMinute) return (this, default, false);
        if (other.StartMinute > this.FinishMinute) return (this, default, false);

        if (other.CoversAnother(this)) return default;//nothing left as other covers this one

        //Split in 2
        if (this.CoversAnother(other) &&
            other.StartMinute > this.StartMinute &&
            other.FinishMinute < this.FinishMinute)
        {
          var ra = new Span(this.StartMinute, other.StartMinute - this.StartMinute);

          var bStart = other.FinishMinute + 1;
          var bIsNextDay = bStart >= MINUTES_PER_DAY;
          if (bIsNextDay) bStart-=MINUTES_PER_DAY;
          var rb = new Span(bStart, this.FinishMinute - other.FinishMinute);
          return (ra, rb, bIsNextDay);
        }
        else //produce 1
        {
          //at left?
          if (other.StartMinute <= this.StartMinute)
            return (new Span(other.FinishMinute + 1, this.FinishMinute - other.FinishMinute), default, false);

          //at right
          return (new Span(this.StartMinute, other.StartMinute - this.StartMinute), default, false);
        }
      }


      /// <summary>
      /// Returns a single new span which covers both the original and the other spans AND any time in between (if any)
      /// </summary>
      public Span Join(Span other)
      {
        if (!IsAssigned) return other;
        if (!other.IsAssigned) return this;

        var s = Math.Min(this.StartMinute, other.StartMinute);
        return new Span(s, 1 + Math.Max(this.FinishMinute, other.FinishMinute) - s);
      }

      public static bool operator ==(Span a, Span b) =>  a.Equals(b);
      public static bool operator !=(Span a, Span b) => !a.Equals(b);
    }//span

    public static readonly char[] DELIMS = new[] { ',', ';' };

    public static HourList Parse(string v)
    {
      var data = parse(v);
      if (data==null) throw new TimeoutException(StringConsts.ARGUMENT_ERROR + " Bad hour list syntax, out of order, or overlap in `{0}`".Args(v.TakeFirstChars(64)));;
      var result = new HourList(v);
      result.m_Spans = data;
      return result;
    }

    /// <summary>
    /// "Unparses" the span[] to string (which can be parsed back). Returns null for null input, empty string for empty array
    /// </summary>
    public static string Unparse(IEnumerable<Span> data)
    {
      if (data == null) return null;
      var sb = new StringBuilder(128);
      foreach (var one in data)
      {
        sb.Append(one.ToString());
        sb.Append(',');
      }
      return sb.ToString();
    }

    public static bool TryParse(string v, out HourList result)
    {
      var data = parse(v);
      if (data == null)
      {
        result = default;
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

    public HourList(IEnumerable<Span> data)
    {
      Data = Unparse(data);
      m_Spans = data?.ToArray();
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
        if (i<0) return default;
        if (m_Spans != null) return i < m_Spans.Length ? m_Spans[i] : default;
        if (!IsAssigned) return default;
        parseState();
        return i < m_Spans.Length ? m_Spans[i] : default;
      }
    }


    /// <summary>
    /// Returns true if the time component of the specified DateTime is covered(included)
    /// by one of the `HourList` spans as of THAT DAY
    /// </summary>
    /// <remarks>
    /// Restaurant Schedule
    ///  Lunch 10:30am-2pm
    ///  Dinner 5:30pm-7pm
    ///  Bar    8:00pm-4am  --- goes into another day
    /// </remarks>
    public bool IsCovered(DateTime when, bool isTheNextDay = false)
    {
      var ts = (int)when.TimeOfDay.TotalMinutes;

      if (!isTheNextDay)
      {
        return Spans.Any(s =>
        {
          if (s.EndsTheNextDay) return s.StartMinute <= ts;
          return s.StartMinute <= ts && s.FinishMinute >= ts;
        });
      }

      //the next day
      return Spans.Any(s => s.EndsTheNextDay && s.FinishMinute - MINUTES_PER_DAY >= ts);
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
    {
      if (options?.Purpose == JsonSerializationPurpose.Marshalling)
        JsonWriter.WriteMap(wri, nestingLevel + 1, options,
          new System.Collections.DictionaryEntry("data", Data),
          new System.Collections.DictionaryEntry("parsed", Spans.Select(one =>
              new
              {
                sta = one.StartMinute,
                fin = one.FinishMinute,
                dur = one.DurationMinutes,
                disp  = one.ToString()
              })
          )
        );
      else
        JsonWriter.EncodeString(wri, Data, options);
    }

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is string str) return (true, new HourList(str));
      if (data is JsonDataMap map) return (true, new HourList(map["data"].AsString()));
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

    /// <summary>
    /// Creates an enumeration of ordered spans (suitable for creation of `HourList`) by including time spans from another one and merging them together in a new enumeration
    /// </summary>
    public static IEnumerable<Span> Include(IEnumerable<Span> self, IEnumerable<Span> other)
    {
      if (self == null) return other;
      if (other == null) return self;

      var all = self.Concat(other)
                    .OrderBy(one => one.StartMinute).ToList();

      for (var i = 0; i < all.Count - 1;)
      {
        if (all[i].IntersectsWith(all[i + 1]))
        {
          var sum = all[i].Join(all[i + 1]);
          all[i] = sum;
          all.RemoveAt(i + 1);
        }
        else i++;
      }

      return all;
    }

    /// <summary>
    /// Creates a new HourList by including time spans from another one and merging them together
    /// </summary>
    public HourList Include(HourList other) => new HourList(Include(this.Spans, other.Spans));

    /// <summary>
    /// Creates an enumeration of ordered spans (suitable for creation of `HourList`) by excluding (punch out) time spans from another
    /// </summary>
    public static IEnumerable<Span> Exclude(IEnumerable<Span> self, IEnumerable<Span> other)
    {
      if (self == null) return null;
      if (other == null) return self;

      var result = self.OrderBy(one => one.StartMinute).ToList();

      for (var i = 0; i < result.Count;)
      {
        var another = other.FirstOrDefault(one => one.IntersectsWith(result[i]));
        if (another.IsAssigned)
        {
          var (x1, x2, x2n) = result[i].Exclude(another);//this may produce > 1 span
          if (!x1.IsAssigned)
          {
            result.RemoveAt(i);
            continue;
          }
          result[i] = x1;
          if (x2.IsAssigned)
          {
            result.Insert(i + 1, x2);
            continue;
          }
        }
        else i++;
      }

      return result;
    }


    /// <summary>
    /// Creates a new HourList by excluding time spans from another one and merging them together
    /// </summary>
    public HourList Exclude(HourList other) => new HourList(Exclude(this.Spans, other.Spans));


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

        if (result > MINUTES_PER_HALFDAY) return -1;
        if (result == MINUTES_PER_HALFDAY) result = 0;
      }
      else if(str.EndsWith("pm", StringComparison.InvariantCultureIgnoreCase))
      {
        str = str.Substring(0, str.Length - 2);
        result = parseMinutes(str);

        if (result>=MINUTES_PER_HALFDAY) result -= MINUTES_PER_HALFDAY;//#713 20220628 JPK

        if (result>=0) result += MINUTES_PER_HALFDAY;//PM
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
