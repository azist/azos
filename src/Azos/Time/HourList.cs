/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Azos.Serialization.JSON;

//namespace Azos.Time
//{
//  /// <summary>
//  /// Efficiently represents a list of minute-aligned time spans within a day
//  /// which is typically used for operation hours/schedules
//  /// </summary>
//  public struct HourList : IEquatable<HourList>, IJsonWritable, IJsonReadable
//  {
//    public struct Span : IEquatable<Span>, IComparable<Span>
//    {
//      public const int MINUTES_PER_DAY = 24 * 60;

//      internal Span(int start, int duration)
//      {
//        StartMinute = start;
//        DurationMinutes = duration;
//      }

//      public readonly int StartMinute;
//      public readonly int DurationMinutes;

//      public int FinishMinute => StartMinute + DurationMinutes;

//      public string Start
//      {
//        get
//        {
//          var hr = StartMinute / 60;
//          var min = StartMinute % 60;
//          return "{0:n0}:{1:n0}".Args(hr, min);
//        }
//      }

//      public string Finish
//      {
//        get
//        {
//          var to = StartMinute + DurationMinutes;
//          if (to > MINUTES_PER_DAY) to = to % MINUTES_PER_DAY;
//          var hr = to / 60;
//          var min = to % 60;
//          return "{0:n0}:{1:n0}".Args(hr, min);
//        }
//      }

//      public override string ToString() => Start + "-" + Finish;
//      public override int GetHashCode() => StartMinute;
//      public override bool Equals(object obj) => obj is Span span ? this.Equals(span) : false;
//      public bool Equals(Span other) => this.Start == other.Start && this.DurationMinutes == other.DurationMinutes;
//      public int CompareTo(Span other) => this.StartMinute < other.StartMinute ? -1 : this.StartMinute > other.StartMinute ? +1 : 0;
//    }

//    public static readonly char[] DELIMS = new []{',', ';'};

//    public HourList(string data)
//    {
//      Data = data;
//      m_Spans = null;
//    }

//    private Span[] m_Spans;

//    public readonly string Data;

//    public bool IsAssigned => Data.IsNotNullOrWhiteSpace();

//    public IEnumerable<Span> Spans
//    {
//      get
//      {
//        if (m_Spans!=null) return m_Spans;
//        m_Spans = parse(Data);
//        if (m_Spans==null) throw new TimeoutException(StringConsts.ARGUMENT_ERROR + " Bad hour list syntax, out of order, or overlap in `{0}`".Args(Data.TakeFirstChars(64)));
//        return m_Spans;
//      }
//    }

//    private static Span[] parse(string content)
//    {
//      if (content.IsNullOrWhiteSpace()) return null;

//      var kvps = content.Split(DELIMS)
//                      .Where(p => p.IsNotNullOrWhiteSpace())
//                      .Select(p => p.SplitKVP('-') );

//      var data = new List<Span>();
//      foreach(var kvp in kvps)
//      {
//        var rawStart = parseComponent(kvp.Key);
//        if (rawStart < 0) return null;
//        var rawFinish = parseComponent(kvp.Value);
//        if (rawFinish <0 || rawFinish < rawStart) return null;
//        var span = new Span(rawStart, rawFinish - rawStart);
//        data.Add(span);
//      }

//      //order it time
//      var result = data.OrderBy(s => s.StartMinute).ToArray();

//      //check for overlap
//      var start =0;
//      foreach(var span in result)
//       if (span.StartMinute < start)
//         return null;
//       else
//         start = span.FinishMinute;

//      return result;
//    }

//    private static int parseComponent(string c)
//    {
//      if (c.IsNullOrWhiteSpace()) return -1;
//      return 0;
//    }




//  }
//}
