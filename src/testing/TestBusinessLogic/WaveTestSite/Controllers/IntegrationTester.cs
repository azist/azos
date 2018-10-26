
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Azos;
using Azos.Data;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Wave.Mvc;

namespace WaveTestSite.Controllers
{
  public class IntegrationTester: Controller
  {
    #region Nested classes

      public enum TestStatus { Ok, Err};

      public class SpanDoc: TypedDoc
      {
        [Field] public TimeSpan Span { get; set; }
      }

      public class TestDoc: TypedDoc
      {
        [Field] public int ID { get; set; }
        [Field] public string Name { get; set; }

        [Field] public DateTime Date { get; set; }
        [Field] public DateTime? DateNullable { get; set; }

        [Field] public TimeSpan Span { get; set; }
        [Field] public TimeSpan? SpanNullable { get; set; }

        [Field] public TestStatus Status { get; set; }
        [Field] public TestStatus? StatusNullable { get; set; }

        [Field] public bool Is { get; set; }
        [Field] public bool? IsNullable { get; set; }

        [Field] public decimal Money { get; set; }
        [Field] public decimal? MoneyNullable { get; set; }

        [Field] public float Float { get; set; }
        [Field] public float? FloatNullable { get; set; }

        [Field] public double Double { get; set; }
        [Field] public double? DoubleNullable { get; set; }
      }

      public class TestComplexDoc: TypedDoc
      {
        [Field] public int ID { get; set; }
        [Field] public TestDoc Doc1 { get; set; }
        [Field] public TestDoc Doc2 { get; set; }
        [Field] public TestDoc[] ErrorDocs { get; set; }
      }

    #endregion

    [Action]
    public string Empty()
    {
      return string.Empty;
    }

    [Action("ActionName1", 0)]
    public string ActionName0()
    {
      return "ActionName1";
    }

    [Action("ActionGet", 0, "match{methods=GET}")]
    public string ActionGet()
    {
      return "ActionGet";
    }

    [Action("ActionPost", 0, "match{methods=POST}")]
    public string ActionPost()
    {
      return "ActionPost";
    }

    [Action("IsLocalAction", 0, "match{is-local=true}")]
    public string IsLocalAction()
    {
      return "IsLocalAction";
    }

    [Action("IsNotLocalAction", 0, "match{is-local=false}")]
    public string IsNotLocalAction()
    {
      return "IsNotLocalAction";
    }

    [Action]
    public string Add(int a, int b)
    {
      return (a + b).ToString();
    }

    [Action]
    public string AddDefault(int a = 5, int b = 7)
    {
      return (a + b).ToString();
    }

    [Action]
    public object GetList()
    {
      return new List<int>(new int[] {1, 2, 3});
    }

    [Action]
    public object GetArray()
    {
      return new int[] {1, 2, 3};
    }

    [Action]
    public string GetSetTimeSpan(TimeSpan ts)
    {
      return ts.Add(TimeSpan.FromDays(1)).ToString();
    }

    [Action]
    [AdHocPermission("TestPath", "TestPermission", 10)]
    public object GetWithPermission()
    {
      return string.Empty;
    }

    [Action]
    public object InboundJSONMapEcho(JSONDataMap data)
    {
      return data;
    }

    [Action]
    public object GetAnonymousObject()
    {
      return new { ID=55, Name="test"};
    }

    [Action("RowGet", 0, "match{methods=GET}")]
    public object RowGet()
    {
      var row = new TestDoc(){
        ID = 777,
        Name = "Test Name",
        Date = DateTime.Now,
        Status = TestStatus.Ok,
        Span = TimeSpan.FromSeconds(1)
      };

      return row;
    }

    [Action("RowSet", 0, "match{methods=POST}")]
    public object RowSet(TestDoc doc)
    {
      doc.Date = DateTime.Now;
      return doc;
    }

    [Action("ComplexRowSet", 0, "match{methods=POST}")]
    public object ComplexRowSet(TestComplexDoc doc)
    {
      doc.ID += 1;
      doc.Doc1.ID += 2;
      doc.ErrorDocs[2].Date = doc.ErrorDocs[2].Date.AddDays(-2);
      return doc;//new JSONResult(row, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object RowAndPrimitive_RowFirst(TestDoc doc, int n, string s)
    {
      doc.ID = n;
      doc.Name = s;
      return doc;//new JSONResult(row, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object RowAndPrimitive_RowLast(int n, string s, TestDoc doc)
    {
      doc.ID = n;
      doc.Name = s;
      return doc;//new JSONResult(row, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object RowAndPrimitive_RowMiddle(int n, TestDoc doc, string s)
    {
      doc.ID = n;
      doc.Name = s;
      return doc;//new JSONResult(row, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object JSONMapAndPrimitive_JSONFirst(JSONDataMap map, int n, string s)
    {
      map["ID"] = n;
      map["Name"] = s;
      return map; // or you could write: new JSONResult(map, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object JSONMapAndPrimitive_JSONLast(int n, string s, JSONDataMap map)
    {
      map["ID"] = n;
      map["Name"] = s;
      return map;//new JSONResult(map, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object JSONMapAndPrimitive_JSONMiddle(int n, JSONDataMap map, string s)
    {
      map["ID"] = n;
      map["Name"] = s;
      return map;//new JSONResult(map, JSONWritingOptions.CompactRowsAsMap);
    }

    [Action]
    public object RowDifferentFieldTypes(TestDoc doc)
    {
      return doc;// new JSONResult(row, JSONWritingOptions.CompactRowsAsMap);
    }

    //[Action]
    //public object DoSomething(bool? a1, TestStatus b1)
    //{
    //  var a = WorkContext.MatchedVars["a"].AsBool();
    //  var b = WorkContext.MatchedVars["b"].AsEnum<TestStatus>(TestStatus.Ok);

    //  return new FileDownload("1.txt");
    //}

    [Action]
    public object LoginUser(string id, string pwd)
    {
      WorkContext.NeedsSession();

      WorkContext.Session.User = App.SecurityManager.Authenticate(new IDPasswordCredentials(id, pwd));

      return new {Status = WorkContext.Session.User.Status, Name = WorkContext.Session.User.Name};
    }

    [Action]
    public DateTime? RelaxedDateTime(DateTime? dt = null)
    {
      return dt;
    }

    [Action("StrictDateTime", 1, true)]
    public DateTime? StrictDateTime(DateTime? dt = null)
    {
      return dt;
    }


    [Action]
    public void MultipartByteArray(string field, string text, byte[] bin)
    {
      var fld = Encoding.UTF8.GetBytes(field);
      var txt = Encoding.UTF8.GetBytes(text);
      var output = WorkContext.Response.GetDirectOutputStreamForWriting();
      output.Write(fld, 0, fld.Length);
      output.Write(txt, 0, txt.Length);
      output.Write(bin, 0, bin.Length);
    }

    [Action]
    public void MultipartMap(JSONDataMap map)
    {
      var fld = Encoding.UTF8.GetBytes(map["field"].AsString());
      var txt = Encoding.UTF8.GetBytes(map["text"].AsString());
      var bin = map["bin"] as byte[];
      var output = WorkContext.Response.GetDirectOutputStreamForWriting();
      output.Write(fld, 0, fld.Length);
      output.Write(txt, 0, txt.Length);
      output.Write(bin, 0, bin.Length);
    }

    [Action]
    public void MultipartRow(MultipartTestDoc doc)
    {
      var fld = Encoding.UTF8.GetBytes(doc.Field);
      var txt = Encoding.UTF8.GetBytes(doc.Text);
      var bin = doc.Bin;
      var output = WorkContext.Response.GetDirectOutputStreamForWriting();
      output.Write(fld, 0, fld.Length);
      output.Write(txt, 0, txt.Length);
      output.Write(bin, 0, bin.Length);
    }

    [Action]
    public void MultipartStream(string field, string text, Stream bin)
    {
      var fld = Encoding.UTF8.GetBytes(field);
      var txt = Encoding.UTF8.GetBytes(text);
      var output = WorkContext.Response.GetDirectOutputStreamForWriting();
      output.Write(fld, 0, fld.Length);
      output.Write(txt, 0, txt.Length);
      bin.CopyTo(output);
    }

    [Action]
    public void MultipartEncoding(string field)
    {
      var fld = Encoding.GetEncoding(1251).GetBytes(field);
      WorkContext.Response.GetDirectOutputStreamForWriting().Write(fld, 0, fld.Length);
    }

    //protected override System.Reflection.MethodInfo FindMatchingAction(Azos.Wave.WorkContext work, string action, out object[] args)
    //{
    //  return base.FindMatchingAction(work, action, out args);
    //}

    //multipart (byte array as well)
    //public object RowSet(different data types: decimal, bool, float, double, DateTime, TimeSpan and their Nullable versions)
    //public object RowSet(JSONDataMap row, int a, string b)
    //public object RowSet(int a, string b, JSONDataMap row)
    //public object RowSet(TestRow row, int a, string b)
    //match{is-local=false}

    public class MultipartTestDoc : TypedDoc
    {
      [Field]
      public string Field { get; set;}

      [Field]
      public string Text { get; set;}

      [Field]
      public string Text_filename { get; set;}

      [Field]
      public string Text_contenttype { get; set;}

      [Field]
      public byte[] Bin { get; set;}

      [Field]
      public string Bin_filename { get; set;}

      [Field]
      public string Bin_contenttype { get; set;}
    }
  }
}
