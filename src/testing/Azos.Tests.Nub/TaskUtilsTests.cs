/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class TaskUtilsTests
  {
    [Run]
    public void TryGetCompletedTaskResultAsObject_0()
    {
      Task task = Task.CompletedTask;
      var got = task.TryGetCompletedTaskResultAsObject();
      Aver.IsFalse(got.ok);
      Aver.IsNull(got.result);
    }


    [Run]
    public void TryGetCompletedTaskResultAsObject_1()
    {
      Task task = Task.FromResult(123);
      var got = task.TryGetCompletedTaskResultAsObject();
      Aver.IsTrue(got.ok);
      Aver.AreEqual(123, (int) got.result);

      task = Task.FromResult("abcd");
      got = task.TryGetCompletedTaskResultAsObject();
      Aver.IsTrue(got.ok);
      Aver.AreEqual("abcd", (string)got.result);
    }

    [Run]
    public async Task TryGetCompletedTaskResultAsObject_2()
    {
      Task task = Task.Delay(1000).ContinueWith( a => 1234);
      var got = task.TryGetCompletedTaskResultAsObject();
      Aver.IsFalse(got.ok);

      await task;

      got = task.TryGetCompletedTaskResultAsObject();
      Aver.IsTrue(got.ok);
      Aver.AreEqual(1234, (int)got.result);
    }

    [Run]
    public void TryGetCompletedTaskResultAsObject_3()
    {
      Task task = Task.Delay(500).ContinueWith(a => throw new Exception("XXX"));
      var got = task.TryGetCompletedTaskResultAsObject();
      Aver.IsFalse(got.ok);

      Thread.Sleep(1500);

      got = task.TryGetCompletedTaskResultAsObject();
      Aver.IsFalse(got.ok);

      Console.WriteLine(task.Exception.Flatten().InnerException.ToMessageWithType());//XXX
    }

  }
}