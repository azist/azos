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

namespace Azos.Data.Access.Rpc
{
  /// <summary>
  /// Provides client for consuming IRpcHandler remote services
  /// </summary>
  public class RpcHandlerWebClientLogic : ModuleBase, IRpcHandler
  {
    public const string CONFIG_SERVICE_SECTION = "service";

    public RpcHandlerWebClientLogic(IApplication application) : base(application) { }
    public RpcHandlerWebClientLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }

    private HttpService m_Server;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;


    /// <summary>
    /// Logical service address of RpcHandler server
    /// </summary>
    [Config]
    public string RpcHandlerServiceAddress{  get; set; }

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
      RpcHandlerServiceAddress.NonBlank(nameof(RpcHandlerServiceAddress));

      return base.DoApplicationAfterInit();
    }

    /// <summary>
    /// Override to impose a client-side limitations on ReadRequest instances
    /// </summary>
    public virtual ValidState ValidateReadRequest(ValidState state, ReadRequest request) => state;

    /// <summary>
    /// Override to impose a client-side limitations on TransactRequest instances
    /// </summary
    public virtual ValidState ValidateTransactRequest(ValidState state, TransactRequest request) => state;

    public async Task<JsonDataMap> ReadAsync(ReadRequest request)
    {
      var response = await m_Server.Call(RpcHandlerServiceAddress,
                                          nameof(IRpcHandler),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.PostAndGetJsonMapAsync("reader",
                                                                new
                                                                {
                                                                  request = request
                                                                }).ConfigureAwait(false));

      var result = response.UnwrapPayloadMap();
      return result;
    }

    public async Task<ChangeResult> TransactAsync(TransactRequest request)
    {
      var response = await m_Server.Call(RpcHandlerServiceAddress,
                                          nameof(IRpcHandler),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.PostAndGetJsonMapAsync("transaction",
                                                                new
                                                                {
                                                                  request = request
                                                                }).ConfigureAwait(false));

      var result = response.UnwrapChangeResult();
      return result;
    }
  }
}
