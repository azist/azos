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
    public HubModule() : base(){ }
    public HubModule(IModule parent) : base(parent){ }
    public HubModule(IModule parent, int order) : base(parent, order){ }
    public override bool IsHardcodedModule => false;

    //Hub module returns children declared right within it (without "modules" sub-section)
    protected override IEnumerable<Conf.IConfigSectionNode> DoGetAllChildModuleConfigNodes(Conf.IConfigSectionNode node)
    {
      if (node==null || !node.Exists) return Enumerable.Empty<Conf.IConfigSectionNode>();
      return node.Children.Where(c => c.IsSameName(CommonApplicationLogic.CONFIG_MODULE_SECTION));
    }
  }
}
