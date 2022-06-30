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
using static Azos.Aver;

namespace Azos.Tests.Nub.Time
{
  [Runnable]
  public class HourListTestsJpk
  {

    [Run]
    public void Single_ZeroHourOnly()
    {
      var got = new HourList("0-1");
      var span = got.Spans.First();
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(0, got.Spans.First().StartMinute);
      Aver.AreEqual(60, got.Spans.First().DurationMinutes);
      "{0} {1}".SeeArgs(span.Start, span.Finish);
    }

    [Run]
    public void Single_23ToZeroHourOnly()
    {
      var got = new HourList("23-24");
      var span = got.Spans.First();
      got.Spans.Count().See();
      "{0} {1}".SeeArgs(span.Start, span.Finish);

      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(1380, got.Spans.First().StartMinute);
      Aver.AreEqual(60, got.Spans.First().DurationMinutes);
    }

    [Run]
    public void Day_ZeroTo24Hour()
    {
      var got = new HourList("0-24");
      var span = got.Spans.First();
      got.Spans.Count().See();
      "{0} {1}".SeeArgs(span.Start, span.Finish);

      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(0, got.Spans.First().StartMinute);
      Aver.AreEqual(1440, got.Spans.First().DurationMinutes);
    }

    //[Run]
    //[Throws(ExceptionType = typeof(TimeException))]
    //public void Day_Zero1MinTo24Hour1Min()
    //{
    //  var got = new HourList("0:01-24:01");
    //  var span = got.Spans.First();
    //  got.Spans.Count().See();
    //}

    [Run]
    public void Day_ZeroTo23Hour()
    {
      var got = new HourList("0-23");
      var span = got.Spans.First();
      got.Spans.Count().See();
      "{0} {1}".SeeArgs(span.Start, span.Finish);

      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(0, got.Spans.First().StartMinute);
      Aver.AreEqual(1380, got.Spans.First().DurationMinutes);
    }

    //[Run]
    //[Throws(ExceptionType =typeof(TimeException))]
    //public void Throw_OnOverlap()
    //{
    //  var got = new HourList("22:66-23");
    //  Aver.AreEqual(1, got.Spans.Count());
    //}


    [Run]
    public void CaryToNextDayPM()
    {
      var got = new HourList("23-1pm");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(60 * 23, got.Spans.First().StartMinute);
      Aver.AreEqual(14 * 60, got.Spans.First().DurationMinutes);
      got.See();
    }


    [Run]
    public void CaryToNextDayPMtoPM()
    {
      var got = new HourList("11pm-1pm");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(60 * 23, got.Spans.First().StartMinute);
      Aver.AreEqual(14 * 60, got.Spans.First().DurationMinutes);
      got.See();
    }


    [Run]
    public void MidnightToNoonAMtoPM()
    {
      var got = new HourList("12am-12pm");
      Aver.AreEqual(1, got.Spans.Count());
      Aver.AreEqual(0, got.Spans.First().StartMinute);
      Aver.AreEqual(HourList.MINUTES_PER_HALFDAY, got.Spans.First().FinishMinute);
      Aver.AreEqual(12 * 60, got.Spans.First().DurationMinutes);
      got.See();
    }

    [Run]
    public void IsCoveredOnCaryToNextDay()
    {
      var got = new HourList("23-1pm, 13:30-6pm");

      Aver.AreEqual((int)(60 * 13.5d), got.Spans.First().StartMinute);
      Aver.AreEqual((int)(4.5d * 60), got.Spans.First().DurationMinutes);

      var noon = new DateTime(1980, 1, 1,     12, 00, 00, DateTimeKind.Utc);
      Aver.IsFalse( got.IsCovered(noon) );
      Aver.IsTrue(got.IsCovered(noon, isTheNextDay: true));


      var twelveFortyFivePM = DateTime.Today.AddHours(12.75);
      Aver.IsFalse(got.IsCovered(twelveFortyFivePM));
      Aver.IsTrue(got.IsCovered(twelveFortyFivePM, true));

      var oneFifteenPM = DateTime.Today.AddHours(13.25);
      Aver.IsFalse(got.IsCovered(oneFifteenPM));
      Aver.IsFalse(got.IsCovered(oneFifteenPM, true));

      var twoFifteenPM = DateTime.Today.AddHours(14.25);
      Aver.IsTrue(got.IsCovered(twoFifteenPM));
      Aver.IsTrue(got.IsCovered(twoFifteenPM, false));
      Aver.IsFalse(got.IsCovered(twoFifteenPM, true));

      var sevenPM = DateTime.Today.AddHours(19);
      Aver.IsFalse(got.IsCovered(sevenPM));
      Aver.IsFalse(got.IsCovered(sevenPM, true));

      var elevenFifteenPM = DateTime.Today.AddHours(23);
      Aver.IsTrue(got.IsCovered(elevenFifteenPM));
      Aver.IsFalse(got.IsCovered(elevenFifteenPM, true));

      got.See();
    }

  }
}
