/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Drawing;

using Azos.Platform.Abstraction.Graphics;

namespace Azos.Platform.Abstraction.NetFramework.Graphics
{
  /// <summary>
  /// Implements brush using .NET framework GDI+ wrapper
  /// </summary>
  public sealed class NetBrush : DisposableObject, IPALCanvasBrush
  {
    internal NetBrush(Color color)
    {
      m_Brush = new SolidBrush(color);
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Brush);
    }

    private SolidBrush m_Brush;

    internal Brush GDIBrush => m_Brush;
    public Color Color => m_Brush.Color;
  }
}
