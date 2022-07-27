/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

using Azos.Graphics;

namespace Azos.Platform.Abstraction.NetFramework.Graphics
{
  internal static class xlator
  {
    internal static PenDashStyle xlat(System.Drawing.Drawing2D.DashStyle style)
    {
      switch(style)
      {
        case System.Drawing.Drawing2D.DashStyle.Dot         : return PenDashStyle.Dot;
        case System.Drawing.Drawing2D.DashStyle.Dash        : return PenDashStyle.Dash;
        case System.Drawing.Drawing2D.DashStyle.DashDot     : return PenDashStyle.DashDot;
        case System.Drawing.Drawing2D.DashStyle.DashDotDot  : return PenDashStyle.DashDotDot;
        case System.Drawing.Drawing2D.DashStyle.Solid       : return PenDashStyle.Solid;
        default: return PenDashStyle.Solid;
      }
    }

    internal static System.Drawing.Drawing2D.DashStyle xlat(PenDashStyle style)
    {
      switch(style)
      {
        case PenDashStyle.Dot         :  return System.Drawing.Drawing2D.DashStyle.Dot;
        case PenDashStyle.Dash        :  return System.Drawing.Drawing2D.DashStyle.Dash;
        case PenDashStyle.DashDot     :  return System.Drawing.Drawing2D.DashStyle.DashDot;
        case PenDashStyle.DashDotDot  :  return System.Drawing.Drawing2D.DashStyle.DashDotDot;
        case PenDashStyle.Solid       :  return System.Drawing.Drawing2D.DashStyle.Solid;
        default: return System.Drawing.Drawing2D.DashStyle.Solid;
      }
    }

    internal static System.Drawing.Imaging.PixelFormat xlat(ImagePixelFormat pf)
    {
      switch(pf)
      {
        case ImagePixelFormat.BPP1Indexed: return System.Drawing.Imaging.PixelFormat.Format1bppIndexed;
        case ImagePixelFormat.BPP4Indexed: return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
        case ImagePixelFormat.BPP8Indexed: return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
        case ImagePixelFormat.BPP16Gray:   return System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
        case ImagePixelFormat.RGB24:       return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
        case ImagePixelFormat.RGB32:       return System.Drawing.Imaging.PixelFormat.Format32bppRgb;
        case ImagePixelFormat.RGBA32:      return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        default: return System.Drawing.Imaging.PixelFormat.Canonical;
      }
    }

    internal static ImagePixelFormat xlat(System.Drawing.Imaging.PixelFormat pf)
    {
      switch(pf)
      {
        case  System.Drawing.Imaging.PixelFormat.Format1bppIndexed:     return ImagePixelFormat.BPP1Indexed;
        case  System.Drawing.Imaging.PixelFormat.Format4bppIndexed:     return ImagePixelFormat.BPP4Indexed;
        case  System.Drawing.Imaging.PixelFormat.Format8bppIndexed:     return ImagePixelFormat.BPP8Indexed;
        case  System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:  return ImagePixelFormat.BPP16Gray;
        case  System.Drawing.Imaging.PixelFormat.Format24bppRgb:        return ImagePixelFormat.RGB24;
        case  System.Drawing.Imaging.PixelFormat.Format32bppRgb:        return ImagePixelFormat.RGB32;
        case  System.Drawing.Imaging.PixelFormat.Format32bppArgb:       return ImagePixelFormat.RGBA32;
        default: return ImagePixelFormat.Default;
      }
    }

    internal static InterpolationMode xlat(System.Drawing.Drawing2D.InterpolationMode mode)
    {
      switch(mode)
      {
        case System.Drawing.Drawing2D.InterpolationMode.Low:                 return InterpolationMode.Low;
        case System.Drawing.Drawing2D.InterpolationMode.High:                return InterpolationMode.High;
        case System.Drawing.Drawing2D.InterpolationMode.Bilinear:            return InterpolationMode.Bilinear;
        case System.Drawing.Drawing2D.InterpolationMode.Bicubic:             return InterpolationMode.Bicubic;
        case System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor:     return InterpolationMode.NearestNeighbor;
        case System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear: return InterpolationMode.HQBilinear;
        case System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic:  return InterpolationMode.HQBicubic;
        //Invalid = -1,
        //Default,
        default: return InterpolationMode.Default;
      }
    }

    internal static System.Drawing.Drawing2D.InterpolationMode xlat(InterpolationMode mode)
    {
      switch(mode)
      {
        case  InterpolationMode.Low:              return System.Drawing.Drawing2D.InterpolationMode.Low                 ;
        case  InterpolationMode.High:             return System.Drawing.Drawing2D.InterpolationMode.High                ;
        case  InterpolationMode.Bilinear:         return System.Drawing.Drawing2D.InterpolationMode.Bilinear            ;
        case  InterpolationMode.Bicubic:          return System.Drawing.Drawing2D.InterpolationMode.Bicubic             ;
        case  InterpolationMode.NearestNeighbor:  return System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor     ;
        case  InterpolationMode.HQBilinear:       return System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear ;
        case  InterpolationMode.HQBicubic:        return System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic  ;
        //Invalid = -1,
        //Default,
        default: return System.Drawing.Drawing2D.InterpolationMode.Default;
      }
    }


    internal static FontStyling xlat(System.Drawing.FontStyle style)
    {
      switch (style)
      {
        case System.Drawing.FontStyle.Bold:      return FontStyling.Bold;
        case System.Drawing.FontStyle.Italic:    return FontStyling.Italic;
        case System.Drawing.FontStyle.Regular:   return FontStyling.Regular;
        case System.Drawing.FontStyle.Strikeout: return FontStyling.Strikeout;
        case System.Drawing.FontStyle.Underline: return FontStyling.Underline;
        default: return FontStyling.Regular;
      }
    }

    internal static System.Drawing.FontStyle xlat(FontStyling style)
    {
      switch (style)
      {
        case FontStyling.Bold:      return System.Drawing.FontStyle.Bold;
        case FontStyling.Italic:    return System.Drawing.FontStyle.Italic;
        case FontStyling.Regular:   return System.Drawing.FontStyle.Regular;
        case FontStyling.Strikeout: return System.Drawing.FontStyle.Strikeout;
        case FontStyling.Underline: return System.Drawing.FontStyle.Underline;
        default: return System.Drawing.FontStyle.Regular;
      }
    }

    internal static MeasureUnit xlat(System.Drawing.GraphicsUnit unit)
    {
      switch (unit)
      {
        case System.Drawing.GraphicsUnit.Display:    return MeasureUnit.Display;
        case System.Drawing.GraphicsUnit.Document:   return MeasureUnit.Document;
        case System.Drawing.GraphicsUnit.Inch:       return MeasureUnit.Inch;
        case System.Drawing.GraphicsUnit.Millimeter: return MeasureUnit.Millimeter;
        case System.Drawing.GraphicsUnit.Pixel:      return MeasureUnit.Pixel;
        case System.Drawing.GraphicsUnit.Point:      return MeasureUnit.Point;
        case System.Drawing.GraphicsUnit.World:      return MeasureUnit.World;
        default: return MeasureUnit.World;
      }
    }

    internal static System.Drawing.GraphicsUnit xlat(MeasureUnit unit)
    {
      switch (unit)
      {
        case MeasureUnit.Display:    return System.Drawing.GraphicsUnit.Display;
        case MeasureUnit.Document:   return System.Drawing.GraphicsUnit.Document;
        case MeasureUnit.Inch:       return System.Drawing.GraphicsUnit.Inch;
        case MeasureUnit.Millimeter: return System.Drawing.GraphicsUnit.Millimeter;
        case MeasureUnit.Pixel:      return System.Drawing.GraphicsUnit.Pixel;
        case MeasureUnit.Point:      return System.Drawing.GraphicsUnit.Point;
        case MeasureUnit.World:      return System.Drawing.GraphicsUnit.World;
        default: return System.Drawing.GraphicsUnit.World;
      }
    }

    internal static ImageFormat xlat(System.Drawing.Imaging.ImageFormat format)
    {
      if      (format.Guid.Equals(System.Drawing.Imaging.ImageFormat.Png.Guid))  return PngImageFormat.Standard;
      else if (format.Guid.Equals(System.Drawing.Imaging.ImageFormat.Gif.Guid))  return GifImageFormat.Standard;
      else if (format.Guid.Equals(System.Drawing.Imaging.ImageFormat.Jpeg.Guid)) return JpegImageFormat.Standard;
      else if (format.Guid.Equals(System.Drawing.Imaging.ImageFormat.Bmp.Guid))  return BitmapImageFormat.Standard;

      return null;
    }

  }
}
