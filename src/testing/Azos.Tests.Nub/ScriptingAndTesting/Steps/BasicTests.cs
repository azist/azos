/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Scripting;
using Azos.Scripting.Steps;
using Azos.MySql.ConfForest;

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

        step{ type='See' text='Step number one'}
        step{ type='See' text='Step number two'}
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

        step{ type='See' text='Step number one' name='loop'} //loop label
        step{ type='See' text='Step number two'}
        step{ type='Goto' goto-name='loop' name='goto1'}
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

    //[Run]
    //[Aver.Throws(typeof(RunnerException), Message = "Timeout")]
    //[Aver.RunTime(MaxSec = 1.634)]
    //public void Test3()
    //{
    //  var runnable = ForestInstaller.FromFile(NOPApplication.Instance, @"D:\devhome\ghub\azos\src\testing\Azos.Tests.Nub\ScriptingAndTesting\Steps\step-sample-01.json");
    //  var got = runnable.Run();

    //  got.See();
    //}
  }
}
