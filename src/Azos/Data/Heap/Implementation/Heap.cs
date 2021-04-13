/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;

namespace Azos.Data.Heap.Implementation
{
  /// <summary>
  /// Provides default implementation of IHeapLogic
  /// </summary>
  public sealed class Heap : ModuleBase, IHeapLogic
  {
    public Heap(IApplication application) : base(application) { }
    public Heap(IModule parent) : base(parent) { }

    protected override void Destructor() { cleanupAreas();   base.Destructor(); }

    private void cleanupAreas()
    {
      if (m_Areas==null) return;
      var toClean = m_Areas.Values;
      m_Areas = null;

      toClean.ForEach( a => this.DontLeak(() => a.Dispose()) );
    }

    private Registry<Area> m_Areas;

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

    public IRegistry<IArea> Areas => m_Areas;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node==null) return;

      cleanupAreas();
      m_Areas = new Registry<Area>();
      foreach(var narea in node.Children.Where(n => n.IsSameName(Area.CONFIG_AREA_SECTION)))
      {
        var area = FactoryUtils.MakeDirectedComponent<Area>(this, narea, typeof(Area), new object[] { narea });
        m_Areas.Register(area).IsTrue("Duplicate area: `{0}`".Args(area.Name));
      }
    }

    protected override bool DoApplicationAfterInit()
    {
      (m_Areas.Count > 0).IsTrue("Areas.Count > 0");
      return base.DoApplicationAfterInit();
    }
  }
}
