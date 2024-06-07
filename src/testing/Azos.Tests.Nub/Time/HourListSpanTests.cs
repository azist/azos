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
using HLS = Azos.Time.HourList.Span;


namespace Azos.Tests.Nub.Time
{
  [Runnable]
  public class HourListSpanTests
  {
    [Run]
    public void IsAssigned()
    {
      Aver.IsFalse(new HLS().IsAssigned);
      Aver.IsFalse(new HLS(0, 0).IsAssigned);
      Aver.IsTrue(new HLS(0, 1).IsAssigned);
      Aver.IsTrue(new HLS(1, 0).IsAssigned);
    }


    [Run]
    public void IntersectsWith()
    {
      Aver.IsFalse(new HLS().IntersectsWith(new HLS()));
      Aver.IsTrue(new HLS(0, 1).IntersectsWith(new HLS(0, 1)));

      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(100, 23)));
      Aver.IsFalse(new HLS(123, 100).IntersectsWith(new HLS(100, 22)));

      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(100, 50)));
      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(100, 500)));
      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(128, 1)));
      Aver.IsFalse(new HLS(123, 100).IntersectsWith(new HLS(128, 0)));
      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(223, 1)));
      Aver.IsFalse(new HLS(123, 100).IntersectsWith(new HLS(224, 1)));
    }


  }
}
