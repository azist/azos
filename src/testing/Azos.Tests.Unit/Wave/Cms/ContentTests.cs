/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Wave.Cms;

namespace Azos.Tests.Unit.Wave.Cms
{
  [Runnable]
  public class ContentTests
  {
    [Run]
    public void Test1()
    {
      var sut = new Content(
        new ContentId("portal1", "ns1", "b1"),
        new NLSMap.Builder().Add("eng", "English", "English").Map,
        "string content",
        new byte[] {1,2,3,4,5},
        "mixed",
        new LangInfo(Atom.Encode("eng"), "English"),
        "file.txt",
        "user1",
        "user2",
        new DateTime(1980, 1, 1),
        new DateTime(1990, 1, 1)
      );

      sut.See("SUT ORIGINAL");

      var ms = new MemoryStream();

      sut.WriteToStream(ms);
      ms.Position.See();
      Aver.IsTrue(ms.Position > 0);

      ms.Position = 0;

      var got = new Content(ms);
      Aver.IsNotNull(got);
      sut.See("SUT GOT");

      Aver.AreEqual(sut.Id, got.Id);
      Aver.AreEqual(sut.ETag, got.ETag);
      Aver.AreEqual(sut.ContentType, got.ContentType);
      Aver.AreEqual(sut.StringContent, got.StringContent);
      Aver.IsTrue(sut.BinaryContent.MemBufferEquals(got.BinaryContent));

      Aver.AreEqual(sut.CreateUser, got.CreateUser);
      Aver.AreEqual(sut.CreateDate, got.CreateDate);
      Aver.AreEqual(sut.ModifyUser, got.ModifyUser);
      Aver.AreEqual(sut.ModifyDate, got.ModifyDate);

      Aver.AreEqual(sut.Language.ISO, got.Language.ISO);
      Aver.AreEqual(sut.Language.Name, got.Language.Name);

      Aver.AreEqual(sut.Name["eng"].Name, got.Name["eng"].Name);
    }
  }
}
