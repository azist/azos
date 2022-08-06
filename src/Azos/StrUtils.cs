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
using System.Threading;
using System.Threading.Tasks;

namespace Azos
{
  /// <summary>
  /// Provides core string utility functions used by the majority of projects
  /// </summary>
  public static class StrUtils
  {
    public static readonly string[] WIN_UNIX_LINE_BRAKES = new string[] { "\r\n", "\n" };

    /// <summary>
    /// Shortcut helper for string.Format(tpl, params object[] args)
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string Args(this string tpl, params object[] args)
    {
      return string.Format(tpl, args);
    }

    /// <summary>
    /// Interprets template of the form:  Some text {@value_name@:C} by replacing with property/field values.
    /// Note: this function does not recognize escapes for simplicity (as escapes can be replaced by regular strings instead)
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string ArgsTpl(this string tpl, object args)
    {
      bool matched;
      return tpl.ArgsTpl(args, out matched);
    }


    /// <summary>
    /// Interprets template of the form:  Some text {@value_name@:C} by replacing with property/field values.
    /// Note: this function does not recognize escapes for simplicity (as escapes can be replaced by regular strings instead).
    /// Matched is set to true if at least one property match was made
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string ArgsTpl(this string tpl, object args, out bool matched)
    {
      matched = false;
      if (tpl == null || args == null) return null;
      var result = tpl;
      var t = args.GetType();
      var lst = new List<object>();
      foreach (var pi in t.GetProperties())
      {
        var val = pi.GetValue(args);
        if (val == null) val = string.Empty;
        lst.Add(val);

        var replacedResult = result.Replace('@' + pi.Name + '@', (lst.Count - 1).ToString());

        if (!string.Equals(result, replacedResult, StringComparison.Ordinal))
          matched = true;

        result = replacedResult;
      }
      return result.Args(lst.ToArray());
    }


    /// <summary>
    /// Capitalizes first character of string
    /// </summary>
    public static string CapitalizeFirstChar(this string str)
    {
      if (string.IsNullOrEmpty(str)) return string.Empty;

      char[] arr = str.ToCharArray();
      arr[0] = char.ToUpper(arr[0]);

      return new String(arr);
    }

    /// <summary>
    /// Splits string into lines using Win or .nix line brakes
    /// </summary>
    public static string[] SplitLines(this string str)
    {
      return str.Split(WIN_UNIX_LINE_BRAKES, StringSplitOptions.None);
    }



    /// <summary>
    /// Takes the first X chars from a string. If string is null returns null. If string does not have enough
    /// the function returns what the string has
    /// </summary>
    public static string TakeFirstChars(this string str, int count, string ellipsis = null)
    {
      if (str == null) return null;
      if (count<=0) return string.Empty;
      if (str.Length <= count) return str;
      ellipsis = ellipsis ?? string.Empty;

      if (count > ellipsis.Length)
        count -= ellipsis.Length;
      else
        return ellipsis.Substring(0, count);

      return str.Substring(0, count) + ellipsis;
    }

    /// <summary>
    /// Takes the last X chars from a string. If string is null returns null. If string does not have enough
    /// the function returns what the string has
    /// </summary>
    public static string TakeLastChars(this string str, int count, string ellipsis = null)
    {
      if (str == null) return null;
      if (count <= 0) return string.Empty;
      if (str.Length <= count) return str;
      ellipsis = ellipsis ?? string.Empty;

      if (count > ellipsis.Length)
        count -= ellipsis.Length;
      else
        return ellipsis.Substring(0, count);

      return ellipsis + str.Substring(str.Length-count);
    }

    /// <summary>
    /// Takes the last string segment delimited by the div character. If string is null returns null
    /// </summary>
    public static string TakeLastSegment(this string str, char div)
    {
      if (str == null) return null;
      var i = str.LastIndexOf(div);
      if (i >= 0 && i < str.Length) return str.Substring(i+1);
      return str;
    }

    /// <summary>
    /// Helper function that performs GetHashcode for string using invariant comparison.
    /// Use this in conjunction with EqualsSenseCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int GetHashCodeSenseCase(this string str)
    {
      return StringComparer.InvariantCulture.GetHashCode(str);
    }

    /// <summary>
    /// Helper function that performs comparison between strings using invariant comparison.
    /// Either lhs and rhs can be null.
    /// Use this in conjunction with GetHashCodeSenseCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool EqualsSenseCase(this string lhs, string rhs)
    {
      return StringComparer.InvariantCulture.Equals(lhs, rhs);
    }


    /// <summary>
    /// Helper function that performs case-insensitive GetHashcode for string using invariant comparison.
    /// Use this in conjunction with EqualsIgnoreCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int GetHashCodeIgnoreCase(this string str)
    {
      return StringComparer.InvariantCultureIgnoreCase.GetHashCode(str);
    }

    /// <summary>
    /// Helper function that performs case-insensitive comparison between strings using invariant comparison.
    /// Either lhs and rhs can be null.
    /// Use this in conjunction with GetHashCodeIgnoreCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCase(this string lhs, string rhs)
    {
      return StringComparer.InvariantCultureIgnoreCase.Equals(lhs, rhs);
    }


    /// <summary>
    /// Helper function that performs case-insensitive GetHashcode for string using ordinal comparison.
    /// Use this in conjunction with EqualsOrdSenseCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int GetHashCodeOrdSenseCase(this string str)
    {
      return StringComparer.Ordinal.GetHashCode(str);
    }

    /// <summary>
    /// Helper function that performs case-insensitive comparison between strings using ordinal comparison.
    /// Either lhs and rhs can be null.
    /// Use this in conjunction with GetHashCodeOrdSenseCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool EqualsOrdSenseCase(this string lhs, string rhs)
    {
      return StringComparer.Ordinal.Equals(lhs, rhs);
    }


    /// <summary>
    /// Helper function that performs case-insensitive GetHashcode for string using ordinal comparison.
    /// Use this in conjunction with EqualsOrdIgnoreCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static int GetHashCodeOrdIgnoreCase(this string str)
    {
      return StringComparer.OrdinalIgnoreCase.GetHashCode(str);
    }

    /// <summary>
    /// Helper function that performs case-insensitive comparison between strings using ordinal comparison.
    /// Either lhs and rhs can be null.
    /// Use this in conjunction with GetHashCodeOrdIgnoreCase
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool EqualsOrdIgnoreCase(this string lhs, string rhs)
    {
      return StringComparer.OrdinalIgnoreCase.Equals(lhs, rhs);
    }

    /// <summary>
    /// Helper function that calls string.IsNullOrEmpty()
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty(this string s)
    {
      return string.IsNullOrEmpty(s);
    }

    /// <summary>
    /// Helper function that calls !string.IsNullOrEmpty()
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNullOrEmpty(this string s)
    {
      return !string.IsNullOrEmpty(s);
    }

    /// <summary>
    /// Helper function that calls string.IsNullOrWhiteSpace()
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace(this string s)
    {
      return string.IsNullOrWhiteSpace(s);
    }

    /// <summary>
    /// Helper function that calls !string.IsNullOrWhiteSpace()
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNullOrWhiteSpace(this string s)
    {
      return !string.IsNullOrWhiteSpace(s);
    }

    /// <summary>
    /// Defaults string if it is null or whitespace
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string Default(this string str, string val = "")
    {
      if (str.IsNullOrWhiteSpace()) return val;
      return str;
    }


    /// <summary>
    /// Converts an index in a string into a line number, counting new line characters
    /// </summary>
    public static int IndexToLineNumber(this string str, int index)
    {
      if (str == null) return 0;
      var result = 1;
      for (var i = 0; i < str.Length && i < index; i++)
        if (str[i] == '\n') result++;

      return result;
    }


    /// <summary>
    /// Appends the string followed by new line and returned by processing a composite format string, which contains zero or more format items, to this instance.
    /// Each format item is replaced by the string representation of a single argument.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static StringBuilder AppendFormatLine(this StringBuilder builder, string str, object arg0)//this exists for perf reasons (vs params object[])
    {
      builder.AppendFormat(str, arg0);
      return builder.AppendLine();
    }

    /// <summary>
    /// Appends the string followed by new line and returned by processing a composite format string, which contains zero or more format items, to this instance.
    /// Each format item is replaced by the string representation of a single argument.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static StringBuilder AppendFormatLine(this StringBuilder builder, string str, object arg0, object arg1)//this exists for perf reasons (vs params object[])
    {
      builder.AppendFormat(str, arg0, arg1);
      return builder.AppendLine();
    }

    /// <summary>
    /// Appends the string followed by new line and returned by processing a composite format string, which contains zero or more format items, to this instance.
    /// Each format item is replaced by the string representation of a single argument.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static StringBuilder AppendFormatLine(this StringBuilder builder, string str, object arg0, object arg1, object arg2)//this exists for perf reasons (vs params object[])
    {
      builder.AppendFormat(str, arg0, arg1, arg2);
      return builder.AppendLine();
    }

    /// <summary>
    /// Appends the string followed by new line and returned by processing a composite format string, which contains zero or more format items, to this instance.
    /// Each format item is replaced by the string representation of a single argument.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static StringBuilder AppendFormatLine(this StringBuilder builder, string str, params object[] args)//this exists for perf reasons (vs params object[])
    {
      builder.AppendFormat(str, args);
      return builder.AppendLine();
    }

    /// <summary>
    /// Returns true if the specified string is one of the specified.
    /// The comparison is invariant case-insensitive. No trimming is performed
    /// </summary>
    public static bool IsOneOf(this string val, params string[] strings)
      => val.IsOneOf(strings as IEnumerable<string>);

    /// <summary>
    /// Returns true if the specified string is one of the specified.
    /// The comparison is invariant case-insensitive. No trimming is performed
    /// </summary>
    public static bool IsOneOf(this string val, IEnumerable<string> strings)
    {
      if (strings == null) return false;

      foreach(var str in strings)
        if (str.EqualsIgnoreCase(val)) return true;

      return false;
    }


    /// <summary>
    /// Swaps string letters that "obfuscates" string- useful for generation of keys from strings that have to become non-obvious to user.
    /// This function does not offer any real protection (as it is easy to decipher the original value), just visual.
    /// The name comes from non-existing science "Burmatography" used in "Neznaika" kids books
    /// </summary>
    public static string Burmatographize(this string src, bool rtl = false)
    {
      if (src.IsNullOrWhiteSpace()) return src;
      var sb = new StringBuilder(src.Length);

      var odd = rtl;
      for (int f = 0, b = src.Length - 1; f <= b;)
      {
        if (odd) { sb.Append(src[f]); f++; }
        else { sb.Append(src[b]); b--; }
        odd = !odd;
      }
      return sb.ToString();
    }

    /// <summary>
    /// Replaces CRLF with LF
    /// </summary>
    public static string ToLinuxLines(this string src)
    => src==null ? null : src.Replace(WIN_UNIX_LINE_BRAKES[0], WIN_UNIX_LINE_BRAKES[1]);

    /// <summary>
    /// Replaces LF with CRLF
    /// </summary>
    public static string ToWindowsLines(this string src)
    {
      if (src==null) return null;

      var linux = src.ToLinuxLines();

      return linux.Replace(WIN_UNIX_LINE_BRAKES[1], WIN_UNIX_LINE_BRAKES[0]);
    }


    /// <summary>
    /// Creates a hex editor like hex / char dump of a string value
    /// printing verbatim binary char values
    /// </summary>
    public static string ToDumpString(this string str)
    {
      if (str==null) return null;
      var result = new StringBuilder();
      var txt = new StringBuilder();

      var wide = str.Any(c => c > 0xff);

      var i=0;
      for(; i<str.Length; i++)
      {
        var c = str[i];
        if (i % 16==0)
        {
          if (txt.Length>0)
          {
            result.Append("   ");
            result.Append(txt.ToString());
            txt.Clear();
          }
          if (i>0) result.AppendLine();
          result.Append(i.ToString("X4"));
          result.Append(": ");
        }else if (i % 8 ==0)
        {
          result.Append("| ");
        }

        result.Append(((int)c).ToString(wide ? "X4" : "X2"));
        result.Append(' ');
        txt.Append(c<'!' ? '·' : c);
      }

      for(var j=0; i++ % 16!=0; j++)
      {
        if (j==8) result.Append("  ");
        result.Append(wide ? "     " : "   ");
      }

      if (txt.Length > 0)
      {
        result.Append("   ");
        result.Append(txt.ToString());
      }

      return result.ToString();
    }


    /// <summary>
    /// Creates a string listing char-by char difference between strings
    /// </summary>
    public static string DiffStrings(this string a, string b, int limit = 0)
    {
      string ch(char c) =>
          "#{0:x4} {1}".Args((int)c, Azos.Serialization.JSON.JsonWriter.Write(c, Azos.Serialization.JSON.JsonWritingOptions.CompactASCII));

      var result = new StringBuilder();
      result.AppendLine("A is {0} |  B is {1}".Args(a==null?CoreConsts.NULL_STRING:$"[{a.Length}]", b == null ? CoreConsts.NULL_STRING : $"[{b.Length}]"));
      if (string.Equals(a, b, StringComparison.Ordinal))
      {
        result.AppendLine("Identical");
        return result.ToString();
      }
      if (a==null || b==null) return result.ToString();
      if (a.Length!=b.Length) result.AppendLine("Different length");

      if (a.Length >= b.Length)
      {
        if (a.Length>b.Length) result.AppendLine("A is longer than B by {0} chars".Args(a.Length-b.Length));

        for(var i=0; i<a.Length && (i<limit || limit<=0); i++)
        {
          result.Append("A[{0}] = {1}  | ".Args(i, ch(a[i])));
          if (i<b.Length)
            result.Append("B[{0}] = {1}".Args(i, ch(b[i])));
          else
            result.Append("     n/a ");
          result.AppendLine();
        }
      } else
      {
        result.AppendLine("B is longer than A by {0} chars".Args(b.Length - a.Length));

        for (var i = 0; i < b.Length && (i < limit || limit <= 0); i++)
        {
          if (i < a.Length)
            result.Append("A[{0}] = {1}  | ".Args(i, ch(a[i])));
          else
            result.Append("       n/a        | ");
          result.Append("B[{0}] = {1}  | ".Args(i, ch(b[i])));
          result.AppendLine();
        }
      }
      if (limit>0)
        result.AppendLine("....Capped at {0} chars".Args(limit));

      return result.ToString();
    }

    /// <summary>
    /// Default chars to be trimmed by TrimAll function
    /// </summary>
    public static readonly char[] TRIM_ALL_CHARS_DEFAULT = new []{'\r','\n',' '};

    /// <summary>
    /// Trims all characters from the string including the inner content
    /// </summary>
    public static string TrimAll(this string src, params char[] chars)
    {
      if (src==null) return null;
      if(chars==null || chars.Length==0) chars = TRIM_ALL_CHARS_DEFAULT;
      return new string(src.Where(c => !chars.Any(c2 => c2 == c)).ToArray());
    }

    /// <summary>
    /// Parses a string of a form:  key:value into a KeyValuePair. Trips on a first delimiter L2R
    /// </summary>
    public static KeyValuePair<string, string> SplitKVP(this string src, char delimiter = '=')
    {
      if (src.IsNullOrWhiteSpace()) return new KeyValuePair<string, string>(string.Empty, string.Empty);
      var i = src.IndexOf(delimiter);
      if (i>=0 && i<src.Length) return new KeyValuePair<string, string>(src.Substring(0, i), src.Substring(i + 1));
      return new KeyValuePair<string, string>(src, string.Empty);
    }

    /// <summary>
    /// Parses a string of a form:  key:value into a KeyValuePair. Trips on a first delimiter L2R
    /// </summary>
    public static KeyValuePair<string, string> SplitKVP(this string src, char delimiter1, char delimiter2)
    {
      if (src.IsNullOrWhiteSpace()) return new KeyValuePair<string, string>(string.Empty, string.Empty);
      var i = src.IndexOf(delimiter1);
      if (i >= 0 && i < src.Length) return new KeyValuePair<string, string>(src.Substring(0, i), src.Substring(i + 1));
      i = src.IndexOf(delimiter2);
      if (i >= 0 && i < src.Length) return new KeyValuePair<string, string>(src.Substring(0, i), src.Substring(i + 1));
      return new KeyValuePair<string, string>(src, string.Empty);
    }

    /// <summary>
    /// Parses a string of a form:  key:value into a KeyValuePair. Trips on a first delimiter L2R
    /// </summary>
    public static KeyValuePair<string, string> SplitKVP(this string src, char delimiter1, char delimiter2, char delimiter3)
    {
      if (src.IsNullOrWhiteSpace()) return new KeyValuePair<string, string>(string.Empty, string.Empty);
      var i = src.IndexOf(delimiter1);
      if (i >= 0 && i < src.Length) return new KeyValuePair<string, string>(src.Substring(0, i), src.Substring(i + 1));
      i = src.IndexOf(delimiter2);
      if (i >= 0 && i < src.Length) return new KeyValuePair<string, string>(src.Substring(0, i), src.Substring(i + 1));
      i = src.IndexOf(delimiter3);
      if (i >= 0 && i < src.Length) return new KeyValuePair<string, string>(src.Substring(0, i), src.Substring(i + 1));
      return new KeyValuePair<string, string>(src, string.Empty);
    }

    /// <summary>
    /// Provides string to byte[] conversion using a rented buffer.
    /// You must release the common buffer back by calling TextBytes.Dispose().
    /// </summary>
    /// <remarks>
    /// The algorithm with get/release pattern is built around the IDisposable finalization
    /// block, because the usage of function would have required to use a delegate which could
    /// introduce additional instance allocation. The whole point of this type is to avoid
    /// any instance allocations, hence the IDisposable() pattern on a struct which enables use of `using`
    /// </remarks>
    public struct TextBytes : IDisposable
    {
      private const int THRESHOLD_MAX_BYTE_COUNT = 8 * 1024;
      private const int THRESHOLD_POOL_ALLOC_COUNT = 1024;


      private static byte[] s_Cache1;
      private static byte[] s_Cache2;

      /// <summary>
      /// Creates text bytes from non-null string, using the supplied encoding or UTF8 by default.
      /// If the string is sensitive then the array is cleared after use
      /// </summary>
      public TextBytes(string text, Encoding encoding = null, bool sensitive = true)
      {
        Text = text.NonNull(nameof(text));
        Encoding = encoding ?? Encoding.UTF8;
        Sensitive = sensitive;

        var len = text.Length;
        var mbc = Encoding.GetMaxByteCount(len);
        var byteCount = mbc < THRESHOLD_MAX_BYTE_COUNT ? mbc : Encoding.GetByteCount(Text);
        byte[] buffer;
        if (byteCount > THRESHOLD_POOL_ALLOC_COUNT)
        {
          buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(byteCount);
          FromPool = true;
        }
        else
        {
          buffer = Interlocked.Exchange(ref s_Cache1, null);
          if (buffer == null)
          {
            buffer = Interlocked.Exchange(ref s_Cache2, null);
            if (buffer == null)
            {
              buffer = new byte[THRESHOLD_POOL_ALLOC_COUNT];
            }
          }
          FromPool = false;
        }

        var encodedLength = Encoding.GetBytes(Text, 0, len, buffer, 0);
        Buffer = new ArraySegment<byte>(buffer, 0, encodedLength);
      }

      public void Dispose()
      {
        if (Buffer.Array == null ) return;//unassigned or not from pool
        if (FromPool)
        {
          System.Buffers.ArrayPool<byte>.Shared.Return(Buffer.Array, clearArray: Sensitive);
        }
        else
        {
          if (Sensitive) Array.Clear(Buffer.Array);
          if (null == Interlocked.CompareExchange(ref s_Cache1, Buffer.Array, null)) return;
          Interlocked.CompareExchange(ref s_Cache2, Buffer.Array, null);
        }
      }

      /// <summary>
      /// Original string that Buffer was encoded from
      /// </summary>
      public readonly string Text;

      /// <summary>
      /// Encoding that is used; UTF8 by default
      /// </summary>
      public readonly Encoding Encoding;

      /// <summary>
      /// Buffer filled with data. WARNING: The length of the underlying buffer is usually LONGER than length of encoded text,
      /// you must use array segment of that buffer
      /// </summary>
      public readonly ArraySegment<byte> Buffer;

      public readonly bool FromPool;
      public readonly bool Sensitive;

      public bool Assigned => Buffer.Array != null;


      /// <summary>
      /// Asynchronously writes an instance into a stream
      /// </summary>
      public async Task WriteToStreamAsync(Stream stream, CancellationToken? cancelToken = null)
      {
        Assigned.IsTrue(nameof(Assigned));
        stream.NonNull(nameof(stream)).CanWrite.IsTrue("stream.CanWrite");

        if (cancelToken.HasValue)
          await stream.WriteAsync(Buffer.Array, 0, Buffer.Count, cancelToken.Value).ConfigureAwait(false);
        else
          await stream.WriteAsync(Buffer.Array, 0, Buffer.Count).ConfigureAwait(false);
      }

      /// <summary>
      /// Asynchronously writes a string into a stream
      /// </summary>
      public static async Task WriteToStreamAsync(Stream stream, string text, Encoding encoding = null, bool sensitive = true, CancellationToken? cancelToken = null)
      {
        using var encoded = new TextBytes(text, encoding, sensitive);
        await encoded.WriteToStreamAsync(stream, cancelToken).ConfigureAwait(false);
      }

    }
  }
}
