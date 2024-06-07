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
    public void ToStringTest()
    {
      Aver.AreEqual("13:10-13:10", new HLS(13 * 60 + 10, 1).ToString());
      Aver.AreEqual("13:10-13:11", new HLS(13 * 60 + 10, 2).ToString());
      Aver.AreEqual("13:10-13:34", new HLS(13 * 60 + 10, 25).ToString());

      Aver.AreEqual("0:59-0:59", new HLS(59, 1).ToString());
      Aver.AreEqual("0:59-1:00", new HLS(59, 2).ToString());
      Aver.AreEqual("0:59-1:01", new HLS(59, 3).ToString());
      Aver.AreEqual("0:59-1:06", new HLS(59, 8).ToString());

      Aver.AreEqual("3:59-4:06", new HLS(3 * 60 + 59, 8).ToString());
    }


    [Run]
    public void IntersectsWith()
    {
      Aver.IsFalse(new HLS().IntersectsWith(new HLS()));
      Aver.IsTrue(new HLS(0, 1).IntersectsWith(new HLS(0, 1)));

      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(100, 24)));
      Aver.IsFalse(new HLS(123, 100).IntersectsWith(new HLS(100, 23)));

      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(100, 50)));
      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(100, 500)));
      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(128, 1)));
      Aver.IsFalse(new HLS(123, 100).IntersectsWith(new HLS(128, 0)));

      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(222, 1)));
      Aver.IsFalse(new HLS(123, 100).IntersectsWith(new HLS(223, 1)));


      Aver.IsFalse(new HLS(123, 100).IntersectsWith(new HLS(0, 123)));
      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(0, 124)));

      Aver.IsFalse(new HLS(123, 100).IntersectsWith(new HLS(223, 1)));
      Aver.IsTrue(new HLS(123, 100).IntersectsWith(new HLS(222, 1)));

      Aver.IsTrue(new HLS(123, 1).IntersectsWith(new HLS(123, 1)));
      Aver.IsFalse(new HLS(123, 1).IntersectsWith(new HLS(124, 1)));
    }

    [Run]
    public void Intersect()
    {
      Aver.AreEqual( new HLS(9,1), new HLS(0, 10).Intersect(new HLS(9, 1)));
    }


  }
}
