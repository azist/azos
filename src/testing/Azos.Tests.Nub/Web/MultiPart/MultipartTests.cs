/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Text;

using Azos.Scripting;
using Azos.Data;
using Azos.Web;

namespace Azos.Tests.Nub.Web.MultiPart
{
  [Runnable]
  public class MultipartTests
  {
    private const string POSTFIX_CONTENT_TYPE = "_contenttype";
    private const string POSTFIX_FILENAME = "_filename";
    private static readonly byte[] BYTES =
      { 0x12, 0x34, 0xaa, 0xfe, 0x10, 0x24, 0x1a, 0xfd,
          0x34, 0x00, 0x00, 0x2d, 0x2d, 0x27, 0x2a, 0xff,
          0x7f, 0x8a, 0x2d, 0x2d, 0x0d, 0x0a, 0x55, 0x49,
          0x0d, 0x0a, 0x75, 0xdb, 0x1e, 0x84, 0xf0, 0x63 };

    [Run]
    public void EncodeDecode_OneField()
    {
      var part = getDefaultField();

      var mpE = new Multipart(new Multipart.Part[] { part });
      var encCont = mpE.Encode();

      string boundary = null;
      var mpD = Multipart.ReadFromBytes(encCont.Buffer, ref boundary);

      Aver.AreEqual(encCont.Boundary, boundary);
      Aver.AreEqual(1, mpD.Parts.Count);
      Aver.AreEqual(part.Content.AsString(), mpD.Parts[part.Name].Content.AsString());
      Aver.IsNull(mpD.Parts[part.Name].ContentType);
      Aver.IsNull(mpD.Parts[part.Name].FileName);
    }

    [Run]
    public void EncodeDecode_OneField_UTF8()
    {
      var encoding = Encoding.UTF8;
      var part = new Multipart.Part("Field");
      part.Content = "Значение of the word is გთხოვთ ახლავე Οὐχὶ ταὐτὰ παρίσταταί μοι";

      var mpE = new Multipart(new Multipart.Part[] { part });
      var encCont = mpE.Encode(encoding);

      string boundary = null;
      var mpD = Multipart.ReadFromBytes(encCont.Buffer, ref boundary, encoding);

      Aver.AreEqual(encCont.Boundary, boundary);
      Aver.AreObjectsEqual(encCont.Encoding, encoding);
      Aver.AreEqual(1, mpD.Parts.Count);
      Aver.AreEqual(part.Content.AsString(), mpD.Parts[part.Name].Content.AsString());
      Aver.IsNull(mpD.Parts[part.Name].ContentType);
      Aver.IsNull(mpD.Parts[part.Name].FileName);
    }

    [Run]
    public void EncodeDecode_OneFile()
    {
      var part = getDefaultFile();

      var mpE = new Multipart(new Multipart.Part[] { part });
      var encCont = mpE.Encode();

      string boundary = null;
      var mpD = Multipart.ReadFromBytes(encCont.Buffer, ref boundary);

      Aver.AreEqual(encCont.Boundary, boundary);
      Aver.AreEqual(1, mpD.Parts.Count);
      Aver.IsTrue(IOUtils.MemBufferEquals(part.Content as byte[], mpD.Parts[part.Name].Content as byte[]));
      Aver.AreEqual(part.FileName, mpD.Parts[part.Name].FileName);
      Aver.AreEqual(part.ContentType, mpD.Parts[part.Name].ContentType);
    }

    [Run]
    public void EncodeDecode_FieldAndFile()
    {
      var partField = getDefaultField();
      var partFile = getDefaultFile();

      var mpE = new Multipart(new Multipart.Part[] { partField, partFile });
      var encCont = mpE.Encode();

      string boundary = null;
      var mpD = Multipart.ReadFromBytes(encCont.Buffer, ref boundary);

      Aver.AreEqual(encCont.Boundary, boundary);
      Aver.AreEqual(2, mpD.Parts.Count);

      Aver.AreEqual(partField.Content.AsString(), mpD.Parts[partField.Name].Content.AsString());
      Aver.IsNull(mpD.Parts[partField.Name].ContentType);
      Aver.IsNull(mpD.Parts[partField.Name].FileName);
      Aver.AreEqual(partFile.FileName, mpD.Parts[partFile.Name].FileName);
      Aver.AreEqual(partFile.ContentType, mpD.Parts[partFile.Name].ContentType);
      Aver.IsTrue(IOUtils.MemBufferEquals(partFile.Content as byte[], mpD.Parts[partFile.Name].Content as byte[]));
    }

    [Run]
    public void EncodeToStreamDecode()
    {
      var partField = getDefaultField();
      var partFile = getDefaultFile();

      var stream = new MemoryStream();
      stream.WriteByte(0xFF);
      stream.WriteByte(0xFF);

      var mpE = new Multipart(new Multipart.Part[] { partField, partFile });
      var enc = mpE.Encode(stream);
      Aver.AreEqual(enc.StartIdx, 2);

      var src = new byte[enc.Length];
      Array.Copy(enc.Buffer, enc.StartIdx, src, 0, src.Length);

      string boundary = null;
      var mpD = Multipart.ReadFromBytes(src, ref boundary);

      Aver.AreEqual(enc.Boundary, boundary);
      Aver.AreEqual(partField.Content.AsString(), mpD.Parts[partField.Name].Content.AsString());
      Aver.IsNull(mpD.Parts[partField.Name].ContentType);
      Aver.IsNull(mpD.Parts[partField.Name].FileName);
      Aver.AreEqual(partFile.FileName, mpD.Parts[partFile.Name].FileName);
      Aver.AreEqual(partFile.ContentType, mpD.Parts[partFile.Name].ContentType);
      Aver.IsTrue(IOUtils.MemBufferEquals(partFile.Content as byte[], mpD.Parts[partFile.Name].Content as byte[]));
    }

    [Run]
    public void FieldAndFile_ToJSONDataMap()
    {
      var partField = getDefaultField();
      var partFile = getDefaultFile();

      var mp = new Multipart(new Multipart.Part[] { partField, partFile });
      Aver.AreEqual(2, mp.Parts.Count);

      var map = mp.ToJSONDataMap();

      Aver.AreEqual(partField.Content.AsString(), map[partField.Name].AsString());
      Aver.AreEqual(partFile.FileName, map[partFile.Name + POSTFIX_FILENAME].AsString());
      Aver.AreEqual(partFile.ContentType, map[partFile.Name + POSTFIX_CONTENT_TYPE].AsString());
      Aver.IsTrue(IOUtils.MemBufferEquals(partFile.Content as byte[], map[partFile.Name] as byte[]));
    }

    [Run]
    public void ReadFromBytes()
    {
      var test = getEmbeddedFileBytes("test.dat");
      string boundary = null;
      var mp = Multipart.ReadFromBytes(test, ref boundary);
      Aver.AreEqual(3, mp.Parts.Count);
      Aver.AreEqual("---------------------------7de31e2f5200d8", boundary);

      Aver.AreEqual("value", mp.Parts["name"].Content.AsString());
      Aver.IsNull(mp.Parts["name"].ContentType);
      Aver.IsNull(mp.Parts["name"].FileName);

      var bmp = getEmbeddedFileBytes("bmp.dat");
      Aver.IsTrue(IOUtils.MemBufferEquals(bmp, mp.Parts["content1"].Content as byte[]));
      Aver.AreEqual("image/bmp", mp.Parts["content1"].ContentType);
      Aver.AreEqual("bmp.dat", mp.Parts["content1"].FileName);

      var txt = getEmbeddedFileBytes("txt.dat");
      Aver.AreEqual(Encoding.UTF8.GetString(txt), mp.Parts["content2"].Content.AsString());
      Aver.AreEqual("text/plain", mp.Parts["content2"].ContentType);
      Aver.AreEqual("text.dat", mp.Parts["content2"].FileName);
    }

    [Run]
    public void ReadAndToMap()
    {
      var test = getEmbeddedFileBytes("test.dat");
      string boundary = null;
      var mp = Multipart.ReadFromBytes(test, ref boundary);

      var map = mp.ToJSONDataMap();

      Aver.AreEqual(7, map.Count);

      Aver.AreEqual("value", map["name"].AsString());

      var bmp = getEmbeddedFileBytes("bmp.dat");
      Aver.IsTrue(IOUtils.MemBufferEquals(bmp, map["content1"] as byte[]));
      Aver.AreEqual("image/bmp", map["content1" + POSTFIX_CONTENT_TYPE].AsString());
      Aver.AreEqual("bmp.dat", map["content1" + POSTFIX_FILENAME].AsString());

      var txt = getEmbeddedFileBytes("txt.dat");
      Aver.AreEqual(Encoding.UTF8.GetString(txt), map["content2"].AsString());
      Aver.AreEqual("text/plain", map["content2" + POSTFIX_CONTENT_TYPE].AsString());
      Aver.AreEqual("text.dat", map["content2" + POSTFIX_FILENAME].AsString());
    }

    [Run]
    public void ValidBoundaryFromOutside()
    {
      var test = Encoding.UTF8.GetBytes(
@"--7de23d3b1d07fe
Content-Disposition: form-data; name=""name1""

value 1
--7de23d3b1d07fe
Content-Disposition: form-data; name=""name2""

value 2
--7de23d3b1d07fe--
");
      string boundary = "7de23d3b1d07fe";
      var mp = Multipart.ReadFromBytes(test, ref boundary);

      Aver.AreEqual(2, mp.Parts.Count);
      Aver.AreEqual("7de23d3b1d07fe", boundary);

      Aver.AreEqual("value 1", mp.Parts["name1"].Content.AsString());
      Aver.IsNull(mp.Parts["name1"].ContentType);
      Aver.IsNull(mp.Parts["name1"].FileName);

      Aver.AreEqual("value 2", mp.Parts["name2"].Content.AsString());
      Aver.IsNull(mp.Parts["name2"].ContentType);
      Aver.IsNull(mp.Parts["name2"].FileName);
    }

    [Run]
    public void ValidBoundaryFromOutside_Preamble()
    {
      var test = Encoding.UTF8.GetBytes(
@"This is a preamble
--7de23d3b1d07fe
Content-Disposition: form-data; name=""name1""

value 1
--7de23d3b1d07fe
Content-Disposition: form-data; name=""name2""

value 2
--7de23d3b1d07fe--
");
      string boundary = "7de23d3b1d07fe";
      var mp = Multipart.ReadFromBytes(test, ref boundary);

      Aver.AreEqual(2, mp.Parts.Count);
      Aver.AreEqual("7de23d3b1d07fe", boundary);

      Aver.AreEqual("value 1", mp.Parts["name1"].Content.AsString());
      Aver.IsNull(mp.Parts["name1"].ContentType);
      Aver.IsNull(mp.Parts["name1"].FileName);

      Aver.AreEqual("value 2", mp.Parts["name2"].Content.AsString());
      Aver.IsNull(mp.Parts["name2"].ContentType);
      Aver.IsNull(mp.Parts["name2"].FileName);
    }

    [Run]
    public void EncodingUTF8()
    {
      var part = new Multipart.Part("field");
      part.FileName = "text";
      part.ContentType = "Content-type: text/plain; charset=utf8";
      part.Content = Encoding.UTF8.GetBytes("Значение of the word is გთხოვთ Οὐχὶ");

      var mpE = new Multipart(new Multipart.Part[] {part});
      var enc = mpE.Encode();

      var boundary = enc.Boundary;
      var mpD = Multipart.ReadFromBytes(enc.Buffer, ref boundary);
      Aver.AreObjectsEqual("Значение of the word is გთხოვთ Οὐχὶ", mpD.Parts["field"].Content);
    }

    [Run]
    public void WithoutCharset()
    {
      var part = new Multipart.Part("field");
      part.FileName = "text";
      part.ContentType = "Content-type: text/plain";
      part.Content = Encoding.UTF8.GetBytes("Значение of the word is გთხოვთ Οὐχὶ");

      var mpE = new Multipart(new Multipart.Part[] {part});
      var enc = mpE.Encode();

      var boundary = enc.Boundary;
      var mpD = Multipart.ReadFromBytes(enc.Buffer, ref boundary);
      Aver.AreObjectsEqual("Значение of the word is გთხოვთ Οὐχὶ", mpD.Parts["field"].Content);
    }

    #region Exceptions

    [Run]
    public void TryCreatePart_NullName()
    {
      try
      {
        var part = new Multipart.Part(null);
        Aver.Fail("Invalid name!");
      }
      catch (Exception e)
      {
        Conout.Write(e.ToMessageWithType());
        Aver.IsTrue(e.Message.Contains(StringConsts.MULTIPART_PART_EMPTY_NAME_ERROR));
      }
    }

    [Run]
    public void TryCreateMultipart_NullParts()
    {
      try
      {
        var part = new Multipart(null);
        Aver.Fail("Invalid parts!");
      }
      catch (Exception e)
      {
        Conout.Write(e.ToMessageWithType());
        Aver.IsTrue(e.Message.Contains(StringConsts.MULTIPART_PARTS_COULDNT_BE_EMPTY_ERROR));
      }
    }

    [Run]
    public void EmptyPart()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
-----------------------------7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Aver.Fail("Empty part!");
      }
      catch (Exception e)
      {
        Conout.Write(e.ToMessageWithType());
        Aver.IsTrue(e.Message.Contains(StringConsts.MULTIPART_PART_COULDNT_BE_EMPTY_ERROR));
      }
    }

    [Run]
    public void EmptyName()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""""

value
-----------------------------7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Aver.Fail("Empty name!");
      }
      catch (Exception e)
      {
        Conout.Write(e.ToMessageWithType());
        Aver.IsTrue(e.Message.Contains(StringConsts.MULTIPART_PART_EMPTY_NAME_ERROR));
      }
    }

    [Run]
    public void InvalidEOL()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""name""

value-----------------------------7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Aver.Fail("Invalid end of part!");
      }
      catch (Exception e)
      {
        Conout.Write(e.ToMessageWithType());
        Aver.IsTrue(e.Message.Contains(StringConsts.MULTIPART_PART_MUST_BE_ENDED_WITH_EOL_ERROR));
      }
    }

    [Run]
    public void NoDoubleEOLAfterHeader()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""name""
value
-----------------------------7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Aver.Fail("No double EOL after header!");
      }
      catch (Exception e)
      {
        Conout.Write(e.ToMessageWithType());
        Aver.IsTrue(e.Message.Contains(StringConsts.MULTIPART_DOUBLE_EOL_ISNT_FOUND_AFTER_HEADER_ERROR));
      }
    }

    [Run]
    public void DuplicatedNames()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""name""

value 1
-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""name""

value 2
-----------------------------7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Aver.Fail("Repeated name!");
      }
      catch (Exception e)
      {
        Conout.Write(e.ToMessageWithType());
        Aver.IsTrue(e.Message.Contains("is already registered."));
      }
    }

    [Run]
    public void DuplicatedNamesFiles()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""files""; filename=""fileOne.txt""
Content-Type: text/plain

this is fileOne.txt
-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""files""; filename=""fileTwo.txt""
Content-Type: text/plain

this is fileTwo.txt
-----------------------------7de23d3b1d07fe--
");
      string boundary = null;
      var mp = Multipart.ReadFromBytes(test, ref boundary);
      Aver.AreEqual(mp.Parts.Count, 2);
      var files0 =  mp.Parts["files"];
      Aver.IsTrue(files0.PartName.EqualsOrdIgnoreCase("files"));
      Aver.IsTrue(files0.Content.AsString().EqualsOrdIgnoreCase("this is fileOne.txt"));
      var files1 =  mp.Parts["files1"];
      Aver.IsTrue(files1.PartName.EqualsOrdIgnoreCase("files"));
      Aver.IsTrue(files1.Content.AsString().EqualsOrdIgnoreCase("this is fileTwo.txt"));

      var mp1 = new Multipart(mp.Parts);
      Aver.AreEqual(mp1.Parts.Count, 2);
    }

    [Run]
    public void InvalidBoundaryFromOutside()
    {
      var test = Encoding.UTF8.GetBytes(
@"--7de23d3b1d07fe
Content-Disposition: form-data; name=""name1""

value 1
--7de23d3b1d07fe
Content-Disposition: form-data; name=""name2""

value 2
--7de23d3b1d07fe--
");
      string boundary = "8asd56sge";
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Aver.Fail("Invalid explicit boundary");
      }
      catch (Exception ex)
      {
        Conout.Write(ex.ToMessageWithType());
        Aver.IsTrue(ex.Message.Contains(StringConsts.MULTIPART_SPECIFIED_BOUNDARY_ISNT_FOUND_ERROR));
      }
    }

    [Run]
    public void TooShortBoundary()
    {
      var test = Encoding.UTF8.GetBytes(
@"--
Content-Disposition: form-data; name=""name""

value 1
----
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Aver.Fail("Invalid boundary");
      }
      catch (Exception e)
      {
        Conout.Write(e.ToMessageWithType());
        Aver.IsTrue(e.Message.Contains(StringConsts.MULTIPART_BOUNDARY_IS_TOO_SHORT));
      }
    }

    [Run]
    public void FullBoundaryNotStartWithHyphens()
    {
      var test = Encoding.UTF8.GetBytes(
@"7de23d3b1d07fe
Content-Disposition: form-data; name=""name""

value 1
7de23d3b1d07fe--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Aver.Fail("Invalid boundary");
      }
      catch (Exception e)
      {
        Conout.Write(e.ToMessageWithType());
        Aver.IsTrue(e.Message.Contains(StringConsts.MULTIPART_BOUNDARY_SHOULD_START_WITH_HYPHENS));
      }
    }

    [Run]
    public void InvalidEOF()
    {
      var test = Encoding.UTF8.GetBytes(
@"-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""field1""

value 1
-----------------------------7de23d3b1d07fe
Content-Disposition: form-data; name=""field2""

value 2--
");
      string boundary = null;
      try
      {
        var mp = Multipart.ReadFromBytes(test, ref boundary);
        Aver.Fail("Invalid EOF!");
      }
      catch (Exception e)
      {
        Conout.Write(e.ToMessageWithType());
        Aver.IsTrue(e.Message.Contains(StringConsts.MULTIPART_TERMINATOR_ISNT_FOUND_ERROR));
      }
    }

    #endregion

    private Multipart.Part getDefaultField()
    {
      var part = new Multipart.Part("SomeField");
      part.Content = "Field's value";
      return part;
    }

    private Multipart.Part getDefaultFile()
    {
      var part = new Multipart.Part("SomeFile");
      part.Content = BYTES;
      part.FileName = "five_numbers.dat";
      part.ContentType = "application/octet-stream";
      return part;
    }

    private byte[] getEmbeddedFileBytes(string resourceName)
    {
      var resourceFullName = "Resources." + resourceName;
      using (var stream = Platform.EmbeddedResource.GetBinaryStream(typeof(MultipartTests), resourceFullName))
      {
        var buf = new byte[stream.Length];
        stream.Read(buf, 0, buf.Length);
        return buf;
      }
    }
  }
}
