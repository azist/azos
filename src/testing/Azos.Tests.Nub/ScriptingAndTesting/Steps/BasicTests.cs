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

        step{ type='Log' text='Step number one'}
        step{ type='Log' text='Step number two'}



      }
    ";

    [Run]
    public void Test1()
    {
       var runnable = new StepRunner(NOPApplication.Instance, S1.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
       runnable.Run();
    }
  }
}
