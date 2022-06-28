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
       name{ eng{n='city' d='City'} deu{n='stadt' d='Ein Stadt'}}
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
   }
 ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      var sched = new Schedule();
      sched.Configure(CFG);

      sched.See();
    }
  }
}
