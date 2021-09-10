/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace Azos
{
  /// <summary>
  /// Provides core date/time-related utility functions used by the majority of projects
  /// </summary>
  public static class DateUtils
  {
    /// <summary>
    /// Unix epoch start. This value MUST be UTC
    /// </summary>
    public static readonly DateTime UNIX_EPOCH_START_DATE = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Gets number of seconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static long ToSecondsSinceUnixEpochStart(this DateTime when)
    {
      return when.ToMillisecondsSinceUnixEpochStart() / 1000L;
    }

    /// <summary>
    /// Gets UTC DateTime from number of seconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromSecondsSinceUnixEpochStart(this long when)
    {
      return UNIX_EPOCH_START_DATE.AddSeconds(when);
    }

    /// <summary>
    /// Gets UTC DateTime from number of seconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromSecondsSinceUnixEpochStart(this ulong when)
    {
      return UNIX_EPOCH_START_DATE.AddSeconds(when);
    }

    /// <summary>
    /// Gets UTC DateTime from number of milliseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromMillisecondsSinceUnixEpochStart(this long when)
    {
      return UNIX_EPOCH_START_DATE.AddMilliseconds(when);
    }

    /// <summary>
    /// Gets UTC DateTime from number of microseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromMicrosecondsSinceUnixEpochStart(this long when)
    {
      return UNIX_EPOCH_START_DATE.AddTicks(when * 10);
    }

    /// <summary>
    /// Gets UTC DateTime from number of microseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromMicrosecondsSinceUnixEpochStart(this ulong when)
    {
      return UNIX_EPOCH_START_DATE.AddTicks((long)when * 10);
    }

    /// <summary>
    /// Gets UTC DateTime from number of milliseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static DateTime FromMillisecondsSinceUnixEpochStart(this ulong when)
    {
      return UNIX_EPOCH_START_DATE.AddMilliseconds(when);
    }

    /// <summary>
    /// Gets number of milliseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static long ToMillisecondsSinceUnixEpochStart(this DateTime when)
    {
      return (long)(when.ToUniversalTime() - UNIX_EPOCH_START_DATE).TotalMilliseconds;
    }

    /// <summary>
    /// Gets number of milliseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static ulong ToUnsignedMillisecondsSinceUnixEpochStart(this DateTime when)
    {
      var utcWhen = when.ToUniversalTime().IsTrue(v => v >= UNIX_EPOCH_START_DATE, "date past nix epoch");
      return (ulong)(utcWhen - UNIX_EPOCH_START_DATE).TotalMilliseconds;
    }

    /// <summary>
    /// Gets number of microseconds since Unix epoch start (1970/1/1 0:0:0)
    /// </summary>
    public static long ToMicrosecondsSinceUnixEpochStart(this DateTime when)
    {
      return (long)((when.ToUniversalTime() - UNIX_EPOCH_START_DATE).Ticks / 10);
    }

    /// <summary>
    /// Round date to specific week day
    /// </summary>
    public static DateTime RoundToWeekDay(this DateTime now, DayOfWeek dayOfWeek, bool keepTime = false)
    {
      var dt = now.DayOfWeek - dayOfWeek;
      var result = now.AddDays((7 - dt) % 7);
      return keepTime ? result : result.Date;
    }

    /// <summary>
    /// Round date to next specific week day
    /// </summary>
    public static DateTime RoundToNextWeekDay(this DateTime now, DayOfWeek dayOfWeek, bool keepTime = false)
    {
      var dt = now.DayOfWeek - dayOfWeek;
      var result = now.AddDays(dt == 0 ? 7 : (7 - dt) % 7);
      return keepTime ? result : result.Date;
    }

    /// <summary>
    /// Truncate date to specific resolution
    /// </summary>
    public static DateTime Truncate(this DateTime now, long tickResolution)
    {
      return new DateTime(now.Ticks - now.Ticks % tickResolution, now.Kind);
    }

    /// <summary>
    /// Aligns DateTime on an adjacent minuteBoudary as of midnight.
    /// The time component is adjusted accordingly, e.g. if you align on 60 min boundary
    /// you align the timestamp on hourly basis relative to midnight.
    /// Note: alignment resets as of midnight, so pass the boundary which is divisible equally within a day,
    /// e.g. 15 min, 6 hrs, 2 hrs, are all good, however if you align by 5 hrs (as an example), you are not going to get
    /// equal number of divisions in a 24 hr period.
    /// </summary>
    /// <param name="now">DateTime to change</param>
    /// <param name="minuteBoundary">Alignment boundary, must be greater than zero, e.g. 60 = one hour</param>
    /// <returns>
    /// DateTime shifted forward by the remainder of the division of elapsed minute component
    /// with alignment boundary. The Kind is preserved (e.g. UTC)
    /// </returns>
    public static DateTime AlignDailyMinutes(this DateTime now, int minuteBoundary)
    {
      minuteBoundary.IsTrue(v => v > 0);

      var result = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
      var dayMinutes = result.TimeOfDay.TotalMinutes;
      var rem = dayMinutes % minuteBoundary;
      if (rem > 0)
      {
        result = result.AddMinutes(minuteBoundary - rem);
      }
      return result;
    }

    /// <summary>
    /// Returns an approximate string representation of the point in time relative to this one,
    /// in the most suitable scale, that is:  "1 year ago" or "in 1 year"; "45 minutes ago" or "in 45 minutes". Supports ISO_LANG=eng only
    /// </summary>
    public static string ApproximateTimeDistance(this DateTime fromDate, DateTime toDate)
    {
      var diff = fromDate - toDate;
      var totalDays = Math.Abs(diff.TotalDays);
      var totalHours = Math.Abs(diff.TotalHours);
      var totalMinutes = Math.Abs(diff.TotalMinutes);
      var years = totalDays / 365.25;
      var months = totalDays / 30.5;
      string result;

      if (years > 1) result = "{0:n0} years".Args(years);
      else if (months > 1) result = "{0:n0} months".Args(months);
      else if (totalDays > 1) result = "{0:n0} days".Args(totalDays);
      else if (totalHours > 1) result = "{0:n0} hours".Args(totalHours);
      else if (totalMinutes > 1) result = "{0:n0} minutes".Args(totalMinutes);
      else result = "{0:n0} seconds".Args(Math.Abs(diff.TotalSeconds));

      return fromDate < toDate ? ("in " + result) : (result + " ago");
    }


  }
}
