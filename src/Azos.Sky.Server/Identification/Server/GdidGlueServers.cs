/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Sky.Contracts;

namespace Azos.Sky.Identification.Server
{

  /// <summary>
  /// Implements GDIDAuthority contract trampoline that uses a IGdidAuthorityModule to allocate blocks
  /// </summary>
  public sealed class GdidAuthority : IGdidAuthority
  {
    [Inject] IGdidAuthorityModule m_Authority;

    /// <summary>
    /// Implements IGDIDAuthority contract - allocates block
    /// </summary>
    public GdidBlock AllocateBlock(string scopeName, string sequenceName, int blockSize, ulong? vicinity = GDID.COUNTER_MAX)
    {
      Instrumentation.AuthAllocBlockCalledEvent.Happened(m_Authority.App.Instrumentation, scopeName, sequenceName);
      return m_Authority.AllocateBlock(scopeName, sequenceName, blockSize, vicinity);
    }
  }
}
