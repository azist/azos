/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Azos.Wave
{
  public static class AspUtils
  {

    /// <summary>
    /// Returns the combined components of the request URL in a fully escaped form suitable
    /// for use in HTTP headers and other HTTP operations.
    /// </summary>
    public static string ToEncodedUrl(this HttpRequest request) => UriHelper.GetEncodedUrl(request);

    /// <summary>
    /// Returns the combined components of the request URL in a fully un-escaped form
    /// (except for the QueryString) suitable only for display. This format should not
    /// be used in HTTP headers or other HTTP operations.
    /// </summary>
    public static string ToDisplayUrl(this HttpRequest request) => UriHelper.GetDisplayUrl(request);
  }
}
