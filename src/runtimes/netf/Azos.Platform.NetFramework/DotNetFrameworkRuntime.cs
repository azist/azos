
using Azos.Platform.Abstraction.Graphics;

namespace Azos.Platform.Abstraction.NetFramework
{
  /// <summary>
  /// Represents .NET Framework platform runtime
  /// </summary>
  public sealed class DotNetFrameworkRuntime : PALImplementation
  {
    public DotNetFrameworkRuntime() : this(false)
    {
      PlatformAbstractionLayer.____SetImplementation(this);
    }

    internal DotNetFrameworkRuntime(bool testMode) : base()
    {
      m_Machine  = new PALMachineInfo();
      m_FS       = new PALFileSystem();
      m_Graphics = new Graphics.NetGraphics();
    }

    private PALMachineInfo m_Machine;
    private PALFileSystem m_FS;
    private IPALGraphics m_Graphics;

    public override string Name => nameof(DotNetFrameworkRuntime);
    public override bool IsNetCore => false;
    public override IPALFileSystem FileSystem => m_FS;
    public override IPALMachineInfo MachineInfo => m_Machine;
    public override IPALGraphics Graphics => m_Graphics;
  }
}
