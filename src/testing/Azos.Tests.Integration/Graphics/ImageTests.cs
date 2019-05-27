/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Azos.Graphics;
using Azos.Scripting;

namespace Azos.Tests.Integration.Graphics
{
  [Runnable]
  public class ImageTests : IRunHook
  {
    #region CONSTS

      public const string TEST_JPG_FILENAME = @"Graphics/test.jpg";
      public const string TEST_PNG_FILENAME = @"Graphics/test.png";
      public const string STUB_JPG_FILENAME = @"Graphics/__saved.jpg";
      public const string STUB_PNG_FILENAME = @"Graphics/__saved.png";

    #endregion

    #region Init/TearDown

      bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
      {
        if (File.Exists(STUB_JPG_FILENAME)) File.Delete(STUB_JPG_FILENAME);
        if (File.Exists(STUB_PNG_FILENAME)) File.Delete(STUB_PNG_FILENAME);
        return false; //<--- The exception is NOT handled here, do default handling
      }

      bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
      {
        if (File.Exists(STUB_JPG_FILENAME)) File.Delete(STUB_JPG_FILENAME);
        if (File.Exists(STUB_PNG_FILENAME)) File.Delete(STUB_PNG_FILENAME);
        return false; //<--- The exception is NOT handled here, do default handling
      }

    #endregion

    [Run]
    public void Image_FromFile()
    {
      using (var img = Image.FromFile(TEST_PNG_FILENAME))
      {
        averTestImage(img);
      }
    }

    [Run]
    public void Image_FromStream()
    {
      using (var file = File.OpenRead(TEST_PNG_FILENAME))
      using (var img = Image.FromStream(file))
      {
        averTestImage(img);
      }
    }

    [Run]
    public void Image_FromBytes()
    {
      using (var file = File.OpenRead(TEST_PNG_FILENAME))
      using (var ms = new MemoryStream())
      {
        file.CopyTo(ms);

        using (var img = Image.FromBytes(ms.ToArray()))
        {
          averTestImage(img);
        }
      }
    }

    [Run]
    public void Image_SaveToFile()
    {
      using (var imgSrc = Image.FromFile(TEST_PNG_FILENAME))
      {
        imgSrc.Save(STUB_PNG_FILENAME, new PngImageFormat());

        using (var imgTrg = Image.FromFile(STUB_PNG_FILENAME))
        {
          averTestImage(imgTrg);
        }
      }
    }

    [Run]
    public void Image_SaveToStream()
    {
      using (var ms = new MemoryStream())
      using (var imgSrc = Image.FromFile(TEST_PNG_FILENAME))
      {
        imgSrc.Save(ms, new PngImageFormat(16));

        using (var imgTrg = Image.FromStream(ms))
        {
          averTestImage(imgTrg);
        }
      }
    }

    [Run]
    public void Image_SaveToBytes()
    {
      using (var ms = new MemoryStream())
      using (var imgSrc = Image.FromFile(TEST_PNG_FILENAME))
      {
        imgSrc.Save(ms, new PngImageFormat(16));

        using (var imgTrg = Image.FromBytes(ms.ToArray()))
        {
          averTestImage(imgTrg);
        }
      }
    }

    #region .pvt

      private void averTestImage(Image img)
      {
        //[ g, b, r, b, b, r, b, g ]
        //[ g, b, b, b, b, b, b, g ]
        //[ g, b, r, b, b, r, b, g ]
        //[ g, g, b, r, r, b, g, g ]

        var r = Color.FromArgb(255, 0, 0);
        var g = Color.FromArgb(0, 255, 0);
        var b = Color.FromArgb(0, 0, 255);

        Aver.AreEqual(8, img.Width);
        Aver.AreEqual(4, img.Height);

        Aver.AreObjectsEqual(ImagePixelFormat.RGBA32, img.PixelFormat);

        Aver.AreObjectsEqual(g, img.GetPixel(0, 0));
        Aver.AreObjectsEqual(b, img.GetPixel(1, 0));
        Aver.AreObjectsEqual(r, img.GetPixel(2, 0));
        Aver.AreObjectsEqual(b, img.GetPixel(3, 0));
        Aver.AreObjectsEqual(g, img.GetPixel(7, 0));
        Aver.AreObjectsEqual(g, img.GetPixel(0, 1));
        Aver.AreObjectsEqual(b, img.GetPixel(2, 1));
        Aver.AreObjectsEqual(b, img.GetPixel(5, 1));
        Aver.AreObjectsEqual(g, img.GetPixel(7, 1));
        Aver.AreObjectsEqual(b, img.GetPixel(1, 2));
        Aver.AreObjectsEqual(b, img.GetPixel(3, 2));
        Aver.AreObjectsEqual(r, img.GetPixel(5, 2));
        Aver.AreObjectsEqual(b, img.GetPixel(2, 3));
        Aver.AreObjectsEqual(r, img.GetPixel(4, 3));
        Aver.AreObjectsEqual(g, img.GetPixel(6, 3));
      }

    #endregion
  }
}
