/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.IO;
using System.Drawing;

using Azos.Graphics;
using Azos.Platform.Abstraction.Graphics;

namespace Azos.Platform.Abstraction.NetCore.Graphics
{
  public sealed class CoreGraphics : IPALGraphics
  {
    public bool CanvasOwnsAssets => false;

    public IPALImage CreateImage(string fileName)
    {
      throw new NetCorePALException("Not implemented");
    }

    public IPALImage CreateImage(byte[] data)
    {
      throw new NetCorePALException("Not implemented");
    }

    public IPALImage CreateImage(Stream stream)
    {
      throw new NetCorePALException("Not implemented");
    }

    public IPALImage CreateImage(Size size, Size resolution, ImagePixelFormat pixFormat)
    {
      throw new NetCorePALException("Not implemented");
    }

  }
}
