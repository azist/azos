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
    public async Task EchoParams_GET()
    {
      var response = await Client.GetAsync("echoparams?a=hello&b=2&c=true");
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);
      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
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
      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
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
      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
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
      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
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
      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual(4, got.Count);
      Aver.AreEqual("hello", got["a"].AsString());
      Aver.AreEqual(789, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());

      var got2 = got["got"] as JSONDataMap;
      Aver.IsNotNull(got2);
      Console.WriteLine(got2.ToJSON());

      Aver.IsTrue( got2.Count >= 5);
      Aver.AreEqual("hello", got2["a"].AsString());
      Aver.AreEqual(789, got2["b"].AsInt());
      Aver.AreEqual(true, got2["c"].AsBool());
      Aver.AreEqual("ok", got2["d"].AsString());
      Aver.AreEqual("false", got2["e"].AsString());

    }

    [Run]
    public async Task EchoModelA_GET()
    {
      var response = await Client.GetAsync("echomodela?id=xz1234&name=Alexz%20Tester&dob=August%2015%201980");
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);
      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
      Aver.IsNotNull(got);

      var data = JSONReader.ToRow<Controllers.ModelA>(got);

      Aver.IsNotNull(data);
      Aver.AreEqual("xz1234", data.ID);
      Aver.AreEqual("Alexz Tester", data.Name);
      Aver.IsNotNull(data.DOB);
      Aver.AreEqual(1980, data.DOB.Value.Year);
      Aver.AreEqual(8,    data.DOB.Value.Month);
      Aver.AreEqual(15,   data.DOB.Value.Day);
    }

    [Run]
    public async Task EchoModelA_POST()
    {
      var jsonToSend = new Controllers.ModelA
      {
        ID = "xz1234",
        Name = "Alexz Tester",
        DOB = new DateTime(1980, 8, 15)
      }.ToJSON(JSONWritingOptions.CompactRowsAsMap);

      var content = new StringContent(
         jsonToSend,
         System.Text.Encoding.UTF8,
         ContentType.JSON);

      var response = await Client.PostAsync("echomodela", content);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      Aver.AreEqual(ContentType.JSON, response.Content.Headers.ContentType.MediaType);

      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
      Aver.IsNotNull(got);

      var data = JSONReader.ToRow<Controllers.ModelA>(got);

      Aver.IsNotNull(data);
      Aver.AreEqual("xz1234", data.ID);
      Aver.AreEqual("Alexz Tester", data.Name);
      Aver.IsNotNull(data.DOB);
      Aver.AreEqual(1980, data.DOB.Value.Year);
      Aver.AreEqual(8, data.DOB.Value.Month);
      Aver.AreEqual(15, data.DOB.Value.Day);
    }


    [Run]
    public async Task EchoBuffer_POST()
    {
      var toSend = new byte[]{1, 2, 3, 255, 254, 253};

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
      var toSend = new byte[] { 1, 2, 3, 255, 254, 253 };

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

  }
}
