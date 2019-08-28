/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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

    private const int FS_IO_WAIT_GRANULARITY_MS = 200;
    private const int FS_IO_WAIT_MIN_TIMEOUT = 100;
    private const int FS_IO_WAIT_DEFAULT_TIMEOUT = 2000;

    private const int CHAR_BUFFER_SZ = 4 * 1024;

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
    /// Encodes string with standard UTF8 encoder
    /// </summary>
    public static byte[] ToUTF8Bytes(this string str)
    {
      if (str == null) return null;

      return Encoding.UTF8.GetBytes(str);
    }

    /// <summary>
    /// Decode string with standard UTF8 decoder
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
      if (string.IsNullOrWhiteSpace(epoint)) throw new AzosException(string.Format(StringConsts.INVALID_EPOINT_ERROR, CoreConsts.NULL_STRING, "null arg"));

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
    /// Returns a MD5 hash of a byte[]represented as hex string
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
    /// Returns a MD5 hash of a stream represented as hex string
    /// </summary>
    public static string ToMD5String(this Stream input)
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
    /// Returns a MD5 hash of a stream
    /// </summary>
    public static byte[] ToMD5(this Stream input)
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


    /// <summary>
    /// Reads first line from the string
    /// </summary>
    public static string ReadLine(this string str)
    {
      StringBuilder buf = new StringBuilder();

      for (int i = 0; i < str.Length; i++)
        if ((str[i] == '\n') || (str[i] == '\r'))
          break;
        else
          buf.Append(str[i]);

      return buf.ToString();
    }

    [ThreadStatic] private static byte[] ts_CopyBuffer;
    /// <summary>
    /// Copies one stream into another using temp buffer
    /// </summary>
    public static void CopyStream(Stream from, Stream to, bool noCache = false)
    {
      if (ts_CopyBuffer == null)
      {
        var sz = Ambient.MemoryModel < Ambient.MemoryUtilizationModel.Regular ? 8 : 128;
        ts_CopyBuffer = new byte[sz * 1024];
      }
      int read;
      while ((read = from.Read(ts_CopyBuffer, 0, ts_CopyBuffer.Length)) > 0)
        to.Write(ts_CopyBuffer, 0, read);

      if (noCache)
        ts_CopyBuffer = null;
    }

    /// <summary>
    /// Reads an integer encoded as big endian from buffer at index 0
    /// </summary>
    public static int ReadBEInt32(this byte[] buf)
    {
      int n = 0;
      return ReadBEInt32(buf, ref n);
    }

    /// <summary>
    /// Reads an integer encoded as little endian from buffer at index 0
    /// </summary>
    public static int ReadLEInt32(this byte[] buf)
    {
      int n = 0;
      return ReadLEInt32(buf, ref n);
    }

    /// <summary>
    /// Reads an unsigned integer encoded as big endian from buffer at index 0
    /// </summary>
    public static uint ReadBEUInt32(this byte[] buf)
    {
      int n = 0;
      return ReadBEUInt32(buf, ref n);
    }

    /// <summary>
    /// Reads an unsigned integer encoded as little endian from buffer at index 0
    /// </summary>
    public static uint ReadLEUInt32(this byte[] buf)
    {
      int n = 0;
      return ReadLEUInt32(buf, ref n);
    }

    /// <summary>
    /// Reads an integer encoded as big endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static int ReadBEInt32(this byte[] buf, ref long idx)
    {
      int n = (int)idx;
      int result = ReadBEInt32(buf, ref n);
      idx = n;
      return result;
    }

    /// <summary>
    /// Reads an integer encoded as little endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static int ReadLEInt32(this byte[] buf, ref long idx)
    {
      int n = (int)idx;
      int result = ReadLEInt32(buf, ref n);
      idx = n;
      return result;
    }

    /// <summary>
    /// Reads an integer encoded as big endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static int ReadBEInt32(this byte[] buf, ref int idx)
    {
      return ((int)buf[idx++] << 24) +
              ((int)buf[idx++] << 16) +
              ((int)buf[idx++] << 8) +
               (int)buf[idx++];
    }

    /// <summary>
    /// Reads an integer encoded as little endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static int ReadLEInt32(this byte[] buf, ref int idx)
    {
      return (int)buf[idx++] +
              ((int)buf[idx++] << 8) +
              ((int)buf[idx++] << 16) +
              ((int)buf[idx++] << 24);
    }

    /// <summary>
    /// Reads an unsigned integer encoded as big endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static uint ReadBEUInt32(this byte[] buf, ref int idx)
    {
      return ((uint)buf[idx++] << 24) +
              ((uint)buf[idx++] << 16) +
              ((uint)buf[idx++] << 8) +
               (uint)buf[idx++];
    }

    /// <summary>
    /// Reads an unsigned integer encoded as little endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static uint ReadLEUInt32(this byte[] buf, ref int idx)
    {
      return (uint)buf[idx++] +
              ((uint)buf[idx++] << 8) +
              ((uint)buf[idx++] << 16) +
              ((uint)buf[idx++] << 24);
    }

    /// <summary>
    /// Reads an unsigned integer encoded as big endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static uint ReadBEUInt32(this byte[] buf, ref long idx)
    {
      return ((uint)buf[idx++] << 24) +
              ((uint)buf[idx++] << 16) +
              ((uint)buf[idx++] << 8) +
               (uint)buf[idx++];
    }

    /// <summary>
    /// Reads an unsigned integer encoded as little endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static uint ReadLEUInt32(this byte[] buf, ref long idx)
    {
      return (uint)buf[idx++] +
              ((uint)buf[idx++] << 8) +
              ((uint)buf[idx++] << 16) +
              ((uint)buf[idx++] << 24);
    }

    /// <summary>
    /// Reads an integer encoded as big endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static UInt64 ReadBEUInt64(this byte[] buf, ref int idx)
    {
      return ((ulong)buf[idx++] << 56) +
             ((ulong)buf[idx++] << 48) +
             ((ulong)buf[idx++] << 40) +
             ((ulong)buf[idx++] << 32) +
             ((ulong)buf[idx++] << 24) +
             ((ulong)buf[idx++] << 16) +
             ((ulong)buf[idx++] << 8) +
             ((ulong)buf[idx++]);
    }

    /// <summary>
    /// Reads an integer encoded as little endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static UInt64 ReadLEUInt64(this byte[] buf, ref int idx)
    {
      return ((ulong)buf[idx++]) +
             ((ulong)buf[idx++] << 8) +
             ((ulong)buf[idx++] << 16) +
             ((ulong)buf[idx++] << 24) +
             ((ulong)buf[idx++] << 32) +
             ((ulong)buf[idx++] << 40) +
             ((ulong)buf[idx++] << 48) +
             ((ulong)buf[idx++] << 56);
    }

    /// <summary>
    /// Reads an integer encoded as big endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static UInt64 ReadBEUInt64(this byte[] buf, int idx = 0)
    {
      return ((ulong)buf[idx++] << 56) +
             ((ulong)buf[idx++] << 48) +
             ((ulong)buf[idx++] << 40) +
             ((ulong)buf[idx++] << 32) +
             ((ulong)buf[idx++] << 24) +
             ((ulong)buf[idx++] << 16) +
             ((ulong)buf[idx++] << 8) +
             ((ulong)buf[idx++]);
    }

    /// <summary>
    /// Reads an integer encoded as little endian from buffer at the specified index
    /// and increments the idx by the number of bytes read
    /// </summary>
    public static UInt64 ReadLEUInt64(this byte[] buf, int idx = 0)
    {
      return ((ulong)buf[idx++]) +
             ((ulong)buf[idx++] << 8) +
             ((ulong)buf[idx++] << 16) +
             ((ulong)buf[idx++] << 24) +
             ((ulong)buf[idx++] << 32) +
             ((ulong)buf[idx++] << 40) +
             ((ulong)buf[idx++] << 48) +
             ((ulong)buf[idx++] << 56);
    }

    /// <summary>
    /// Reads a short encoded as big endian from buffer at the specified index
    /// </summary>
    public static short ReadBEShort(this byte[] buf, ref int idx)
    {
      return (short)(
                   (((int)buf[idx++] << 8) & 0xff00) +
                   (((int)buf[idx++]) & 0xff)
                  );
    }

    /// <summary>
    /// Reads a short encoded as little endian from buffer at the specified index
    /// </summary>
    public static short ReadLEShort(this byte[] buf, ref int idx)
    {
      return (short)(
                   (((int)buf[idx++]) & 0xff) +
                   (((int)buf[idx++] << 8) & 0xff00)
                  );
    }


    /// <summary>
    /// Reads a short encoded as big endian from stream
    /// </summary>
    public static short ReadBEShort(this Stream s)
    {
      var b1 = s.ReadByte();
      if (b1 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEShort()");

      var b2 = s.ReadByte();
      if (b2 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEShort()");

      return (short)(
                   (b1 << 8) +
                   (b2)
                  );
    }

    /// <summary>
    /// Reads a short encoded as little endian from stream
    /// </summary>
    public static short ReadLEShort(this Stream s)
    {
      var b1 = s.ReadByte();
      if (b1 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEShort()");

      var b2 = s.ReadByte();
      if (b2 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEShort()");

      return (short)((b2 << 8) + b1);
    }

    /// <summary>
    /// Reads an ushort encoded as big endian from stream
    /// </summary>
    public static ushort ReadBEUShort(this Stream s)
    {
      var b1 = s.ReadByte();
      if (b1 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEUShort()");

      var b2 = s.ReadByte();
      if (b2 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEUShort()");

      return (ushort)(
                   (b1 << 8) +
                   (b2)
                  );
    }

    /// <summary>
    /// Reads an ushort encoded as little endian from stream
    /// </summary>
    public static ushort ReadLEUShort(this Stream s)
    {
      var b1 = s.ReadByte();
      if (b1 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEUShort()");

      var b2 = s.ReadByte();
      if (b2 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEUShort()");

      return (ushort)((b2 << 8) + b1);
    }

    /// <summary>
    /// Reads an integer encoded as big endian from buffer at the specified index
    /// </summary>
    public static Int32 ReadBEInt32(this Stream s)
    {
      var b1 = s.ReadByte();
      if (b1 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEInt32()");

      var b2 = s.ReadByte();
      if (b2 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEInt32()");

      var b3 = s.ReadByte();
      if (b3 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEInt32()");

      var b4 = s.ReadByte();
      if (b4 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEInt32()");

      return (b1 << 24) +
             (b2 << 16) +
             (b3 << 8) +
             (b4);
    }

    /// <summary>
    /// Reads an integer encoded as little endian from buffer at the specified index
    /// </summary>
    public static int ReadLEInt32(this Stream s)
    {
      var b1 = s.ReadByte();
      if (b1 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEInt32()");

      var b2 = s.ReadByte();
      if (b2 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEInt32()");

      var b3 = s.ReadByte();
      if (b3 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEInt32()");

      var b4 = s.ReadByte();
      if (b4 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEInt32()");

      return (b4 << 24) +
             (b3 << 16) +
             (b2 << 8) +
             (b1);
    }

    /// <summary>
    /// Reads an integer encoded as big endian from buffer at the specified index
    /// </summary>
    public static ulong ReadBEUInt64(this Stream s)
    {
      var b1 = s.ReadByte();
      if (b1 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEUInt64()");

      var b2 = s.ReadByte();
      if (b2 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEUInt64()");

      var b3 = s.ReadByte();
      if (b3 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEUInt64()");

      var b4 = s.ReadByte();
      if (b4 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEUInt64()");

      var b5 = s.ReadByte();
      if (b5 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEUInt64()");

      var b6 = s.ReadByte();
      if (b6 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEUInt64()");

      var b7 = s.ReadByte();
      if (b7 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEUInt64()");

      var b8 = s.ReadByte();
      if (b8 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadBEUInt64()");



      return ((UInt64)b1 << 56) +
             ((UInt64)b2 << 48) +
             ((UInt64)b3 << 40) +
             ((UInt64)b4 << 32) +
             ((UInt64)b5 << 24) +
             ((UInt64)b6 << 16) +
             ((UInt64)b7 << 8) +
             ((UInt64)b8);
    }

    /// <summary>
    /// Reads an integer encoded as little endian from buffer at the specified index
    /// </summary>
    public static ulong ReadLEUInt64(this Stream s)
    {
      var b1 = s.ReadByte();
      if (b1 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEUInt64()");

      var b2 = s.ReadByte();
      if (b2 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEUInt64()");

      var b3 = s.ReadByte();
      if (b3 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEUInt64()");

      var b4 = s.ReadByte();
      if (b4 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEUInt64()");

      var b5 = s.ReadByte();
      if (b5 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEUInt64()");

      var b6 = s.ReadByte();
      if (b6 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEUInt64()");

      var b7 = s.ReadByte();
      if (b7 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEUInt64()");

      var b8 = s.ReadByte();
      if (b8 < 0) throw new IO.AzosIOException(StringConsts.STREAM_READ_EOF_ERROR + "ReadLEUInt64()");

      return ((ulong)b8 << 56) +
             ((ulong)b7 << 48) +
             ((ulong)b6 << 40) +
             ((ulong)b5 << 32) +
             ((ulong)b4 << 24) +
             ((ulong)b3 << 16) +
             ((ulong)b2 << 8) +
             ((ulong)b1);
    }


    /// <summary>
    /// Writes an integer encoded as big endian to buffer at index 0
    /// </summary>
    public static void WriteBEInt32(this byte[] buf, Int32 value)
    {
      WriteBEInt32(buf, 0, value);
    }

    /// <summary>
    /// Writes an integer encoded as little endian to buffer at index 0
    /// </summary>
    public static void WriteLEInt32(this byte[] buf, Int32 value)
    {
      WriteLEInt32(buf, 0, value);
    }

    /// <summary>
    /// Writes an integer encoded as big endian to buffer at the specified index
    /// </summary>
    public static void WriteBEInt32(this byte[] buf, int idx, Int32 value)
    {
      buf[idx + 0] = (byte)(value >> 24);
      buf[idx + 1] = (byte)(value >> 16);
      buf[idx + 2] = (byte)(value >> 8);
      buf[idx + 3] = (byte)(value);
    }

    /// <summary>
    /// Writes an integer encoded as little endian to buffer at the specified index
    /// </summary>
    public static void WriteLEInt32(this byte[] buf, int idx, Int32 value)
    {
      buf[idx + 0] = (byte)(value);
      buf[idx + 1] = (byte)(value >> 8);
      buf[idx + 2] = (byte)(value >> 16);
      buf[idx + 3] = (byte)(value >> 24);
    }

    /// <summary>
    /// Writes an unsigned integer encoded as big endian to buffer at index 0
    /// </summary>
    public static void WriteBEUInt32(this byte[] buf, UInt32 value)
    {
      WriteBEUInt32(buf, 0, value);
    }

    /// <summary>
    /// Writes an unsigned integer encoded as little endian to buffer at index 0
    /// </summary>
    public static void WriteLEUInt32(this byte[] buf, UInt32 value)
    {
      WriteLEUInt32(buf, 0, value);
    }

    /// <summary>
    /// Writes an unsigned integer encoded as big endian to buffer at the specified index
    /// </summary>
    public static void WriteBEUInt32(this byte[] buf, int idx, UInt32 value)
    {
      buf[idx + 0] = (byte)(value >> 24);
      buf[idx + 1] = (byte)(value >> 16);
      buf[idx + 2] = (byte)(value >> 8);
      buf[idx + 3] = (byte)(value);
    }

    /// <summary>
    /// Writes an unsigned integer encoded as little endian to buffer at the specified index
    /// </summary>
    public static void WriteLEUInt32(this byte[] buf, int idx, UInt32 value)
    {
      buf[idx + 0] = (byte)(value);
      buf[idx + 1] = (byte)(value >> 8);
      buf[idx + 2] = (byte)(value >> 16);
      buf[idx + 3] = (byte)(value >> 24);
    }

    /// <summary>
    /// Writes an unsigned long integer encoded as big endian to buffer at the beginning
    /// </summary>
    public static void WriteBEUInt64(this byte[] buf, UInt64 value)
    {
      buf.WriteBEUInt64(0, value);
    }

    /// <summary>
    /// Writes an unsigned long integer encoded as little endian to buffer at the beginning
    /// </summary>
    public static void WriteLEUInt64(this byte[] buf, UInt64 value)
    {
      buf.WriteLEUInt64(0, value);
    }

    /// <summary>
    /// Writes an unsigned long integer encoded as big endian to buffer at the specified index
    /// </summary>
    public static void WriteBEUInt64(this byte[] buf, int idx, UInt64 value)
    {
      buf[idx + 0] = (byte)(value >> 56);
      buf[idx + 1] = (byte)(value >> 48);
      buf[idx + 2] = (byte)(value >> 40);
      buf[idx + 3] = (byte)(value >> 32);
      buf[idx + 4] = (byte)(value >> 24);
      buf[idx + 5] = (byte)(value >> 16);
      buf[idx + 6] = (byte)(value >> 8);
      buf[idx + 7] = (byte)(value);
    }

    /// <summary>
    /// Writes an unsigned long integer encoded as little endian to buffer at the specified index
    /// </summary>
    public static void WriteLEUInt64(this byte[] buf, int idx, UInt64 value)
    {
      buf[idx + 0] = (byte)(value);
      buf[idx + 1] = (byte)(value >> 8);
      buf[idx + 2] = (byte)(value >> 16);
      buf[idx + 3] = (byte)(value >> 24);
      buf[idx + 4] = (byte)(value >> 32);
      buf[idx + 5] = (byte)(value >> 40);
      buf[idx + 6] = (byte)(value >> 48);
      buf[idx + 7] = (byte)(value >> 56);
    }

    /// <summary>
    /// Writes a short encoded as big endian to buffer at the specified index
    /// </summary>
    public static void WriteBEShort(this byte[] buf, int idx, short value)
    {
      buf[idx + 0] = (byte)(value >> 8);
      buf[idx + 1] = (byte)(value);
    }

    /// <summary>
    /// Writes a short encoded as little endian to buffer at the specified index
    /// </summary>
    public static void WriteLEShort(this byte[] buf, int idx, short value)
    {
      buf[idx + 0] = (byte)(value);
      buf[idx + 1] = (byte)(value >> 8);
    }


    /// <summary>
    /// Writes a short encoded as big endian to the given stream
    /// </summary>
    public static void WriteBEShort(this Stream s, short value)
    {
      s.WriteByte((byte)(value >> 8));
      s.WriteByte((byte)value);
    }

    /// <summary>
    /// Writes a short encoded as little endian to the given stream
    /// </summary>
    public static void WriteLEShort(this Stream s, short value)
    {
      s.WriteByte((byte)value);
      s.WriteByte((byte)(value >> 8));
    }

    /// <summary>
    /// Writes an ushort encoded as big endian to the given stream
    /// </summary>
    public static void WriteBEUShort(this Stream s, ushort value)
    {
      s.WriteByte((byte)(value >> 8));
      s.WriteByte((byte)value);
    }

    /// <summary>
    /// Writes an ushort encoded as little endian to the given stream
    /// </summary>
    public static void WriteLEUShort(this Stream s, ushort value)
    {
      s.WriteByte((byte)value);
      s.WriteByte((byte)(value >> 8));
    }

    /// <summary>
    /// Writes an integer encoded as big endian to the given stream
    /// </summary>
    public static void WriteBEInt32(this Stream s, Int32 value)
    {
      s.WriteByte((byte)(value >> 24));
      s.WriteByte((byte)(value >> 16));
      s.WriteByte((byte)(value >> 8));
      s.WriteByte((byte)(value));
    }

    /// <summary>
    /// Writes an integer encoded as little endian to the given stream
    /// </summary>
    public static void WriteLEInt32(this Stream s, int value)
    {
      s.WriteByte((byte)(value));
      s.WriteByte((byte)(value >> 8));
      s.WriteByte((byte)(value >> 16));
      s.WriteByte((byte)(value >> 24));
    }

    /// <summary>
    /// Writes an unsigned long integer encoded as big endian to the given stream
    /// </summary>
    public static void WriteBEUInt64(this Stream s, UInt64 value)
    {
      s.WriteByte((byte)(value >> 56));
      s.WriteByte((byte)(value >> 48));
      s.WriteByte((byte)(value >> 40));
      s.WriteByte((byte)(value >> 32));
      s.WriteByte((byte)(value >> 24));
      s.WriteByte((byte)(value >> 16));
      s.WriteByte((byte)(value >> 8));
      s.WriteByte((byte)(value));
    }

    /// <summary>
    /// Writes an unsigned long integer encoded as little endian to the given stream
    /// </summary>
    public static void WriteLEUInt64(this Stream s, ulong value)
    {
      s.WriteByte((byte)(value));
      s.WriteByte((byte)(value >> 8));
      s.WriteByte((byte)(value >> 16));
      s.WriteByte((byte)(value >> 24));
      s.WriteByte((byte)(value >> 32));
      s.WriteByte((byte)(value >> 40));
      s.WriteByte((byte)(value >> 48));
      s.WriteByte((byte)(value >> 56));
    }

    /// <summary>
    /// Treats Stream contents as a enumerable of chars
    /// </summary>
    public static IEnumerable<char> AsCharEnumerable(this Stream stream)
    {
      using (var reader = new StreamReader(stream))
      {
        char[] chars = new char[CHAR_BUFFER_SZ];
        int length;
        while ((length = reader.Read(chars, 0, chars.Length)) != 0)
          for (int i = 0; i < length; i++)
            yield return chars[i];
      }
    }

    /// <summary>
    /// Treats TextReader contents as a enumerable of chars
    /// </summary>
    public static IEnumerable<char> AsCharEnumerable(this TextReader reader)
    {
      char[] chars = new char[CHAR_BUFFER_SZ];
      int length;
      while ((length = reader.Read(chars, 0, chars.Length)) != 0)
        for (int i = 0; i < length; i++)
          yield return chars[i];
    }


    /// <summary>
    /// Deleted file if it exists - does not block until file is deleted, the behavior is up to the OS
    /// </summary>
    /// <param name="fileName">Full file name with path</param>
    public static void EnsureFileEventuallyDeleted(string fileName)
    {
      if (File.Exists(fileName))
        File.Delete(fileName);
    }

    /// <summary>
    /// Tries to delete the specified directory if it exists BLOCKING for up to the specified interval until directory is PHYSICALLY deleted.
    /// Returns true when directory either did not exist in the first place or was successfully deleted (with confirmation).
    /// Returns false when directory could not be confirmed to be deleted within the specified timeout, this does not mean
    ///  that the OS will not delete the directory later, so calling this function in a loop is expected.
    ///  NOTE: Directory.Delete() does not guarantee that directory is no longer on disk upon its return
    /// </summary>
    /// <param name="dirName">Directory to delete</param>
    /// <param name="timeoutMs">Timeout in ms</param>
    public static bool EnsureDirectoryDeleted(string dirName, int timeoutMs = FS_IO_WAIT_DEFAULT_TIMEOUT)
    {
      if (dirName.IsNullOrWhiteSpace()) return false;
      if (timeoutMs < FS_IO_WAIT_MIN_TIMEOUT) timeoutMs = FS_IO_WAIT_MIN_TIMEOUT;


      if (!Directory.Exists(dirName)) return true;

      Directory.Delete(dirName, true);//MARKS directory for deletion, but does not delete it

      var sw = System.Diagnostics.Stopwatch.StartNew();
      while (sw.ElapsedMilliseconds < timeoutMs)
      {
        if (!Directory.Exists(dirName)) return true;//actual check for physical presence on disk
        System.Threading.Thread.Sleep(FS_IO_WAIT_GRANULARITY_MS);
      }
      return false;
    }

    /// <summary>
    /// Creates directory and immediately grants it accessibility rules for everyone if it does not exists,
    ///  or returns the existing directory
    /// </summary>
    public static DirectoryInfo EnsureAccessibleDirectory(string path)
    {
      return Azos.Platform.Abstraction.PlatformAbstractionLayer.FileSystem.EnsureAccessibleDirectory(path);
    }

    /// <summary>
    /// Returns true if both buffers contain the same number of the same bytes.
    /// The implementation uses quad-word comparison as much as possible for speed.
    /// Requires UNSAFE switch
    /// </summary>
    public static unsafe bool MemBufferEquals(this byte[] buf1, byte[] buf2)
    {
      if (buf1 == null || buf2 == null) return false;

      var len = buf1.Length;
      if (len != buf2.Length) return false;

      var len8 = len >> 3;

      fixed (byte* pb1 = buf1, pb2 = buf2)
      {
        byte* p1 = pb1, p2 = pb2;

        for (int i = 0; i < len8; i++, p1 += 8, p2 += 8)
          if (*((long*)p1) != *((long*)p2)) return false;

        //remainders after loop

        if ((len & 4) != 0)
        {
          if (*((int*)p1) != *((int*)p2)) return false;
          p1 += 4; p2 += 4;
        }

        if ((len & 2) != 0)
        {
          if (*((short*)p1) != *((short*)p2)) return false;
          p1 += 2; p2 += 2;
        }

        if ((len & 1) != 0)
          if (*((byte*)p1) != *((byte*)p2)) return false;

        return true;
      }//fixed
    }

    /// <summary>
    /// Represents an ISO code as 4 byte integer filled with
    /// up to 3 ASCII chars converted to upper case, the highest byte is free to be used by the application
    /// </summary>
    public static int PackISO3CodeToInt(string iso)
    {
      if (iso.IsNullOrWhiteSpace()) return 0;

      var l = iso.Length;

      if (l > 3)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + "PackISO3CodeToInt(iso>3)");


      //note: ISO codes are in ASCII plane
      var isoChar0 = (byte)iso[0];
      var isoChar1 = l > 1 ? (byte)iso[1] : (byte)0x00;
      var isoChar2 = l > 2 ? (byte)iso[2] : (byte)0x00;

      if (isoChar0 > 0x60) isoChar0 -= 0x20;//convert to upper case
      if (isoChar1 > 0x60) isoChar1 -= 0x20;//convert to upper case
      if (isoChar2 > 0x60) isoChar2 -= 0x20;//convert to upper case

      var result = (isoChar2 << 16) + (isoChar1 << 8) + isoChar0;
      return result;
    }

    /// <summary>
    /// Unpacks an ISO code from int which was packed with PackISO3CodeToInt
    /// </summary>
    public static unsafe string UnpackISO3CodeFromInt(int iso)
    {
      if (iso == 0) return null;

      char* buf = stackalloc char[3];
      char* pc = buf;

      var c = (char)(iso & 0xff);
      if (c != 0) *pc++ = c;

      c = (char)((iso >> 8) & 0xff);
      if (c != 0) *pc++ = c;

      c = (char)((iso >> 16) & 0xff);
      if (c != 0) *pc++ = c;

      return new string(buf);
    }

    /// <summary>
    /// Encodes GUID using predictable Big Endian network byte ordering, use with complementing GuidFromNetworkByteOrder(byte[]) method
    /// </summary>
    public static byte[] ToNetworkByteOrder(this Guid guid)
    {
      var result = guid.ToByteArray();

      var t = result[3];
      result[3] = result[0];
      result[0] = t;

      t = result[2];
      result[2] = result[1];
      result[1] = t;

      t = result[5];
      result[5] = result[4];
      result[4] = t;

      t = result[7];
      result[7] = result[6];
      result[6] = t;

      return result;
    }

    /// <summary>
    /// Reads GUID using predictable Big Endian network byte ordering, use with complementing ToNetworkByteOrder(guid) method
    /// </summary>
    public static Guid GuidFromNetworkByteOrder(this byte[] buf, int offset = 0)
    {
      var a = ReadBEInt32(buf, ref offset);
      var b = ReadBEShort(buf, ref offset);
      var c = ReadBEShort(buf, ref offset);

      return new Guid(a, b, c,
                      buf[offset++],
                      buf[offset++],
                      buf[offset++],
                      buf[offset++],
                      buf[offset++],
                      buf[offset++],
                      buf[offset++],
                      buf[offset]);
    }

    /// <summary>
    /// Represents byte[] as a web-safe string, replacing `+` with `-` and `/` with `_`
    /// so the value may be included in URI without any extra encoding. Returns null for null buffer
    /// </summary>
    public static string ToWebSafeBase64(this byte[] buf) => ToWebSafeBase64(new ArraySegment<byte>(buf));

    /// <summary>
    /// Represents a segment of byte[] as a web-safe string, replacing `+` with `-` and `/` with `_`
    /// so the value may be included in URI without any extra encoding. Returns null for null buffer
    /// </summary>
    public static unsafe string ToWebSafeBase64(this ArraySegment<byte> buf)
    {
      if (buf == null) return null;
      var str = Convert.ToBase64String(buf.Array, buf.Offset, buf.Count,  Base64FormattingOptions.None);
      char* chars = stackalloc char[str.Length];
      var cnt = 0;
      for(var i=0; i<str.Length; i++)
      {
        var c = str[i];
        if (c=='=') break;
        if (c=='+') c = '-';
        else if (c=='/') c = '_';
        chars[i] = c;
        cnt++;
      }

      return new string(chars, 0, cnt);
    }

    /// <summary>
    /// Complementing method for ToWebSafeBase64() - reads web-safe base64 encoded string into a byte[].
    /// Returns null for empty string.
    /// Web-safe encoding uses `-` instead of base64 `+` and `_` instead of base64 `/`
    /// </summary>
    public static byte[] FromWebSafeBase64(this string content)
    {
      if (content.IsNullOrWhiteSpace()) return null;

      var cl = content.Length;
      var pl = cl % 4;
      if (pl==2) pl = 2;
      else if (pl==3) pl = 1;
      else pl = 0;

      var chars = new char[cl + pl];
      for (var i = 0; i < chars.Length; i++)
      {
        if (i<content.Length)
        {
          var c = content[i];
          if (c == '-') c = '+';
          else if (c == '_') c = '/';
          chars[i] = c;
        }
        else
          chars[i] = '=';
      }

      return Convert.FromBase64CharArray(chars, 0, chars.Length);
    }

  }
}
