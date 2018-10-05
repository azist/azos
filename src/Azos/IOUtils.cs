using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Azos
{

  /// <summary>
  /// Format of the String Dump
  /// </summary>
  public enum DumpFormat
  {
    /// <summary>
    /// Perform no conversion - data copied as is
    /// </summary>
    Binary,

    /// <summary>
    /// Decimal string representation. E.g. "&lt;&lt;39, 16, 25, ...>>"
    /// </summary>
    Decimal,

    /// <summary>
    /// Hex string representation. E.g. "A1 B9 16 ..."
    /// </summary>
    Hex,

    /// <summary>
    /// Human readable string representation. E.g. "...Test 123\n..."
    /// </summary>
    Printable
  }

  /// <summary>
  /// Provides IO-related utility extensions
  /// </summary>
  public static class IOUtils
  {
    /// <summary>
    /// Walks all file names that match the pattern in a directory
    /// </summary>
    public static IEnumerable<string> AllFileNamesThatMatch(this string fromFolder, string pattern, bool recurse)
    {
      return Directory.GetFiles(fromFolder,
                         pattern,
                         recurse ? SearchOption.AllDirectories :
                                   SearchOption.TopDirectoryOnly);
    }

    /// <summary>
    /// Walks all file names in a directory
    /// </summary>
    public static IEnumerable<string> AllFileNames(this string fromFolder, bool recurse)
    {
      return fromFolder.AllFileNamesThatMatch("*.*", recurse);
    }


    /// <summary>
    /// Encodes string with standart UTF8 encoder
    /// </summary>
    public static byte[] ToUTF8Bytes(this string str)
    {
      if (str == null) return null;

      return Encoding.UTF8.GetBytes(str);
    }

    /// <summary>
    /// Decode string with standart UTF8 decoder
    /// </summary>
    public static string FromUTF8Bytes(this byte[] buf, int idx = -1, int cnt = -1)
    {
      if (buf == null) return null;

      return idx >= 0
            ? Encoding.UTF8.GetString(buf, idx, cnt < 0 ? buf.Length - idx : cnt)
            : Encoding.UTF8.GetString(buf);
    }

    /// <summary>
    /// Resolve IP address by Name
    /// </summary>
    /// <param name="epoint">An ip address or DNS host name with optional port separated by ':'</param>
    /// <param name="dfltPort">Port number to use if not supplied in endpoint string</param>
    /// <returns>IPEndPoint instance or null supplied string could not be parsed</returns>
    public static IPEndPoint ToIPEndPoint(this string epoint, int dfltPort = 0)
    {
      if (string.IsNullOrWhiteSpace(epoint)) throw new AzosException(string.Format(StringConsts.INVALID_EPOINT_ERROR, StringConsts.NULL_STRING, "null arg"));

      try
      {
        string[] parts = epoint.Split(':');
        var port = parts.Length > 1 && !parts[1].IsNullOrEmpty() ? int.Parse(parts[1]) : dfltPort;

        // Note that the GetHostEntry("127.0.0.1") call looks up the host entry that
        // may not contain localhost, even though it does do IPAddress.TryParse(epoint) call,
        // So we have no other way but to TryParse it outselves.
        IPAddress address;
        if (parts[0] == "*")
          return new IPEndPoint(IPAddress.Any, port);
        else if (IPAddress.TryParse(parts[0], out address))
          return new IPEndPoint(address, port);

        var hostEntry = Dns.GetHostEntry(parts[0]);

        return new IPEndPoint(hostEntry.AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork), port);
      }
      catch (Exception error)
      {
        throw new AzosException(string.Format(StringConsts.INVALID_EPOINT_ERROR, epoint, error.ToMessageWithType()), error);
      }
    }


    /// <summary>
    /// Generates GUID based on a string MD5 hash
    /// </summary>
    public static Guid ToGUID(this string input)
    {
      using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
        return new Guid(md5.ComputeHash(Encoding.Default.GetBytes(input)));
    }


    /// <summary>
    /// Returns a MD5 hash of a UTF8 string represented as hex string
    /// </summary>
    public static string ToMD5String(this string input)
    {
      if (string.IsNullOrEmpty(input))
        return "00000000000000000000000000000000";

      using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
      {
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

        var result = new StringBuilder();

        for (var i = 0; i < hash.Length; i++)
          result.Append(hash[i].ToString("x2"));

        return result.ToString();
      }
    }


    /// <summary>
    /// Returns a MD5 hash of a UTF8 string represented as hex string
    /// </summary>
    public static string ToMD5String(this byte[] input)
    {
      if (input == null)
        return "00000000000000000000000000000000";

      using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
      {
        var hash = md5.ComputeHash(input);

        var result = new StringBuilder();

        for (var i = 0; i < hash.Length; i++)
          result.Append(hash[i].ToString("x2"));

        return result.ToString();
      }
    }

    /// <summary>
    /// Returns a MD5 hash of a UTF8 string represented as byte[]
    /// </summary>
    public static byte[] ToMD5(this string input)
    {
      if (string.IsNullOrEmpty(input)) return new byte[16];

      using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
      {
        return md5.ComputeHash(Encoding.UTF8.GetBytes(input));
      }
    }

    /// <summary>
    /// Returns a MD5 hash of a byte array
    /// </summary>
    public static byte[] ToMD5(this byte[] input)
    {
      if (input == null) return new byte[16];

      using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
      {
        return md5.ComputeHash(input);
      }
    }


    /// <summary>
    /// Convert a buffer to a printable string
    /// </summary>
    /// <param name="buf">Buffer to convert</param>
    /// <param name="fmt">Dumping format</param>
    /// <param name="offset">Starting index</param>
    /// <param name="count">Number of bytes to process (-1 means all bytes in the buffer)</param>
    /// <param name="eol">If true, terminate with end-of-line</param>
    /// <param name="encoding">Encoding to use for writing data in Binary format</param>
    /// <param name="maxLen">Max length of the returned string. Pass 0 for unlimited length</param>
    /// <returns>String dump</returns>
    public static string ToDumpString(this byte[] buf, DumpFormat fmt, int offset = 0,
        int count = -1, bool eol = false, Encoding encoding = null, int maxLen = 0)
    {
      if (count == -1) count = buf.Length - offset;

      int n;

      switch (fmt)
      {
        case DumpFormat.Decimal: n = count * 4 + 4; break;
        case DumpFormat.Hex: n = count * 3; break;
        case DumpFormat.Printable: n = count * 4; break;
        default: throw new AzosException(StringConsts.OPERATION_NOT_SUPPORTED_ERROR + fmt.ToString());
      }

      var sb = new StringBuilder(n);

      int k = offset + count;
      int m = maxLen > 0 ? Math.Max(2, maxLen) : Int16.MaxValue;
      bool shrink = maxLen > 0;

      switch (fmt)
      {
        case DumpFormat.Decimal:
          m -= 2;
          sb.Append("<<");
          for (int i = offset; i < k && sb.Length + 1/*comma*/ < m; i++)
          {
            sb.Append(buf[i]);
            sb.Append(',');
          }
          if (sb.Length > 2)
            sb.Remove(sb.Length - 1, 1);
          sb.Append(!shrink || sb.Length + 1 < m ? ">>" : "...>>");
          break;
        case DumpFormat.Hex:
          m -= 3;
          for (int i = offset, j = 0; i < k && sb.Length < m; i++, j++)
            sb.AppendFormat("{0:X2}{1}", buf[i], (j & 3) == 3 ? " " : "");
          if (sb.Length > 0 && sb[sb.Length - 1] == ' ')
            sb.Remove(sb.Length - 1, 1);
          if (shrink) sb.Append("...");
          break;
        case DumpFormat.Printable:
          m -= 3;
          for (int i = offset; i < k && sb.Length < m; i++)
          {
            byte c = buf[i];
            if (c >= 32 && c < 127)
              sb.Append((char)c);
            else
              switch (c)
              {
                case 10: sb.Append("\n"); break;
                case 13: sb.Append("\r"); break;
                case 8: sb.Append("\t"); break;
                default: sb.AppendFormat("\\{0,3:D3}", c); break;
              }
          }
          if (shrink) sb.Append("...");
          break;
      }

      if (eol)
        sb.Append('\n');

      return sb.ToString();
    }



    /// <summary>
    /// Runs specified process and waits for termination returning standard process output.
    /// This is a blocking call
    /// </summary>
    public static string RunAndCompleteProcess(string name, string args)
    {
      string std_out = string.Empty;

      Process p = new Process();
      p.StartInfo.FileName = name;
      p.StartInfo.Arguments = args;
      p.StartInfo.UseShellExecute = false;
      p.StartInfo.CreateNoWindow = true;
      p.StartInfo.RedirectStandardOutput = true;
      p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      p.Start();

      try
      {
        std_out = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
      }
      finally
      {
        p.Close();
      }

      return std_out;
    }


  }
}
