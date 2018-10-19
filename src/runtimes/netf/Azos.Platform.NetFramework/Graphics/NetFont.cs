using System.Drawing;

using Azos.Graphics;
using Azos.Platform.Abstraction.Graphics;

namespace Azos.Platform.Abstraction.NetFramework.Graphics
{
  /// <summary>
  /// Implements font using .NET framework GDI+ wrapper
  /// </summary>
  public sealed class NetFont : DisposableObject, IPALCanvasFont
  {
    internal NetFont(string name, float size, FontStyling style, MeasureUnit unit)
    {
      m_Font = new Font(name, size, xlator.xlat(style), xlator.xlat(unit));
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Font);
    }

    private Font m_Font;

    internal Font GDIFont => m_Font;

    public string Name => m_Font.Name;

    public float Size => m_Font.Size;

    public FontStyling Style => xlator.xlat(m_Font.Style);

    public MeasureUnit Unit => xlator.xlat(m_Font.Unit);
  }
}
