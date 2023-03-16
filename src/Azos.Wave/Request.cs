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
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Azos.Wave
{
  /// <summary>
  /// Facade for working with
  /// </summary>
  public struct Request
  {
    internal Request(HttpRequest request)
    {
      AspRequest = request;
    }

    public readonly HttpRequest AspRequest;
    public string UserAgent => AspRequest.Headers.UserAgent.ToString();
    public string Method => AspRequest.Method;
    public string Scheme => AspRequest.Scheme;

    public string Host => AspRequest.Host.Host;
    public string Referer => AspRequest.Headers.Referer.ToString();
    public string ContentType => AspRequest.ContentType;
    public long ContentLength => AspRequest.ContentLength ?? -1;

    public IHeaderDictionary Headers => AspRequest.Headers;
    public IRequestCookieCollection Cookies => AspRequest.Cookies;

    public string HeaderAsString(string name) => AspRequest.Headers[name];
    public string QueryVarAsString(string name) => AspRequest.Query[name];

    public IQueryCollection Query => AspRequest.Query;

    public Stream BodyStream => AspRequest.Body;


    public PathString Path => AspRequest.Path;

    public string Url => AspRequest.PathBase + AspRequest.Path + AspRequest.QueryString;

    public bool IsLocal
    {
      get
      {
        var connection = AspRequest.HttpContext.Connection;
        if (connection.RemoteIpAddress.IsSpecified())
        {
          return connection.LocalIpAddress.IsSpecified()
              //Is local is same as remote, then we are local
              ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
              //else we are remote if the remote IP address is not a loopback address
              : IPAddress.IsLoopback(connection.RemoteIpAddress);
        }
        return true;
      }
    }

    public bool RequestedJson
    {
      get
      {
        var result = AspRequest.Headers.Accept.Any(at => at != null && at.IndexOf(Azos.Web.ContentType.JSON, StringComparison.OrdinalIgnoreCase) != -1);

        return result;
      }
    }
  }
}