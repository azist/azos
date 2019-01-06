/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos;
using Azos.Data;
using Azos.Serialization.BSON;
using Azos.Serialization.JSON;

namespace Agnivo
{
  public class AgnivoHelper
  {
    public static string Base64BSONToJSON(string str)
    {
      byte[] raw = Convert.FromBase64String(str);
      var document = BSONDocument.FromArray(raw);
      var map = DataDocConverter.DefaultInstance.BSONDocumentToJSONMap(document);
      return map.ToJSON(JSONWritingOptions.PrettyPrint);
    }

    public static string ParseGDID(string str)
    {
      if (str.IsNullOrWhiteSpace()) return string.Empty;

      GDID? gdid = null;

      ELink link = null;
      try
      {
        link = new ELink(str);
        gdid = link.GDID;
      }
      catch
      {
      }

      if (!gdid.HasValue && !GDID.TryParse(str, out gdid))
        try
        {
          byte[] raw = getGDIDBytes(str);
          gdid = new GDID(raw);
        }
        catch (Exception)
        {
          throw new FormatException("Invalid GDID format");
        }

      return getAllGDIDFormats(gdid.Value);
    }

    private static byte[] getGDIDBytes(string str)
    {
      try
      {
        return Convert.FromBase64String(str);
      }
      catch (Exception)
      {
      }

      return str.Split(new char[] {',', ' ', '"', '[', ']'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => Convert.ToByte(e))
                .ToArray();
    }


    private static string getAllGDIDFormats(GDID gdid)
    {
      var sb = new StringBuilder();

      // native
      sb.AppendLine(gdid.ToString());

      // hex
      sb.AppendLine("0x" + gdid.ToHexString());

      // BASE64
      sb.AppendLine(Convert.ToBase64String(gdid.Bytes));

      // Elink
      var link = new ELink(gdid);
      for(var cnt = 0; cnt <= ELink.VARIATIONS; cnt++)
        sb.AppendLine(link.Encode((byte)cnt));

      // byte array
        sb.AppendLine("[" + string.Join(", ", gdid.Bytes) + "]");

      return sb.ToString();
    }
  }
}
