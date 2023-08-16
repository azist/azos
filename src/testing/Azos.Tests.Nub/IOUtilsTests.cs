/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class IOUtilsTests
  {
    [Run]
    public void Base64DecodeWithSpaces_256()
    {
      var expect = "CD,7D,B1,13,A8,54,9C,5E,0F,14,E9,15,02,E4,1C,70,7C,E6,A3,34,AD,F4,95,34,DC,0F,9D,E2,C3,BD,CE,CD".AsByteArray();
      Aver.AreEqual(0xcd, expect[0]);
      Aver.AreEqual(0x7d, expect[1]);
      Aver.AreEqual(0xcd, expect[^1]);
      var got = "zX2xE6hUnF4PFOkVAuQccHzmozSt9JU03A-d4sO9zs0".FromWebSafeBase64();
      Aver.AreArraysEquivalent(expect, got);

      got = "zX2x E6\nhU n F 4        PFO\r\r\nkVAuQc  \t  c H zmoz \t \tSt9JU03A- d4sO    9zs0".FromWebSafeBase64();
      Aver.AreArraysEquivalent(expect, got);

      got = @"zX2xE6hU
              nF4PFOkV
              AuQccHzm
              ozSt9JU0
              3A-d4sO9zs0".FromWebSafeBase64();
      Aver.AreArraysEquivalent(expect, got);

      got = @"zX2xE6hU nF4PFOkV
              AuQccH zmozSt9JU0
              3A-d4sO 9zs0".FromWebSafeBase64();
      Aver.AreArraysEquivalent(expect, got);
    }

    [Run]
    public void Base64DecodeWithSpaces_SizeLoop()
    {
      for (var cnt = 1; cnt < 1024; cnt++)
      {
        var expect = Ambient.Random.NextRandomBytes(cnt);
        var a = expect.ToWebSafeBase64();

        for(var i=0; i< 10; i++)
        {
          a = a.Insert(Ambient.Random.NextScaledRandomInteger(0, a.Length-1), " \n ");
        }

        var got = a.FromWebSafeBase64();
        Aver.AreArraysEquivalent(expect, got);
      }
    }




    [Run("a='0'")]
    [Run("a='0,0,0'")]
    [Run("a='255,255,255'")]
    [Run("a='255,255,255,255'")]
    [Run("a='255,255,255,0'")]
    [Run("a='1'")]
    [Run("a='255'")]
    [Run("a='127'")]
    [Run("a='0,255'")]
    [Run("a='1,2,3,4,5,6,7,8,9,0'")]
    [Run("a='1,2,3,4,5,6,7,8,9,254,255'")]
    [Run("a='1,2,3,4,5,6,7,8,9,254,127'")]
    [Run("a='1,2,3,4,5,6,7,8,9,254,0'")]
    public void Base64FullCycle(byte[] a)
    {
      var encoded = a.ToWebSafeBase64();
      var b = encoded.FromWebSafeBase64();

      //a.ToDumpString(DumpFormat.Hex).See();
      //b.ToDumpString(DumpFormat.Hex).See();
      //encoded.See();

      //We use two equality comparisons to protect against possible bug in one
      //as To/FromBase64WebSafeString() is a very important method used in security throughout
      Aver.AreArraysEquivalent(a, b);
      Aver.IsTrue(a.MemBufferEquals(b));
    }

    [Run]
    public void Base64FullCycle_Loop()
    {
      for (var cnt = 1; cnt < 1024; cnt++)
      {
        var a = Platform.RandomGenerator.Instance.NextRandomBytes(cnt);
        Base64FullCycle(a);
      }
    }

    [Run("v='1883739E-48F0-4FA7-9DF2-AF6B067D605D'")]
    [Run("v='F13C9A02-C1BF-4C1A-89F7-19DB1CD441F7'")]
    [Run("v='3FAF3D3E-1DEA-43E8-8684-DF046CC10498'")]
    [Run("v='450F098F-FA0B-4CE1-BE3C-03665945C77E'")]
    public void GuidCasting(Guid v)
    {
      var tpl = IOUtils.CastGuidToLongs(v);
      var got = IOUtils.CastGuidFromLongs(tpl.s1, tpl.s2);
      Aver.AreEqual(v, got);
      v.See();
      got.See();
    }

    [Run("v='1883739E-48F0-4FA7-9DF2-AF6B067D605D'")]
    [Run("v='F13C9A02-C1BF-4C1A-89F7-19DB1CD441F7'")]
    [Run("v='3FAF3D3E-1DEA-43E8-8684-DF046CC10498'")]
    [Run("v='450F098F-FA0B-4CE1-BE3C-03665945C77E'")]
    public void FastGuidEncoding(Guid v)
    {
      var buf = new byte[32];
      buf.FastEncodeGuid(0, v);
      var got = buf.FastDecodeGuid(0);
      Aver.AreEqual(v, got);
      v.See();
      got.See();
    }

    //// - BenchmarkFastGuidEncoding
    ////ToByteArray()    does  69,902,950 ops/sec
    ////FastEncodeGuid() does 161,775,859 ops/sec

    [Run("!guidbench", "")]
    public void BenchmarkFastGuidEncoding()
    {
      const int CNT = 25_000_000;
      var v = Guid.Parse("3FAF3D3E-1DEA-43E8-8684-DF046CC10498");
      var buf = new byte[32];

      var t = Azos.Time.Timeter.StartNew();
      for (var i = 0; i < CNT; i++)
      {
        v.ToByteArray();
      }
      t.Stop();

      var e1 = t.ElapsedSec;
      t = Azos.Time.Timeter.StartNew();
      for (var i = 0; i < CNT; i++)
      {
        buf.FastEncodeGuid(0, v);
        //  IOUtils.CastGuidToLongs(v);
      }
      t.Stop();
      var e2 = t.ElapsedSec;

      "\nToByteArray() does {0:n0} ops/sec".SeeArgs(CNT / e1);
      "\nFastEncodeGuid() does {0:n0} ops/sec".SeeArgs(CNT / e2);
    }

  }
}