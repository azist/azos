using System;

using Azos.Apps.Injection;
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

    [Inject] IApplication m_App;

    public GdidAuthorityService Service => m_App.NonNull(nameof(m_App))
                                              .Singletons
                                              .Get<GdidAuthorityService>()
                                              .NonNull(nameof(GdidAuthorityService));

    /// <summary>
    /// Implements IGDIDAuthority contract - allocates block
    /// </summary>
    public GdidBlock AllocateBlock(string scopeName, string sequenceName, int blockSize, ulong? vicinity = GDID.COUNTER_MAX)
    {
      Instrumentation.AuthAllocBlockCalledEvent.Happened(m_App.Instrumentation, scopeName, sequenceName);
      return Service.AllocateBlock(scopeName, sequenceName, blockSize, vicinity);
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
