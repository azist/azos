/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Platform.Abstraction.Graphics;

namespace Azos.Platform.Abstraction.NetCore
{

  //https://stackoverflow.com/questions/3769405/determining-cpu-utilization

  /// <summary>
  /// Represents NET Core 2.0 Runtime
  /// </summary>
  public sealed class NetCore20Runtime : PALImplementation
  {
    public NetCore20Runtime() : this(false)
     => PlatformAbstractionLayer.____SetImplementation(this);

    internal NetCore20Runtime(bool testMode) : base()
    {
      m_Machine = new CoreMachineInfo();
      m_FS = new CoreFileSystem();
      m_Graphics = new Graphics.CoreGraphics();
      m_Cryptography = new CoreCryptography();
    }

    private CoreMachineInfo m_Machine;
    private CoreFileSystem m_FS;
    private Graphics.CoreGraphics m_Graphics;
    private CoreCryptography m_Cryptography;

    public override string Name => nameof(NetCore20Runtime);
    public override bool IsNetCore => false;
    public override IPALFileSystem   FileSystem => m_FS;
    public override IPALMachineInfo  MachineInfo => m_Machine;
    public override IPALGraphics     Graphics => m_Graphics;
    public override IPALCryptography Cryptography => m_Cryptography;
  }
}
