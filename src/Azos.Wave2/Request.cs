/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public string ContentType => AspRequest.ContentType;

    public Stream BodyStream => AspRequest.Body;

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