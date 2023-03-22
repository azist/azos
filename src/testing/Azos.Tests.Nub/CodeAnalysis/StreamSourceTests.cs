/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using Azos.IO;
using Azos.CodeAnalysis.Source;
using Azos.Scripting;

namespace Azos.Tests.Nub.CodeAnalysis
{
  [Runnable]
  public class StreamSourceTests : IRunnableHook
  {
    private IApplication m_App;

    public void Prologue(Runner runner, FID id)
    {
      m_App = runner.App;
    }

    public bool Epilogue(Runner runner, FID id, Exception error) => false;

    [Run]
    public void Utf8_4Byte_Music_DefaultBufferSize()
    {
      //https://design215.com/toolbox/utf8-4byte-characters.php
      var v = "Treble clef:\ud834\udd1e;BaseClef:\ud834\udd22";
      Aver.AreEqual(26, v.Length);
      Aver.AreEqual("Treble clef:𝄞;BaseClef:𝄢", v);

      var utf8 = StreamHookUse.EncodeStringToBuffer(v);

      "\r{0}".SeeArgs(utf8.ToHexDump());

      Aver.AreEqual(22/*ascii*/ + 8/*2 4byte utf pairs*/, utf8.Length);

      var sut = new StreamSource(new MemoryStream(utf8), null, null);
      Aver.AreEqual('T', sut.ReadChar());
      Aver.AreEqual('r', sut.ReadChar());
      Aver.AreEqual('e', sut.ReadChar());
      Aver.AreEqual('b', sut.ReadChar());
      Aver.AreEqual('l', sut.ReadChar());
      Aver.AreEqual('e', sut.ReadChar());
      Aver.AreEqual(' ', sut.ReadChar());
      Aver.AreEqual('c', sut.ReadChar());
      Aver.AreEqual('l', sut.ReadChar());
      Aver.AreEqual('e', sut.ReadChar());
      Aver.AreEqual('f', sut.ReadChar());
      Aver.AreEqual(':', sut.ReadChar());

      Aver.AreEqual(0xd834, sut.ReadChar());//UTF16 equivalent of  UTF8: f09d849e original
      Aver.AreEqual(0xdd1e, sut.ReadChar());

      Aver.AreEqual(';', sut.ReadChar());
      Aver.AreEqual('B', sut.ReadChar());
      Aver.AreEqual('a', sut.ReadChar());
      Aver.AreEqual('s', sut.ReadChar());
      Aver.AreEqual('e', sut.ReadChar());
      Aver.AreEqual('C', sut.ReadChar());
      Aver.AreEqual('l', sut.ReadChar());
      Aver.AreEqual('e', sut.ReadChar());
      Aver.AreEqual('f', sut.ReadChar());
      Aver.AreEqual(':', sut.ReadChar());

      Aver.AreEqual(0xd834, sut.ReadChar());//UTF16 equivalent of  UTF8: f09d84a2 original
      Aver.AreEqual(0xdd22, sut.ReadChar());

      Aver.IsTrue(sut.IsLastSegment);
      Aver.IsTrue(sut.NearEndOfSegment);
      Aver.IsTrue(sut.EOF);
      // "{0:x2} {1:x2}".SeeArgs((int)sut.ReadChar(), (int)sut.ReadChar());
    }


    [Run("pad=1  bsz=1024 st=1")]
    [Run("pad=16 bsz=1024 st=1")]
    [Run("pad=32 bsz=1024 st=1")]

    [Run("pad=1    bsz=0    st=0")]
    [Run("pad=1000 bsz=1024  st=20")]
    [Run("pad=2000 bsz=1024  st=20")]
    [Run("pad=2000 bsz=1024  st=64")]
    [Run("pad=2000 bsz=1024 st=93")]
    [Run("pad=2000 bsz=250000 st=128000")]

    [Run("pad=64000 bsz=1024 st=0")]
    [Run("pad=64000 bsz=1024 st=0")]
    [Run("pad=64000 bsz=1024 st=739")]
    [Run("pad=64000 bsz=3000 st=1000")]
    [Run("pad=64000 bsz=3000 st=1")]
    [Run("pad=64000 bsz=3000 st=5000")]
    [Run("pad=64000 bsz=3000 st=31")]
    [Run("pad=64000 bsz=3000 st=32")]
    [Run("pad=64000 bsz=3000 st=33")]

    [Run("pad=100 bsz=1024 st=122")]
    [Run("pad=100 bsz=1048 st=100")]

    //failing cases
    [Run("pad=2051 bsz=4129 st=4661")]

    public void Utf8_4Byte_Music(int pad, int bsz, int st)
    {
      //https://design215.com/toolbox/utf8-4byte-characters.php
      var v = (new string('-', pad)) + "Treble clef:\ud834\udd1e;" +(new string('-', pad)) + "BaseClef:\ud834\udd22" + (new string('-', pad));
      Aver.AreEqual((3 * pad) + 26, v.Length);

      var utf8 = StreamHookUse.EncodeStringToBuffer(v);

      //"\r{0}".SeeArgs(utf8.ToHexDump());

      Aver.AreEqual(22 + (3*pad)/*ascii*/ + 8/*2 4byte utf pairs*/, utf8.Length);

      var sut = new StreamSource(new MemoryStream(utf8), null, null, bufferSize: bsz, segmentTailThreshold: st);

      for(var i=0; i<pad; i++) Aver.AreEqual('-', sut.ReadChar());


      Aver.AreEqual('T', sut.ReadChar());
      Aver.AreEqual('r', sut.ReadChar());
      Aver.AreEqual('e', sut.ReadChar());
      Aver.AreEqual('b', sut.ReadChar());
      Aver.AreEqual('l', sut.ReadChar());
      Aver.AreEqual('e', sut.ReadChar());
      Aver.AreEqual(' ', sut.ReadChar());
      Aver.AreEqual('c', sut.ReadChar());
      Aver.AreEqual('l', sut.ReadChar());
      Aver.AreEqual('e', sut.ReadChar());
      Aver.AreEqual('f', sut.ReadChar());
      Aver.AreEqual(':', sut.ReadChar());

      Aver.AreEqual(0xd834, sut.ReadChar());//UTF16 equivalent of  UTF8: f09d849e original
      Aver.AreEqual(0xdd1e, sut.ReadChar());

      Aver.AreEqual(';', sut.ReadChar());

      for (var i = 0; i < pad; i++) Aver.AreEqual('-', sut.ReadChar());

      Aver.AreEqual('B', sut.ReadChar());
      Aver.AreEqual('a', sut.ReadChar());
      Aver.AreEqual('s', sut.ReadChar());
      Aver.AreEqual('e', sut.ReadChar());
      Aver.AreEqual('C', sut.ReadChar());
      Aver.AreEqual('l', sut.ReadChar());
      Aver.AreEqual('e', sut.ReadChar());
      Aver.AreEqual('f', sut.ReadChar());
      Aver.AreEqual(':', sut.ReadChar());

      Aver.AreEqual(0xd834, sut.ReadChar());//UTF16 equivalent of  UTF8: f09d84a2 original
      Aver.AreEqual(0xdd22, sut.ReadChar());

      if (pad > 0) Aver.IsFalse( sut.EOF);
      for (var i = 0; i < pad; i++) Aver.AreEqual('-', sut.ReadChar());

      Aver.IsTrue(sut.IsLastSegment);
      Aver.IsTrue(sut.NearEndOfSegment);
      Aver.IsTrue(sut.EOF);
    }


    [Run("maxpad=1357 maxbsz=8315 maxst=1397   padstep=91  bszstep=199  ststep=23")]
    [Run("maxpad=1357 maxbsz=8315 maxst=1397   padstep=93  bszstep=205  ststep=27")]
    [Run("maxpad=1357 maxbsz=8315 maxst=1397   padstep=93  bszstep=205  ststep=29")]
    [Run("maxpad=1357 maxbsz=8315 maxst=1397   padstep=93  bszstep=205  ststep=30")]
    [Run("maxpad=3973 maxbsz=8315 maxst=5000   padstep=293  bszstep=207  ststep=79")]

    //[Run("maxpad=2000 maxbsz=8024 maxst=1300     padstep=47  bszstep=197  ststep=19")]
    public void Utf8_4Byte_Music_AutoPermute(int maxPad, int padStep,  int maxBsz, int bszStep, int maxSt, int stStep)
    {
      var total = 0;
      for(var pad = 0; m_App.Active && pad < maxPad; pad += padStep)
      {
        "Pad: {0}".SeeArgs(pad);
        for (var bsz = 0; m_App.Active && bsz < maxBsz; bsz += bszStep)
        {
          for (var st = 0; m_App.Active && st < maxSt; st += stStep)
          {
            try
            {
               Utf8_4Byte_Music(pad, 1024 + bsz, st);
            }
            catch(Exception error)
            {
              "Error occurred on iteration: Utf8_4Byte_Music({0}, 1024 + {1}, {2}); ".SeeArgs(pad, bsz, st);
              new WrappedExceptionData(error).See();
            }
            total++;
          }
        }
      }

      "Total cases covered: {0:n0}".SeeArgs(total);
    }
  }
}
