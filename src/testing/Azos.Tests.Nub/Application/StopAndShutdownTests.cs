﻿/*<FILE_LICENSE>
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
      var app = new AzosApplication(null);
      Aver.IsTrue(app.Active);
      Aver.IsFalse(app.Stopping);
      Aver.IsFalse(app.ShutdownStarted);
      Aver.IsFalse(app.WaitForStopOrShutdown(100));

      app.Stop();

      Aver.IsFalse(app.Active);
      Aver.IsTrue(app.Stopping);
      Aver.IsFalse(app.ShutdownStarted);
      Aver.IsTrue(app.WaitForStopOrShutdown(100));

      app.Dispose();

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

        Task.Delay(2000).ContinueWith(_ => app.Stop());

        int i = 0;
        while(!app.WaitForStopOrShutdown(100)) i++;

        Aver.IsTrue( i >= 19 && i<25);
      }
    }
  }
}
