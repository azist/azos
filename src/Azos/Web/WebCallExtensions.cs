/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
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
      var raw = await client.NonNull(nameof(client))
                            .GetStringAsync(uri.NonBlank(nameof(uri)))
                            .ConfigureAwait(false);

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
                                                                      JsonWritingOptions options = null,
                                                                      bool fetchErrorContent = true,
                                                                      IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
     => await CallAndGetJsonMapAsync(client, uri, HttpMethod.Post, body, contentType, options, fetchErrorContent, requestHeaders).ConfigureAwait(false);



    /// <summary>
    /// Puts body into remote endpoint returning a JsonDataMap result on success.
    /// A body is a string, a binary blob or object converted to json using JsonWritingOptions
    /// </summary>
    public static async Task<JsonDataMap> PutAndGetJsonMapAsync(this HttpClient client,
                                                                     string uri,
                                                                     object body,
                                                                     string contentType = null,
                                                                     JsonWritingOptions options = null,
                                                                     bool fetchErrorContent = true,
                                                                     IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
     => await CallAndGetJsonMapAsync(client, uri, HttpMethod.Put, body, contentType, options, fetchErrorContent, requestHeaders).ConfigureAwait(false);



    private static readonly HttpMethod PATCH = new HttpMethod("PATCH");

    /// <summary>
    /// Patches body into remote endpoint returning a JsonDataMap result on success.
    /// A body is a string, a binary blob or object converted to json using JsonWritingOptions
    /// </summary>
    public static async Task<JsonDataMap> PatchAndGetJsonMapAsync(this HttpClient client,
                                                                    string uri,
                                                                    object body,
                                                                    string contentType = null,
                                                                    JsonWritingOptions options = null,
                                                                    bool fetchErrorContent = true,
                                                                    IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
     => await CallAndGetJsonMapAsync(client, uri, PATCH, body, contentType, options, fetchErrorContent, requestHeaders).ConfigureAwait(false);


    /// <summary>
    /// Deletes entity with optional body from the remote endpoint returning a JsonDataMap result on success.
    /// A body is a string, a binary blob or object converted to json using JsonWritingOptions
    /// </summary>
    public static async Task<JsonDataMap> DeleteAndGetJsonMapAsync(this HttpClient client,
                                                                     string uri,
                                                                     object body = null,
                                                                     string contentType = null,
                                                                     JsonWritingOptions options = null,
                                                                     bool fetchErrorContent = true,
                                                                     IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
     => await CallAndGetJsonMapAsync(client, uri, HttpMethod.Delete, body, contentType, options, fetchErrorContent, requestHeaders).ConfigureAwait(false);



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
                                                                      bool fetchErrorContent = true,
                                                                      IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
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

        if (requestHeaders != null)
        {
          foreach(var pair in requestHeaders)
            request.Headers.Add(pair.Key, pair.Value);
        }

        using (var response = await client.NonNull().SendAsync(request, fetchErrorContent ? HttpCompletionOption.ResponseContentRead
                                                                                          : HttpCompletionOption.ResponseHeadersRead)
                                                    .ConfigureAwait(false))
        {
          var isSuccess = response.IsSuccessStatusCode;
          string raw = string.Empty;
          if (isSuccess || fetchErrorContent)
            raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

          if (!isSuccess)
            throw new WebCallException(StringConsts.WEB_CALL_UNSUCCESSFUL_ERROR.Args(uri.SplitKVP('?').Key.TakeLastChars(48),
                                                                                    (int)response.StatusCode,
                                                                                    response.StatusCode),
                                       uri,
                                       method.Method,
                                       (int)response.StatusCode,
                                       response.ReasonPhrase,
                                       raw.TakeFirstChars(512, ".."));

          return (raw.JsonToDataObject() as JsonDataMap).NonNull(StringConsts.WEB_CALL_RETURN_JSONMAP_ERROR.Args(raw.TakeFirstChars(48)));
        }//using response
      }//using request
    }


    /// <summary>
    /// Processes the "wrap" Json protocol such as: '{OK: true, . . . }'
    /// Throws averment exceptions if OK!=true or data is null. Passes-through the original caller
    /// </summary>
    public static JsonDataMap ExpectOK(this JsonDataMap data)
    {
      data.NonNull(nameof(data));
      Aver.IsTrue(data["OK"].AsBool(), "OK != true");
      return data;
    }

    /// <summary>
    /// Processes the "wrap" Json protocol with ChangeResult such as: '{OK: true, change: "Inserted", affected: 3, message: "...", data: {...}}'
    /// Throws averment exceptions if OK!=true, no 'data' or 'change' keys are returned
    /// </summary>
    public static Data.Business.ChangeResult UnwrapChangeResult(this JsonDataMap data)
    {
      data.NonNull(nameof(data)).ExpectOK();

      Aver.IsTrue(data.ContainsKey("change"), "no ['change'] key");
      Aver.IsTrue(data.ContainsKey("data"), "no ['data'] key");

      return new Data.Business.ChangeResult(data);
    }


    /// <summary>
    /// Processes the "wrap" Json protocol with JsonDataMap such as: '{OK: true, data: {...map...}}'
    /// Throws averment exceptions if OK!=true, no 'data' key was returned or data is not a json map
    /// </summary>
    public static JsonDataMap UnwrapPayloadMap(this JsonDataMap data)
    {
      data.NonNull(nameof(data)).ExpectOK();

      Aver.IsTrue(data.ContainsKey("data"), "no ['data'] key");
      var result = data["data"] as JsonDataMap;
      Aver.IsNotNull(result, "['data'] is not map");

      return result;
    }

    /// <summary>
    /// Processes the "wrap" Json protocol with JsonDataArray such as: '{OK: true, data: [...array...]}'
    /// Throws averment exceptions if OK!=true, no 'data' key was returned or data is not a json array
    /// </summary>
    public static JsonDataArray UnwrapPayloadArray(this JsonDataMap data)
    {
      data.NonNull(nameof(data)).ExpectOK();

      Aver.IsTrue(data.ContainsKey("data"), "no ['data'] key");
      var result = data["data"] as JsonDataArray;
      Aver.IsNotNull(result, "['data'] is not array");

      return result;
    }

  }
}
