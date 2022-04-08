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
    public const string SEE = @"
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
    public void TestSee()
    {
       var runnable = new StepRunner(NOPApplication.Instance, SEE.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
       runnable.Run();
    }

    public const string GOTO = @"
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
    public void TestGoto()
    {
      var runnable = new StepRunner(NOPApplication.Instance, GOTO.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      runnable.Run();
    }


    public const string SUBS = @"
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
    public void TestSubs()
    {
      var runnable = new StepRunner(NOPApplication.Instance, SUBS.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      runnable.Run("SUB1");
    }


    public const string EXPR = @"
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
    public void TestExpressions()
    {
      var runnable = new StepRunner(NOPApplication.Instance, EXPR.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      runnable.Run();
      Aver.AreEqual(2_100, runnable.GlobalState["x"].AsInt());
    }


    public const string STRCONCAT = @"
      script
      {
        type-path='Azos.Scripting.Steps, Azos'

        do{ type='Set' global='who' to='\'Sonya\''}
        do{ type='Set' global='who' to='global.who + \' Mamzyan\''}
        do{ type='See' format='Hello, {~global.who}'}
      }
    ";

    [Run]
    public void TestStringConcat()
    {
      var runnable = new StepRunner(NOPApplication.Instance, STRCONCAT.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      runnable.Run();
      Aver.AreEqual("Sonya Mamzyan", runnable.GlobalState["who"].AsString());
    }

    public const string IF = @"
      script
      {
        type-path='Azos.Scripting.Steps, Azos'

        do{ type='Set' global='fuel' to='5.2'}
        do
        {
           type='If' condition='global.fuel < 5'
           then
           {
             do{ type='Set' global='capacity' to='\'low\''}
           }
           else
           {
             do{ type='Set' global='capacity' to='\'normal\''}
           }

        }
        do{ type='See' format='Fuel capacity is {~global.fuel} is {~global.capacity}'}
      }
    ";

    [Run]
    public void TestIf()
    {
      var runnable = new StepRunner(NOPApplication.Instance, IF.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      runnable.Run();
      Aver.AreEqual("normal", runnable.GlobalState["capacity"].AsString());
    }


    public const string VARSCOPE = @"
      script
      {
        type-path='Azos.Scripting.Steps, Azos'

        do{ type='Set' global='x' to='1'}
        do{ type='Set' global='y' to='2'}
        do{ type='Set' local='x' to='-1'}
        do{ type='Set' local='y' to='-2'}

        do{ type='Set' global='z' local='z' to='global.y * local.x'}

        do{ type='See' format='The global result is: {~global.z}'}
        do{ type='See' format='The local result is: {~local.z}'}
      }
    ";

    [Run]
    public void TestVarScope()
    {
      var runnable = new StepRunner(NOPApplication.Instance, VARSCOPE.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      var state = runnable.Run();
      Aver.AreEqual(-2, runnable.GlobalState["z"].AsInt());
      Aver.AreEqual(-2, state["z"].AsInt());
    }

  }
}
