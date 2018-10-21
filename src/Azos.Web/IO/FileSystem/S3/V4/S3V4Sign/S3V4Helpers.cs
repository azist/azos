
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Azos.IO.FileSystem.S3.V4.S3V4Sign
{
  /// <summary>
  /// Implements helpers methods for Amazon S3
  /// </summary>
  public static class S3V4Helpers
  {
    /// <summary>
    /// Returns absolute path of S3 Uri
    /// </summary>
    public static string GetS3AbsoolutePath(this Uri uri)
    {
      return uri.AbsolutePath;
    }

    /// <summary>
    /// Converts byte array into appropriate string hexadecimal representation
    /// </summary>
    public static string ToHexString(this byte[] bytes, bool lowerCase = true)
    {
      string format = lowerCase ? "x2" : "X2";
      string r = string.Join(string.Empty, bytes.Select(b => b.ToString(format)));
      return r;
    }

    /// <summary>
    /// Converts DateTime into appropriate S3 date string representation
    /// </summary>
    public static string S3DateString(this DateTime date)
    {
      return date.ToString(DATESTRING_FORMAT, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts DateTime into appropriate S3 datetime string representation
    /// </summary>
    public static string S3DateTimeString(this DateTime date)
    {
      return date.ToString(ISO8601_DATESTRING_FORMAT, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts string into UTF8 byte array encoded representation
    /// </summary>
    public static byte[] S3Bytes(this string str)
    {
      return Encoding.UTF8.GetBytes(str);
    }

    public const string DATESTRING_FORMAT = "yyyyMMdd";
    public const string ISO8601_DATESTRING_FORMAT = "yyyyMMddTHHmmssZ";

  }
}
