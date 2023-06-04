/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Web
{
  public static partial class WebCallExtensions
  {
    /// <summary>
    /// Used as default when Bixon serializer is used. Enables data documents with Bix code
    /// </summary>
    public static readonly JsonWritingOptions BIXON_MARSHALLING_WITH_TYPE = new JsonWritingOptions(true) {  Purpose = JsonSerializationPurpose.Marshalling };


    /// <summary>
    /// Calls an arbitrary HttpMethod with the specified entity body on a remote endpoint returning a JsonDataMap result on success.
    /// A body is a string, a binary blob or object converted to json using JsonWritingOptions.
    /// Optional fGetIdentityContext is used to create impersonation auth tokens - defines what identity is for the call.
    /// </summary>
    public static Task<JsonDataMap> CallAndGetJsonMapAsync(this HttpClient client,
                                                                string uri,
                                                                HttpMethod method,
                                                                object body,
                                                                string contentType = null,
                                                                JsonWritingOptions options = null,
                                                                bool fetchErrorContent = true,
                                                                IEnumerable<KeyValuePair<string, string>> requestHeaders = null,
                                                                Func<object> fGetIdentityContext = null,
                                                                JsonReadingOptions ropt = null)
    => CallAsync(client,
                  async response =>
                  {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var obj = JsonReader.DeserializeDataObject(json, ropt: ropt ?? JsonReadingOptions.NoLimits);
                    var map = (obj as JsonDataMap).NonNull(StringConsts.WEB_CALL_RETURN_JSONMAP_ERROR.Args(json.TakeFirstChars(48)));
                    return map;
                  },
                  response => response.Content.ReadAsStringAsync(),
                  uri,
                  method,
                  body,
                  contentType,
                  options,
                  fetchErrorContent,
                  requestHeaders,
                  fGetIdentityContext);

    /// <summary>
    /// Calls an arbitrary HttpMethod with the specified entity body on a remote endpoint returning response content as a `byte[]` on success.
    /// A body is a string, a binary blob or object converted to json using JsonWritingOptions.
    /// Optional fGetIdentityContext is used to create impersonation auth tokens - defines what identity is for the call.
    /// </summary>
    public static Task<byte[]> CallAndGetByteArrayAsync(this HttpClient client,
                                                        string uri,
                                                        HttpMethod method,
                                                        object body,
                                                        string contentType = null,
                                                        JsonWritingOptions options = null,
                                                        bool fetchErrorContent = true,
                                                        IEnumerable<KeyValuePair<string, string>> requestHeaders = null,
                                                        Func<object> fGetIdentityContext = null)
    => CallAsync(client,
                  response => response.Content.ReadAsByteArrayAsync(),
                  response => response.Content.ReadAsStringAsync(),
                  uri,
                  method,
                  body,
                  contentType,
                  options,
                  fetchErrorContent,
                  requestHeaders,
                  fGetIdentityContext);

    /// <summary>
    /// Calls an arbitrary HttpMethod with the specified entity body on a remote endpoint returning a JsonDataMap result on success.
    /// A body is a string, a binary blob or object converted to json using JsonWritingOptions.
    /// Optional fGetIdentityContext is used to create impersonation auth tokens - defines what identity is for the call.
    /// </summary>
    public static async Task<TResult> CallAsync<TResult>(this HttpClient client,
                                                          Func<HttpResponseMessage, Task<TResult>> fOkResultGetter,
                                                          Func<HttpResponseMessage, Task<string>> fErrorStringResultGetter,
                                                          string uri,
                                                          HttpMethod method,
                                                          object body,
                                                          string contentType = null,
                                                          JsonWritingOptions options = null,
                                                          bool fetchErrorContent = true,
                                                          IEnumerable<KeyValuePair<string, string>> requestHeaders = null,
                                                          Func<object> fGetIdentityContext = null,
                                                          bool useBixon = false)
    {
      client.NonNull(nameof(client));
      fOkResultGetter.NonNull(nameof(fOkResultGetter));
      fErrorStringResultGetter.NonNull(nameof(fErrorStringResultGetter));
      method.NonNull(nameof(method));
      uri.NonNull(nameof(uri));

      HttpContent content = null;

      if (body != null)
      {
        if (body is HttpContent httpbody)//#874 20230603 DKh
        {
          content = httpbody;
        }
        else if (body is string strbody)
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
        else if (body is ArraySegment<byte> asegbody)//20230501 DKh #859
        {
          content = new ByteArrayContent(asegbody.Array, asegbody.Offset, asegbody.Count);
          var ctp = new MediaTypeHeaderValue(contentType.Default(ContentType.BINARY))
          {
            CharSet = Encoding.UTF8.WebName
          };
          content.Headers.ContentType = ctp;
        }
        else //objects - use Json or Bixon
        {
          if (useBixon)//#874 20230603 DKh
          {
            using(var wscope = new BixWriterBufferScope(1024))
            {
              Bixon.WriteObject(wscope.Writer, body, options ?? BIXON_MARSHALLING_WITH_TYPE);
              content = new ByteArrayContent(wscope.Buffer);
              content.Headers.ContentType = new MediaTypeHeaderValue(contentType.Default(ContentType.BIXON));
            }
          }
          else//json
          {
            var jopt = options ?? JsonWritingOptions.CompactRowsAsMap;
            var jsonToSend = body.ToJson(jopt);
            content = new StringContent(jsonToSend, Encoding.UTF8, contentType.Default(jopt.EnableTypeHints ? ContentType.JSON_WITH_TYPEHINTS : ContentType.JSON));
          }
        }
      }

      try
      {
        using (var request = new HttpRequestMessage(method, uri))
        {
          if (content != null)
          {
            request.Content = content;
          }

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
            var token = await aia.GetAuthImpersonationHeaderAsync(fGetIdentityContext).ConfigureAwait(false);
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

            TResult result = default;
            string rawError = string.Empty;

            try
            {
              if (isSuccess)
              {
                result = await fOkResultGetter(response).ConfigureAwait(false);
              }
              else if (fetchErrorContent)
              {
                rawError = await fErrorStringResultGetter(response).ConfigureAwait(false);
              }
            }
            catch(Exception cause)
            {
              throw new WebCallException(StringConsts.WEB_CALL_UNSUCCESSFUL_RESPONSE_ERROR.Args(uri.SplitKVP('?').Key.TakeLastChars(64),
                                                                                      (int)response.StatusCode,
                                                                                      response.StatusCode,
                                                                                      cause.ToMessageWithType()),
                                         uri,
                                         method.Method,
                                         (int)response.StatusCode,
                                         response.ReasonPhrase,
                                         "...",
                                         cause);
            }

            //#834 ------------------------------------
            if (client is IRequestBodyErrorAspect bea)
            {
              var hdrn = bea.BodyErrorHeader;
              if (hdrn.IsNotNullOrWhiteSpace())
              {
                if (response.Headers.TryGetValues(hdrn, out var hdrValues))
                {
                  await bea.ProcessBodyErrorAsync(uri, method, body, contentType, options, request, response, isSuccess, rawError, hdrValues).ConfigureAwait(false);
                }
              }
            }//#834 -----------------------------------

            if (!isSuccess)
            {
              throw new WebCallException(StringConsts.WEB_CALL_UNSUCCESSFUL_ERROR.Args(uri.SplitKVP('?').Key.TakeLastChars(64),
                                                                                      (int)response.StatusCode,
                                                                                      response.StatusCode),
                                         uri,
                                         method.Method,
                                         (int)response.StatusCode,
                                         response.ReasonPhrase,
                                         rawError.TakeFirstChars(CALL_ERROR_CONTENT_MAX_LENGTH, "..."));
            }

            return result;
          }//using response
        }//using request
      }
      finally//#874 20230603 DKh
      {
        if (content != null && content != body) content.Dispose();
      }
    }//CallAsync
  }
}
