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
