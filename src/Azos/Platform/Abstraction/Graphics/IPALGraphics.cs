/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

using Azos.Graphics;

namespace Azos.Platform.Abstraction.Graphics
{
  /// <summary>
  /// Provides functions for working with Images
  /// </summary>
  public interface IPALGraphics
  {
    bool CanvasOwnsAssets { get;}

    IPALImage CreateImage(string fileName);
    IPALImage CreateImage(byte[] data);
    IPALImage CreateImage(Stream stream);
    IPALImage CreateImage(Size size, Size resolution, ImagePixelFormat pixFormat);
  }
}
