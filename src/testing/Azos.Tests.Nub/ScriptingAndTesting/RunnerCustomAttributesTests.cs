/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Scripting;
using Azos.Conf;
using System.Reflection;

namespace Azos.Tests.Nub.ScriptingAndTesting
{
  [Runnable]
  public class RunnerCustomAttributesTests : IRunHook
  {
    public bool Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
    => false;

    public bool Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
    {
      if (error is AvermentException aerr)//expected throws from tests below
      {
        Aver.IsTrue(aerr.Message.Contains("at least 750") || aerr.Message.Contains("at most 150"));
        "Expected and got: {0}".Info(aerr.ToMessageWithType());
        return true;
      }
      return false;
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class CustomRunHookA : RunHookAttribute
    {
      public override object Before(Runner runner, object runnable, FID id, MethodInfo mi, RunAttribute attr, ref object[] args)
      => new List<string>{ "a-before1", "a-before2" };

      public override void After(Runner runner, object runnable, FID id, MethodInfo mi, RunAttribute attr, object state, ref Exception error)
      {
        if (state is List<string> lst) lst.See("{0} After:".Args(GetType().Name));
        if (error!=null)
          "(Expected testing) Error is: {0}".Error(error.ToMessageWithType());
      }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class CustomRunHookB : RunHookAttribute
    {
      public override object Before(Runner runner, object runnable, FID id, MethodInfo mi, RunAttribute attr, ref object[] args)
      => new List<string> { "b-before1", "b-before2", "b-before3" };

      public override void After(Runner runner, object runnable, FID id, MethodInfo mi, RunAttribute attr, object state, ref Exception error)
      {
        if (state is List<string> lst) lst.See("{0} After got: ".Args(GetType().Name));
        if (error != null)
          "(Expected testing) Error is: {0}".Error(error.ToMessageWithType());
        error = null;//hide IT!!!!
      }

    }

    [Run, CustomRunHookA, CustomRunHookA, CustomRunHookB, CustomRunHookA, CustomRunHookA, CustomRunHookB]
    public void CustomAttributes()
    {
      throw new Exception("There is a problem");
    }

    [Run, Aver.RunTime(MinMs = 750)]
    public void TakesMin()
    {
      //return instantly
    }

    [Run, Aver.RunTime(MaxMs = 150)]
    public void TakesMax()
    {
      //impose a delay
      System.Threading.Thread.Sleep(1000);
    }

    [Run, Aver.RunTime(MinSec = 0.750d)]
    public void TakesMinSec()
    {
      //return instantly
    }

    [Run, Aver.RunTime(MaxSec = 0.15d)]
    public void TakesMaxSec()
    {
      //impose a delay
      System.Threading.Thread.Sleep(1000);
    }

  }
}
