
using System;


namespace Azos.Time
{
  /// <summary>
  /// Days of week bitmask enumeration
  /// </summary>
  [Flags]
  public enum DaysOfWeek
  {
      None = 0,

      Sun = 1,
      Mon = 2,
      Tue = 4,
      Wed = 8,
      Thu = 16,
      Fri = 32,
      Sat = 64,

      Sunday = Sun,
      Monday = Mon,
      Tuesday = Tue,
      Wednesday = Wed,
      Thursday = Thu,
      Friday = Fri,
      Saturday = Sat,


      All = Sun | Mon | Tue | Wed | Thu | Fri | Sat
  }

  public static class DaysOfWeekUtils
  {

    public static bool Contains(this DaysOfWeek set, DayOfWeek day)
    {
      return 0<((int)set & (1 << (int)day));
    }

  }
}
