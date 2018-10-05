/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Azos.Platform.Abstraction.Graphics;

namespace Azos.Graphics
{
  public sealed partial class Canvas : DisposableObject
  {
    public sealed class Brush : Asset<IPALCanvasBrush>
    {
      internal Brush(IPALCanvasBrush handle) : base(handle)
      {

      }

      public Color Color => Handle.Color;
    }
  }
}
