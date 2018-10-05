/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Parsing;
using Azos.CodeAnalysis;

namespace Azos.CodeAnalysis.CSharp
{

  /// <summary>
  /// Provides C# string escape parsing
  /// </summary>
  public static class CSStrings
  {

    public static string UnescapeString(string str)
    {
      if (str.IndexOf('\\')==-1) return str;//20131215 DKh 6.3% speed improvement in JSON Integration test - similar problem
      StringBuilder sb = new StringBuilder();

      for (int i = 0; i < str.Length; i++)
      {
        char c = str[i];
        if ((i < str.Length - 1) && (c == '\\'))
        {
          i++;
          char n = str[i];

          switch (n)
          {
            case '\\': sb.Append('\\'); break;
            case '0': sb.Append((char)CharCodes.Char0); break;
            case 'a': sb.Append((char)CharCodes.AlertBell); break;
            case 'b': sb.Append((char)CharCodes.Backspace); break;
            case 'f': sb.Append((char)CharCodes.Formfeed); break;
            case 'n': sb.Append((char)CharCodes.LF); break;
            case 'r': sb.Append((char)CharCodes.CR); break;
            case 't': sb.Append((char)CharCodes.Tab); break;
            case 'v': sb.Append((char)CharCodes.VerticalQuote); break;
            case 'u': //  \uFFFF
              string hex = string.Empty;
              int cnt = 0;
              //loop through UNICODE hex number chars
              while ((i < str.Length - 1) && (cnt < 4))
              {
                i++;
                hex += str[i];
                cnt++;
              }

              try
              {
                sb.Append(Char.ConvertFromUtf32(Convert.ToInt32(hex, 16)));
              }
              catch
              {
                throw new StringEscapeErrorException(hex);
              }

              break;

            default:
              throw new StringEscapeErrorException(String.Format("{0}{1}", c, n));
          }

        }
        else
          sb.Append(c);

      }//for

      return sb.ToString();
    }





  }

}
