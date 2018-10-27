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
  /// <summary>
  /// Base class for representing various formats of images, such as Hi-Quality Jpeg, or monochrome Png etc.
  /// </summary>
  public abstract class ImageFormat
  {
    public ImageFormat(int colors)
    {
      Colors = colors;
    }

    public readonly int Colors;

    public abstract string WebContentType { get;}
  }

  /// <summary>
  /// Represents Bitmap image format
  /// </summary>
  public sealed class BitmapImageFormat : ImageFormat
  {
    public static readonly BitmapImageFormat Monochrome = new BitmapImageFormat(2);

    public static readonly BitmapImageFormat Standard = new BitmapImageFormat();

    public BitmapImageFormat() : base(Int32.MaxValue) //create full resolution
    {
    }

    public BitmapImageFormat(int colors) : base(colors)
    {
    }

    public override string WebContentType { get => "image/bmp"; }
  }

  /// <summary>
  /// Represents Png image format
  /// </summary>
  public sealed class PngImageFormat : ImageFormat
  {
    public static readonly PngImageFormat Monochrome = new PngImageFormat(2);

    public static readonly PngImageFormat Standard = new PngImageFormat();


    public PngImageFormat() : base(Int32.MaxValue) //create full resolution
    {
    }

    public PngImageFormat(int colors) : base(colors)
    {

    }

    public override string WebContentType { get => "image/png"; }
  }

  /// <summary>
  /// Represents Gif image format
  /// </summary>
  public sealed class GifImageFormat : ImageFormat
  {
    public static readonly GifImageFormat Monochrome = new GifImageFormat(2);
    public static readonly GifImageFormat Standard = new GifImageFormat();

    public GifImageFormat() : base(0xff)
    {

    }

    public GifImageFormat(int colors) : base(colors)
    {

    }

    public override string WebContentType { get => "image/gif"; }
  }


  /// <summary>
  /// Represents Jpeg image format of the specified quality
  /// </summary>
  public sealed class JpegImageFormat : ImageFormat
  {
    /// <summary>
    /// Standard Jpeg compression of 80 quality
    /// </summary>
    public static readonly JpegImageFormat Standard = new JpegImageFormat(80);

    public JpegImageFormat(int quality) : base(Int32.MaxValue)
    {
      if (quality<0 || quality>100)
        throw new GraphicsException(StringConsts.ARGUMENT_ERROR+"{0}.ctor(quality<0|>100)".Args(nameof(JpegImageFormat)));

      Quality = quality;
    }

    public readonly int Quality;

    public override string WebContentType { get => "image/jpeg"; }
  }
}
