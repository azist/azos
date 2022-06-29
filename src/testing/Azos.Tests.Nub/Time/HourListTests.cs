/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Time;
using Azos.Scripting;
using Azos.Serialization.JSON;
using System.Linq;

namespace Azos.Tests.Nub.Time
{
  [Runnable]
  public class HourListTests
  {
    [Run]
    public void Unassigned()
    {
      var x = new HourList();
      Aver.IsFalse(x.IsAssigned);
    }

    [Run]
    public void Single_HourOnly()
    {
      var got = new HourList("1-2");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(60, got.Spans.First().StartMinute);
      Aver.AreEqual(60, got.Spans.First().DurationMinutes);
    }

    [Run]
    public void Single_FractionMinute()
    {
      var got = new HourList("1-2.25");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(60, got.Spans.First().StartMinute);
      Aver.AreEqual(75, got.Spans.First().DurationMinutes);
    }

    [Run]
    public void Single_WithMinute()
    {
      var got = new HourList("1-2:15");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(60, got.Spans.First().StartMinute);
      Aver.AreEqual(75, got.Spans.First().DurationMinutes);
    }


    [Run]
    public void Single_AM()
    {
      var got = new HourList("1am-2:0am");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(60, got.Spans.First().StartMinute);
      Aver.AreEqual(60, got.Spans.First().DurationMinutes);
    }

    [Run]
    public void Single_AM_Minutes()
    {
      var got = new HourList("1am-2:15am");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(60, got.Spans.First().StartMinute);
      Aver.AreEqual(75, got.Spans.First().DurationMinutes);
    }

    [Run]
    public void Single_PM()
    {
      var got = new HourList("1pm-2pm");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(13*60, got.Spans.First().StartMinute);
      Aver.AreEqual(60, got.Spans.First().DurationMinutes);
    }

    [Run]
    public void Single_PM_Minutes()
    {
      var got = new HourList("1:10pm-2:45pm");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(13 * 60 + 10, got.Spans.First().StartMinute);
      Aver.AreEqual(60 + 45 - 10, got.Spans.First().DurationMinutes);
    }

    [Run]
    public void Spaces()
    {
      var got = new HourList(" 1:00am  - 2:15");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(60, got.Spans.First().StartMinute);
      Aver.AreEqual(75, got.Spans.First().DurationMinutes);
    }

    [Run]
    public void Spaces02()
    {
      var got = new HourList(" 1.00      -     2.25am        ");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(60, got.Spans.First().StartMinute);
      Aver.AreEqual(75, got.Spans.First().DurationMinutes);
    }

    [Run]
    public void Spaces03()
    {
      var got = new HourList(" 1.00      -     2.25am        ");
      Aver.AreEqual(1, got.Spans.Count());
      var span = got.Spans.First();

      "{0} {1}".SeeArgs(span.Start, span.Finish);
      Aver.AreEqual("1:00", span.Start);
      Aver.AreEqual("2:15", span.Finish);
    }

    [Run]
    public void TwentyFour()
    {
      var got = new HourList(" 1.00 - 23 ");
      Aver.AreEqual(1, got.Spans.Count());
      var span = got.Spans.First();
      Aver.AreEqual(60, span.StartMinute);
      Aver.AreEqual(22 * 60, span.DurationMinutes);

      "{0} {1}".SeeArgs(span.Start, span.Finish);
      Aver.AreEqual("1:00", span.Start);
      Aver.AreEqual("23:00", span.Finish);
    }

    [Run]
    public void IsCovered()
    {
      var got = new HourList("1-14");
      Aver.AreEqual(1, got.Spans.Count());
      var span = got.Spans.First();
      Aver.AreEqual(60, span.StartMinute);
      Aver.AreEqual(13 * 60, span.DurationMinutes);

      Aver.IsFalse(got.IsCovered(new DateTime(1980, 1, 1,   0,  0, 0)));
      Aver.IsFalse(got.IsCovered(new DateTime(1980, 1, 1,   0, 59, 59)));
      Aver.IsTrue(got.IsCovered( new DateTime(1980, 1, 1,   1,  0, 0)));
      Aver.IsTrue(got.IsCovered( new DateTime(1980, 1, 1,   13,59, 59)));
      Aver.IsTrue(got.IsCovered( new DateTime(1980, 1, 1,   14, 0, 0)));
      Aver.IsFalse(got.IsCovered(new DateTime(1980, 1, 1,   14, 1, 0)));
      Aver.IsFalse(got.IsCovered(new DateTime(1980, 1, 1,   15, 1, 0)));
    }

    [Run]
    public void Equals_IsEquivalent_GetHachCode01()
    {
      var got = new HourList("1-14");
      Aver.AreEqual(1, got.Spans.Count());
      var got2 = new HourList("1am-2pm");
      Aver.IsFalse(got.Equals(got2));
      Aver.AreNotEqual(got.GetHashCode(), got2.GetHashCode());
      Aver.IsTrue(got.IsEquivalent(got2));
    }

    [Run]
    public void quals_IsEquivalent_GetHachCode02()
    {
      var got = new HourList("1-14");
      Aver.AreEqual(1, got.Spans.Count());
      var got2 = new HourList("1-14");
      Aver.IsTrue(got.Equals(got2));
      Aver.AreEqual(got.GetHashCode(), got2.GetHashCode());
      Aver.IsTrue(got.IsEquivalent(got2));
    }

    [Run]
    public void quals_IsEquivalent_GetHachCode03()
    {
      var got = new HourList("1-14, 2:30pm-7:15pm, 23-24");
      Aver.AreEqual(3, got.Spans.Count());
      var got2 = new HourList("1:00am-2:00pm; 14.5-19.25; 11pm-24");
      Aver.AreEqual(3, got.Spans.Count());
      Aver.IsFalse(got.Equals(got2));
      Aver.AreNotEqual(got.GetHashCode(), got2.GetHashCode());
      Aver.IsTrue(got.IsEquivalent(got2));
      "{0}   {1}".SeeArgs(got, got2);
    }

    [Run]
    public void TwelveAM()
    {
      var got = new HourList("23-12am");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(60*23, got.Spans.First().StartMinute);
      Aver.AreEqual(60, got.Spans.First().DurationMinutes);
      got.See();
    }

    [Run]
    public void CaryToNextDay()
    {
      var got = new HourList("23-1am");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(60 * 23, got.Spans.First().StartMinute);
      Aver.AreEqual(2*60, got.Spans.First().DurationMinutes);
      got.See();
    }

    [Run]
    public void Multiple01()
    {
      var got = new HourList("8-12, 12:30-5pm, 23-1:12am");
      Aver.AreEqual(3, got.Spans.Count());
      Aver.AreEqual(8 * 60, got.Spans.First().StartMinute);
      Aver.AreEqual(4 * 60, got.Spans.First().DurationMinutes);

      Aver.AreEqual(12 * 60 + 30, got.Spans.Skip(1).First().StartMinute);
      Aver.AreEqual(4 * 60 + 30, got.Spans.Skip(1).First().DurationMinutes);

      Aver.AreEqual(23 * 60, got.Spans.Skip(2).First().StartMinute);
      Aver.AreEqual(2 * 60 + 12, got.Spans.Skip(2).First().DurationMinutes);

      got.See();
    }

    [Run]
    public void TryParse_Blank()
    {
      Aver.IsFalse(HourList.TryParse("", out var got));
      Aver.IsFalse(HourList.TryParse(null, out got));
    }

    [Run]
    public void TryParse_Single()
    {
      Aver.IsTrue(HourList.TryParse("1-2", out var got));
      Aver.IsTrue(HourList.TryParse("2-3", out got));
      Aver.IsTrue(HourList.TryParse("14.5-18:30", out got));
      Aver.IsTrue(HourList.TryParse("14.5-18.62", out got));
    }

    [Run]
    public void TryParse_Single_Malformed()
    {
      Aver.IsFalse(HourList.TryParse("14.z-18:30", out var got));
      Aver.IsFalse(HourList.TryParse("14.5-18:t", out got));
      Aver.IsFalse(HourList.TryParse("14.5-18:", out got));
      Aver.IsFalse(HourList.TryParse("14.5-:2", out got));
      Aver.IsFalse(HourList.TryParse("14.5-", out got));
      Aver.IsFalse(HourList.TryParse("14.5-18:62", out got));
    }



    //Build failed?
    //todo:  parse, try parse,  exceptions, JSOPn serialization, refactor exception constant

  }
}
