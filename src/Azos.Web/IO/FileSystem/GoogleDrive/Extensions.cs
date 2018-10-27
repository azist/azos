/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Globalization;

using Azos.Serialization.JSON;
using Azos.IO.FileSystem.GoogleDrive.V2;

namespace Azos.IO.FileSystem.GoogleDrive
{
  static class Extensions
  {
    public static Uri FormatUri(this string template, params object[] args)
    {
      return new Uri(string.Format(CultureInfo.InvariantCulture, template, args), UriKind.RelativeOrAbsolute);
    }

    public static string GetString(this HttpWebResponse res)
    {
      using (var stream = res.GetResponseStream())
      {
        using (var reader = new StreamReader(stream))
        {
          return reader.ReadToEnd();
        }
      }
    }

    public static dynamic GetJsonAsDynamic(this HttpWebResponse res)
    {
      var str = res.GetString();
      return str.IsNotNullOrEmpty() ? str.JSONToDynamic() : null;
    }

    public static string ToFormEncoded(this GoogleDriveRequestBody data)
    {
      var parameters = data.Select(p => Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value.ToString()));
      return string.Join("&", parameters);
    }

    public static Stream ToStream(this string value)
    {
      return new MemoryStream(Encoding.UTF8.GetBytes(value));
    }
  }
}
