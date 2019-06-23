/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
    public static decimal? ToDecimal(this object val, int scale = 100, decimal? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.Throw)
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
    public static long? ToLong(this object val, long? dflt = null, ConvertErrorHandling handling = ConvertErrorHandling.Throw)
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
  }
}
