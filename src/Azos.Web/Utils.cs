/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Globalization;


namespace Azos.Web
{
  /// <summary>
  /// Provides various utilities for web technologies
  /// </summary>
  public static class Utils
  {
    /// <summary>
    /// Converts UTC date timne string suitable for use as Cookie expiration filed
    /// </summary>
    public static string DateTimeToHTTPCookieDateTime(this DateTime utcDateTime)
    {
      return utcDateTime.ToString("ddd, dd-MMM-yyyy HH':'mm':'ss 'GMT'", DateTimeFormatInfo.InvariantInfo);
    }

  }
}
