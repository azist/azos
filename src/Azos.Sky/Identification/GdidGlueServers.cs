using System;

using Azos.Data;
using Azos.Sky.Contracts;

namespace Azos.Sky.Identification
{
  /// <summary>
  /// Implements GDIDAuthority contract trampoline that uses a singleton instance of GDIDAuthorityService to
  ///  allocate blocks
  /// </summary>
  public sealed class GdidAuthority : IGdidAuthority
  {
    /// <summary>
    /// Implements IGDIDAuthority contract - allocates block
    /// </summary>
    public GdidBlock AllocateBlock(string scopeName, string sequenceName, int blockSize, ulong? vicinity = GDID.COUNTER_MAX)
    {
      Instrumentation.AuthAllocBlockCalledEvent.Happened(scopeName, sequenceName);
      return GdidAuthorityService.Instance.AllocateBlock(scopeName, sequenceName, blockSize, vicinity);
    }
  }

  /// <summary>
  /// Implements IGdidPersistenceLocation contract trampoline that uses a singleton instance of GDIDPersistenceLocationService to
  ///  store gdids
  /// </summary>
  public sealed class GdidPersistenceRemoteLocation : IGdidPersistenceRemoteLocation
  {
    public GDID? Read(byte authority, string sequenceName, string scopeName)
    {
      throw new NotImplementedException();
    }

    public void Write(string sequenceName, string scopeName, GDID value)
    {
      throw new NotImplementedException();
    }
  }


}
