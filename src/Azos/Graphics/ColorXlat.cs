/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

namespace Azos.Graphics
{
  /// <summary>
  /// Translates color values Color/Web
  /// </summary>
  public static class ColorXlat
  {

    public static Color FromHTML(string html)
    {
      if (html.IsNullOrWhiteSpace()) return Color.Empty;

      var i = html.IndexOf('#');
      if (i<0) return Color.FromName(html);
      i++;

      if (html.Length-i>=6)
      {
        var r = readH2(html, ref i);
        if (r>0xff) return Color.Empty;

        var g = readH2(html, ref i);
        if (g>0xff) return Color.Empty;

        var b = readH2(html, ref i);
        if (b>0xff) return Color.Empty;

        return Color.FromArgb(r, g, b);
      }
      if (html.Length-i>=3)
      {
        var r = readH1(html, ref i);
        if (r>0xf) return Color.Empty;
        r |= (r << 4);

        var g = readH1(html, ref i);
        if (g>0xf) return Color.Empty;
        g |= (g << 4);

        var b = readH1(html, ref i);
        if (b>0xf) return Color.Empty;
        b |= (b << 4);

        return Color.FromArgb(r, g, b);
      }

      return Color.Empty;
    }

    public static string ToHTML(Color clr)
    {
      if (clr.IsEmpty) return "#000";

      return "#" + (clr.ToArgb() & 0x00ffffff).ToString("X6");
    }


    private static int readH1(string s, ref int i)
    {
      int c = s[i++];
      c = c < ':' ? c - '0' : 10 + (c > 'F' ? c - 'a' : c - 'A');
      return c;
    }

    private static int readH2(string s, ref int i)
    {
      return (readH1(s, ref i) << 4) | readH1(s, ref i);
    }


  }
}
