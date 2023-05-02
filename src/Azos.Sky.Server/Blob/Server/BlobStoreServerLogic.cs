/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Adlib;
using Azos.Data.Idgen;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Sky.Identification;

namespace Azos.Sky.Blob.Server
{
  public sealed class BlobStoreServerLogic : ModuleBase, IBlobStoreLogic
  {
    public BlobStoreServerLogic(IApplication application) : base(application) { }
    public BlobStoreServerLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      //DisposeAndNull(ref m_LogArchiveGraph);
      base.Destructor();
    }

    [Inject] IGdidProviderModule m_Gdid;

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;
    public bool IsServerImplementation => true;


    #region IBlobStoreLogic
    public Task<IEnumerable<Atom>> GetSpaceNamesAsync()
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<Atom>> GetVolumeNamesAsync(Atom space)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<BlobInfo>> FindBlobsAsync(BlobFilter filter)
    {
      throw new NotImplementedException();
    }

    public Task<BlobHandle> CreateAsync(EntityId id, IConfigSectionNode permissions = null, IConfigSectionNode headers = null, IEnumerable<Tag> tags = null, DateTime? endUtc = null, int blockSize = 0)
    {
      throw new NotImplementedException();
    }

    public Task<BlobHandle> OpenAsync(EntityId id, bool readOnly = false)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> UpdateAsync(EntityId id, IConfigSectionNode headers, IEnumerable<Tag> tags, DateTime? endUtc)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> DeleteAsync(EntityId id)
    {
      throw new NotImplementedException();
    }

    public Task<(VolatileBlobInfo info, byte[] data)> ReadBlockAsync(RGDID rgBlob, long offset, int count, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();
    }

    public Task<VolatileBlobInfo> WriteBlockAsync(RGDID rgBlob, long offset, ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
