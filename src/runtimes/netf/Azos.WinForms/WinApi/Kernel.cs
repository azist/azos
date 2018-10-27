/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
