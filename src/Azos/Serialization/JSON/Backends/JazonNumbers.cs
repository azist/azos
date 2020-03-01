/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Globalization;

using Azos.CodeAnalysis.JSON;

namespace Azos.Serialization.JSON.Backends
{

  public static class JazonNumbers
  {
    /// <summary>
    /// Tries to convert string to integer number, returns false if conversion could not be made
    /// </summary>
    public static bool ConvertInteger(string str, out JsonTokenType type, out ulong value)
    {
      if (strToInt(str, out value))
      {
        type = value > Int32.MaxValue ? type = JsonTokenType.tLongIntLiteral : JsonTokenType.tIntLiteral;
        return true;
      }

      type = JsonTokenType.tUnknown;
      value = 0;
      return false;
    }

    /// <summary>
    /// Tries to convert string to double, returns false if conversion could not be made
    /// </summary>
    public static bool ConvertDouble(string str, out JsonTokenType type, out double value)
    {
      if (strToFloat(str, out value))
      {
        type = JsonTokenType.tDoubleLiteral;

        return true;
      }


      type = JsonTokenType.tUnknown;
      value = 0d;
      return false;
    }


    private static bool strToInt(string str, out ulong num)
    {
      num = 0;
      var ok = false;
      for(var i=0; i<str.Length; i++)
      {
        var c = str[i];
        if (c<'0' || c>'9') { ok = false; break; }
        checked
        {
          num = (num * 10) + ((uint)c - '0');
        }
        ok = true;
      }
      if (ok) return true;


      //quick reject for speed not to check StartsWith()
      if (str.Length>2 && str[0]=='0' && str[1]>'a')
      {
        try
        {
          if (str.StartsWith("0x"))   //HEX
          {
            num = System.Convert.ToUInt64(str.Substring(2), 16);
            return true;
          }
          else if (str.StartsWith("0b")) //BIN
          {
            num = System.Convert.ToUInt64(str.Substring(2), 2);
            return true;
          }
          else if (str.StartsWith("0o")) //OCT
          {
            num = System.Convert.ToUInt64(str.Substring(2), 8);
            return true;
          }
        }
        catch
        {
        }
      }

      return false;
    }//strToInt

    private static bool strToFloat(string str, out double num)
    {
      for (var i=0; i<str.Length; i++)
      {
        var c = str[i];
        if ((c > '9' && c!='e' && c!='E') || (c<'0' && c!='.' && c!='-' && c!='+'))
        {
          num=0;
          return false;
        }
      }

      return Double.TryParse(str,
                             NumberStyles.Float,
                             CultureInfo.InvariantCulture,  out num);
    }

    //private static bool strToDecimal(string str, out decimal num)
    //{
    //  return Decimal.TryParse(str, out num);
    //}



  }

}
