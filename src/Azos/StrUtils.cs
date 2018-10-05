/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Text;

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
    /// Takes first X chars from a string. If string is null returns null. If string does not have enough
    /// the function returns what the string has
    /// </summary>
    public static string TakeFirstChars(this string str, int count, string ellipsis = null)
    {
      if (str == null) return null;
      if (str.Length <= count) return str;
      ellipsis = ellipsis ?? string.Empty;

      if (count > ellipsis.Length)
        count -= ellipsis.Length;
      else
        return ellipsis.Substring(0, count);

      return str.Substring(0, count) + ellipsis;
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
    /// Swaps string letters that "obfuscates" string- usefull for generation of keys from strings that have to become non-obvious to user.
    /// This function does not offer any real protection (as it is easy to decipher the original value), just visual.
    /// The name comes from non-existing science "Burmatography" used in "Neznaika" kids books
    /// </summary>
    public static string Burmatographize(string src, bool rtl = false)//no need to make this an extension method - hence no "this"
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


  }
}
