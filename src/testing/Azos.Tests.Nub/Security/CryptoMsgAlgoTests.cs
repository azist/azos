/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Security;
using Azos.Serialization.JSON;

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
        name='aes-dflt'  default=true
        type='Azos.Security.HMACAESCryptoMessageAlgorithm, Azos'
        hmac{key='0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3'}
        aes{key='0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1'}
      }

      algorithm
      {
        name='jwt-1'  default=true
        type='Azos.Security.JWTHS256CryptoMessageAlgorithm, Azos'
        hmac{key='base64:Mrb6L5KuBwuUsDwMiIgQvwi2Wmh6vWtUWS102C3Ep00fI1vKPEH2xnqbUPqF6NrOAa1WrPoSuI0lxpSEkr7kXw'}
      }

      algorithm
      {
        name='jwt-2'  default=true
        type='Azos.Security.JWTHS256CryptoMessageAlgorithm, Azos'
        hmac{key='base64:M23138tnECZp_hV2wpu5oi5Xt8Lu1PrLYJXc7LTFMLDuMDOGBaOI9DRskbZzYlh37eq1ya5YLsqVPO2DYx-YYw'}
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
    public void Cipher_HMACAES_Basic()
    {
      var msg = new { a = 1, b = 2, c = true, d = false, s = "Snake Hassan", ts = new DateTime(1980, 1,1) };

      var pvt = m_App.SecurityManager.PublicProtectAsString(msg);
      var got = m_App.SecurityManager.PublicUnprotectMap(pvt);

      pvt.See();

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
    public void Cipher_HMACAES_Tamper()
    {
      var msg = new { a = 1, b = 2, c = true, d = false, s = "Snake Hassan", ts = new DateTime(1980, 1, 1) };

      var pvt = m_App.SecurityManager.GetDefaultPublicCipher().ProtectAsBuffer(msg);

      pvt.See("Original: ");
      pvt[7] = (byte)~pvt[7];
      pvt.See("Tampered: ");

      var got = m_App.SecurityManager.GetDefaultPublicCipher().UnprotectObject(new ArraySegment<byte>(pvt)) as JsonDataMap;
      Aver.IsNull(got);
      pvt[7] = (byte)~pvt[7]; //Un-Tamper
      got = m_App.SecurityManager.GetDefaultPublicCipher().UnprotectObject(new ArraySegment<byte>(pvt)) as JsonDataMap;
      Aver.IsNotNull(got);

      got.See("Deciphered from original:");

      Aver.AreEqual(6, got.Count);
      Aver.AreEqual(msg.a, got["a"].AsInt());
      Aver.AreEqual(msg.b, got["b"].AsInt());
      Aver.AreEqual(msg.c, got["c"].AsBool());
      Aver.AreEqual(msg.d, got["d"].AsBool());
      Aver.AreEqual(msg.s, got["s"].AsString());
      Aver.AreEqual(msg.ts, got["ts"].AsDateTime());
    }

    [Run]
    public void Cipher_HMACAES_Padding()
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
    public void Cipher_HMACAES_Benchmark(int cnt)
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
    public void Cipher_HMACAES_Parallel_Benchmark(int cnt)
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


    [Run]
    public void JWTHS256_Basic()
    {
      var claims = new JsonDataMap{ {"aud", "pub"}, {"sub", "dima"}, { "admin", "false" } };

      var jwt = m_App.SecurityManager.PublicProtectJWTPayload(claims);

      jwt.See("Encoded JWT:");

      var got = m_App.SecurityManager.PublicUnprotectJWTPayload(jwt);

      got.See("Deciphered from JWT:");

      Aver.IsNotNull(got);
      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(claims["aud"].AsString(), got["aud"].AsString());
      Aver.AreEqual(claims["sub"].AsString(), got["sub"].AsString());
      Aver.AreEqual(claims["admin"].AsString(), got["admin"].AsString());
    }


    [Run]
    public void JWTHS256_Tamper()
    {
      var claims = new JsonDataMap { { "aud", "ZZZ" }, { "sub", "KKK" } };

      var jwt = m_App.SecurityManager.GetDefaultPublicJWT().ProtectJWTPayloadAsBuffer(claims);

      Encoding.UTF8.GetString(jwt).See("Encoded JWT:");

      var got = m_App.SecurityManager.GetDefaultPublicJWT().UnprotectJWTPayload(new ArraySegment<byte>(jwt));

      got.See("Deciphered from JWT:");

      Aver.IsNotNull(got);
      Aver.AreEqual(2, got.Count);
      Aver.AreEqual(claims["aud"].AsString(), got["aud"].AsString());
      Aver.AreEqual(claims["sub"].AsString(), got["sub"].AsString());

      var was = jwt[3];
      jwt[3] = was!=(byte)'A' ? (byte)'A' : (byte)'B';

      Encoding.UTF8.GetString(jwt).See("Tampered JWT:");
      got = m_App.SecurityManager.GetDefaultPublicJWT().UnprotectJWTPayload(new ArraySegment<byte>(jwt));
      Aver.IsNull(got);//tampered
    }

    [Run]
    public void JWTHS256_ReadDifferentKey()
    {
      var claims = new JsonDataMap { { "aud", "AAAA" }, { "sub", "BBBBBBBBBB!!!!!" } };

      var alg1 = m_App.SecurityManager.Cryptography.MessageProtectionAlgorithms["jwt-1"];
      var alg2 = m_App.SecurityManager.Cryptography.MessageProtectionAlgorithms["jwt-2"];

      Aver.IsNotNull(alg1);
      Aver.IsNotNull(alg2);

      var jwt = alg1.ProtectJWTPayloadAsString(claims);

      jwt.See("Encoded JWT:");

      var got = alg1.UnprotectJWTPayload(jwt);

      got.See("Deciphered from JWT:");

      Aver.IsNotNull(got);
      Aver.AreEqual(2, got.Count);
      Aver.AreEqual(claims["aud"].AsString(), got["aud"].AsString());
      Aver.AreEqual(claims["sub"].AsString(), got["sub"].AsString());

      got = alg2.UnprotectJWTPayload(jwt);//<-- different hmac key can not unprotect
      Aver.IsNull(got);

      // -----------------------------------

      jwt = alg2.ProtectJWTPayloadAsString(claims);//protect with key2
      jwt.See("Encoded JWT another key:");

      got = alg2.UnprotectJWTPayload(jwt);//this time it reads jwt OK

      got.See("Deciphered from JWT another key:");

      Aver.IsNotNull(got);
      Aver.AreEqual(2, got.Count);
      Aver.AreEqual(claims["aud"].AsString(), got["aud"].AsString());
      Aver.AreEqual(claims["sub"].AsString(), got["sub"].AsString());
    }


  }
}


