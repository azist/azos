/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Azos.IO.FileSystem.S3.V4
{
  public static class S3V4HttpHelpers
  {
    public static IDictionary<string, string> FilterHeaders(this IDictionary<string, string> headers)
    {
      headers.Remove("Host");
      headers.Remove("content-type");
      headers.Remove("content-length");

      return headers;
    }

    public static HttpWebRequest ConstructWebRequest(this Uri uri, string method, Stream contentStream, IDictionary<string, string> headers,
      int timeout = 0)
    {
      if (headers != null)
        headers.FilterHeaders();

      HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

      var t = timeout == 0 ? Azos.Web.WebSettings.WebDavDefaultTimeoutMs : timeout;
      if (t > 0) request.Timeout = t;

      request.Method = method;
      request.ContentType = "text/plain";

      if (method == "PUT")
        request.ContentLength = contentStream.Length;

      foreach (var kvp in headers)
        request.Headers.Add(kvp.Key, kvp.Value);

      if (method == "PUT")
      {
        using (Stream requestStream = request.GetRequestStream())
        {
          contentStream.Position = 0;
          contentStream.CopyTo(requestStream);

          requestStream.Flush();
          requestStream.Close();
        }
      }

      return request;
    }

    public static string GetResponseStr(this HttpWebRequest request)
    {
      string responseBody = string.Empty;

      using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
      {
        if (response.StatusCode == HttpStatusCode.OK)
        {
          using (var responseStream = response.GetResponseStream())
          {
            using (var reader = new StreamReader(responseStream))
            {
              responseBody = reader.ReadToEnd();
            }
          }
        }
      }

      return responseBody;
    }

    public static void GetResponseBytes(this HttpWebRequest request, Stream stream)
    {
      using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
      {
        if (response.StatusCode == HttpStatusCode.OK)
        {
          using (var responseStream = response.GetResponseStream())
          {
            responseStream.CopyTo(stream);
          }
        }
        else
          throw new WebException( string.Format("Response status code={0} description=\"{1}\"", response.StatusCode, response.StatusDescription));
      }
    }

    public static IDictionary<string, string> GetHeaders(this HttpWebRequest request, HttpStatusCode successStatus = HttpStatusCode.OK)
    {
      string responseBody = string.Empty;

      using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
      {
        if (response.StatusCode == successStatus)
        {
          IDictionary<string, string> headers = response.Headers.AllKeys.ToDictionary(k => k, k => response.Headers[k]);
          return headers;
        }

        throw new WebException(Azos.Web.StringConsts.DELETE_MODIFY_ERROR + typeof(S3V4) + ".GetHeaders: folder couldn't be deleted");
      }
    }

  }
}
