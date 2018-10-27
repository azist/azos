/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using Azos.ApplicationModel;
using Azos.Environment;
using Azos.Glue;
using Azos.Glue.Implementation;
using Azos.Glue.Native;
using Azos.Security;
using Azos.Scripting;

using BusinessLogic;

namespace Azos.Tests.Integration.Glue
{
  [Runnable]
  public class JokeCalculator: JokeTestBase
  {
    [Run]
    [Aver.Throws(typeof(RemoteException))]
    public void ExceptionBeforeInit()
    {
      using (JokeHelper.MakeApp())
      {
        var cl = new JokeCalculatorClient(TestServerSyncNode);
        cl.Headers.Add(new Azos.Glue.Protocol.AuthenticationHeader(TestCredentials));

        int result = cl.Add(1);
      }
    }

    [Run]
    [Aver.Throws(typeof(RemoteException))]
    public void ExceptionAfterDestructor()
    {
      using (JokeHelper.MakeApp())
      {
        var cl = new JokeCalculatorClient(TestServerSyncNode);
        cl.Headers.Add(new Azos.Glue.Protocol.AuthenticationHeader(TestCredentials));

        cl.Init(0);
        cl.Done();

        int result = cl.Add(1);
      }
    }

    [Run]
    public void Sync_JokeCalculator_TestAdd()
    {
      using (JokeHelper.MakeApp())
      {
        var cl = new JokeCalculatorClient(TestServerSyncNode);
        cl.Headers.Add(new Azos.Glue.Protocol.AuthenticationHeader(TestCredentials));

        using (Scope.OnExit(() => cl.Done()))
        {
          cl.Init(0);
          cl.Add(10);
          int result = cl.Sub(3);

          Aver.AreEqual(7, result);
        }
      }
    }
  }
}
