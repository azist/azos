/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Data.Business;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Configuration.Business
{
  [Runnable]
  public class ScheduleTests
  {
    [Run]
    public void FullLaconicRoundtrip()
    {
      var CFG =
 @"
 r
 {
   schedule
   {
     name='default709'
     title{ eng{n='Default' d='The Default Schedule'} deu{n='Farter' d='Ein Farter Godülmeshteizenkraknüng'}}

     span
     {
       name='city'
       title{ eng{n='city' d='The City'} deu{n='stadt' d='Die Stadt'}}
       range{ start='1/1/2010' end='12/31/2010'}

       week-day='9am-12pm; 12:30pm-6pm'

       monday=$($week-day)
       tuesday=$($week-day)
       wednesday=$($week-day)
       thursday=$($week-day)
       friday=$($week-day)
       saturday='10am-2pm'
       sunday=''
     }

     override
     {
       name='j4th'
       title{ eng{n='ind' d='Independence Day'}}
       date='7/4/2010'
       hours=''
     }

     override
     {
       name='labor'
       title{ eng{n='ld' d='Labor Day'}}
       date='9/10/2010'
       hours=''
     }
   }
 }
 ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      var sut = new Schedule();
      sut.Configure(CFG["schedule"]);

      sut.See();

      ensureInvariants(sut);

      var root = Azos.Conf.Configuration.NewEmptyRoot("TOad");
      sut.PersistConfiguration(root, "schedule");

      var cfgContent2 = root.ToLaconicString();
      cfgContent2.See();

      var cfg2 = cfgContent2.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

      var sut2 = new Schedule();
      sut2.Configure(cfg2["schedule"]);

      sut2.See();
      ensureInvariants(sut2);

      var comparer = new DocLogicalComparer();
      var result = comparer.Compare(sut, sut2);
      //result.See();
      Aver.IsTrue(result.AreSame);
    }


    private void ensureInvariants(Schedule sut)
    {
      Aver.AreEqual("default709", sut.Name);

      Aver.AreEqual("Default", sut.Title.Get(NLSMap.GetParts.Name));
      Aver.AreEqual("The Default Schedule", sut.Title.Get(NLSMap.GetParts.Description));

      Aver.AreEqual("Farter", sut.Title.Get(NLSMap.GetParts.Name, "deu"));
      Aver.AreEqual("Ein Farter Godülmeshteizenkraknüng", sut.Title.Get(NLSMap.GetParts.Description, "deu"));

      Aver.AreEqual("Default", sut.Title.Get(NLSMap.GetParts.Name, "esp", "eng"));
      Aver.AreEqual("The Default Schedule", sut.Title.Get(NLSMap.GetParts.Description, "esp", "eng"));


      Aver.IsNotNull(sut.Spans);
      Aver.IsNotNull(sut.Overrides);

      Aver.AreEqual(1, sut.Spans.Count);
      Aver.AreEqual(2, sut.Overrides.Count);

      Aver.AreEqual("The City", sut.Spans[0].Title.Get(NLSMap.GetParts.Description));
      Aver.AreEqual("Die Stadt", sut.Spans[0].Title.Get(NLSMap.GetParts.Description, "deu"));
      Aver.AreEqual("The City", sut.Spans[0].Title.Get(NLSMap.GetParts.Description, "rus", "eng"));


      Aver.AreEqual("Independence Day", sut.Overrides[0].Title.Get(NLSMap.GetParts.Description));
      Aver.AreEqual(4, sut.Overrides[0].Date.Day);
      Aver.AreEqual("Labor Day", sut.Overrides[1].Title.Get(NLSMap.GetParts.Description));
      Aver.AreEqual(10, sut.Overrides[1].Date.Day);

      //more cases
      Aver.AreEqual("9am-12pm; 12:30pm-6pm", sut.Spans[0].Monday.Data);
      Aver.IsTrue(sut.Spans[0].Monday.Data == sut.Spans[0].Tuesday.Data);
      Aver.IsTrue(sut.Spans[0].Saturday.Data != sut.Spans[0].Sunday.Data);

      Aver.AreEqual("10am-2pm", sut.Spans[0].Saturday.Data);
      Aver.IsNull(sut.Spans[0].Sunday.Data);

      var satHrList = sut.Spans[0].Saturday;
      var satHr = satHrList.Spans.FirstOrDefault();

      Aver.AreEqual("10:00", satHr.Start);
      Aver.AreEqual("14:00", satHr.Finish);

      var satHrUtcTrue = new DateTime(2010, 7, 3, 11, 0, 5, DateTimeKind.Utc);
      Aver.IsTrue(satHrList.IsCovered(satHrUtcTrue));

      var satHrUtcFalse = new DateTime(2010, 7, 3, 14, 1, 5, DateTimeKind.Utc);
      Aver.IsFalse(satHrList.IsCovered(satHrUtcFalse));

      var friHrList = sut.Spans[0].Friday;
      var friHr = satHrList.Spans.FirstOrDefault();

      var friHrUtcTrue = new DateTime(2010, 7, 2, 17, 59, 59, DateTimeKind.Utc);
      Aver.IsTrue(friHrList.IsCovered(friHrUtcTrue));

      var friHrUtcFalse = new DateTime(2010, 7, 2, 18, 0, 5, DateTimeKind.Utc);
      Aver.IsFalse(friHrList.IsCovered(friHrUtcFalse));

      var friHrUtcFalse2 = new DateTime(2010, 7, 2, 12, 15, 5, DateTimeKind.Utc);
      Aver.IsFalse(friHrList.IsCovered(friHrUtcFalse2));
    }
  }
}
