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

    protected override bool DoApplicationAfterInit()//add a comment
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

    public async Task<IEnumerable<Atom>> GetVolumeNamesAsync(Atom space)
    {
      space.HasRequiredValue(nameof(space));
      var uri = new UriQueryBuilder("volumes")
               .Add("space", space)
               .ToString();

      var response = await m_Server.Call(BlobServiceAddress,
                                         nameof(IBlobStore),
                                         new ShardKey(DateTime.UtcNow),
                                         async (http, ct) => await http.Client.GetJsonMapAsync(uri).ConfigureAwait(false));
      var result = response.UnwrapPayloadArray()
              .OfType<string>()
              .Select(sn => Atom.Encode(sn));

      return result;
    }

    public async Task<IEnumerable<BlobInfo>> FindBlobsAsync(BlobFilter filter)
    {
      filter.AsValidIn(App, nameof(filter));

      var response = await m_Server.Call(BlobServiceAddress,
                                         nameof(IBlobStore),
                                         new ShardKey(DateTime.UtcNow),
                                         (http, ct) => http.Client.PostAndGetJsonMapAsync("filter", new { filter = filter })).ConfigureAwait(false);

      var result = response.UnwrapPayloadArray()
              .OfType<JsonDataMap>()
              .Select(imap => JsonReader.ToDoc<BlobInfo>(imap));

      return result;
    }

    public async Task<BlobHandle> CreateAsync(EntityId id, IConfigSectionNode permissions = null, IConfigSectionNode headers = null, IEnumerable<Tag> tags = null, DateTime? endUtc = null, int blockSize = 0)
    {
      var response = await m_Server.Call(BlobServiceAddress,
                                        nameof(IBlobStore),
                                        new ShardKey(DateTime.UtcNow),
                                        (http, ct) => http.Client.PostAndGetJsonMapAsync("handle",
                                        new { descriptor = new
                                              {
                                                id = id.HasRequiredValue(nameof(id)),
                                                permissions,
                                                headers,
                                                tags,
                                                endUtc,
                                                blockSize
                                              }
                                        })).ConfigureAwait(false);

      var bhd = response.UnwrapPayloadDoc<BlobHandleDescriptor>();

      var result = new BlobHandle(this, bhd, false);
      return result;
    }

    public async Task<BlobHandle> OpenAsync(EntityId id, bool readOnly = false)
    {
      id.HasRequiredValue(nameof(id));
      var uri = new UriQueryBuilder("handle")
               .Add("id", id)
               .ToString();

      var response = await m_Server.Call(BlobServiceAddress,
                                         nameof(IBlobStore),
                                         new ShardKey(DateTime.UtcNow),
                                         async (http, ct) => await http.Client.GetJsonMapAsync(uri).ConfigureAwait(false));
      var bhd = response.UnwrapPayloadDoc<BlobHandleDescriptor>();

      var result = new BlobHandle(this, bhd, readOnly);
      return result;
    }

    public async Task<ChangeResult> UpdateAsync(EntityId id, IConfigSectionNode headers, IEnumerable<Tag> tags, DateTime? endUtc)
    {
      var response = await m_Server.Call(BlobServiceAddress,
                                        nameof(IBlobStore),
                                        new ShardKey(DateTime.UtcNow),
                                        (http, ct) => http.Client.PutAndGetJsonMapAsync("blob",
                                        new
                                        {
                                          descriptor = new
                                          {
                                            id = id.HasRequiredValue(nameof(id)),
                                            headers,
                                            tags,
                                            endUtc
                                          }
                                        })).ConfigureAwait(false);

      var result = response.UnwrapChangeResult();
      return result;
    }

    public async Task<ChangeResult> DeleteAsync(EntityId id)
    {
      var response = await m_Server.Call(BlobServiceAddress,
                                        nameof(IBlobStore),
                                        new ShardKey(DateTime.UtcNow),
                                        (http, ct) => http.Client.DeleteAndGetJsonMapAsync("blob",
                                        new
                                        {
                                          id = id.HasRequiredValue(nameof(id)),
                                        })).ConfigureAwait(false);

      var result = response.UnwrapChangeResult();
      return result;
    }

    public async Task<(VolatileBlobInfo info, byte[] data)> ReadBlockAsync(RGDID rgBlob, long offset, int count, CancellationToken cancellationToken)
    {
      rgBlob.HasRequiredValue(nameof(rgBlob));
      (offset >= 0 && count >= 0).IsTrue("offset>=0 && count>=0");

      var uri = new UriQueryBuilder("block")
               .Add("rgBlob", rgBlob)
               .Add("offset", offset)
               .Add("count", count)
               .ToString();

      var raw = await m_Server.Call(BlobServiceAddress,
                                         nameof(IBlobStore),
                                         new ShardKey(DateTime.UtcNow),
                                         async (http, ct) => await http.Client.CallAndGetByteArrayAsync(uri, System.Net.Http.HttpMethod.Get, null).ConfigureAwait(false));

      var result = Constraints.UnpackServerBlockResponse(raw);
      return result;
    }

    public async Task<VolatileBlobInfo> WriteBlockAsync(RGDID rgBlob, long offset, ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
      rgBlob.HasRequiredValue(nameof(rgBlob));
      (offset >= 0).IsTrue("offset>=0");
      buffer.Array.NonNull(nameof(buffer));

      var uri = new UriQueryBuilder("block")
               .Add("rgBlob", rgBlob)
               .Add("offset", offset)
               .ToString();

      var response = await m_Server.Call(BlobServiceAddress,
                                         nameof(IBlobStore),
                                         new ShardKey(DateTime.UtcNow),
                                         async (http, ct) => await http.Client.PostAndGetJsonMapAsync(uri, buffer).ConfigureAwait(false));

      var result = response.UnwrapPayloadDoc<VolatileBlobInfo>();
      return result;
    }

  }
}
