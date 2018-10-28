/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.Reflection;

using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Access.Subscriptions;
using Azos.Data.Access.Erlang;
using Azos.Erlang;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Integration.CRUD
{
  /// <summary>
  /// Mongo CRUD tests
  /// </summary>
  [Runnable("!ErlangTests")]
  public class ErlangTests : IRunHook
  {
     private static readonly string[] CCY_PAIRS = new string[]
     {
        "AUD/CAD", "AUD/CHF", "AUD/HKD", "AUD/JPY", "AUD/NZD", "AUD/USD",
        "CAD/JPY", "CHF/JPY", "CHF/NOK", "CHF/SEK", "EUR/AUD", "EUR/CAD",
        "EUR/CHF", "EUR/CZK", "EUR/DKK", "EUR/GBP", "EUR/HKD", "EUR/HUF",
        "EUR/JPY", "EUR/NOK", "EUR/NZD", "EUR/PLN", "EUR/RUB", "EUR/SEK",
        "EUR/TRY", "EUR/USD", "EUR/ZAR", "GBP/AUD", "GBP/CAD", "GBP/CHF",
        "GBP/CZK", "GBP/HUF", "GBP/JPY", "GBP/NOK", "GBP/NZD", "GBP/PLN",
        "GBP/SEK", "GBP/USD", "HKD/JPY", "NOK/SEK", "NZD/JPY", "NZD/USD",
        "USD/CAD", "USD/CHF", "USD/CNH", "USD/CZK", "USD/DKK", "USD/HKD",
        "USD/HUF", "USD/ILS", "USD/JPY", "USD/MXN", "USD/NOK", "USD/PLN",
        "USD/RUB", "USD/SEK", "USD/SGD", "USD/THB", "USD/TRY", "USD/ZAR",
        "XAG/USD", "XAU/USD", "ZAR/JPY"
     };



    private const string SCRIPT_ASM = "Azos.Tests.Integration";

    private const string REMOTE_NAME = "nfx@localhost";
    private const string REMOTE_COOKIE = "klubnika";


    private ErlDataStore store;

    bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
    {
      var node = new ErlLocalNode("test@localhost", true, false);
      node.Start();
      ErlApp.Node = node;

      store = new ErlDataStore();

      store.RemoteName = REMOTE_NAME;
      store.RemoteCookie = REMOTE_COOKIE;
      store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
      //store.QueryResolver.RegisterHandlerLocation("Azos.Tests.Integration.CRUD.ErlSpecific, Azos.Tests.Integration");
      store.Start();

      clearAll();
      return false;
    }

    bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
    {
      DisposableObject.DisposeAndNull(ref store);
      ErlApp.Node.Dispose();
      ErlApp.Node = null;
      return false;
    }

    private void clearAll()
    {

    }

    //==========================================================================================


    [Run]
    public void ErlConnect()
    {
      var qry = new Query("CRUD.Echo")
        {
          new Query.Param("Msg", "Lenin!")
        };

      var row = store.LoadOneDoc(qry);

      Aver.IsNotNull(row);
      Aver.AreObjectsEqual("You said: Lenin!", row["echoed_msg"]);
      Aver.AreEqual(DateTime.UtcNow.Year, row["ts"].AsDateTime().Year);
    }


    [Run("cnt=20 msecDelay=50")]
    public void ErlConnect(int cnt, int msecDelay)
    {
      var qry = new Query("CRUD.Echo")
        {
          new Query.Param("Msg", "Lenin!")
        };

      for(var i=0; i<cnt; i++)
      {
        var row = store.LoadOneDoc(qry);

        Aver.IsNotNull(row);
        Aver.AreObjectsEqual("You said: Lenin!", row["echoed_msg"]);
        Aver.AreEqual(DateTime.UtcNow.Year, row["ts"].AsDateTime().Year);

        Console.WriteLine("{0}th iteration is ok".Args(i));

        System.Threading.Thread.Sleep(msecDelay);
      }
    }


    [Run]
    public void ErlInsert()
    {
      var schema = store.GetSchema(new Query("CRUD.SECDEF"));
      var row = new DynamicDoc(schema);

      row.ApplyDefaultFieldValues(store.TargetName);

      row["xchg"]       = "HS";
      row["symbol"]     = "EURUSD";
      row["instr"]      = "EUR/USD";
      row["secid"]      = 1;
      row["xchg_secid"] = 1010;
      row["ccy"]        = "EUR";
      row["settl_ccy"]  = "USD";

      row["contr_mult"] = 1.0d;
      row["px_step"]    = 10e-5d;

      Aver.IsNull(row.Validate());

      var affected = store.Insert(row);
      Aver.AreEqual(1, affected);
    }


    [Run]
    public void ErlInsertManyAndQuery()
    {
      var schema = store.GetSchema(new Query("CRUD.SECDEF"));

      for (var i = 0; i < CCY_PAIRS.Length; i++)
      {
        var row = new DynamicDoc(schema);

        row.ApplyDefaultFieldValues(store.TargetName);

        var ccy1 = CCY_PAIRS[i].Substring(0, 3);
        var ccy2 = CCY_PAIRS[i].Substring(4, 3);

        row["xchg"]       = "CLE";
        row["symbol"]     = ccy1 + ccy2;
        row["instr"]      = CCY_PAIRS[i];
        row["secid"]      = i;
        row["xchg_secid"] = 1000 + i;
        row["ccy"]        = ccy1;
        row["settl_ccy"]  = ccy2;

        row["contr_mult"] = 1.0d;
        row["px_step"]    = 10e-5d;

        Aver.IsNull(row.Validate());

        var affected = store.Insert(row);
        Aver.AreEqual(1, affected);
      }

      var qry = new Query("CRUD.SecDef.ByExchange")
      {
          new Query.Param("Exchange", "CLE")
      };

      var data = store.LoadOneRowset(qry);

      Aver.IsNotNull(data);

      Console.WriteLine(data.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap));

      Aver.AreEqual(CCY_PAIRS.Length, data.Count);
    }


    [Run]
    public void ErlInsertManyDeleteAndQuery()
    {
      var schema = store.GetSchema(new Query("CRUD.SECDEF"));

      for (var i = 0; i < CCY_PAIRS.Length; i++)
      {
        var row = new DynamicDoc(schema);

        row.ApplyDefaultFieldValues(store.TargetName);

        var ccy1 = CCY_PAIRS[i].Substring(0, 3);
        var ccy2 = CCY_PAIRS[i].Substring(4, 3);

        row["xchg"]       = "NYSE";
        row["symbol"]     = ccy1 + ccy2;
        row["instr"]      = CCY_PAIRS[i];
        row["secid"]      = i;
        row["xchg_secid"] = 1000 + i;
        row["ccy"]        = ccy1;
        row["settl_ccy"]  = ccy2;

        row["contr_mult"] = 1.0d;
        row["px_step"]    = 10e-5d;

        Aver.IsNull(row.Validate());

        var affected = store.Upsert(row);
        Aver.AreEqual(1, affected);
      }

      var qry = new Query("CRUD.SecDef.ByExchange")
      {
          new Query.Param("Exchange", "NYSE")
      };

      var data = store.LoadOneRowset(qry);

      Aver.IsNotNull(data);

      Console.WriteLine(data.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap));

      Aver.AreEqual(CCY_PAIRS.Length, data.Count);

      var del = new DynamicDoc(schema);

      del["xchg"]   = "CLE";
      del["symbol"] = "USDMXN";

      Aver.AreEqual(1, store.Delete(del));

      data = store.LoadOneRowset(qry);//NYSE

      Aver.IsNotNull(data);
      Aver.AreEqual(CCY_PAIRS.Length, data.Count);

      qry = new Query("CRUD.SecDef.ByExchange")
      {
          new Query.Param("Exchange", "CLE")
      };

      data = store.LoadOneRowset(qry);//Requery for CLE

      Aver.IsNotNull(data);
      Aver.AreEqual(CCY_PAIRS.Length - 1, data.Count);//1 was deleted!!!!!!!!!!!
    }


    [Run]
    public void ErlSubscribe()
    {
      const int CNT = 10;
      const int PERIOD_MS = 250;

      /*
      ErlApp.Node.TraceLevel = ErlTraceLevel.Send;
      ErlApp.Node.Trace += (object sender, ErlTraceLevel type, Direction dir, string message) =>
      {
        if (message.StartsWith("REGSEND"))
          Console.WriteLine(message);
      };
      */

      var qry = new Query("CRUD.WorldNews")
      {
          new Query.Param("Count",  CNT),
          new Query.Param("Period", PERIOD_MS)
      };

      var mail = store.OpenMailbox("News");
      mail.BufferCapacity = 100;

      var callCount = 0;
      var done = 0;

      mail.Receipt += delegate(Subscription subscription, Mailbox recipient, CRUDSubscriptionEvent data, Exception error)
      {
        if (data.Doc!=null)
        {
          System.Threading.Interlocked.Increment(ref callCount);
          if (callCount >= CNT)
            System.Threading.Interlocked.Increment(ref done);
        }
      };

      var subscribe = store.Subscribe("NewsFeed", qry, mail);

      var sw = Stopwatch.StartNew();
      var maxWaitMs = 2* (CNT * PERIOD_MS);

      while(done==0 && sw.ElapsedMilliseconds<maxWaitMs)
        System.Threading.Thread.Sleep(500);

      Aver.AreEqual(CNT, callCount);

      var buffered = mail.Buffered;

      Console.WriteLine(buffered.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap));
    }

    [Run]
    public void ErlSubscribeUnsubscribe()
    {
      const int CNT = 10;
      const int PERIOD_MS = 500;
      const int CUTOFF = 3;

      var qry = new Query("CRUD.WorldNews")
      {
          new Query.Param("Count",  CNT),
          new Query.Param("Period", PERIOD_MS)
      };


      var mail = store.OpenMailbox("News");
      mail.BufferCapacity = 100;

      var callCount = 0;
      var done = 0;

      Subscription subscribe = null;

      mail.Receipt += delegate(Subscription subscription, Mailbox recipient, CRUDSubscriptionEvent data, Exception error)
      {
        if (data.Doc!=null)
        {
          if (System.Threading.Interlocked.Increment(ref callCount)==CUTOFF)
          {
            System.Threading.Interlocked.Increment(ref done);
            subscription.Dispose();//unsubscribe!!!
          }
        }
      };

      subscribe = store.Subscribe("NewsFeed", qry, mail);

      var sw = Stopwatch.StartNew();
      var maxWaitMs = 2* (CNT * PERIOD_MS);

      while(done==0 && sw.ElapsedMilliseconds<maxWaitMs)
        System.Threading.Thread.Sleep(500);

      Aver.IsTrue(callCount >= CUTOFF && callCount < CNT);

      var buffered = mail.Buffered;

      Console.WriteLine(buffered.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap));
    }

  }
}
