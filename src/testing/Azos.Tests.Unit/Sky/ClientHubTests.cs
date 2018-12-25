using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Sky;
using Azos.Sky.Contracts;

namespace Azos.Tests.Unit.Sky
{
  [Runnable]
  public class ClientHubTests : BaseTestRigWithMetabase
  {

     const string CONFIG1 =
@"
app
{
  glue
  {
    bindings
    {
      binding
      {
        name='sync'
        type='Azos.Glue.Native.SyncBinding, Azos'
      }
    }

    servers
    {
      server
      {
        name='TestServerSync'
        node='$(~SysConsts.SYNC_BINDING)://*:$(~SysConsts.NETWORK_SVC_TESTER_SYNC_PORT)'
        contract-servers='Azos.Tests.Unit.Sky.ClientHubTests+TestingServer, Azos.Tests.Unit'
      }
    }

  }
}
";

     internal class TestingServer : ITester
     {
       public object TestEcho(object data)
       {
         if (data is string  str && str=="FAIL") throw new AzosException("I failed!");
         return data;
       }
     }



      [Run]
      public void CH_EchoOneCall()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new AzosApplication(null, conf))
         {
              using( var tester = app.GetServiceClientHub().MakeNew<ITesterClient>("/us/east/cle/a/ii/wmed0004"))
              {
                 var arg = "Abcdefg";
                 var echoed = tester.TestEcho(arg);
                 Aver.AreObjectsEqual( arg, echoed );
              }
        }
      }

      [Run]
      public void CH_EchoTwoCalls()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new AzosApplication(null, conf))
         {
              using( var tester = app.GetServiceClientHub().MakeNew<ITesterClient>("/us/east/cle/a/ii/wmed0004"))
              {
                 var arg = "Abcdefg";
                 var echoed = tester.TestEcho(arg);
                 Aver.AreObjectsEqual( arg, echoed );

                 arg = "Ze Rozenberg";
                 echoed = tester.TestEcho(arg);
                 Aver.AreObjectsEqual( arg, echoed );
              }
        }
      }


      [Run]
      public void CH_CallWithRetry_FirstHostErr()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new AzosApplication(null, conf))
         {
              Exception err = null;
              var arg = "Abcdefg";
              var echoed = app.GetServiceClientHub().CallWithRetry<ITesterClient, object>
              (
                (cl) => cl.TestEcho( arg ),
                new string[]{ "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/ii/wmed0004"},
                (cl, er) => { err = er; Console.WriteLine(er.ToMessageWithType()); return false; }
              );

              Aver.AreObjectsEqual( arg, echoed );
              Aver.IsTrue( err.ToMessageWithType().Contains("address is not valid"));
        }
      }

      [Run]
      [Aver.Throws(Message="address is not valid", MsgMatch=Aver.ThrowsAttribute.MatchType.Contains)]
      public void CH_CallWithRetry_HostErrAbort()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new AzosApplication(null, conf))
         {
              Exception err = null;
              var arg = "Abcdefg";
              var echoed = app.GetServiceClientHub().CallWithRetry<ITesterClient, object>
              (
                (cl) => cl.TestEcho( arg ),
                new string[]{ "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/i/wmed0001"},
                (cl, er) => { err = er; Console.WriteLine(er.ToMessageWithType()); return true; }
              );
        }
      }


      [Run]
      public void CH_CallWithRetryAsync_FirstTwoHostErr()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new AzosApplication(null, conf))
         {
              var err = new List<Exception>();
              var arg = "Abcdefg";
              var echoed = app.GetServiceClientHub().CallWithRetryAsync<ITesterClient, object>
              (
                (cl) => cl.Async_TestEcho( arg ).AsTaskReturning<object>(),
                new string[]{ "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/ii/wmed0004"},
                (cl, er) => { err.Add(er); Console.WriteLine(er.ToMessageWithType()); return false; }
              );

              Aver.AreObjectsEqual( arg, echoed.Result );
              Aver.IsTrue( err[0].ToMessageWithType().Contains("address is not valid"));
              Aver.IsTrue( err[1].ToMessageWithType().Contains("address is not valid"));
        }
      }

      [Run]
      [Aver.Throws(Message="address is not valid")]
      public void CH_CallWithRetryAsync_FirstHostErrAbort()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new AzosApplication(null, conf))
         {
              Exception err = null;
              var arg = "Abcdefg";
              var echoed = app.GetServiceClientHub().CallWithRetryAsync<ITesterClient, object>
              (
                (cl) => cl.Async_TestEcho( arg ).AsTaskReturning<object>(),
                new string[]{ "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/i/wmed0001"},
                (cl, er) => { err = er; Console.WriteLine(er.ToMessageWithType()); return true; }
              );

              try
              {
                var r = echoed.Result;
              }
              catch (AggregateException ae)
              {
                throw ae.GetBaseException();
              }
        }
      }


      [Run]
      [Aver.Throws(typeof(Azos.Glue.RemoteException), Message="I failed")]
      public void CH_EchoFAIL()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new AzosApplication(null, conf))
         {
              using( var tester = app.GetServiceClientHub().MakeNew<ITesterClient>("/us/east/cle/a/ii/wmed0004"))
              {
                 var arg = "FAIL";
                 var echoed = tester.TestEcho(arg);
              }
        }
      }


      [Run]
      [Aver.Throws(typeof(Azos.Glue.RemoteException), Message="I failed")]
      public void CH_CallWithRetry_TwoHostsErr()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new AzosApplication(null, conf))
         {
              Exception err = null;
              var arg = "FAIL";
              var echoed = app.GetServiceClientHub().CallWithRetry<ITesterClient, object>
              (
                (cl) => cl.TestEcho( arg ),
                new string[]{ "/us/east/cle/a/i/wmed0001", "/us/east/cle/a/ii/wmed0004"},
                (cl, er) => { err = er; Console.WriteLine(er.ToMessageWithType()); return er is Azos.Glue.RemoteException; }
              );
        }
      }


      [Run]
      [Aver.Throws(typeof(Azos.Sky.Clients.SkyClientException), Message="after 2 retries")]
      public void CH_CallWithRetry_TwoHostErrAbortFalse()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new AzosApplication(null, conf))
         {
              Exception err = null;
              var arg = "FAIL";
              var echoed = app.GetServiceClientHub().CallWithRetry<ITesterClient, object>
              (
                (cl) => cl.TestEcho( arg ),
                new string[]{ "/us/east/cle/a/i/wmed0001", "/us/east/cle/a/i/wmed0001"},
                (cl, er) => { err = er; Console.WriteLine(er.ToMessageWithType()); return false; }
              );
        }
      }


      [Run]
      [Aver.Throws(typeof(Azos.Glue.RemoteException), Message="I failed")]
      public void CH_CallWithRetryAsync_TwoHostsErr()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new AzosApplication(null, conf))
         {
              Exception err = null;
              var arg = "FAIL";
              var echoed = app.GetServiceClientHub().CallWithRetryAsync<ITesterClient, object>
              (
                (cl) => cl.Async_TestEcho( arg ).AsTaskReturning<object>(),
                new string[]{ "/us/east/cle/a/i/wmed0001", "/us/east/cle/a/ii/wmed0004"},
                (cl, er) => {
                  err = er; Console.WriteLine(er.ToMessageWithType()); return er is Azos.Glue.RemoteException;
                }
              );

              try
              { var x = echoed.Result; }
              catch(AggregateException ae)
              {
                throw ae.GetBaseException();
              }
        }
      }

      [Run]
      public void CH_ClientCallRetryAsync_TaskVoid()
      {
        var conf = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
        using (var app = new AzosApplication(null, conf))
        {
          var echoed = app.GetServiceClientHub().CallWithRetryAsync<ITesterClient>(
            (cl) => cl.Async_TestEcho( "test" ).AsTaskReturning<object>(),
            new string[]{ "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/i/wmed0001"},
            (cl, er) => {
              Console.WriteLine(er.ToMessageWithType()); return false; }
          );
        }
      }

  }
}
