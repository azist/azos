/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
