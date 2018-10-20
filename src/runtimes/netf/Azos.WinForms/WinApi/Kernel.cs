
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Azos.WinApi
{
  /// <summary>
  /// Provides managed wrappers to Windows Kernel.dll
  /// </summary>
  public static class KernelApi
  {
    private const string KERNEL32 = "KERNEL32.DLL";

    [DllImport(KERNEL32)]
    public static extern bool Beep(UInt32 frequency, UInt32 duration);

  }
}
