/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Web
{
  /// <summary>
  /// Utility methods for making web calls
  /// </summary>
  public static class WebCallExtensions
  {
    /// <summary>
    /// Gets string response containing json and returns it as JsonDataMap
    /// </summary>
    public static async Task<JsonDataMap> GetJsonMapAsync(this HttpClient client, string uri)
    {
      var raw = await client.NonNull(nameof(client)).GetStringAsync(uri.NonBlank(nameof(uri)));
      var jdm = raw.JsonToDataObject() as JsonDataMap;
      return jdm.NonNull(StringConsts.WEB_CALL_RETURN_JSONMAP_ERROR.Args(raw.TakeFirstChars(32)));
    }

    /// <summary>
    /// Posts body into remote endpoint returning a JsonDataMap result on success.
    /// A body is a string, a binary blob or object converted to json using JsonWritingOptions
    /// </summary>
    public static async Task<JsonDataMap> PostAndGetJsonMapAsync(this HttpClient client,
                                                                      string uri,
                                                                      object body,
                                                                      string contentType = null,
                                                                      JsonWritingOptions options = null)
     => await CallAndGetJsonMapAsync(client, uri, HttpMethod.Post, body);



    /// <summary>
    /// Puts body into remote endpoint returning a JsonDataMap result on success.
    /// A body is a string, a binary blob or object converted to json using JsonWritingOptions
    /// </summary>
    public static async Task<JsonDataMap> PutAndGetJsonMapAsync(this HttpClient client,
                                                                     string uri,
                                                                     object body,
                                                                     string contentType = null,
                                                                     JsonWritingOptions options = null)
     => await CallAndGetJsonMapAsync(client, uri, HttpMethod.Put, body);



    private static readonly HttpMethod PATCH = new HttpMethod("PATCH");

    /// <summary>
    /// Patches body into remote endpoint returning a JsonDataMap result on success.
    /// A body is a string, a binary blob or object converted to json using JsonWritingOptions
    /// </summary>
    public static async Task<JsonDataMap> PatchAndGetJsonMapAsync(this HttpClient client,
                                                                       string uri,
                                                                       object body,
                                                                       string contentType = null,
                                                                       JsonWritingOptions options = null)
     => await CallAndGetJsonMapAsync(client, uri, PATCH, body);


    /// <summary>
    /// Deletes entity with optional body from the remote endpoint returning a JsonDataMap result on success.
    /// A body is a string, a binary blob or object converted to json using JsonWritingOptions
    /// </summary>
    public static async Task<JsonDataMap> DeleteAndGetJsonMapAsync(this HttpClient client,
                                                                        string uri,
                                                                        object body = null,
                                                                        string contentType = null,
                                                                        JsonWritingOptions options = null)
     => await CallAndGetJsonMapAsync(client, uri, HttpMethod.Delete, body);



    /// <summary>
    /// Calls an arbitrary HttpMethod with the specified entity body on a remote endpoint returning a JsonDataMap result on success.
    /// A body is a string, a binary blob or object converted to json using JsonWritingOptions
    /// </summary>
    public static async Task<JsonDataMap> CallAndGetJsonMapAsync(this HttpClient client,
                                                                      string uri,
                                                                      HttpMethod method,
                                                                      object body,
                                                                      string contentType = null,
                                                                      JsonWritingOptions options = null,
                                                                      bool fetchErrorContent = true)
    {
      HttpContent content = null;

      if (body != null)
      {
        if (body is string strbody)
        {
          content = new StringContent(strbody, Encoding.UTF8, contentType.Default(ContentType.TEXT));
        }
        else if (body is byte[] binbody)
        {
          content = new ByteArrayContent(binbody);
          var ctp = new MediaTypeHeaderValue(contentType.Default(ContentType.BINARY))
          {
            CharSet = Encoding.UTF8.WebName
          };
          content.Headers.ContentType = ctp;
        }
        else
        {
          var jsonToSend = body.ToJson(options ?? JsonWritingOptions.CompactRowsAsMap);
          content = new StringContent(jsonToSend, Encoding.UTF8, ContentType.JSON);
        }
      }

      using (var request = new HttpRequestMessage(method.NonNull(nameof(method)), uri.NonBlank(nameof(uri))))
      {
        if (content != null)
          request.Content = content;

        using (var response = await client.NonNull().SendAsync(request, fetchErrorContent ? HttpCompletionOption.ResponseContentRead
                                                                                          : HttpCompletionOption.ResponseHeadersRead))
        {
          //20191022 DKh
          //response.EnsureSuccessStatusCode();
          var isSuccess = response.IsSuccessStatusCode;
          string raw = string.Empty;
          if (isSuccess || fetchErrorContent)
            raw = await response.Content.ReadAsStringAsync();

          if (!isSuccess)
            throw new WebCallException(StringConsts.WEB_CALL_UNSUCCESSFUL_ERROR.Args(uri.SplitKVP('?').Key.TakeLastChars(12),
                                                                                    (int)response.StatusCode,
                                                                                    response.StatusCode),
                                       uri,
                                       method.Method,
                                       (int)response.StatusCode,
                                       response.ReasonPhrase,
                                       raw.TakeFirstChars(512, ".."));

          return (raw.JsonToDataObject() as JsonDataMap).NonNull(StringConsts.WEB_CALL_RETURN_JSONMAP_ERROR.Args(raw.TakeFirstChars(32)));
        }//using response
      }//using request
    }


    /// <summary>
    /// Processes the "wrap" Json protocol such as: '{OK: true, data: {...}}'
    /// Throws averment exceptions if OK!=true, no 'data' key was returned or data is not an existing map
    /// </summary>
    public static JsonDataMap UnwrapPayloadMap(this JsonDataMap data)
    {
      data.NonNull(nameof(data));

      Aver.IsTrue(data["OK"].AsBool(), "OK != true");
      Aver.IsTrue(data.ContainsKey("data"), "no ['data'] key");
      var result = data["data"] as JsonDataMap;
      Aver.IsNotNull(result, "['data'] is not map");

      return result;
    }

    /// <summary>
    /// Processes the "wrap" Json protocol such as: '{OK: true, data: [...]}'
    /// Throws averment exceptions if OK!=true, no 'data' key was returned or data is not an existing array
    /// </summary>
    public static JsonDataArray UnwrapPayloadArray(this JsonDataMap data)
    {
      data.NonNull(nameof(data));

      Aver.IsTrue(data["OK"].AsBool(), "OK != true");
      Aver.IsTrue(data.ContainsKey("data"), "no ['data'] key");
      var result = data["data"] as JsonDataArray;
      Aver.IsNotNull(result, "['data'] is not array");

      return result;
    }

  }
}
