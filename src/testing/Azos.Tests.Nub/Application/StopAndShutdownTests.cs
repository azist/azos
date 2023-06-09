/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Scripting;

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class StopAndShutdownTests
  {
    [Run]
    public void Test01()
    {
      //We are already in the current container (trun for unit testing)!!!!!!!!!

      var orgInstance = ExecutionContext.Application;
      var orgInstanceId = orgInstance.InstanceId;

      var app = new AzosApplication(null);

      new { orgInstanceId }.See();
      new { ExecutionContext.Application.InstanceId }.See();

      Aver.AreNotSameRef(orgInstance, ExecutionContext.Application);
      Aver.IsFalse(orgInstanceId == ExecutionContext.Application.InstanceId);

      Aver.IsTrue(Azos.Apps.ExecutionContext.Application is AzosApplication); //#876
      Aver.IsTrue(app.Active);
      Aver.IsFalse(app.Stopping);
      Aver.IsFalse(app.ShutdownStarted);
      Aver.IsFalse(app.WaitForStopOrShutdown(100));

      app.Stop();

      Aver.IsFalse(app.Active);
      Aver.IsTrue(app.Stopping);
      Aver.IsFalse(app.ShutdownStarted);
      Aver.IsTrue(app.WaitForStopOrShutdown(100));

      app.Dispose();//<===========================

      //#876
      Aver.AreSameRef(orgInstance, ExecutionContext.Application);//we are back to original application
      Aver.IsTrue(orgInstanceId == ExecutionContext.Application.InstanceId);

      Aver.IsFalse(app.Active);
      Aver.IsTrue(app.Stopping);
      Aver.IsTrue(app.ShutdownStarted);
      Aver.IsTrue(app.WaitForStopOrShutdown(100));
    }

    [Run]
    public void Test02()
    {
      using(var app = new AzosApplication(null))
      {
        Aver.IsTrue(app.Active);
        Aver.IsFalse(app.Stopping);
        Aver.IsFalse(app.ShutdownStarted);
        Aver.IsFalse(app.WaitForStopOrShutdown(10));

        Task.Delay(2_000).ContinueWith(_ => app.Stop());

        int i = 1;
        while(!app.WaitForStopOrShutdown(100)) i++;

        "Took: {0}".SeeArgs(i);
        Aver.IsTrue( i >= 18 && i < 26);
      }
    }
  }
}
