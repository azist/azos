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
using Azos.Client;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Serialization.JSON;
using Azos.Sky.Contracts;
using Azos.Web;

namespace Azos.Sky.Identification
{
  /// <summary>
  /// Accesses remote GDID generation authority using web
  /// </summary>
  public sealed class GdidAuthorityWebAccessor : ApplicationComponent<IApplicationComponent>, IGdidAuthorityAccessor
  {
    public const string CONFIG_SERVICE_SECTION = "service";

    public GdidAuthorityWebAccessor(IApplicationComponent dir) : base(dir)
    {
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }

    public override string ComponentLogTopic => CoreConsts.TOPIC_ID_GEN;

    private HttpService m_Server;
    private int m_Shard;

    /// <summary>
    /// Logical authority service address
    /// </summary>
    [Config, ExternalParameter(SysConsts.EXT_PARAM_GROUP_IDGEN)]
    public string AuthorityAddress { get; set; }

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
      DisposeAndNull(ref m_Server);
      AuthorityAddress.NonBlank(nameof(AuthorityAddress)+" config");
      if (node==null) return;

      var nServer = node[CONFIG_SERVICE_SECTION];
      m_Server = FactoryUtils.MakeDirectedComponent<HttpService>(this, nServer, typeof(HttpService), new[]{ nServer });
    }

    public async Task<GdidBlock> AllocateBlockAsync(string scopeName, string sequenceName, int blockSize, ulong? vicinity = 1152921504606846975)
    {
      var args = new { scopeName, sequenceName, blockSize, vicinity };

      var got = await m_Server.Call(AuthorityAddress.NonBlank(nameof(AuthorityAddress)),
                                    nameof(IGdidAuthority),
                                    ++m_Shard,
                                    (http, ct) => http.Client.PostAndGetJsonMapAsync("block", args));

      var result = JsonReader.ToDoc<GdidBlock>( got.UnwrapPayloadMap() );

      return result;
    }


  }
}
