/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.IO;
using Azos.CodeAnalysis.Source;
using Azos.Scripting;
using System.IO;

namespace Azos.Tests.Nub.CodeAnalysis
{
  [Runnable]
  public class StreamSourceTests
  {

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
  }
}
