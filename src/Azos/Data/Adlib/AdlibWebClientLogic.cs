/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Client;
using Azos.Conf;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Data.Adlib
{
  /// <summary>
  /// Provides client for consuming IAdlibLogic remote services
  /// </summary>
  public sealed class AdlibWebClientLogic : ModuleBase, IAdlibLogic
  {
    public const string CONFIG_SERVICE_SECTION = "service";

    public AdlibWebClientLogic(IApplication application) : base(application) { }
    public AdlibWebClientLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }

    private HttpService m_Server;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;


    /// <summary>
    /// Logical service address of Adlib server
    /// </summary>
    [Config]
    public string AdlibServiceAddress{  get; set; }

    public bool IsServerImplementation => throw new NotImplementedException();

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
      AdlibServiceAddress.NonBlank(nameof(AdlibServiceAddress));

      return base.DoApplicationAfterInit();
    }

    public async Task<IEnumerable<Atom>> GetSpaceNamesAsync()
    {
      var response = await m_Server.Call(AdlibServiceAddress,
                                          nameof(IAdlibLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.GetJsonMapAsync("spaces").ConfigureAwait(false));
      var result = response.UnwrapPayloadArray()
              .OfType<string>()
              .Select(sn => Atom.Encode(sn));

      return result;
    }

    public async Task<IEnumerable<Atom>> GetCollectionNamesAsync(Atom space)
    {
      space.HasRequiredValue(nameof(space));
      var uri = new UriQueryBuilder("collections")
               .Add("space", space)
               .ToString();

      var response = await m_Server.Call(AdlibServiceAddress,
                                         nameof(IAdlibLogic),
                                         new ShardKey(DateTime.UtcNow),
                                         async (http, ct) => await http.Client.GetJsonMapAsync(uri).ConfigureAwait(false));
      var result = response.UnwrapPayloadArray()
              .OfType<string>()
              .Select(sn => Atom.Encode(sn));

      return result;
    }


    public async Task<IEnumerable<Item>> GetListAsync(ItemFilter filter)
    {
      filter.AsValidIn(App, nameof(filter));

      ShardKey skey;
      if (filter.ShardTopic.IsNotNullOrWhiteSpace()) skey = new ShardKey(filter.ShardTopic);
      else if (!filter.Gdid.IsZero) skey = new ShardKey(Constraints.GdidToShardKey(filter.Gdid));
      else skey = new ShardKey();

      var result = await (skey.Assigned ? getOneShard(filter, skey)
                                        : getCrossShard(filter)).ConfigureAwait(false);
      return result;
    }

    private async Task<IEnumerable<Item>> getOneShard(ItemFilter filter, ShardKey skey)
    {
      var response = await m_Server.Call(AdlibServiceAddress,
                                         nameof(IAdlibLogic),
                                         skey,
                                         (http, ct) => http.Client.PostAndGetJsonMapAsync("filter", new { filter = filter })).ConfigureAwait(false);

      var result = response.UnwrapPayloadArray()
              .OfType<JsonDataMap>()
              .Select(imap => JsonReader.ToDoc<Item>(imap));

      return result;
    }

    private async Task<IEnumerable<Item>> getCrossShard(ItemFilter filter)
    {
      var shards = m_Server.GetEndpointsForAllShards(AdlibServiceAddress, nameof(IAdlibLogic));

      var calls = shards.Select(shard => shard.Call((http, ct) => http.Client.PostAndGetJsonMapAsync("filter", new { filter = filter })));

      var responses = await Task.WhenAll(calls.Select(async call => {
        try
        {
          return await call.ConfigureAwait(false);
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Warning, nameof(getCrossShard), "Shard fetch error: " + error.ToMessageWithType(), error);
          return null;
        }
      })).ConfigureAwait(false);

      var result = responses.SelectMany(response => response.UnwrapPayloadArray()
                                                            .OfType<JsonDataMap>()
                                                            .Select(imap => JsonReader.ToDoc<Item>(imap)))
                            .ToArray();

      return result;
    }

    public async Task<ChangeResult> SaveAsync(Item item)
    {
      var method = item.NonNull(nameof(item)).FormMode == FormMode.Insert ? System.Net.Http.HttpMethod.Post : System.Net.Http.HttpMethod.Put;

      var response = await m_Server.Call(AdlibServiceAddress,
                                         nameof(IAdlibLogic),
                                         item.EffectiveShardKey,
                                         (http, ct) => http.Client.CallAndGetJsonMapAsync("item", method, new { item = item })).ConfigureAwait(false);

      var result = response.UnwrapChangeResult();
      return result;
    }

    public async Task<ChangeResult> DeleteAsync(EntityId id, string shardTopic = null)
    {
      id.HasRequiredValue(nameof(id));
      id.Schema.IsTrue(v => v.IsZero || v == Constraints.SCH_GITEM, "Schema");

      var gItem = id.Address.NonBlank(nameof(id.Address)).AsGDID();

      var sk = new ShardKey(shardTopic);
      if (shardTopic == null)
      {
        sk = new ShardKey(Constraints.GdidToShardKey(gItem));
      }

      var response = await m_Server.Call(AdlibServiceAddress,
                                         nameof(IAdlibLogic),
                                         sk,
                                         (http, ct) => http.Client.DeleteAndGetJsonMapAsync("item", new { id = id })).ConfigureAwait(false);

      var result = response.UnwrapChangeResult();
      return result;
    }
  }
}
