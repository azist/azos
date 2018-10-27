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
  public class ImageUtilsTests : GraphicsTestBase
  {
    #region CONSTS

      public const string TEST_JPG = "test_mc.jpg";
      public const string TEST_PNG = "test_mc.png";

    #endregion

    [Run]
    public void ExtractMainColors_TestPNG()
    {
      using (var stream = GetResource(TEST_PNG))
      using (var img = Image.FromStream(stream))
      {
        var colors = ImageUtils.ExtractMainColors(img);

        Aver.AreEqual(4, colors.Length);
        Aver.AreObjectsEqual(Color.FromArgb(240, 190, 2),   colors[0]);
        Aver.AreObjectsEqual(Color.FromArgb(147, 213, 206), colors[1]);
        Aver.AreObjectsEqual(Color.FromArgb(26, 168, 74),   colors[2]);
        Aver.AreObjectsEqual(Color.FromArgb(144, 72, 143),  colors[3]);
      }
    }

    [Run]
    public void ExtractMainColors_TestJPG()
    {
      using (var stream = GetResource(TEST_JPG))
      using (var img = Image.FromStream(stream))
      {
        var colors = ImageUtils.ExtractMainColors(img);

        Aver.AreEqual(4, colors.Length);
        Aver.AreObjectsEqual(Color.FromArgb(216, 24, 25),   colors[0]);
        Aver.AreObjectsEqual(Color.FromArgb(55, 64, 176),   colors[1]);
        Aver.AreObjectsEqual(Color.FromArgb(185, 210, 30),  colors[2]);
        Aver.AreObjectsEqual(Color.FromArgb(240, 240, 232), colors[3]);
      }
    }
  }
}
