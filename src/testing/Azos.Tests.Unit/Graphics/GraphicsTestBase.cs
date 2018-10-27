/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Drawing;
using System.Reflection;
using Azos.Graphics;
using Azos.Scripting;

namespace Azos.Tests.Unit.Graphics
{
  public static class TestExtensions
  {
    public static Image ToImage(this Color[,] map, ImagePixelFormat format = ImagePixelFormat.RGBA32)
    {
      var H = map.GetLength(0);
      var W = map.GetLength(1);
      var result = Image.Of(W, H);

      for (int h=0; h<H; h++)
      for (int w=0; w<W; w++)
        result.SetPixel(w, h, map[h, w]);

      return result;
    }
  }

  public abstract class GraphicsTestBase : IRunHook
  {
    protected Color R  = Color.FromArgb(255,   0,   0);
    protected Color G  = Color.FromArgb(  0, 255,   0);
    protected Color B  = Color.FromArgb(  0,   0, 255);

    protected Image TestImg1 { get; private set; }
    protected Image TestImg2 { get; private set; }

    #region Runnable Setup

      bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
      {
        TestImg1 = new Color[4, 8]
        {
          { G, B, R, B, B, R, B, G },
          { G, B, B, B, B, B, B, G },
          { G, B, R, B, B, R, B, G },
          { G, G, B, R, R, B, G, G }
        }.ToImage();

        TestImg2 = new Color[3, 3]
        {
          { G, B, R},
          { G, B, B},
          { G, G, G}
        }.ToImage();

        return false;
      }

      bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
      {
        return false;//<--- The exception is NOT handled here, do default handling
      }

    #endregion


  }
}
