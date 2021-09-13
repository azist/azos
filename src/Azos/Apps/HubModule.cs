/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;
using System.Collections.Generic;

namespace Azos.Apps
{
  /// <summary>
  /// Defines a module that does nothing else but provides a hub/namespace grouping for child modules that it contains.
  /// This module is a kin to NOPModule - the difference is only in the intent. NOPModule signifies the absence of any modules,
  /// whereas HubModule holds child modules
  /// </summary>
  public sealed class HubModule : ModuleBase
  {
    public HubModule(IApplication application) : base(application){ }

    public HubModule(IModule parent) : base(parent){ }

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => CoreConsts.APPLICATION_TOPIC;

    //Hub module returns children declared right within it (without "modules" sub-section)
    protected override IEnumerable<Conf.IConfigSectionNode> DoGetAllChildModuleConfigNodes(Conf.IConfigSectionNode node)
    {
      if (node==null || !node.Exists) return Enumerable.Empty<Conf.IConfigSectionNode>();
      return node.Children.Where(c => c.IsSameName(CommonApplicationLogic.CONFIG_MODULE_SECTION));
    }
  }
}
