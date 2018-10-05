/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Azos.CodeAnalysis.JSON
{

  public static class JSONNumbers
  {
    /// <summary>
    /// Tries to convert string to number, returns null if conversion could not be made
    /// </summary>
    public static object Convert(string str, out JSONTokenType type)
    {
      ulong inum = 0;
      double fnum = 0.0;

      if (strToInt(str, out inum))
      {
        if (inum > UInt32.MaxValue)
        {
          type = JSONTokenType.tLongIntLiteral;
        }
        else
        {
          type = JSONTokenType.tIntLiteral;
          if (inum<Int32.MaxValue)
           return (Int32)inum;
        }
        return inum;
      }

      if (strToFloat(str, out fnum))
      {
        type = JSONTokenType.tDoubleLiteral;

        return fnum;
      }


      type = JSONTokenType.tUnknown;
      return null;
    }//Convert



    private static bool strToInt(string str, out ulong num)
    {

      if (UInt64.TryParse(str, out num))
        return true;

      if (str.Length>2 && str[0]=='0' && str[1]>'a')//20131215 DKh speed improvement
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

      return false;
    }//strToInt

    private static bool strToFloat(string str, out double num)
    {
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
