/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
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
