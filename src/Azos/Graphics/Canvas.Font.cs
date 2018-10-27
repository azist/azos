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
    public sealed class Font : Asset<IPALCanvasFont>
    {
      internal Font(IPALCanvasFont handle) : base(handle)
      {

      }
      public string Name => Handle.Name;
      public float Size => Handle.Size;
      public FontStyling Style => Handle.Style;
      public MeasureUnit Unit => Handle.Unit;
    }
  }
}
