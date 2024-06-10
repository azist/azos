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
    public void Include(bool inc, string a, string b, string c)
    {
      var ha = new HourList(a);
      var hb = new HourList(b);
      var hc = new HourList(c);

      var got = inc ? ha.Include(hb) : ha.Exclude(hb);

      ("\n ({0}) {1} ({2})  " +
       "\n expect :=>  {3} " +
       "\n got    :=>  {4}").SeeArgs(ha, inc ? "inc" : "exc", hb, hc, got);

      Aver.IsTrue(hc.IsEquivalent(got));
    }

  }
}
