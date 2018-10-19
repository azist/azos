using System.Drawing;

using Azos.Graphics;
using Azos.Platform.Abstraction.Graphics;

namespace Azos.Platform.Abstraction.NetFramework.Graphics
{
  /// <summary>
  /// Implements pen using .NET framework GDI+ wrapper
  /// </summary>
  public sealed class NetPen : DisposableObject, IPALCanvasPen
  {
    internal NetPen(Color color, float width, PenDashStyle style)
    {
      m_Pen = new Pen(color, width);
      m_Pen.DashStyle = xlator.xlat(style);
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Pen);
    }

    private Pen m_Pen;

    internal Pen GDIPen => m_Pen;

    public Color Color => m_Pen.Color;

    public float Width => m_Pen.Width;

    public PenDashStyle DashStyle => xlator.xlat(m_Pen.DashStyle);
  }
}
