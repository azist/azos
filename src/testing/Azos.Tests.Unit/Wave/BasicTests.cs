/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Azos.Web;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;
using System.Net.Http.Headers;

namespace Azos.Tests.Unit.Wave
{
  [Runnable(TRUN.BASE)]
  public class BasicTests : ServerTestsBase
  {
    private static readonly string BASE_ADDRESS = BASE_URI.ToString() + "mvc/basic/";

    protected override void DoPrologue(Runner runner, FID id)
    {
      Client.BaseAddress = new Uri(BASE_ADDRESS);
    }

    [Run]
    public async Task ActionPlainText()
    {
      var got = await Client.GetStringAsync("actionplaintext");
      Aver.AreEqual("Response in plain text", got);
    }

    [Run]
    public async Task ActionObjectLiteral()
    {
      var got = (await Client.GetStringAsync("actionobjectliteral")).JsonToDataObject() as JsonDataMap;
      Aver.AreEqual(1, got["a"].AsInt());
      Aver.AreEqual(true, got["b"].AsBool());
      Aver.AreEqual(1980, got["d"].AsDateTime().Year);
    }

    [Run]
    public async Task ActionHArdCodedHtml()
    {
      var got = await Client.GetStringAsync("actionhardcodedhtml");
      Aver.AreEqual("<h1>Hello HTML</h1>", got);
    }

    [Run]
    public async Task PatternMatch_GET()
    {
      var response = await Client.GetAsync("pmatch");
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.TEXT, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("get", got);
    }

    [Run]
    public async Task PatternMatch_PUT()
    {
      var content = new StringContent("v=abcd");
      content.Headers.ContentType.MediaType = ContentType.FORM_URL_ENCODED;
      var response = await Client.PutAsync("pmatch", content);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.TEXT, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("put: abcd", got);
    }

    [Run]
    public async Task PatternMatch_POST()
    {
      var content = new StringContent("v=abcd1");
      content.Headers.ContentType.MediaType = ContentType.FORM_URL_ENCODED;
      var response = await Client.PostAsync("pmatch", content);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.TEXT, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("post: abcd1", got);
    }

    [Run]
    public async Task PatternMatch_POST_JSON()
    {
      var content = new StringContent("v=abcd1");
      content.Headers.ContentType.MediaType = ContentType.FORM_URL_ENCODED;

      var request = new HttpRequestMessage(HttpMethod.Post, "pmatch");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType.JSON));
      request.Content = content;

      var response = await Client.SendAsync(request);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.JSON, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("{\"post\":\"abcd1\"}", got);
    }

    [Run]
    public async Task PatternMatch_DELETE()
    {
      var content = new StringContent("v=abcd1");
      content.Headers.ContentType.MediaType = ContentType.FORM_URL_ENCODED;

      var request = new HttpRequestMessage(HttpMethod.Delete, "pmatch");
      request.Content = content;

      var response = await Client.SendAsync(request);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.TEXT, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("delete: abcd1", got);
    }

    [Run]
    public async Task PatternMatch_PATCH()
    {
      var content = new StringContent("v=abcd1");
      content.Headers.ContentType.MediaType = ContentType.FORM_URL_ENCODED;

      var request = new HttpRequestMessage(new HttpMethod("PATCH"), "pmatch");
      request.Content = content;

      var response = await Client.SendAsync(request);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.TEXT, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("patch: abcd1", got);
    }


    [Run]
    public async Task Filter_GET_OK()
    {
      var request = new HttpRequestMessage(HttpMethod.Get, "filter-get");

      var response = await Client.SendAsync(request);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.TEXT, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("get", got);
    }

    [Run]
    public async Task Filter_GET_MISMATCH()
    {
      var request = new HttpRequestMessage(HttpMethod.Put, "filter-get");

      var response = await Client.SendAsync(request);
      Console.WriteLine($"{(int)response.StatusCode} - {response.StatusCode}");
      Aver.IsTrue(HttpStatusCode.MethodNotAllowed == response.StatusCode);
    }


    [Run]
    public async Task Filter_PUT_OK()
    {
      var request = new HttpRequestMessage(HttpMethod.Put, "filter-put");

      var response = await Client.SendAsync(request);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.TEXT, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("put: ", got);
    }

    [Run]
    public async Task Filter_PUT_MISMATCH()
    {
      var request = new HttpRequestMessage(HttpMethod.Get, "filter-put");

      var response = await Client.SendAsync(request);
      Console.WriteLine($"{(int)response.StatusCode} - {response.StatusCode}");
      Aver.IsTrue(HttpStatusCode.MethodNotAllowed == response.StatusCode);
    }


    [Run]
    public async Task Filter_POST_OK()
    {
      var request = new HttpRequestMessage(HttpMethod.Post, "filter-post");

      var response = await Client.SendAsync(request);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.TEXT, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("post: ", got);
    }

    [Run]
    public async Task Filter_POST_MISMATCH()
    {
      var request = new HttpRequestMessage(HttpMethod.Delete, "filter-post");

      var response = await Client.SendAsync(request);
      Console.WriteLine($"{(int)response.StatusCode} - {response.StatusCode}");
      Aver.IsTrue(HttpStatusCode.MethodNotAllowed == response.StatusCode);
    }

    [Run]
    public async Task Filter_POST_JSON_OK()
    {
      var content = new StringContent("v=xyz2000");
      content.Headers.ContentType.MediaType = ContentType.FORM_URL_ENCODED;

      var request = new HttpRequestMessage(HttpMethod.Post, "filter-post-json");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType.JSON));
      request.Content = content;

      var response = await Client.SendAsync(request);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.JSON, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("{\"post\":\"xyz2000\"}", got);
    }

    [Run]
    public async Task Filter_POST_JSON_MISMATCH()
    {
      var content = new StringContent("v=xyz2000");
      content.Headers.ContentType.MediaType = ContentType.FORM_URL_ENCODED;

      var request = new HttpRequestMessage(HttpMethod.Post, "filter-post-json");
      request.Content = content;

      var response = await Client.SendAsync(request);
      Console.WriteLine($"{(int)response.StatusCode} - {response.StatusCode}");
      Aver.IsTrue(HttpStatusCode.NotAcceptable == response.StatusCode);  //because is not explicitly setting Accept: application/json
                                                                         //header which is required by filter
    }

    [Run]
    public async Task Filter_DELETE_OK()
    {
      var request = new HttpRequestMessage(HttpMethod.Delete, "filter-delete");

      var response = await Client.SendAsync(request);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.TEXT, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("delete: ", got);
    }

    [Run]
    public async Task Filter_DELETE_MISMATCH()
    {
      var request = new HttpRequestMessage(HttpMethod.Post, "filter-delete");

      var response = await Client.SendAsync(request);
      Console.WriteLine($"{(int)response.StatusCode} - {response.StatusCode}");
      Aver.IsTrue(HttpStatusCode.MethodNotAllowed == response.StatusCode);
    }


    [Run]
    public async Task Filter_PATCH_OK()
    {
      var request = new HttpRequestMessage(new HttpMethod("PATCH"), "filter-patch");

      var response = await Client.SendAsync(request);
      Aver.IsTrue(HttpStatusCode.OK == response.StatusCode);

      var got = await response.Content.ReadAsStringAsync();
      Aver.AreEqual(ContentType.TEXT, response.Content.Headers.ContentType.MediaType);
      Aver.IsNotNull(got);
      Aver.AreEqual("patch: ", got);
    }

    [Run]
    public async Task Filter_PATCH_MISMATCH()
    {
      var request = new HttpRequestMessage(HttpMethod.Delete, "filter-patch");

      var response = await Client.SendAsync(request);
      Console.WriteLine($"{(int)response.StatusCode} - {response.StatusCode}");
      Aver.IsTrue(HttpStatusCode.MethodNotAllowed == response.StatusCode);
    }


  }
}
