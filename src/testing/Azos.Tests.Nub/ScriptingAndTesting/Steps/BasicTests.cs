/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Scripting.Steps;

namespace Azos.Tests.Nub.ScriptingAndTesting.Steps
{
  [Runnable]
  public class BasicTests
  {
    public const string S1 = @"
      script
      {
        //We do not want to repeat this part in various type references down below,
        //so we set it in the root of run step section for all child section down below
        type-path='Azos.Scripting.Steps, Azos'

        do{ type='See' text='Step number one'}
        do{ type='See' text='Step number two'}
      }
    ";

    [Run]
    public void Test1()
    {
       var runnable = new StepRunner(NOPApplication.Instance, S1.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
       runnable.Run();
    }

    public const string S2 = @"
      script
      {
        type-path='Azos.Scripting.Steps, Azos'
        timeout-sec=0.25

        do{ type='See' text='Step number one' name='loop'} //loop label
        do{ type='See' text='Step number two'}
        do{ type='Goto' label='loop' name='goto1'}
      }
    ";

    [Run]
    [Aver.Throws(typeof(RunnerException), Message = "Timeout")]
    [Aver.RunTime(MaxSec = 0.634)]
    public void Test2()
    {
      var runnable = new StepRunner(NOPApplication.Instance, S2.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      runnable.Run();
    }


    public const string S3 = @"
      script
      {
        type-path='Azos.Scripting.Steps, Azos'
        timeout-sec=0.25

       // do{ type='Set' global='x' to='global.x + 1'}

       // do{ type='See' text='Step number one $(~Global.x)' }


        do{ type='See' text='Step number one' }
        do{ type='Call' sub='sub1'}
        do{ type='See' text='Step number two'}
        do{ type='Call' sub='sub1'}
        do{ type='Halt'}

        do
        {
          type='Sub' name='sub1'
          source
          {
            do{ type='See' text='Sub 1 says 1' }
            do{ type='See' text='Sub 1 says 2' }
            do{ type='See' text='Sub 1 says 3' }
            do{ type='Halt'}
          }
        }

      }
    ";

    [Run]
    public void Test3()
    {
      var runnable = new StepRunner(NOPApplication.Instance, S3.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      runnable.Run("SUB1");
    }


    public const string S4 = @"
      script
      {
        type-path='Azos.Scripting.Steps, Azos'

        do{ type='Set' global='x' to='100'}
        do{ type='See' format='Step number one says: {~global.x}'}
        do{ type='Set' global='x' to='(global.x * (5 + 37)) / 2'}
        do{ type='See' format='But now it is: {~global.x}, however times minus two it will be: {~-2*global.x}'}
      }
    ";

    [Run]
    public void Test4()
    {
      var runnable = new StepRunner(NOPApplication.Instance, S4.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      runnable.Run();
      Aver.AreEqual(2_100, runnable.GlobalState["x"].AsInt());
    }

  }
}
