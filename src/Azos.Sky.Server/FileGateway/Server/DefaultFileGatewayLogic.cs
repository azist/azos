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
using Azos.Collections;
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
      cleanup();
      base.Destructor();
    }

    private void cleanup()
    {
      var was = m_Systems.ToArray();
      m_Systems = new AtomRegistry<GatewaySystem>();
      was.ForEach(one => this.DontLeak(() => one.Dispose(), errorFrom: nameof(cleanup)));
    }

    private AtomRegistry<GatewaySystem> m_Systems;

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.IO_TOPIC;

    /// <summary>
    /// Returns a system by name or throws if not found
    /// </summary>
    public GatewaySystem this[Atom system] => m_Systems[system.IsValidNonZero(nameof(system))] ?? throw $"Gateway system `{system}`".IsNotFound();

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      cleanup();
      if (node == null) return;

      m_Systems = new AtomRegistry<GatewaySystem>();

      foreach(var nSystem in node.ChildrenNamed(GatewaySystem.CONFIG_SYSTEM_SECTION))
      {
        var one = FactoryUtils.MakeDirectedComponent<GatewaySystem>(this, nSystem, typeof(GatewaySystem), new[]{ nSystem });
        m_Systems.Register(one).IsTrue($"Unique {one.GetType().DisplayNameWithExpandedGenericArgs()}(`{one.Name}`)");
      }
    }

    //check preconditions/config
    protected override bool DoApplicationAfterInit()
    {
      (m_Systems.NonNull(nameof(m_Systems)).Count > 0).IsTrue("Configured systems");
      return base.DoApplicationAfterInit();
    }

    #endregion

  }
}
