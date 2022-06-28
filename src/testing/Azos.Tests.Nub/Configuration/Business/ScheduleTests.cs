/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
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
    public void Read010()
    {
      var CFG =
 @"
   schedule
   {
     span
     {
       name{ eng{n='city' d='The City'} deu{n='stadt' d='Die Stadt'}}
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
       name{ eng{n='ind' d='Independence Day'}}
       date='7/4/2010'
       hours=''
     }

     override
     {
       name{ eng{n='ld' d='Labor Day'}}
       date='9/10/2010'
       hours=''
     }
   }
 ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      var sut = new Schedule();
      sut.Configure(CFG);

      sut.See();

      Aver.IsNotNull(sut.Spans);
      Aver.IsNotNull(sut.Overrides);

      Aver.AreEqual(1, sut.Spans.Count);
      Aver.AreEqual(2, sut.Overrides.Count);

      Aver.AreEqual("The City", sut.Spans[0].Name.Get(NLSMap.GetParts.Description));
      Aver.AreEqual("Die Stadt", sut.Spans[0].Name.Get(NLSMap.GetParts.Description, "deu"));
      Aver.AreEqual("The City", sut.Spans[0].Name.Get(NLSMap.GetParts.Description, "rus", "eng"));


      Aver.AreEqual("Independence Day", sut.Overrides[0].Name.Get(NLSMap.GetParts.Description));
      Aver.AreEqual(4, sut.Overrides[0].Date.Day);
      Aver.AreEqual("Labor Day", sut.Overrides[1].Name.Get(NLSMap.GetParts.Description));
      Aver.AreEqual(10, sut.Overrides[1].Date.Day);

      //more cases
    }
  }
}
