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

    public Task<IEnumerable<Atom>> GetSpaceNamesAsync()
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetCollectionNamesAsync(string space)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<Item>> GetListAsync(ItemFilter filter)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> SaveAsync(Item item)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> DeleteAsync(EntityId id)
    {
      throw new NotImplementedException();
    }
  }
}
