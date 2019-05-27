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

namespace Azos.Tests.Nub.Time
{
  [Runnable]
  public class DateRangeTests
  {
    [Run]
    public void Unassigned()
    {
      var x = new DateRange();
      Aver.IsTrue(x.IsUnassigned);
    }

    [Run]
    public void Basic_Closed()
    {
      var x = new DateRange(new DateTime(1980,1,1), new DateTime(1990,1,1));

      Aver.IsTrue( DateTimeKind.Unspecified == x.Kind );
      Aver.IsFalse(x.IsUnassigned);
      Aver.IsFalse(x.IsOpen);
      Aver.IsTrue(x.IsClosed);

      Aver.IsTrue(x.ClosedSpan.HasValue);
      Aver.AreEqual(10, (int)x.ClosedSpan.Value.TotalDays / 365);

    }

    [Run]
    public void Basic_OpenStart()
    {
      var x = new DateRange(null, new DateTime(1990, 1, 1));

      Aver.IsTrue(DateTimeKind.Unspecified == x.Kind);
      Aver.IsFalse(x.IsUnassigned);
      Aver.IsTrue(x.IsOpen);
      Aver.IsFalse(x.IsClosed);

      Aver.IsFalse(x.ClosedSpan.HasValue);

      Aver.IsNull(x.Start);
      Aver.IsNotNull(x.End);
      Aver.AreEqual(1990, x.End.Value.Year);
    }

    [Run]
    public void Basic_OpenEnd()
    {
      var x = new DateRange(new DateTime(1980, 1, 1), null);

      Aver.IsTrue(DateTimeKind.Unspecified == x.Kind);
      Aver.IsFalse(x.IsUnassigned);
      Aver.IsTrue(x.IsOpen);
      Aver.IsFalse(x.IsClosed);

      Aver.IsFalse(x.ClosedSpan.HasValue);

      Aver.IsNull(x.End);
      Aver.IsNotNull(x.Start);
      Aver.AreEqual(1980, x.Start.Value.Year);
    }


    [Run]
    public void Contains()
    {
      var x = new DateRange(new DateTime(1980, 1, 1), new DateTime(1990, 1, 1));

      Aver.IsTrue(x.Contains(new DateTime(1980, 1, 1)));
      Aver.IsTrue(x.Contains(new DateTime(1990, 1, 1)));

      Aver.IsTrue(x.Contains(new DateTime(1980, 1, 2)));
      Aver.IsTrue(x.Contains(new DateTime(1989, 12, 31)));

      Aver.IsFalse(x.Contains(new DateTime(1979, 12, 31)));
      Aver.IsFalse(x.Contains(new DateTime(1990, 1, 2)));

      Aver.IsFalse(x.Contains(new DateTime(1908, 12, 31)));
      Aver.IsFalse(x.Contains(new DateTime(2003, 1, 2)));

      Aver.IsTrue(x.Contains(new DateTime(1985, 03, 27)));
      Aver.IsTrue(x.Contains(new DateTime(1989, 08, 15)));
    }


    [Run]
    public void Intersect_1()
    {
      var x = new DateRange(new DateTime(1980, 1, 1), new DateTime(1990, 1, 1));
      var y = new DateRange(new DateTime(1900, 1, 1), new DateTime(1917, 1, 1));
      var z = x.Intersect(y);

      Aver.IsFalse(z.HasValue);
    }

    [Run]
    public void Intersect_2()
    {
      var x = new DateRange(new DateTime(1980, 1, 1), new DateTime(1990, 1, 1));
      var y = new DateRange(new DateTime(2000, 1, 1), new DateTime(2010, 1, 1));
      var z = x.Intersect(y);

      Aver.IsFalse(z.HasValue);
    }

    [Run]
    public void Intersect_3()
    {
      var x = new DateRange(new DateTime(1980, 1, 1), new DateTime(1990, 1, 1));
      var y = new DateRange(new DateTime(1900, 1, 1), new DateTime(2010, 1, 1));
      var z = x.Intersect(y);

      Aver.IsTrue(z.HasValue);
      Aver.AreEqual(x.Start, z.Value.Start);
      Aver.AreEqual(x.End, z.Value.End);
    }

    [Run]
    public void Intersect_4()
    {
      var x = new DateRange(new DateTime(1980, 1, 1), new DateTime(1990, 1, 1));
      var y = new DateRange(new DateTime(1985, 2, 11), new DateTime(1987, 3, 12));
      var z = x.Intersect(y);

      Aver.IsTrue(z.HasValue);
      Aver.AreEqual(y.Start, z.Value.Start);
      Aver.AreEqual(y.End, z.Value.End);
    }

    [Run]
    public void Intersect_5()
    {
      var x = new DateRange(new DateTime(1980, 1, 1), new DateTime(1990, 1, 1));
      var y = new DateRange(new DateTime(1910, 2, 11), new DateTime(1987, 3, 12));
      var z = x.Intersect(y);

      Aver.IsTrue(z.HasValue);
      Aver.AreEqual(x.Start, z.Value.Start);
      Aver.AreEqual(y.End, z.Value.End);
    }

    [Run]
    public void Intersect_6()
    {
      var x = new DateRange(new DateTime(1980, 1, 1), new DateTime(1990, 1, 1));
      var y = new DateRange(new DateTime(1985, 11, 3), new DateTime(1997, 1, 28));
      var z = x.Intersect(y);

      Aver.IsTrue(z.HasValue);
      Aver.AreEqual(y.Start, z.Value.Start);
      Aver.AreEqual(x.End, z.Value.End);
    }

    [Run]
    public void Intersect_7()
    {
      var x = new DateRange(new DateTime(1980, 1, 1), null);
      var y = new DateRange(null, new DateTime(1971, 1, 28));
      var z = x.Intersect(y);

      Aver.IsFalse(z.HasValue);
    }

    [Run]
    public void Intersect_8()
    {
      var x = new DateRange(null, new DateTime(1980, 1, 1));
      var y = new DateRange(new DateTime(1989, 1, 28), null);
      var z = x.Intersect(y);

      Aver.IsFalse(z.HasValue);
    }

    [Run]
    public void Intersect_9()
    {
      var x = new DateRange(new DateTime(1980, 1, 1), null);
      var y = new DateRange(null, new DateTime(1997, 1, 28));
      var z = x.Intersect(y);

      Aver.IsTrue(z.HasValue);
      Aver.AreEqual(x.Start, z.Value.Start);
      Aver.AreEqual(y.End, z.Value.End);
    }

    [Run]
    public void Intersect_10()
    {
      var x = new DateRange(null, new DateTime(1997, 1, 28));
      var y = new DateRange(new DateTime(1980, 1, 1), null);
      var z = x.Intersect(y);

      Aver.IsTrue(z.HasValue);
      Aver.AreEqual(y.Start, z.Value.Start);
      Aver.AreEqual(x.End, z.Value.End);
    }

    [Run]
    public void Intersect_11()
    {
      var x = new DateRange(new DateTime(1800, 1, 1), null);
      var y = new DateRange(null, new DateTime(1900, 2, 2));
      var z = x.Intersect(y);

      Aver.IsTrue(z.HasValue);
      Aver.AreEqual(x.Start, z.Value.Start);
      Aver.AreEqual(y.End, z.Value.End);
    }


    [Run]
    public void Equals()
    {
      var x = new DateRange(new DateTime(1800, 1, 1), new DateTime(1900, 1, 1, 3, 18, 7));
      var y = new DateRange(new DateTime(1800, 1, 1), new DateTime(1900, 1, 1, 3, 18, 7));

      Aver.AreEqual(x, y);
      Aver.AreEqual(x.GetHashCode(), y.GetHashCode());
      Aver.IsTrue( x == y);
    }

    [Run]
    public void Equals_1()
    {
      var x = new DateRange(new DateTime(1800, 1, 1), null);
      var y = new DateRange(new DateTime(1800, 1, 1), null);

      Aver.AreEqual(x, y);
      Aver.AreEqual(x.GetHashCode(), y.GetHashCode());
      Aver.IsTrue(x == y);
    }

    [Run]
    public void Equals_2()
    {
      var x = new DateRange(null, new DateTime(1800, 1, 1));
      var y = new DateRange(null, new DateTime(1800, 1, 1));

      Aver.AreEqual(x, y);
      Aver.AreEqual(x.GetHashCode(), y.GetHashCode());
      Aver.IsTrue(x == y);
    }

    [Run]
    public void Equals_3()
    {
      var x = new DateRange(null, new DateTime(1800, 1, 21));
      var y = new DateRange(null, new DateTime(1800, 1, 1));

      Aver.AreNotEqual(x, y);
      Aver.AreNotEqual(x.GetHashCode(), y.GetHashCode());
      Aver.IsTrue(x != y);
    }

    [Run]
    public void TestToString()
    {
      var x = new DateRange(new DateTime(1800, 11, 12), new DateTime(1900, 11, 12, 23, 18, 17));

      Console.WriteLine(x.ToString());

      Aver.AreEqual("[11/12/1800 12:00 AM - 11/12/1900 11:18 PM]", x.ToString("MM/dd/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture));
    }

    [Run]
    public void TestToJson_Local()
    {
      var x = new DateRange(new DateTime(1800, 1, 1), new DateTime(1900, 1, 1, 3, 18, 7));

      Console.WriteLine(x.ToJson());

      var got = x.ToJson().JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(got);

      Aver.AreEqual(x.Start, got["start"].AsDateTime());
      Aver.AreEqual(x.End, got["end"].AsDateTime());
    }

    [Run]
    public void TestToJson_Utc()
    {
      var x = new DateRange(new DateTime(1800, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(1900, 1, 1, 3, 18, 7, DateTimeKind.Utc));

      Console.WriteLine(x.ToJson());

      var got = x.ToJson().JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(got);

      Aver.AreEqual(x.Start, got["start"].AsDateTime().ToUniversalTime());
      Aver.AreEqual(x.End, got["end"].AsDateTime().ToUniversalTime());
    }


    [Run, Aver.Throws(typeof(TimeException), Message=".null")]
    public void Error_1()
    {
      var x = new DateRange(null, null);
    }

    [Run, Aver.Throws(typeof(TimeException), Message = ".Kind")]
    public void Error_2()
    {
      var x = new DateRange(new DateTime(1800, 1, 1 ,1,1,1,DateTimeKind.Local), new DateTime(1800, 1, 1, 1, 1, 1, DateTimeKind.Utc));
    }

    [Run, Aver.Throws(typeof(TimeException), Message = "<")]
    public void Error_3()
    {
      var x = new DateRange(new DateTime(1900, 1, 1), new DateTime(1822, 1, 1));
    }


    public class moonStation : ILocalizedTimeProvider
    {
      private TimeLocation m_Moon = new TimeLocation(TimeSpan.FromHours(-5), "Moon", true); //moon is 5 hrs behind

      public TimeLocation TimeLocation => m_Moon;

      public DateTime LocalizedTime => DateTime.Now;

      public DateTime LocalizedTimeToUniversalTime(DateTime local)
      => DateTime.SpecifyKind( local - TimeLocation.UTCOffset, DateTimeKind.Utc);

      public DateTime UniversalTimeToLocalizedTime(DateTime utc)
       => DateTime.SpecifyKind( utc + TimeLocation.UTCOffset, DateTimeKind.Local);
    }

    [Run]
    public void Extensions_MakeLocalDateRange_1()
    {
      var moon = new moonStation();

      var x = moon.MakeLocalDateRange(new DateTime(1980,1,1, 13,0,0,DateTimeKind.Local), new DateTime(1980, 1, 1, 14, 0, 0, DateTimeKind.Local));

      Aver.IsTrue(x.Kind==DateTimeKind.Local);
      Aver.AreEqual(13, x.Start.Value.Hour);
      Aver.AreEqual(14, x.End.Value.Hour);
    }

    [Run]
    public void Extensions_MakeLocalDateRange_2()
    {
      var moon = new moonStation();

      var x = moon.MakeLocalDateRange(new DateTime(1980, 1, 1, 13, 0, 0, DateTimeKind.Utc), new DateTime(1980, 1, 1, 14, 0, 0, DateTimeKind.Utc));

      Aver.IsTrue(x.Kind == DateTimeKind.Local);
      Aver.AreEqual(8, x.Start.Value.Hour); //moon is 5 hrs behind UTC
      Aver.AreEqual(9, x.End.Value.Hour);
    }

    [Run]
    public void Extensions_MakeLocalDateRange_3()
    {
      var moon = new moonStation();

      //out-of-order
      var x = moon.MakeLocalDateRange(new DateTime(1980, 1, 1, 15, 0, 0, DateTimeKind.Utc), new DateTime(1980, 1, 1, 13, 0, 0, DateTimeKind.Utc));

      Aver.IsTrue(x.Kind == DateTimeKind.Local);
      Aver.AreEqual(8, x.Start.Value.Hour); //moon is 5 hrs behind UTC
      Aver.AreEqual(10, x.End.Value.Hour);
    }


    [Run]
    public void Extensions_MakeUtcDateRange_1()
    {
      var moon = new moonStation();

      var x = moon.MakeUtcDateRange(new DateTime(1980, 1, 1, 13, 0, 0, DateTimeKind.Utc), new DateTime(1980, 1, 1, 14, 0, 0, DateTimeKind.Utc));

      Aver.IsTrue(x.Kind == DateTimeKind.Utc);
      Aver.AreEqual(13, x.Start.Value.Hour);
      Aver.AreEqual(14, x.End.Value.Hour);
    }

    [Run]
    public void Extensions_MakeUtcDateRange_2()
    {
      var moon = new moonStation();

      var x = moon.MakeUtcDateRange(new DateTime(1980, 1, 1, 13, 0, 0, DateTimeKind.Local), new DateTime(1980, 1, 1, 14, 0, 0, DateTimeKind.Local));

      Aver.IsTrue(x.Kind == DateTimeKind.Utc);
      Aver.AreEqual(18, x.Start.Value.Hour); //UTC is 5 hrs ahead of moon
      Aver.AreEqual(19, x.End.Value.Hour);
    }

    [Run]
    public void Extensions_MakeUtcDateRange_3()
    {
      var moon = new moonStation();

      //out-of-order
      var x = moon.MakeUtcDateRange(new DateTime(1980, 1, 1, 15, 0, 0, DateTimeKind.Local), new DateTime(1980, 1, 1, 13, 0, 0, DateTimeKind.Local));

      Aver.IsTrue(x.Kind == DateTimeKind.Utc);
      Aver.AreEqual(18, x.Start.Value.Hour); //UTC is 5 hrs ahead of moon
      Aver.AreEqual(20, x.End.Value.Hour);
    }

  }
}
