/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Client;
using Azos.Conf;
using Azos.Data;
using Azos.Web;
using Azos.Instrumentation;
using Azos.Serialization.JSON;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// Implements IMinIdpStore by calling a remote MinIdpServer using Web
  /// </summary>
  public sealed class WebClientStore : DaemonWithInstrumentation<MinIdpSecurityManager>, IMinIdpStoreImplementation
  {
    public const string CONFIG_SERVER_SECTION = "server";

    public WebClientStore(MinIdpSecurityManager dir) : base(dir) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }

    private HttpService m_Server;

    private string m_RemoteAddress;

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;


    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set; }


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Server);
      if (node==null) return;

      var nServer = node[CONFIG_SERVER_SECTION];
      m_Server = FactoryUtils.MakeDirectedComponent<HttpService>(this,
                                                                 nServer,
                                                                 typeof(HttpService),
                                                                 new object[] { nServer });
    }

    protected override void DoStart()
    {
      m_Server.NonNull("Not configured Server of config section `{0}`".Args(CONFIG_SERVER_SECTION));
      base.DoStart();
    }

    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id)
    {
      var map = await m_Server.Call(m_RemoteAddress,
                                    nameof(IMinIdpStore),
                                    id,
                                    (tx, c) => tx.Client.PostAndGetJsonMapAsync(nameof(GetByIdAsync), new { realm, id}));

      return JsonReader.ToDoc<MinIdpUserData>(map);
    }

    public async Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken)
    {
      var map = await m_Server.Call(m_RemoteAddress,
                                    nameof(IMinIdpStore),
                                    sysToken,
                                    (tx, c) => tx.Client.PostAndGetJsonMapAsync(nameof(GetBySysAsync), new { realm, sysToken }));

      return JsonReader.ToDoc<MinIdpUserData>(map);
    }

    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri)
    {
      var map = await m_Server.Call(m_RemoteAddress,
                                    nameof(IMinIdpStore),
                                    uri,
                                    (tx, c) => tx.Client.PostAndGetJsonMapAsync(nameof(GetByUriAsync), new { realm, uri }));

      return JsonReader.ToDoc<MinIdpUserData>(map);
    }
  }
}
