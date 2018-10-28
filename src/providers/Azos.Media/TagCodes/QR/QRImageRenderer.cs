/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

/*
 *Based on zXing / Apache 2.0; See NOTICE and CHANGES for attribution
 */

using System.Drawing;


using Azos.Graphics;

namespace Azos.Media.TagCodes.QR
{

  public static class QRImageRenderer
  {
    #region CONSTS

      private static Color DEFAULT_TRUE_COLOR = Color.FromArgb(0, 0, 0);
      private static Color DEFAULT_FALSE_COLOR = Color.FromArgb(0, 0, 0);

    #endregion

    #region Inner Types

      public enum ImageScale { Scale1x = 1, Scale2x = 2, Scale3x = 3, Scale4x = 4, Scale5x = 5, Scale6x = 6, Scale7x = 7, Scale8x = 8};

    #endregion

    #region Public

      public static void ToBMP(this QRMatrix matrix,
                               System.IO.Stream stream,
                               Color? trueColor = null,
                               Color? falseColor = null,
                               ImageScale? scale = ImageScale.Scale1x)
      {
        var output = createOutput(matrix, trueColor, falseColor, scale);
        output.Save(stream, BitmapImageFormat.Monochrome);
      }

      public static void ToPNG(this QRMatrix matrix,
                               System.IO.Stream stream,
                               Color? trueColor = null,
                               Color? falseColor = null,
                               ImageScale? scale = ImageScale.Scale1x)
      {
        var output = createOutput(matrix, trueColor, falseColor, scale);
        output.Save(stream, PngImageFormat.Monochrome);
      }

      public static void ToJPG(this QRMatrix matrix,
                               System.IO.Stream stream,
                               Color? trueColor = null,
                               Color? falseColor = null,
                               ImageScale? scale = ImageScale.Scale1x)
      {
        var output = createOutput(matrix, trueColor, falseColor, scale);
        output.Save(stream, JpegImageFormat.Standard);
      }

      public static void ToGIF(this QRMatrix matrix,
                               System.IO.Stream stream,
                               Color? trueColor = null,
                               Color? falseColor = null,
                               ImageScale? scale = ImageScale.Scale1x)
      {
        var output = createOutput(matrix, trueColor, falseColor, scale);
        output.Save(stream, GifImageFormat.Monochrome);
      }

      public static Image CreateOutput(this QRMatrix matrix,
                                       Color? trueColor = null,
                                       Color? falseColor = null,
                                       ImageScale? scale = ImageScale.Scale1x)
      {
        return createOutput(matrix, trueColor, falseColor, scale);
      }

    #endregion

    #region .pvt. impl.

      private static Image createOutput(QRMatrix matrix,
                                        Color? trueColor = null,
                                        Color? falseColor = null,
                                        ImageScale? scale = ImageScale.Scale1x)
      {
        var black = trueColor ?? Color.Black;
        var white = falseColor ?? Color.White;

        if (black == white)
          throw new AzosException(StringConsts.ARGUMENT_ERROR + typeof(QRImageRenderer).Name + ".ToBitmap(trueColor!=falseColor)");

        int scaleFactor = (int)scale;

        int canvasWidth = matrix.Width * scaleFactor;
        int canvasHeight = matrix.Height * scaleFactor;

        var result = Image.Of(canvasWidth, canvasHeight);

        using (var canvas = result.CreateCanvas())
        using (var blackBrush = canvas.CreateSolidBrush(black))
        using (var whiteBrush = canvas.CreateSolidBrush(white))
        {
          canvas.FillRectangle(blackBrush, 0, 0, canvasWidth, canvasHeight);

          for (int yMatrix=0; yMatrix<matrix.Height; yMatrix++)
          for (int xMatrix=0; xMatrix<matrix.Width;  xMatrix++)
          {
            if (matrix[xMatrix, yMatrix] == 0)
            {
               int scaledX = xMatrix * scaleFactor;
               int scaledY = yMatrix * scaleFactor;

               canvas.FillRectangle(whiteBrush, scaledX, scaledY, scaleFactor, scaleFactor);
            }
          }

          return result;
        }
      }

    #endregion

  }//class

}
