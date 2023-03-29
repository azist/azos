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
using Azos.Serialization.JSON;

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

      var utf8 = StreamHookUse.EncodeStringToBufferNoBom(v);

      "\r{0}".SeeArgs(utf8.ToHexDump());

      Aver.AreEqual(22/*ascii*/ + 8/*2 4byte utf pairs*/, utf8.Length);

      var sut = new StreamSource(new MemoryStream(utf8), null, false, null);
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

      Aver.AreEqual(1, sut.SegmentCount);
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
    //The cases below failed before the use of Decoder
    [Run("pad=2051 bsz=4128 st=0")]    //4129..4131
    [Run("pad=2051 bsz=4129 st=0")]    //4129..4131
    [Run("pad=2051 bsz=4130 st=0")]    //4129..4131
    [Run("pad=2051 bsz=4131 st=0")]    //4129..4131
    [Run("pad=2051 bsz=4132 st=0")]    //4129..4131
    //failing on Mar 29 - problem was incorrect EOF detection
    [Run("pad=9908 bsz=1026 st=0")]
    public void Utf8_4Byte_Music(int pad, int bsz, int st)
    {
      //https://design215.com/toolbox/utf8-4byte-characters.php
      var v = (new string('-', pad)) + "Treble clef:\ud834\udd1e;" +(new string('-', pad)) + "BaseClef:\ud834\udd22" + (new string('-', pad));
      Aver.AreEqual((3 * pad) + 26, v.Length);

      var utf8 = StreamHookUse.EncodeStringToBufferNoBom(v);

      //"\r{0}".SeeArgs(utf8.ToHexDump());

      Aver.AreEqual(22 + (3*pad)/*ascii*/ + 8/*2 4byte utf pairs*/, utf8.Length);

      var sut = new StreamSource(new MemoryStream(utf8), null, useBom: false,language: null, bufferSize: bsz, segmentTailThreshold: st);

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

      if (pad > 0) Aver.IsFalse(sut.EOF);
      for (var i = 0; i < pad; i++) Aver.AreEqual('-', sut.ReadChar());

      ////////for(var i=0; i<10000000000; i++)
      ////////{
      ////////  var c = sut.ReadChar();
      ////////  "At [{0}] = '{1}' #{2:x4}".SeeArgs(i, c, (int)c);
      ////////  if (c==0) break;
      ////////}

      Aver.IsTrue(sut.IsLastSegment);
      Aver.IsTrue(sut.NearEndOfSegment);
      Aver.IsTrue(sut.EOF);

  //    if (sut.SegmentCount > 1) "Segment count: {0}".SeeArgs(sut.SegmentCount);
    }


    [Run("maxpad=1357 maxbsz=8315 maxst=1397   padstep=91  bszstep=199  ststep=23")]
    [Run("maxpad=1357 maxbsz=8315 maxst=1397   padstep=93  bszstep=205  ststep=27")]
    [Run("maxpad=1357 maxbsz=8315 maxst=1397   padstep=93  bszstep=205  ststep=29")]
    [Run("maxpad=1357 maxbsz=8315 maxst=1397   padstep=93  bszstep=205  ststep=30")]
    [Run("maxpad=3973 maxbsz=8315 maxst=5000   padstep=293  bszstep=207  ststep=79")]

    //[Run("maxpad=32000 maxbsz=32000 maxst=8000  padstep=795  bszstep=1000  ststep=1793")]
    //[Run("maxpad=32000 maxbsz=32000 maxst=8000  padstep=796  bszstep=1000  ststep=1793")]
    //[Run("maxpad=32000 maxbsz=32000 maxst=8000  padstep=795  bszstep=123  ststep=979")]

    //[Run("!sst-failed1",     "maxpad=24000 maxbsz=5 maxst=1  padstep=1  bszstep=1  ststep=1")]
    //[Run("!sst-coverall32k", "maxpad=32000 maxbsz=3000 maxst=1400  padstep=1  bszstep=1  ststep=1")]
    public void Utf8_4Byte_Music_AutoPermute(int maxPad, int padStep,  int maxBsz, int bszStep, int maxSt, int stStep)
    {
      const int MAXEC = 100;

      var total = 0;
      var ec = 0;
      var lck = new object();
      Parallel.For(0, maxPad / padStep, (i) =>
      {
        if (ec > MAXEC || !m_App.Active)  return;
        var pad = i * padStep;

        lock(lck) "Pad: {0}".SeeArgs(pad);

        for (var bsz = 0; ec < MAXEC && m_App.Active && bsz < maxBsz; bsz += bszStep)
        {
          for (var st = 0; ec < MAXEC && m_App.Active && st < maxSt; st += stStep)
          {
            try
            {
               Utf8_4Byte_Music(pad, 1024 + bsz, st);
            }
            catch(Exception error)
            {
              lock(lck)
              {
                Conout.Warning("Error occurred on iteration: Utf8_4Byte_Music({0}, 1024 + {1}, {2}); ".Args(pad, bsz, st));
                Conout.Error("{0}", new WrappedExceptionData(error).ToJson(JsonWritingOptions.PrettyPrintRowsAsMapASCII));
                ec++;
              }
            }
            total++;
          }
        }
      });

      "Total cases covered: {0:n0}".SeeArgs(total);

      if (ec > 0)
      {
         Aver.Fail($"The test run generated {ec} errors while permuting the parameters, see the conout error reports above");
      }
    }
  }
}
