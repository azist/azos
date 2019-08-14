/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;

namespace Azos.Standards
{
  /// <summary>
  /// Facilitates working with Overpunch data as described in:
  /// https://en.wikipedia.org/wiki/Signed_overpunch
  /// </summary>
  public static class Overpunch
  {
    /// <summary>
    /// Converts overpunched string into nullable decimal scaled to the specified precision
    /// </summary>
    /// <param name="val">Value, coerced to string if needed</param>
    /// <param name="scale">100, by default int to decimal divider</param>
    /// <param name="dflt">default value or null</param>
    /// <param name="handling">Throws or return dflt</param>
    public static decimal? ToDecimal(object val, int scale = 100, decimal? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.Throw)
    {
      if (scale<=0) scale = 1;
      var ld = dflt.HasValue ? (long)(dflt * scale) : (long?)null;
      var l = ToLong(val, ld, handling);
      if (l.HasValue) return (decimal)l / scale;
      return null;
    }

    /// <summary>
    /// Converts overpunched string into nullable long
    /// </summary>
    public static long? ToLong(object val, long? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.Throw)
    {
    //https://en.wikipedia.org/wiki/Signed_overpunch
      if (val == null) return null;
      if (val is long lval) return lval;

      long result = 0;
      var str = val.ToString();

      for (var i = 0; i < str.Length; i++)
      {
        result *= 10;
        var ch = str[i];
        int d;

        if (i == str.Length - 1) //last char
        {
          if (ch == '}') return -result;
          if (ch == '{') return +result;

          d = 1 + (ch - 'A');
          if (d > 0 && d < 10)
          {
            result += d;
            return +result;
          }

          d = 1 + (ch - 'a');
          if (d > 0 && d < 10)
          {
            result += d;
            return +result;
          }

          d = 1 + (ch - 'J');
          if (d > 0 && d < 10)
          {
            result += d;
            return -result;
          }

          d = 1 + (ch - 'j');
          if (d > 0 && d < 10)
          {
            result += d;
            return -result;
          }

        }//last char
        else
        {
          d = ch - '0';
          if (d >= 0 && d < 10) //decimal digit
          {
            result += d;
            continue;
          }
        }

        //exception
        if (handling == ConvertErrorHandling.ReturnDefault) return dflt;
        throw new AzosException(StringConsts.OVERPUNCH_TO_NUMBER_ERROR.Args(str.TakeFirstChars(16, "..")));
      }//for

      return result;
    }


    /// <summary>
    /// Converts nullable long to a string formatted per overpunch specification
    /// </summary>
    public unsafe static string FromLong(long? val)
    {
      if (val == null) return null;
      if (val == 0) return "{";

      const int MAX = 32;

      var neg = val < 0;
      var v = Math.Abs(val.Value);
      var a = stackalloc char[MAX];
      var i = MAX;
      var least = true;//least significant digit
      while(v>0)
      {
        i--;
        var rem = v % 10;

        if (least)
        {
          least = false;
          if (rem==0)
            a[i] = neg ? '}' : '{';
          else
            a[i] = (char)( (neg ? 'J' : 'A') + rem - 1);//1-based index, 'J' == 1, not 0
        }
        else
          a[i] = (char)('0' + rem);//zero-based index '0' + 0 = '0'

        v /= 10;
      }

      return new string(a, i, MAX - i);
    }

  }
}
