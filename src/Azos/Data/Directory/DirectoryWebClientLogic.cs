/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Client;
using Azos.Conf;
using Azos.Data.AST;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Data.Directory
{
  /// <summary>
  /// Provides client for consuming IDirectory remote services using Web/Http
  /// </summary>
  public sealed class DirectoryWebClientLogic : ModuleBase, IDirectoryLogic
  {
    public const string CONFIG_SERVICE_SECTION = "service";

    public DirectoryWebClientLogic(IApplication application) : base(application) { }
    public DirectoryWebClientLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }

    private HttpService m_Server;

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

    /// <summary>
    /// Logical service address of Directory server
    /// </summary>
    [Config]
    public string DirectoryServiceAddress{  get; set; }

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
      DirectoryServiceAddress.NonBlank(nameof(DirectoryServiceAddress));

      return base.DoApplicationAfterInit();
    }

    public async Task<Item> GetAsync(EntityId id, bool touch = false)
    {
      if (!id.IsAssigned) throw new CallGuardException(nameof(TouchAsync), nameof(id), "No required EntityId");

      var response = await m_Server.Call(DirectoryServiceAddress,
                                         nameof(IDirectory),
                                         new ShardKey(id),
                                        (http, ct) => http.Client.PostAndGetJsonMapAsync("one", new { id, touch }));

      var map = response.UnwrapPayloadMap();
      var item = new Item();
      var (match, _) = item.ReadAsJson(map, false, null);
      return match ? item : null;
    }

    public async Task<ChangeResult> SaveAsync(Item item)
    {
      item.NonNull(nameof(item));

      var response = await m_Server.Call(DirectoryServiceAddress,
                                         nameof(IDirectory),
                                         new ShardKey(item.Id),
                                        (http, ct) => http.Client.PostAndGetJsonMapAsync("item", item));
      return response.UnwrapChangeResult();
    }

    public async Task<ChangeResult> TouchAsync(IEnumerable<EntityId> ids)
    {
      var batch = ids.NonNull(nameof(ids)).Where(one => one.IsAssigned).ToArray();
      if (batch.Length == 0) return new ChangeResult(ChangeResult.ChangeType.Undefined, 0, "Nothing to send", null);


      var response = await m_Server.Call(DirectoryServiceAddress,
                                         nameof(IDirectory),
                                         new ShardKey(batch[0]),
                                        (http, ct) => http.Client.PostAndGetJsonMapAsync("touch", new{ batch } ));

      return response.UnwrapChangeResult();
    }

    public async Task<ChangeResult> DeleteAsync(EntityId id)
    {
      if (!id.IsAssigned) throw new CallGuardException(nameof(TouchAsync), nameof(id), "No required EntityId");

      var response = await m_Server.Call(DirectoryServiceAddress,
                                         nameof(IDirectory),
                                         new ShardKey(id),
                                        (http, ct) => http.Client.DeleteAndGetJsonMapAsync("item", new { id }));

      return response.UnwrapChangeResult();
    }

    public async Task<IEnumerable<Item>> QueryAsync(EntityId entity, Expression queryExpression)
    {
      if (!entity.IsAssigned) throw new CallGuardException(nameof(TouchAsync), nameof(entity), "No required entity designator");
      queryExpression.NonNull(nameof(queryExpression));

      var response = await m_Server.Call(DirectoryServiceAddress,
                                         nameof(IDirectory),
                                         new ShardKey(entity),
                                        (http, ct) => http.Client.PostAndGetJsonMapAsync("query", new { queryExpression }));

      var result = response.UnwrapPayloadArray()
                .OfType<JsonDataMap>()
                .Select(imap => {
                   var item = new Item();
                   var (match, _) = item.ReadAsJson(imap, false, null);
                   return match ? item : null;
                }).Where( i => i != null);

      return result;
    }

  }
}
