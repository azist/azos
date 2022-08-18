/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Scripting;
using Azos.Sky.Contracts;

namespace Azos.Sky.Identification
{
  /// <summary>
  /// Implements a GDID generation authority accessor which generates GDIDs locally in memory.
  /// This is used exclusively for testing since the generated gdids are not really unique
  /// </summary>
  public sealed class MockGdidAuthorityAccessor : ApplicationComponent,  IGdidAuthorityAccessor
  {

    public MockGdidAuthorityAccessor(IApplication app) : base(app){ }
    public MockGdidAuthorityAccessor(IApplicationComponent director) : base(director) { }


    private NamedInterlocked m_Data = new NamedInterlocked();

    public override string ComponentLogTopic => CoreConsts.TOPIC_ID_GEN;

    public Task<GdidBlock> AllocateBlockAsync(string scopeName, string sequenceName, int blockSize, ulong? vicinity = 1152921504606846975)
    {
      const int MAX_BLOCK=12000;

      var key = scopeName+"::"+sequenceName;
      if (blockSize>MAX_BLOCK) blockSize = MAX_BLOCK;

"FETCHED!!!!!!!!!!!!!!!!!!!!!!!".See();

      var start = m_Data.AddLong(key, blockSize);

      return Task.FromResult( new GdidBlock
      {
        ScopeName = scopeName,
        SequenceName = sequenceName,
        Authority = 1,
        AuthorityHost = "/localhost",
        Era = 0,
        StartCounterInclusive = (ulong)(start - blockSize),
        BlockSize = blockSize,
        ServerUTCTime = Ambient.UTCNow
      });
    }

    public void Configure(IConfigSectionNode node)
    {
    }
  }
}
