/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;

using Azos.Apps;
using Azos.Scripting;

namespace Azos.Tests.Nub
{
    [Runnable]
    public class RunnerHookTests : IRunnableHook, IRunHook
    {
        [Run] public void M01(){ }
        [Run] public void M02(){ }
        [Run] public void M03(){ }
        [Run] public void M04(){ }
        [Run, Run, Run] public void M05(){ }

        private int m_RunnableState;
        private int m_RunPrologueCount;
        private int m_RunEpilogueCount;

        void IRunnableHook.Prologue(Runner runner, FID id)
        {
          Aver.AreEqual(0, m_RunnableState);
          m_RunnableState++;
          Console.WriteLine("Runnable prologue");
        }

        bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
        {
          Aver.AreEqual(1, m_RunnableState);
          Aver.AreEqual(7, m_RunPrologueCount);
          Aver.AreEqual(7, m_RunEpilogueCount);
          Console.WriteLine("Runnable epilogue");
          return false;
        }

        bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
        {
          Aver.AreEqual(1, m_RunnableState);
          m_RunPrologueCount++;
          Console.WriteLine("Method prologue: "+method.Name);
          return false;
        }

        bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
        {
          m_RunEpilogueCount++;
          Console.WriteLine("Method epilogue: "+method.Name);
          return false;
        }
    }

    [Runnable(category: "runner", order: -5999)]
    public class RunnerHookExceptions_RunnableProlog : IRunnableHook
    {
        [Run("!crash-runnable-prologue", "")]
        public void BadMethod(){ }

        void IRunnableHook.Prologue(Runner runner, FID id)
        {
          Aver.Fail("I crashed in Runnable prologue");
        }

        bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
        {
          return false;
        }
    }

    [Runnable(category: "runner", order: -5000)]
    public class RunnerHookExceptions_RunnableEpilogue : IRunnableHook
    {
        [Run("!crash-runnable-epilogue", "")]
        public void BadMethod(){ }

        void IRunnableHook.Prologue(Runner runner, FID id)
        {
        }

        bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
        {
          Aver.Fail("I crashed in Runnable epilogue");
          return false;
        }
    }

    [Runnable(category: "runner", order: -1999)]
    public class RunnerHookExceptions_RunPrologue : IRunHook
    {
        [Run("!crash-run-prologue", "")]
        public void BadMethod(){ }

        bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
        {
          Aver.Fail("I crashed in Run prologue");
          return false;
        }

        bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
        {
          return false;
        }
    }

    [Runnable(category: "runner", order: -1000)]
    public class RunnerHookExceptions_RunEpilogue : IRunHook
    {
        [Run("!crash-run-epilogue", "")]
        public void BadMethod(){ }

        bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
        {
          return false;
        }

        bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
        {
          Aver.Fail("I crashed in Run epilogue");
          return false;
        }
    }


}
