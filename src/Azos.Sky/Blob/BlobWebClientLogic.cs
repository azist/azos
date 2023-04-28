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
using Azos.Client;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Adlib;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Sky.Blob
{
  /// <summary>
  /// Provides client for consuming <see cref="IBlobStoreLogic"/> remote services
  /// </summary>
  public sealed class BlobStoreWebClientLogic : ModuleBase, IBlobStoreLogic
  {
    public const string CONFIG_SERVICE_SECTION = "service";

    public BlobStoreWebClientLogic(IApplication application) : base(application) { }
    public BlobStoreWebClientLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }

    private HttpService m_Server;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;


    /// <summary>
    /// Logical service address of <see cref="IBlobStore"/> server
    /// </summary>
    [Config]
    public string BlobServiceAddress{  get; set; }

    public bool IsServerImplementation => false;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Server);
      if (node == null) return;

      var nServer = node[CONFIG_SERVICE_SECTION];
      m_Server = FactoryUtils.MakeDirectedComponent<HttpService>(this,
                                                                 nServer,
                                                                 typeof(HttpService),
                                                                 new object[] { nServer });
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Server.NonNull("Not configured Server of config section `{0}`".Args(CONFIG_SERVICE_SECTION));
      BlobServiceAddress.NonBlank(nameof(BlobServiceAddress));

      return base.DoApplicationAfterInit();
    }

    public async Task<IEnumerable<Atom>> GetSpaceNamesAsync()
    {
      var response = await m_Server.Call(BlobServiceAddress,
                                          nameof(IBlobStore),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.GetJsonMapAsync("spaces").ConfigureAwait(false));
      var result = response.UnwrapPayloadArray()
              .OfType<string>()
              .Select(sn => Atom.Encode(sn));

      return result;
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

    public Task<BlobHandle> OpenAsync(EntityId id, int bufferSize = 0, bool readOnly = false)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> UpdateAsync(EntityId id, IConfigSectionNode headers, IEnumerable<Tag> tags, DateTime? endDateUtc)
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

  }
}
