/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.ScriptingAndTesting.Dsl
{
  [Runnable]
  public class BasicTests
  {
    public const string SEE = @"
      script
      {
        //We do not want to repeat this part in various type references down below,
        //so we set it in the root of run step section for all child section down below
        type-path='Azos.Scripting.Dsl, Azos'

        do{ type='See' text='Step number one'}
        do{ type='See' text='Step number two'}
      }
    ";

    [Run]
    public async Task TestSee()
    {
       var runnable = new StepRunner(NOPApplication.Instance, SEE.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
       await runnable.RunAsync();
    }


    public const string JSON = @"
{
  'script': {
    'type-path': 'Azos.Scripting.Dsl, Azos',
    'timeout-sec': 1.25,
    'step': [
      {
        'type': 'See',
        'text': 'Step number one',
        'name': 'loop'
      },
      {
        'type': 'See',
        'text': 'Step number two'
      },
      {
        'type': 'Delay',
        'seconds': 0.5
      },
      {
        'type': 'Goto',
        'goto-name': 'loop',
        'name': 'goto1'
      }
    ]
  }
}";

    [Run]
    public async Task Json()
    {
      var runnable = new StepRunner(NOPApplication.Instance, JSON.AsJSONConfig(handling: Data.ConvertErrorHandling.Throw));
      await runnable.RunAsync();
    }



    public const string GOTO = @"
      script
      {
        type-path='Azos.Scripting.Dsl, Azos'
        timeout-sec=0.25

        do{ type='See' text='Step number one' name='loop'} //loop label
        do{ type='See' text='Step number two'}
        do{ type='Goto' step='loop' name='goto1'}
      }
    ";

    [Run]
    [Aver.Throws(typeof(RunnerException), Message = "Timeout")]
    [Aver.RunTime(MaxSec = 0.634)]
    public async Task TestGoto()
    {
      var runnable = new StepRunner(NOPApplication.Instance, GOTO.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      await runnable.RunAsync();
    }



    public const string SUBS = @"
      script
      {
        type-path='Azos.Scripting.Dsl, Azos'
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
    public async Task TestSubs()
    {
      var runnable = new StepRunner(NOPApplication.Instance, SUBS.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      await runnable.RunAsync("SUB1");
    }


    public const string EXPR = @"
      script
      {
        type-path='Azos.Scripting.Dsl, Azos'

        do{ type='Set' global='x' to='100'}
        do{ type='See' format='Step number one says: {~global.x}'}
        do{ type='Set' global='x' to='(global.x * (5 + 37)) / 2'}
        do{ type='See' format='But now it is: {~global.x}, however times minus two it will be: {~-2*global.x}'}
      }
    ";

    [Run]
    public async Task TestExpressions()
    {
      var runnable = new StepRunner(NOPApplication.Instance, EXPR.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      await runnable.RunAsync();
      Aver.AreEqual(2_100, runnable.GlobalState["x"].AsInt());

      Aver.AreEqual("~global.x", StepRunnerVarResolver.Eval("~~global.x", runnable, new JsonDataMap() ));
      Aver.AreEqual("2100", StepRunnerVarResolver.Eval("~global.x", runnable, new JsonDataMap()));
      Aver.AreEqual("-4200", StepRunnerVarResolver.Eval("~-2*global.x", runnable, new JsonDataMap()));
    }


    public const string STRCONCAT = @"
      script
      {
        type-path='Azos.Scripting.Dsl, Azos'

        do{ type='Set' global='who' to='\'Sonya\''}
        do{ type='Set' global='who' to='global.who + \' Mamzyan\''}
        do{ type='See' format='Hello, {~global.who}'}
      }
    ";

    [Run]
    public async Task TestStringConcat()
    {
      var runnable = new StepRunner(NOPApplication.Instance, STRCONCAT.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      await runnable.RunAsync();
      Aver.AreEqual("Sonya Mamzyan", runnable.GlobalState["who"].AsString());
    }

    public const string IF = @"
      script
      {
        type-path='Azos.Scripting.Dsl, Azos'

        do
        {
            type='If' condition='2 + 2 == 4'
            then
            {
                do{ type='See' text='Yessss'}
            }
            else
            {
                do{ type='See' text='Maybe Not?'}
            }
        }

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
    public async Task TestIf()
    {
      var runnable = new StepRunner(NOPApplication.Instance, IF.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      await runnable.RunAsync();
      Aver.AreEqual("normal", runnable.GlobalState["capacity"].AsString());
    }


    public const string VARSCOPE = @"
      script
      {
        type-path='Azos.Scripting.Dsl, Azos'

        do{ type='Set' global='x' to='1'}
        do{ type='Set' global='y' to='2'}
        do{ type='Set' local='x' to='-1'}
        do{ type='Set' local='y' to='-2'}

        do{ type='Set' global='z' local='z' to='global.y * local.x'}

        do{ type='See' format='The global result is: {~global.z}'}
        do{ type='See' format='The local result is: {~local.z}'}

        do{ type='DumpGlobalState'}
        do{ type='DumpLocalState'}
      }
    ";

    [Run]
    public async Task TestVarScope()
    {
      var runnable = new StepRunner(NOPApplication.Instance, VARSCOPE.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      var state = await runnable.RunAsync();
      Aver.AreEqual(-2, runnable.GlobalState["z"].AsInt());
      Aver.AreEqual(-2, state["z"].AsInt());
    }


    public const string SETRESULT = @"
      script
      {
        type-path='Azos.Scripting.Dsl, Azos'

        do{ type='SetResult' to='123'}
        do{ type='Set' global='x' to='-runner.result'} //-123

        do{ type='SetResult' to='\'Yes!\''}

        do{ type='Set' global='y' to='runner.result+\'ok\''}
      }
    ";

    [Run]
    public async Task TestSETRESULT()
    {
      var runnable = new StepRunner(NOPApplication.Instance, SETRESULT.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      var state = await runnable.RunAsync();
      Aver.AreEqual(-123, runnable.GlobalState["x"].AsInt());
      Aver.AreEqual("Yes!ok", runnable.GlobalState["y"].AsString());
      Aver.AreEqual("Yes!", runnable.Result.AsString());
    }


    public const string NAV1 = @"
      script
      {
        type-path='Azos.Scripting.Dsl, Azos'

        do{ type='Set' global='x' to='\'{a: 2, b: 3}\''}
        do{ type='Set' global='y' to='global.x.a * global.x.b'}//6
        do{ type='See' format='x = {~global.x + 1} and one more {~global.x + 2}, however y = {~global.y}'}

      }
    ";

    [Run]
    public async Task TestNav1()
    {
      var runnable = new StepRunner(NOPApplication.Instance, NAV1.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      var state = await runnable.RunAsync();
      Aver.AreEqual(6, runnable.GlobalState["y"].AsInt());
    }


    public const string JSON_LOAD_ITERATE = @"
      script
      {
        type-path='Azos.Scripting.Dsl, Azos;Azos.Data.Dsl, Azos'

        do{ type='JsonObjectLoader' name=d1 json='[1, 2, {a: 1, b: 2, c: {z: -123}}]'}
        do
        {
          type='ForEachData'
          data-source-name=d1
          body
          {
            do{type='Set' global=obj to='runner.result'}
            do{type='Set' global=a to='global.obj.a'}
            do{type='Set' global=b to='global.obj.b'}
            do{type='Set' global=c to='global.obj.c'}
            do{type='See' format='Member is: {~global.obj} A = {~global.a} B = {~global.b} C = {~global.c}'}
          }
        }
      }
    ";

    [Run]
    public async Task JsonLoadIterate()
    {
      var runnable = new StepRunner(NOPApplication.Instance, JSON_LOAD_ITERATE.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      var state = await runnable.RunAsync();
      Aver.AreEqual(1, runnable.GlobalState["a"].AsInt());
      Aver.AreEqual(2, runnable.GlobalState["b"].AsInt());
      Aver.AreEqual(-123, runnable.GlobalState["c"].AsString().JsonToDataObject()["z"].AsInt());
    }


  }
}
