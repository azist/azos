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

using Azos.Apps;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Web
{
  /// <summary>
  /// Utility methods for making web calls
  /// </summary>
  public static class WebCallExtensions
  {
    /// <summary> Marker interface for traits applicable to making web calls</summary>
    public interface ICallerAspect { }

    /// <summary> Indicates that the caller is capable of providing DistributedCallFlow context </summary>
    public interface IDistributedCallFlowAspect : ICallerAspect
    {
      /// <summary> If non-empty provides header name to be used for sending DistributedCallFlow object, otherwise a default name is used</summary>
      string DistributedCallFlowHeader {  get; }

      /// <summary>
      /// Gets the distributed call flow or NULL if not present or not enabled
      /// </summary>
      DistributedCallFlow GetDistributedCallFlow();
    }

    /// <summary>
    /// Designates the caller as authentication provider for remote impersonation - provides
    /// header name and header value if caller impersonation is used
    /// </summary>
    public interface IAuthImpersonationAspect : ICallerAspect
    {
      /// <summary>
      /// When non empty provides header name used for impersonation, otherwise standard HTTP `Authorization` header is used
      /// </summary>
      string AuthImpersonationHeader {  get; }

      /// <summary>
      /// Gets auth header value if impersonation is used or null
      /// </summary>
      string GetAuthImpersonationHeader();
    }


    /// <summary>
    /// Sets maximum error content length in characters
    /// </summary>
    public const int CALL_ERROR_CONTENT_MAX_LENGTH = 32 * 1024;


    private const string SYSTOKEN_SCHEME = WebConsts.AUTH_SCHEME_SYSTOKEN + " ";
    /// <summary>
    /// Creates a header entry `Authorization: Systoken [token]` for the specified user object.
    /// User object may not be null
    /// </summary>
    public static KeyValuePair<string, string> MakeSysTokenAuthHeader(this Security.User user)
      =>  new KeyValuePair<string, string>(WebConsts.HTTP_HDR_AUTHORIZATION,
                                           SYSTOKEN_SCHEME + user.NonNull(nameof(user)).AuthToken.ToString());


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
      client.NonNull(nameof(client));
      method.NonNull(nameof(method));
      uri.NonNull(nameof(uri));

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

      using (var request = new HttpRequestMessage(method, uri))
      {
        if (content != null)
          request.Content = content;

        if (requestHeaders != null)
        {
          foreach(var pair in requestHeaders)
            request.Headers.Add(pair.Key, pair.Value);
        }

        if (client is IDistributedCallFlowAspect dca)
        {
          var dcf = dca.GetDistributedCallFlow();
          if (dcf != null)
          {
            var hdr = dca.DistributedCallFlowHeader.Default(CoreConsts.HTTP_HDR_DEFAULT_CALL_FLOW);
            request.Headers.Add(hdr, dcf.ToHeaderValue());
          }
        }

        if (client is IAuthImpersonationAspect aia)
        {
          var token = aia.GetAuthImpersonationHeader();
          if (token.IsNotNullOrWhiteSpace())
          {
            var hdr = aia.AuthImpersonationHeader.Default(WebConsts.HTTP_HDR_AUTHORIZATION);
            request.Headers.Add(hdr, token);
          }
        }

        using (var response = await client.SendAsync(request, fetchErrorContent ? HttpCompletionOption.ResponseContentRead
                                                                                : HttpCompletionOption.ResponseHeadersRead)
                                          .ConfigureAwait(false))
        {
          var isSuccess = response.IsSuccessStatusCode;
          string raw = string.Empty;
          if (isSuccess || fetchErrorContent)
            raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

          if (!isSuccess)
            throw new WebCallException(StringConsts.WEB_CALL_UNSUCCESSFUL_ERROR.Args(uri.SplitKVP('?').Key.TakeLastChars(64),
                                                                                    (int)response.StatusCode,
                                                                                    response.StatusCode),
                                       uri,
                                       method.Method,
                                       (int)response.StatusCode,
                                       response.ReasonPhrase,
                                       raw.TakeFirstChars(CALL_ERROR_CONTENT_MAX_LENGTH, "..."));

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
    public static ChangeResult UnwrapChangeResult(this JsonDataMap data)
    {
      data.NonNull(nameof(data)).ExpectOK();

      Aver.IsTrue(data.ContainsKey("change"), "no ['change'] key");
      Aver.IsTrue(data.ContainsKey("data"), "no ['data'] key");

      return new ChangeResult(data);
    }


    /// <summary>
    /// Processes the "wrap" Json protocol with JsonDataMap such as: '{OK: true, data: object}'
    /// Throws averment exceptions if OK!=true, no 'data' key was returned.
    /// The property 'data' may be NULL
    /// </summary>
    public static object UnwrapPayloadObject(this JsonDataMap data)
    {
      data.NonNull(nameof(data)).ExpectOK();

      Aver.IsTrue(data.ContainsKey("data"), "no ['data'] key");
      var result = data["data"];
      return result;
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
