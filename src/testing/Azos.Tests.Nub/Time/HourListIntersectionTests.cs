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
  public class HourListIntersectionTests
  {
    [Run("inc=true  a='1am-2am'  b='2am-3pm'  c='1am-2am,2am-3pm'")]
    [Run("inc=true  a='1am-2am'  b='1:15am-1:25am'  c='1am-2am'")]
    [Run("inc=true  a='1am-2am'  b='1:15am-2:25am'  c='1am-2:25am'")]

    [Run("inc=true  a='0-0.75,13:00-14:45'  b='8pm-9pm'  c='0am-0:45am,1pm-2:45pm,8pm-9pm'")]
    [Run("inc=true  a='12am-0.75,13:00-14:45'  b='8pm-9pm'  c='12am-0:45am,1pm-2:45pm,8pm-9pm'")]

    /*
      hl.Exclude() needs to return possibly TWO hour lists, as the time may get spilled into the next adjacent day,
      this is because HOUR LIST does not support StartTime which is beyond 1 day in principle

      span.Exclude() and Hl.Exclude need to be reworked.

      Include does not have such problem as it always "joins" time within an existing DAY, it never punched holes in a timeline,
      an exclude method punches holes and since a new time may not start in the next day, this combination MAY NOT be represented by the same HourList instance
    */


    [Run("inc=false  a='0-18:00,21:00-6:00'  b='19:00-19:15,1am-1:30am'  c='0-6pm,21:00-1am' cnext='1:30am-6am'")]
    [Run("inc=false  a='0-13:00,14-18:00,8pm-3am'  b='0:15-0:30,14:30-15:00'  c='0am-0:15am,0:30am-13:00,14-14:30,15:00-18:00,8pm-3am'")]
    public void IncludeOrExclude(bool inc, string a, string b, string c, string cnext = null)
    {
      var ha = new HourList(a);
      var hb = new HourList(b);
      var hc = new HourList(c);
      var hcn = new HourList(cnext);

      var (got, gotn) = inc ? (ha.Include(hb), new HourList()) : ha.Exclude(hb);

      ("\n ({0}) {1} ({2})  " +
       "\n expect :=>  {3} / {4}" +
       "\n got    :=>  {5} / {6}").SeeArgs(ha, inc ? "inc" : "exc", hb, hc, hcn, got, gotn);

      Aver.IsTrue(hc.IsEquivalent(got));
      Aver.IsTrue(hcn.IsEquivalent(gotn));
    }

  }
}
