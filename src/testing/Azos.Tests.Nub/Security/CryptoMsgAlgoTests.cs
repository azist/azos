/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Security;

namespace Azos.Tests.Nub.Security
{
  [Runnable]
  public class CryptoMsgAlgoTests : IRunnableHook
  {
      private static string conf =
      @"
app
{
  security
  {
    cryptography
    {
      algorithm
      {
        name='aes1'  default=true
        type='Azos.Security.HMACAESCryptoMessageAlgorithm, Azos'
        hmac{key='0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3'}
        aes{key='0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1'}
      }
    }
  }//security
}//app
";
    private AzosApplication m_App;

    void IRunnableHook.Prologue(Runner runner, FID id)
     => m_App = new AzosApplication(null, conf.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_App);
      return false;
    }


    [Run]
    public void Message_1()
    {
      var msg = new { a = 1, b = 2, c = true, d = false, s = "Snake Hassan", ts = new DateTime(1980, 1,1) };

      var pvt = m_App.SecurityManager.PublicProtectAsString(msg);
      var got = m_App.SecurityManager.PublicUnprotectMap(pvt);

      Console.WriteLine(pvt);

      Aver.IsNotNull(got);
      Aver.AreEqual(6, got.Count);
      Aver.AreEqual(msg.a, got["a"].AsInt());
      Aver.AreEqual(msg.b, got["b"].AsInt());
      Aver.AreEqual(msg.c, got["c"].AsBool());
      Aver.AreEqual(msg.d, got["d"].AsBool());
      Aver.AreEqual(msg.s, got["s"].AsString());
      Aver.AreEqual(msg.ts, got["ts"].AsDateTime());
    }

    [Run]
    public void Message_Padding()
    {
      for(var i=1; i<3*1024; i++)
      {
        var msg = new {v = new String('a', i)};
        var pvt = m_App.SecurityManager.PublicProtectAsString(msg);
        var got = m_App.SecurityManager.PublicUnprotectMap(pvt);
        Aver.AreEqual(msg.v, got["v"].AsString());
      }
    }

    [Run("cnt=100")]
    [Run("cnt=1000")]
    [Run("cnt=10000")]
    public void Benchmark(int cnt)
    {
      var msg = new { a = 1, b = 2, c = true, d = false, s = "hdfiasdifuhasudihfuiashfouihaisuhfiouash", ts = DateTime.Now };

      var sw = Stopwatch.StartNew();
      for(var i=0; i<cnt; i++)
      {
        var pvt = m_App.SecurityManager.PublicProtectAsString(msg);
        var got = m_App.SecurityManager.PublicUnprotectMap(pvt);
        Aver.IsNotNull(got);
        Aver.AreEqual(6, got.Count);
        Aver.AreEqual(1, got["a"].AsInt());
        Aver.AreEqual(2, got["b"].AsInt());
      }
      Console.WriteLine("Did {0:n} in {1:n} ms at {2:n}/sec".Args(cnt, sw.ElapsedMilliseconds, cnt / (sw.ElapsedMilliseconds /1000d)));
    }

    [Run("cnt=100")]
    [Run("cnt=1000")]
    [Run("cnt=10000")]
    public void Parallel_Benchmark(int cnt)
    {
      var msg = new { a = 1, b = 2, c = true, d = false, s = "hdfiasdifuhasudihfuiashfouihaisuhfiouash", ts = DateTime.Now };

      var sw = Stopwatch.StartNew();
      Parallel.For(0,cnt, i =>
      {
        var pvt = m_App.SecurityManager.PublicProtectAsString(msg);
        var got = m_App.SecurityManager.PublicUnprotectMap(pvt);
        Aver.IsNotNull(got);
        Aver.AreEqual(6, got.Count);
        Aver.AreEqual(1, got["a"].AsInt());
        Aver.AreEqual(2, got["b"].AsInt());
      });
      Console.WriteLine("Did {0:n} in {1:n} ms at {2:n}/sec".Args(cnt, sw.ElapsedMilliseconds, cnt / (sw.ElapsedMilliseconds / 1000d)));
    }


  }
}


