/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Web;
using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Serialization.Bix;

namespace Azos.Tests.Unit.Wave
{
  [Runnable(TRUN.BASE)]
  public class ParamBindingTests : ServerTestsBase
  {
    private static readonly string BASE_ADDRESS = BASE_URI.ToString() + "mvc/parambinding/";

    protected override void DoPrologue(Runner runner, FID id)
    {
      Client.BaseAddress = new Uri(BASE_ADDRESS);
    }

    [Run]
    public async Task EchoMap_POST()
    {
      var jsonToSend = new
      {
        got = new
        {
          a = 123,
          b = -10,
          c = true
        }
      }.ToJson(JsonWritingOptions.CompactRowsAsMap);

      var content = new StringContent(
         jsonToSend,
         System.Text.Encoding.UTF8,
         ContentType.JSON);

      var response = await Client.PostAsync("echomap", content);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      Aver.AreEqual(ContentType.JSON, response.Content.Headers.ContentType.MediaType);

      var got = (await response.Content.ReadAsStringAsync()).JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(got);

      Aver.IsNotNull(got);
      Aver.AreEqual(123, got["a"].AsInt());
      Aver.AreEqual(-10, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());
    }

    [Run]
    public async Task EchoMap_POST_Json_WithTypeHints_HttpClient()
    {
      var jsonToSend = new
      {
        got = new
        {
          atm = Atom.Encode("123"),
          dt  = new DateTime(1980, 05, 12, 14, 20, 0, DateTimeKind.Utc),
          buf = new byte[]{0,1,2,3,4,5,6,7,8,9},
          gd = new GDID(10, 123456789ul)
        }
      }.ToJson(JsonWritingOptions.CompactRowsAsMapWithTypeHints);

      using var content = new StringContent(
         jsonToSend,
         System.Text.Encoding.UTF8,
         ContentType.JSON_WITH_TYPEHINTS);

      using var req = new HttpRequestMessage(HttpMethod.Post, "echomap")
      {
        Content = content
      };

      req.Headers.TryAddWithoutValidation("Accept", ContentType.JSON_WITH_TYPEHINTS);

      using var response = await Client.SendAsync(req);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      Aver.AreEqual(ContentType.JSON_WITH_TYPEHINTS, response.Content.Headers.ContentType.MediaType);
      response.Content.Headers.See();

      var got = (await response.Content.ReadAsStringAsync()).JsonToDataObject(new JsonReadingOptions(JsonReadingOptions.Default){ EnableTypeHints = true} ) as JsonDataMap;
      got.See();

      Aver.IsNotNull(got);
      Aver.IsTrue(got["atm"] is Atom);
      Aver.IsTrue(got["dt"] is DateTime);
      Aver.IsTrue(got["buf"] is byte[]);
      Aver.IsTrue(got["gd"] is GDID);

      Aver.AreEqual(Atom.Encode("123"), got["atm"].AsAtom());
      Aver.AreEqual(1980, got["dt"].AsDateTime().Year);
      Aver.AreEqual(10, ((byte[])got["buf"]).Length);
      Aver.AreEqual(123456789ul, got["gd"].AsGDID().Counter);
    }

    [Run]
    public async Task EchoMap_POST_Bixon_WithTypeHints_HttpClient()
    {
      var objToSend = new
      {
        got = new
        {
          atm = Atom.Encode("123"),
          dt = new DateTime(1980, 05, 12, 14, 20, 0, DateTimeKind.Utc),
          buf = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
          gd = new GDID(10, 123456789ul)
        }
      };

      using var wscope = new BixWriterBufferScope(1024);
      Bixon.WriteObject(wscope.Writer, objToSend);

      using var content = new ByteArrayContent(wscope.Buffer);
      content.Headers.TryAddWithoutValidation("Content-Type", ContentType.BIXON);

      using var req = new HttpRequestMessage(HttpMethod.Post, "echomap")
      {
        Content = content
      };

      req.Headers.TryAddWithoutValidation("Accept", ContentType.BIXON);

      using var response = await Client.SendAsync(req);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);
      response.Content.Headers.See();

      Aver.AreEqual(ContentType.BIXON, response.Content.Headers.ContentType.MediaType);

      var gotBuf = await response.Content.ReadAsByteArrayAsync();
      using var rscope = new BixReaderBufferScope(gotBuf);
      var got = Bixon.ReadObject(rscope.Reader) as JsonDataMap;

      got.See();

      Aver.IsNotNull(got);
      Aver.IsTrue(got["atm"] is Atom);
      Aver.IsTrue(got["dt"] is DateTime);
      Aver.IsTrue(got["buf"] is byte[]);
      Aver.IsTrue(got["gd"] is GDID);

      Aver.AreEqual(Atom.Encode("123"), got["atm"].AsAtom());
      Aver.AreEqual(1980, got["dt"].AsDateTime().Year);
      Aver.AreEqual(10, ((byte[])got["buf"]).Length);
      Aver.AreEqual(123456789ul, got["gd"].AsGDID().Counter);
    }


    [Run]
    public async Task EchoParams_GET()
    {
      var response = await Client.GetAsync("echoparams?a=hello&b=2&c=true");
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);
      var got = (await response.Content.ReadAsStringAsync()).JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual("hello", got["a"].AsString());
      Aver.AreEqual(2, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());
    }

    [Run]
    public async Task EchoParams_POST()
    {
      var content = new FormUrlEncodedContent(new[]{
        new KeyValuePair<string, string>("a", "hello"),
        new KeyValuePair<string, string>("b", "2"),
        new KeyValuePair<string, string>("c", "true"),
      });

      var response = await Client.PostAsync("echoparams", content);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);
      var got = (await response.Content.ReadAsStringAsync()).JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual("hello", got["a"].AsString());
      Aver.AreEqual(2, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());
    }


    [Run]
    public async Task EchoParamsWithDefaults_GET()
    {
      var response = await Client.GetAsync("echoparamswithdefaults?a=hello");
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);
      var got = (await response.Content.ReadAsStringAsync()).JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual("hello", got["a"].AsString());
      Aver.AreEqual(127, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());
    }

    [Run]
    public async Task EchoParamsWithDefaults_POST()
    {
      var content = new FormUrlEncodedContent(new[]{
        new KeyValuePair<string, string>("a", "hello"),
      });

      var response = await Client.PostAsync("echoparamswithdefaults", content);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);
      var got = (await response.Content.ReadAsStringAsync()).JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual("hello", got["a"].AsString());
      Aver.AreEqual(127, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());
    }


    [Run]
    public async Task EchoMixMap()
    {
      var response = await Client.GetAsync("echomixmap?a=hello&b=789&c=true&d=ok&e=false");
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);
      var got = (await response.Content.ReadAsStringAsync()).JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual(4, got.Count);
      Aver.AreEqual("hello", got["a"].AsString());
      Aver.AreEqual(789, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());

      var got2 = got["got"] as JsonDataMap;
      Aver.IsNotNull(got2);
      Console.WriteLine(got2.ToJson());

      Aver.IsTrue( got2.Count >= 5);
      Aver.AreEqual("hello", got2["a"].AsString());
      Aver.AreEqual(789, got2["b"].AsInt());
      Aver.AreEqual(true, got2["c"].AsBool());
      Aver.AreEqual("ok", got2["d"].AsString());
      Aver.AreEqual("false", got2["e"].AsString());

    }

    [Run]
    public async Task EchoModelA_POST()
    {
      var jsonToSend = new {
       got = new Controllers.ModelA
        {
          ID = "xz1234",
          Name = "Alexz Tester",
          DOB = new DateTime(1980, 8, 15)
        }
      }.ToJson(JsonWritingOptions.CompactRowsAsMap);

      var content = new StringContent(
         jsonToSend,
         System.Text.Encoding.UTF8,
         ContentType.JSON);

      var response = await Client.PostAsync("echomodela", content);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      Aver.AreEqual(ContentType.JSON, response.Content.Headers.ContentType.MediaType);

      var got = (await response.Content.ReadAsStringAsync()).JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(got);

      var data = JsonReader.ToDoc<Controllers.ModelA>(got);

      Aver.IsNotNull(data);
      Aver.AreEqual("xz1234", data.ID);
      Aver.AreEqual("Alexz Tester", data.Name);
      Aver.IsNotNull(data.DOB);
      Aver.AreEqual(1980, data.DOB.Value.Year);
      Aver.AreEqual(8, data.DOB.Value.Month);
      Aver.AreEqual(15, data.DOB.Value.Day);
    }


    [Run]
    public async Task EchoMixModelA_POST()
    {
      var jsonToSend = new {
        id ="980",
        another = "barsuk-egor",
        model = new Controllers.ModelA
        {
          ID = "model-xz1234",
          Name = "model-Alexz Tester",
          DOB = new DateTime(1980, 8, 15)
        }
      }.ToJson(JsonWritingOptions.CompactRowsAsMap);

      var content = new StringContent(
         jsonToSend,
         System.Text.Encoding.UTF8,
         ContentType.JSON);

      var response = await Client.PostAsync("echomixmodela", content);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      Aver.AreEqual(ContentType.JSON, response.Content.Headers.ContentType.MediaType);

      var got = (await response.Content.ReadAsStringAsync()).JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(got);

      Aver.AreEqual("980", got["id"].AsString());
      Aver.AreEqual("barsuk-egor", got["another"].AsString());

      var modelDoc = JsonReader.ToDoc<Controllers.ModelA>(got["model"] as JsonDataMap);

      Aver.IsNotNull(modelDoc);
      Aver.AreEqual("model-xz1234", modelDoc.ID);
      Aver.AreEqual("model-Alexz Tester", modelDoc.Name);
      Aver.IsNotNull(modelDoc.DOB);
      Aver.AreEqual(1980, modelDoc.DOB.Value.Year);
      Aver.AreEqual(8, modelDoc.DOB.Value.Month);
      Aver.AreEqual(15, modelDoc.DOB.Value.Day);
    }



    [Run]
    public async Task EchoBuffer_POST()
    {
      var toSend = new byte[]
      {
        1, 2, 3, 255, 254, 253, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 65, 97, 12, 102, 103, 104, 105, 250, 190, 190, 78, 94, 89, 12, 17,
        4, 5, 6, 7, 8, 9, 1, 2, 3, 34, 34, 87, 98, 98, 92, 93, 94, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,
        0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,32,11,57,
        1,2,7,8,9,3,67,79,97,11,11,45,26,61,99,123, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 55, 45,45,45,45,45,45,48,99, 254, 15
      };

      var multipart = new MultipartFormDataContent();
      var content = new ByteArrayContent(toSend);
      content.Headers.Add(WebConsts.HTTP_HDR_CONTENT_TYPE, ContentType.BINARY);
      multipart.Add(content, "buffer", "out.txt");

      var response = await Client.PostAsync("echobuffer", multipart);
      Aver.IsTrue( HttpStatusCode.OK == response.StatusCode );

      Aver.AreEqual(ContentType.BINARY, response.Content.Headers.ContentType.MediaType);
      var got = await response.Content.ReadAsByteArrayAsync();

      Aver.IsNotNull(got);

      Aver.IsTrue( toSend.MemBufferEquals(got) );
    }

    [Run]
    public async Task EchoBuffer_POST_AzosMultipart()
    {
      var toSend = new byte[]
      {
        1, 2, 3, 255, 254, 253, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 65, 97, 12, 102, 103, 104, 105, 250, 190, 190, 78, 94, 89, 12, 17,
        4, 5, 6, 7, 8, 9, 1, 2, 3, 34, 34, 87, 98, 98, 92, 93, 94, 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,
        0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,0x3d,32,11,57,
        1,2,7,8,9,3,67,79,97,11,11,45,26,61,99,123, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 55, 45,45,45,45,45,45,48,99, 254, 15
      };

      var part = new Multipart.Part("buffer")
      {
        Content = toSend,
        FileName = "file.bin",
        ContentType = ContentType.BINARY
      };

      var multipart = new Multipart(new []{part}).Encode();

      var content = new ByteArrayContent(multipart.Buffer, 0, (int)multipart.Length);
      content.Headers.Add(WebConsts.HTTP_HDR_CONTENT_TYPE, ContentType.FORM_MULTIPART_ENCODED);

      var response = await Client.PostAsync("echobuffer", content);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsByteArrayAsync();
      Aver.IsNotNull(got);

      Aver.IsTrue(toSend.MemBufferEquals(got));
    }





    [Run]
    public async Task EchoVariousParams_GET()
    {
      var uri = new UriQueryBuilder("echovariousparams")
      .Add("gd", new GDID(1, 3, 1234))
      .Add("gu", new Guid("4937EE58-A81F-402A-B019-3D5EA6BC12D6"))
      .Add("a", Atom.Encode("abc123"))
      .Add("e", EntityId.Parse("type.sch@sys::address").AsString)
      .Add("dt", new DateTime(1980, 08, 15, 12, 00, 00, DateTimeKind.Utc).ToString("o"))
      .Add("m", -123.4567m)
      .Add("d", -5e3)
      .Add("b", true)
      .Add("li", -8_000_000_000L)
      .Add("s", "my string!");

      uri.ToString().See();

      var response = await Client.GetAsync(uri.ToString());
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);
      var got = (await response.Content.ReadAsStringAsync()).JsonToDataObject() as JsonDataMap;
      invariant(got);

    }

    private void invariant(JsonDataMap got)
    {
      Aver.IsNotNull(got);
      Aver.AreEqual(new GDID(1, 3, 1234), got["gd"].AsGDID());
      Aver.AreEqual(new Guid("4937EE58-A81F-402A-B019-3D5EA6BC12D6"), got["gu"].AsGUID(Guid.Empty));
      Aver.AreEqual(Atom.Encode("abc123"), got["a"].AsAtom());
      Aver.AreEqual(EntityId.Parse("type.sch@sys::address"), got["e"].AsEntityId());

      Aver.AreEqual(new DateTime(1980, 08, 15, 12, 00, 00, DateTimeKind.Utc), got["dt"].AsDateTime(System.Globalization.DateTimeStyles.AdjustToUniversal));
      Aver.AreEqual(-123.4567m, got["m"].AsDecimal());
      Aver.AreEqual(-5e3d, got["d"].AsDouble());
      Aver.AreEqual(true, got["b"].AsBool());
      Aver.AreEqual(-8_000_000_000L, got["li"].AsLong());
      Aver.AreEqual("my string!", got["s"].AsString());
    }

    [Run]
    public async Task EchoVariousParams_POST()
    {
      var obj = new
      {
        gd = new GDID(1, 3, 1234),
        gu = new Guid("4937EE58-A81F-402A-B019-3D5EA6BC12D6"),
        a  = Atom.Encode("abc123"),
        e = EntityId.Parse("type.sch@sys::address"),
        dt = new DateTime(1980, 08, 15, 12, 00, 00, DateTimeKind.Utc),
        m = -123.4567m,
        d = -5e3,
        b =  true,
        li = -8_000_000_000L,
        s = "my string!"
      };
      var got = await Client.PostAndGetJsonMapAsync("echovariousparams", obj);
      invariant(got);
    }






  }
}
