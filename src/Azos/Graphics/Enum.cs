/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Graphics
{
    public enum InterpolationMode
    {
      Default = 0,
      Bicubic,
      Bilinear,
      Low,
      High,
      NearestNeighbor,
      HQBilinear,
      HQBicubic
    }

    /// <summary>
    /// Specifies image pixel format
    /// </summary>
    public enum ImagePixelFormat
    {
      /// <summary>
      /// The default pixel format of 32 bits per pixel. The format specifies 24-bit color depth and an 8-bit alpha channel
      /// </summary>
      Default = 0,

      /// <summary>Monochrome </summary>
      BPP1Indexed,

      /// <summary>4 bit palette </summary>
      BPP4Indexed,

      /// <summary>8 bit palette </summary>
      BPP8Indexed,

      /// <summary>
      /// 2^16 shades of gray
      /// </summary>
      BPP16Gray,

      /// <summary>8 bit per R G and B </summary>
      RGB24,

      /// <summary> R G B take 8 bits remaining 4th byte is unused</summary>
      RGB32,

      /// <summary> R G B take 8 bits remaining 4th byte is Alpha</summary>
      RGBA32
    }

    /// <summary>
    /// Specifies the style of dashed lines drawn with pens
    /// </summary>
    public enum PenDashStyle
    {
      /// <summary>Specifies a solid line.</summary>
      Solid = 0,
      /// <summary>Specifies a line consisting of dashes.</summary>
      Dash,
      /// <summary>Specifies a line consisting of dots.</summary>
      Dot,
      /// <summary>Specifies a line consisting of a repeating pattern of dash-dot.</summary>
      DashDot,
      /// <summary>Specifies a line consisting of a repeating pattern of dash-dot-dot.</summary>
      DashDotDot
    }

    /// <summary>Specifies style information applied to text.</summary>
    [Flags]
    public enum FontStyling
    {
      /// <summary>Normal text.</summary>
      Regular = 0,
      /// <summary>Bold text.</summary>
      Bold = 1,
      /// <summary>Italic text.</summary>
      Italic = 1 << 1,
      /// <summary>Underlined text.</summary>
      Underline = 1 << 2,
      /// <summary>Text with a line through the middle.</summary>
      Strikeout = 1 << 3
    }

    /// <summary>Specifies the unit of measure for graphics</summary>
    public enum MeasureUnit
    {
      /// <summary>Specifies the world coordinate system unit as the unit of measure.</summary>
      World,
      /// <summary>Specifies the unit of measure of the display device. Typically pixels for video displays, and 1/100 inch for printers.</summary>
      Display,
      /// <summary>Specifies a device pixel as the unit of measure.</summary>
      Pixel,
      /// <summary>Specifies a printer's point (1/72 inch) as the unit of measure.</summary>
      Point,
      /// <summary>Specifies the inch as the unit of measure.</summary>
      Inch,
      /// <summary>Specifies the document unit (1/300 inch) as the unit of measure.</summary>
      Document,
      /// <summary>Specifies the millimeter as the unit of measure.</summary>
      Millimeter
    }



}
