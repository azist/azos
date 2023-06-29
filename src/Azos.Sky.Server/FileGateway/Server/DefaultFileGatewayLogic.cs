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
using Azos.Data;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Sky.FileGateway.Server
{
  public sealed partial class DefaultFileGatewayLogic : ModuleBase, IFileGatewayLogic
  {
    public DefaultFileGatewayLogic(IApplication application) : base(application) { }
    public DefaultFileGatewayLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      //DisposeAndNull(ref X);
      base.Destructor();
    }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.IO_TOPIC;

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      //base.DoConfigure(node);
      //DisposeAndNull(ref m_Server);
      //if (node == null) return;

      //var nServer = node[CONFIG_SERVICE_SECTION];
      //m_Server = FactoryUtils.MakeDirectedComponent<HttpService>(this,
      //                                                           nServer,
      //                                                           typeof(HttpService),
      //                                                           new object[] { nServer });
    }

    //check preconditions/config
    protected override bool DoApplicationAfterInit()
    {
      //m_Server.NonNull("Not configured Server of config section `{0}`".Args(CONFIG_SERVICE_SECTION));
      //GatewayServiceAddress.NonBlank(nameof(GatewayServiceAddress));
      return base.DoApplicationAfterInit();
    }

    #endregion

  }
}
