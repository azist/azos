using System;
using System.Collections.Generic;
using System.Text;
using Azos.Collections;
using Azos.Sky.Contracts;

namespace Azos.Sky.Identification
{
  /// <summary>
  /// Implements a GDID generation authority accessor which generates GDIDs locally in memory.
  /// This is used exclusively for testing since the generated gdids are not really unique
  /// </summary>
  public sealed class MockGdidAuthorityAccessor : IGdidAuthorityAccessor
  {
    private NamedInterlocked m_Data = new NamedInterlocked();


    public GdidBlock AllocateBlock(string scopeName, string sequenceName, int blockSize, ulong? vicinity = 1152921504606846975)
    {
      var key = scopeName+"::"+sequenceName;

      var start = m_Data.AddLong(key, blockSize);

      return new GdidBlock
      {
        ScopeName = scopeName,
        SequenceName = sequenceName,
        Authority = 1,
        AuthorityHost = "/localhost",
        Era = 0,
        StartCounterInclusive = (ulong)start,
        BlockSize = blockSize,
        ServerUTCTime = Ambient.UTCNow
      };
    }
  }
}
