/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using Azos;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;
using TestRow = WaveTestSite.Controllers.IntegrationTester.TestDoc;
using TestStatus = WaveTestSite.Controllers.IntegrationTester.TestStatus;
using TestComplexRow = WaveTestSite.Controllers.IntegrationTester.TestComplexDoc;

namespace Azos.Tests.Integration.Wave
{
  [Runnable]
  public class WaveTest: WaveTestBase
  {
    #region Consts

      private const string EXPECTED_400 = "400 expected but wasn't thrown";
      private const string EXPECTED_404 = "404 expected but wasn't thrown";

      private const string USER_ID = "dima";
      private const string USER_PWD = "thejake";
      private const string USER_NAME = "Dima";
      private const string USER_STATUS = "User";

    #endregion

    #region Tests

      #region Action

        [Run]
        public void Action_Empty()
        {
          using (var wc = CreateWebClient())
          {
            var res = wc.DownloadString(INTEGRATION_HTTP_ADDR + "Empty");
            Aver.AreEqual(string.Empty, res);
          }
        }

        [Run]
        public void Action_Action1Name()
        {
          using (var wc = CreateWebClient())
          {
            var res = wc.DownloadString(INTEGRATION_HTTP_ADDR + "ActionName1");
            Aver.AreEqual("ActionName1", res);
          }
        }

        [Run]
        public void Action_Action0Name()
        {
          using (var wc = CreateWebClient())
          {
            try
            {
              wc.DownloadString(INTEGRATION_HTTP_ADDR + "ActionName0");
              throw new Exception(EXPECTED_404);
            }
            catch (WebException ex)
            {
              Aver.IsTrue( Is404(ex));
            }
          }
        }

        [Run]
        public void Action_ActionPost_Found()
        {
          using (var wc = CreateWebClient())
          {
            var res = wc.UploadString(INTEGRATION_HTTP_ADDR + "ActionPost", string.Empty);
            Aver.AreEqual("ActionPost", res);
          }
        }

        [Run]
        public void Action_ActionPost_NotFound()
        {
          using (var wc = CreateWebClient())
          {
            try
            {
              wc.DownloadString(INTEGRATION_HTTP_ADDR + "ActionPost");
              throw new Exception(EXPECTED_404);
            }
            catch (WebException ex)
            {
              Aver.IsTrue( Is404(ex));
            }
          }
        }

        [Run]
        public void Action_IsLocalAction()
        {
          using (var wc = CreateWebClient())
          {
            var res = wc.DownloadString(INTEGRATION_HTTP_ADDR + "IsLocalAction");
            Aver.AreEqual("IsLocalAction", res);
          }
        }

        [Run]
        public void Action_IsNotLocalAction()
        {
          using (var wc = CreateWebClient())
          {
            try
            {
              wc.DownloadString(INTEGRATION_HTTP_ADDR + "IsNotLocalAction");
              throw new Exception(EXPECTED_404);
            }
            catch (WebException ex)
            {
              Aver.IsTrue( Is404(ex));
            }
          }
        }

        [Run]
        public void Action_GetSetTimeSpan()
        {
          var initTs = TimeSpan.FromDays(10);

          using (var wc = CreateWebClient())
          {
            var values = new NameValueCollection();
            values.Add("ts", initTs.AsString());

            byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "GetSetTimeSpan", values);
            string str = GetUTF8StringWOBOM(bytes);

            var gotTs = TimeSpan.Parse(str);

            Aver.AreEqual(initTs.Add(TimeSpan.FromDays(1)), gotTs);
          }
        }

        [Run]
        public void Action_Add_BothArgs()
        {
          int a = 3, b = 14;

          using (var wc = CreateWebClient())
          {
            var values = new NameValueCollection();
            values.Add("a", a.AsString());
            values.Add("b", b.AsString());

            byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "Add", values);
            string str = Encoding.ASCII.GetString(bytes);
            int sum = str.AsInt();
            Aver.AreEqual(a + b, sum);
          }
        }

        [Run]
        public void Action_Add_InsufficientArgs()
        {
          int a = 3, b = 14;

          using (var wc = CreateWebClient())
          {
            {
              var values = new NameValueCollection();
              values.Add("a", a.AsString());

              byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "Add", values);
              string str = Encoding.ASCII.GetString(bytes);
              int sum = str.AsInt();
              Aver.AreEqual(a, sum);
            }

            {
              var values = new NameValueCollection();
              values.Add("b", b.AsString());

              byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "Add", values);
              string str = Encoding.ASCII.GetString(bytes);
              int sum = str.AsInt();
              Aver.AreEqual(b, sum);
            }

            {
              var values = new NameValueCollection();

              byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "Add", values);
              string str = Encoding.ASCII.GetString(bytes);
              int sum = str.AsInt();
              Aver.AreEqual(0, sum);
            }
          }
        }

        [Run]
        public void Action_Add_DefaultArgs()
        {
          int a = 5, b = 7;

          using (var wc = CreateWebClient())
          {
            var values = new NameValueCollection();

            byte[] bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "AddDefault", values);
            string str = Encoding.ASCII.GetString(bytes);
            int sum = str.AsInt();
            Aver.AreEqual(a + b, sum);
          }
        }

        [Run]
        public void Action_GetList()
        {
          using (var wc = CreateWebClient())
          {
            string str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "GetList");

            Aver.IsTrue(wc.ResponseHeaders[HttpResponseHeader.ContentType].Contains(Azos.Web.ContentType.JSON));

            var obj = JsonReader.DeserializeDynamic(str);

            Aver.AreEqual(3, obj.Data.Count);
            Aver.AreEqual(1, obj.Data[0]);
            Aver.AreEqual(2, obj.Data[1]);
            Aver.AreEqual(3, obj.Data[2]);
          }
        }

        [Run]
        public void Action_GetArray()
        {
          using (var wc = CreateWebClient())
          {
            string str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "GetArray");

            Aver.IsTrue(wc.ResponseHeaders[HttpResponseHeader.ContentType].Contains(Azos.Web.ContentType.JSON));

            var obj = JsonReader.DeserializeDynamic(str);

            Aver.AreEqual(3, obj.Data.Count);
            Aver.AreEqual(1, obj.Data[0]);
            Aver.AreEqual(2, obj.Data[1]);
            Aver.AreEqual(3, obj.Data[2]);
          }
        }

        [Run]
        [Aver.Throws(typeof(System.Net.WebException), Message="(403)", MsgMatch= Aver.ThrowsAttribute.MatchType.Contains)]
        public void Action_GetWithNoPermission()
        {
          using (var wc = CreateWebClient())
          {
            string str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "GetWithPermission");
           // Aver.AreEqual("text/html", wc.ResponseHeaders[HttpResponseHeader.ContentType]);
           // Aver.IsTrue( Regex.IsMatch(str, "Authorization to .+/TestPath/TestPermission.+ failed"));
          }
        }

        [Run]
        public void Action_RowGet_JSONDataMap()
        {
          DateTime start = DateTime.Now;

          System.Threading.Thread.Sleep(3000);

          using (var wc = CreateWebClient())
          {
            string str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "RowGet");

            Aver.IsTrue(wc.ResponseHeaders[HttpResponseHeader.ContentType].Contains(Azos.Web.ContentType.JSON));

            var obj = JsonReader.DeserializeDynamic(str);

            Aver.AreEqual(16, obj.Data.Count);
            Aver.AreObjectsEqual(777, obj.Data["ID"]);
            Aver.AreObjectsEqual("Test Name", obj.Data["Name"]);

            var date = DateTime.Parse(obj.Data["Date"]);
            Aver.IsTrue( (DateTime.Now - start).TotalSeconds >= 2.0d );
            Aver.AreEqual("Ok", obj.Data["Status"]);

            var gotTs = TimeSpan.FromTicks((long)(ulong)(obj.Data["Span"]));
            Aver.AreEqual(TimeSpan.FromSeconds(1), gotTs);
          }
        }

        [Run]
        public void TypeRowConversion()
        {
          var r = new WaveTestSite.Controllers.IntegrationTester.SpanDoc() { Span = TimeSpan.FromTicks(1) };

          var str = r.ToJson(JsonWritingOptions.CompactRowsAsMap);

          var map = JsonReader.DeserializeDataObject(str) as JsonDataMap;
          var gotRow = JsonReader.ToDoc<WaveTestSite.Controllers.IntegrationTester.SpanDoc>(map);
        }

        [Run]
        public void Action_RowGet_TypeRow()
        {
          DateTime start = DateTime.Now;

          using (var wc = CreateWebClient())
          {
            string str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "RowGet");


            Aver.IsTrue(wc.ResponseHeaders[HttpResponseHeader.ContentType].Contains(Azos.Web.ContentType.JSON));

            var map = JsonReader.DeserializeDataObject(str) as JsonDataMap;
            var gotRow = JsonReader.ToDoc<TestRow>(map);
          }
        }

        [Run]
        public void Action_ComplexRow()
        {
          var initalRow = new TestComplexRow();

          initalRow.ID = 777;

          initalRow.Doc1 = new TestRow(){ID = 101, Name = "Test Doc 1", Date = DateTime.Now};
          initalRow.Doc2 = new TestRow(){ID = 102, Name = "Test Doc 2", Date = DateTime.Now};

          initalRow.ErrorDocs = new TestRow[] {
            new TestRow() {ID = 201, Name = "Err Doc 1", Date = DateTime.Now},
            new TestRow() {ID = 202, Name = "Err Doc 2", Date = DateTime.Now},
            new TestRow() {ID = 203, Name = "Err Doc 3", Date = DateTime.Now}
          };

          var str = initalRow.ToJson(JsonWritingOptions.CompactRowsAsMap);

 Console.WriteLine(str);

          using (var wc = CreateWebClient())
          {
            wc.Headers[HttpRequestHeader.ContentType] = Azos.Web.ContentType.JSON;
            var res = wc.UploadString(INTEGRATION_HTTP_ADDR + "ComplexRowSet", str);

            var map = JsonReader.DeserializeDataObject(res) as JsonDataMap;
            var gotRow = JsonReader.ToDoc<TestComplexRow>(map);

            Aver.AreEqual(initalRow.ID + 1, gotRow.ID);
            Aver.AreEqual(initalRow.Doc1.ID + 2, gotRow.Doc1.ID);
            Aver.IsTrue(gotRow.ErrorDocs[2].Date - initalRow.ErrorDocs[2].Date.AddDays(-2) < TimeSpan.FromMilliseconds(1) ); // dlat 20140617: date string format preservs 3 signs after decimal second instead of 7 digits preserved by .NET DateTime type
          }
        }

        [Run]
        public void Action_RowAndPrimitive_RowFirst()
        {
          rowAndPrimitive("RowAndPrimitive_RowFirst");
        }

        [Run]
        public void Action_RowAndPrimitive_RowLast()
        {
          rowAndPrimitive("RowAndPrimitive_RowLast");
        }

        [Run]
        public void Action_RowAndPrimitive_RowMiddle()
        {
          rowAndPrimitive("RowAndPrimitive_RowMiddle");
        }

        private void rowAndPrimitive(string actionName)
        {
          var initalRow = new TestRow() { ID = 0, Name = "Name" };
          var str = initalRow.ToJson(JsonWritingOptions.CompactRowsAsMap);

          var values = new NameValueCollection();
          values.Add("n", "777");
          values.Add("s", "sss");

          using (var wc = CreateWebClient())
          {
            wc.QueryString = values;
            wc.Headers[HttpRequestHeader.ContentType] = Azos.Web.ContentType.JSON;
            var res = wc.UploadString(INTEGRATION_HTTP_ADDR + actionName, str);

            var map = JsonReader.DeserializeDataObject(res) as JsonDataMap;
            var gotRow = JsonReader.ToDoc<TestRow>(map);

            Aver.AreEqual(gotRow.ID, 777);
            Aver.AreEqual(gotRow.Name, "sss");
          }
        }

        [Run]
        public void Action_JSONMapAndPrimitive_JSONFirst()
        {
          jsonMapAndPrimitives("JSONMapAndPrimitive_JSONFirst");
        }

        [Run]
        public void Action_JSONMapAndPrimitive_JSONLast()
        {
          jsonMapAndPrimitives("JSONMapAndPrimitive_JSONLast");
        }

        [Run]
        public void Action_JSONMapAndPrimitive_JSONMiddle()
        {
          jsonMapAndPrimitives("JSONMapAndPrimitive_JSONMiddle");
        }

        private void jsonMapAndPrimitives(string actionName)
        {
          var initialMap = new JsonDataMap();
          initialMap["ID"] = 100;
          initialMap["Name"] = "Initial Name";
          var str = initialMap.ToJson(JsonWritingOptions.CompactRowsAsMap);

          var values = new NameValueCollection();
          values.Add("n", "777");
          values.Add("s", "sss");

          using (var wc = CreateWebClient())
          {
            wc.QueryString = values;
            wc.Headers[HttpRequestHeader.ContentType] = Azos.Web.ContentType.JSON;
            var res = wc.UploadString(INTEGRATION_HTTP_ADDR + actionName, str);

            var gotMap = JsonReader.DeserializeDataObject(res) as JsonDataMap;

            Aver.AreObjectsEqual(gotMap["ID"], 777);
            Aver.AreObjectsEqual(gotMap["Name"], "sss");
          }
        }

        //[Run]
        //public void Action_RowDifferentFieldTypes()
        //{
        //  var initalRow = new TestRow() {
        //    ID = 0, Name = "Name", Date { get; set; }

        //    Status Status { get; set; }
        //    Status? StatusNullable { get; set; }

        //    Is { get; set; }
        //    IsNullable { get; set; }

        //    Money { get; set; }
        //    MoneyNullable { get; set; }

        //    Float { get; set; }
        //    FloatNullable { get; set; }

        //    Double { get; set; }
        //    DoubleNullable { get; set; }
        //  };
        //  var str = initalRow.ToJSON(JSONWritingOptions.CompactRowsAsMap);

        //  using (var wc = CreateWebClient())
        //  {
        //    wc.Headers[HttpRequestHeader.ContentType] = Azos.Wave.ContentTypeUtils.JSON;
        //    var res = wc.UploadString(INTEGRATION_HTTP_ADDR + "RowDifferentFieldTypes", str);

        //    var gotRow = JSONReader.ToRow<TestRow>(JSONReader.DeserializeDataObject(res) as JSONDataMap);

        //    Aver.AreEqual(gotRow.ID, 777);
        //    Aver.AreEqual(gotRow.Name, "sss");
        //  }
        //}

        [Run]
        public void Action_GetAnonymousObject()
        {
          using (var wc = CreateWebClient())
          {
            var str = wc.UploadString(INTEGRATION_HTTP_ADDR + "GetAnonymousObject", string.Empty);

            Console.WriteLine(str);

            var obj = JsonReader.DeserializeDynamic(str);
            Aver.AreObjectsEqual(55, obj.Data["ID"]);
            Aver.AreObjectsEqual("test", obj.Data["Name"]);
          }
        }

        [Run]
        public void Action_Login()
        {
          NameValueCollection values = new NameValueCollection();
          values.Add("id", USER_ID);
          values.Add("pwd", USER_PWD);

          using (var wc = CreateWebClient())
          {
            var bytes = wc.UploadValues(INTEGRATION_HTTP_ADDR + "LoginUser", values);
            var str = GetUTF8StringWOBOM(bytes);

            Console.WriteLine(str);

            Aver.IsTrue(wc.ResponseHeaders[HttpResponseHeader.ContentType].Contains(Azos.Web.ContentType.JSON));

            var obj = JsonReader.DeserializeDynamic(str);
            Aver.AreEqual(USER_STATUS, obj["Status"]);
            Aver.AreEqual(USER_NAME, obj["Name"]);

            str = wc.DownloadString(INTEGRATION_HTTP_ADDR + "GetWithPermission");

            Aver.IsFalse( Regex.IsMatch(str, "Authorization to .+/TestPath/TestPermission.+ failed"));
          }
        }

        [Run]
        public void Action_NullableDateNoParameter()
        {
          using (var wc = CreateWebClient())
          {
            var response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "RelaxedDateTime");
            Aver.IsTrue(response.IsNullOrEmpty());
          }
        }

        [Run]
        public void Action_NullableDateTickFormat()
        {
          using (var wc = CreateWebClient())
          {
            var dt = new DateTime(2014, 12, 5);
            var dtStr = dt.ToString();

            var response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "RelaxedDateTime?dt=" + dt.Ticks);
            Aver.IsTrue(response.IsNotNullOrEmpty());
            var responseDt = DateTime.Parse(response.Trim('"'));
            Aver.AreEqual(dt, responseDt);
          }
        }

        [Run]
        public void Action_NullableDateWrongFormat_Relaxed()
        {
          using (var wc = CreateWebClient())
          {
            var response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "RelaxedDateTime?dt=fgtYY");
            Aver.IsTrue(response.IsNullOrEmpty());
          }
        }

        [Run]
        public void Action_NullableDateWrongFormat_Strict()
        {
          using (var wc = CreateWebClient())
          {
            try
            {
              var response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "StrictDateTime?dt=fgtYY");
              throw new Exception(EXPECTED_400);
            }
            catch (WebException wex)
            {
              Aver.IsTrue(Is400(wex));
            }
          }
        }


        [Run]
        public void MultipartByteArray()
        {
          MultipartTest("MultipartByteArray");
        }

        [Run]
        public void MultipartMap()
        {
          MultipartTest("MultipartMap");
        }

        [Run]
        public void MultipartRow()
        {
          MultipartTest("MultipartRow");
        }

        [Run]
        public void MultipartStream()
        {
          MultipartTest("MultipartStream");
        }

        [Run]
        public void MultipartEncoding()
        {
          var encoding = Encoding.GetEncoding("ISO-8859-1");

          var part = new Azos.Web.Multipart.Part("field");
          part.Content = "Value";

          var mp = new Azos.Web.Multipart(new Azos.Web.Multipart.Part[] { part });
          var enc = mp.Encode(encoding);

          var req = HttpWebRequest.CreateHttp(INTEGRATION_HTTP_ADDR + "MultipartEncoding");
          req.Method = "POST";
          req.ContentType = Azos.Web.ContentType.FORM_MULTIPART_ENCODED + "; charset=iso-8859-1";
          req.ContentLength = enc.Length;
          req.CookieContainer = new CookieContainer();
          req.CookieContainer.Add(S_WAVE_URI, S_WAVE_COOKIE);

          using (var reqs = req.GetRequestStream())
          {
            reqs.Write(enc.Buffer, 0, (int)enc.Length);
            var resp = req.GetResponse();

            var ms = new MemoryStream();
            resp.GetResponseStream().CopyTo(ms);

            Aver.AreEqual(part.Content.AsString(), encoding.GetString(ms.ToArray()));
          }
        }

    //[Run]
    //public void Action_NullableDateTimeWrongFormat()
    //{
    //  using (var wc = CreateWebClient())
    //  {
    //    var response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "StrictDateTime");
    //    Aver.IsTrue(response.IsNullOrEmpty());

    //    var dt = new DateTime(2014, 12, 5);
    //    var dtStr = dt.ToString();

    //    response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "StrictDateTime?dt=" + dtStr);
    //    Aver.IsTrue(response.IsNotNullOrEmpty());
    //    var responseDt = DateTime.Parse(response.Trim('"'));
    //    Aver.AreEqual(dt, responseDt);

    //    response = wc.DownloadString(INTEGRATION_HTTP_ADDR + "StrictDateTime?dt=" + dt.Ticks);
    //    Aver.IsTrue(response.IsNotNullOrEmpty());
    //    responseDt = DateTime.Parse(response.Trim('"'));
    //    Aver.AreEqual(dt, responseDt);
    //  }
    //}

    #endregion

    #region Helpers

        private bool Is400(WebException ex)
        {
          return ex.Message.IndexOf("(400)") >= 0;
        }

        private bool Is404(WebException ex)
        {
          return ex.Message.IndexOf("(404)") >= 0;
        }

        private static string GetUTF8StringWOBOM(byte[] buf)
        {
          if (buf.Length > 3 && (buf[0] == 0xEF && buf[1] == 0xBB && buf[2] == 0xBF))
              return Encoding.UTF8.GetString(buf, 3, buf.Length-3);

          return Encoding.UTF8.GetString(buf);
        }

        private void MultipartTest(string type)
        {
          var partField = new Azos.Web.Multipart.Part("field");
          partField.Content = "value";

          var partTxtFile = new Azos.Web.Multipart.Part("text");
          partTxtFile.Content = "Text with\r\nnewline";
          partTxtFile.FileName = "TxtFile";
          partTxtFile.ContentType = "Content-type: text/plain";

          var partBinFile = new Azos.Web.Multipart.Part("bin");
          partBinFile.Content = new byte[] { 0xff, 0xaa, 0x89, 0xde, 0x23, 0x20, 0xff, 0xfe, 0x02 };
          partBinFile.FileName = "BinFile";
          partBinFile.ContentType = "application/octet-stream";

          var mp = new Azos.Web.Multipart(new Azos.Web.Multipart.Part[] { partField, partTxtFile, partBinFile });

          var enc = mp.Encode();

          var req = HttpWebRequest.CreateHttp(INTEGRATION_HTTP_ADDR + type);
          req.Method = "POST";
          req.ContentType = Azos.Web.ContentType.FORM_MULTIPART_ENCODED;
          req.ContentLength = enc.Length;
          req.CookieContainer = new CookieContainer();
          req.CookieContainer.Add(S_WAVE_URI, S_WAVE_COOKIE);

          using (var reqs = req.GetRequestStream())
          {
            reqs.Write(enc.Buffer, 0, (int)enc.Length);
            var resp = req.GetResponse();

            var ms = new MemoryStream();
            resp.GetResponseStream().CopyTo(ms);
            var returned = ms.ToArray();

            var fieldSize = Encoding.UTF8.GetBytes(partField.Content.AsString()).Length;
            var txtFileSize = Encoding.UTF8.GetBytes(partTxtFile.Content.AsString()).Length;
            Aver.AreEqual(partField.Content.AsString(), Encoding.UTF8.GetString(returned.Take(fieldSize).ToArray()));
            Aver.AreEqual(partTxtFile.Content.AsString(), Encoding.UTF8.GetString(returned.Skip(fieldSize).Take(txtFileSize).ToArray()));
            Aver.IsTrue(Azos.IOUtils.MemBufferEquals(partBinFile.Content as byte[], returned.Skip(fieldSize + txtFileSize).ToArray()));
          }
        }

    #endregion

    #endregion
  }
}
