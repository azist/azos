/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using Azos.Collections;
using Azos.Conf;

namespace Azos.Apps.Hosting.Skyod
{
  /// <summary>
  /// Provides logical isolation for sets of software, as the daemon
  /// performs operations (such as install) within the software system
  /// </summary>
  public sealed class SoftwareSet : ApplicationComponent<SkyodDaemon>, INamed
  {
    public const string CONFIG_SOFTWARE_SET_SECTION = "software-set";

    internal SoftwareSet(SkyodDaemon director, IConfigSectionNode cfg) : base(director)
    {
      cfg.NonEmpty("SoftwareSet cfg");
      m_Name = cfg.ValOf(Configuration.CONFIG_NAME_ATTR)
                  .NonBlank($"{nameof(SoftwareSet)}.{Configuration.CONFIG_NAME_ATTR}");

      Constraints.CheckComponentName(m_Name, "{0}.{1}".Args(nameof(SoftwareSet), nameof(Name)));

      Atom.TryEncode(m_Name, out _).IsTrue();
      m_Components = new OrderedRegistry<SetComponent>();

      foreach(var ncmp in cfg.ChildrenNamed(SetComponent.CONFIG_COMPONENT_SECTION))
      {
        var cmp = FactoryUtils.MakeDirectedComponent<SetComponent>(this, ncmp, typeof(SetComponent), new[] { ncmp });
        m_Components.Register(cmp).IsTrue("Unique component name `{0}`".Args(cmp.Name));
      }

      (m_Components.Count > 0).IsTrue("Configured components");
    }

    protected override void Destructor()
    {
      base.Destructor();
      m_Components.ForEach( c => this.DontLeak(() => c.Dispose()) );
      m_Components.Clear();
    }

    private readonly string m_Name;
    private readonly OrderedRegistry<SetComponent> m_Components;

    /// <summary>
    /// Unique software set name. Package labels must start from the set name e.g. `[x9-biz]-20220801-181200.apar`
    /// </summary>
    public string Name => m_Name;

    /// <summary>
    /// Root directory for this software set
    /// </summary>
    public string RootDirectory => Path.Combine(this.ComponentDirector.SoftwareRootDirectory, Name);

    public IOrderedRegistry<SetComponent> Components => m_Components;

    public override string ComponentLogTopic => Sky.SysConsts.LOG_TOPIC_SKYOD;
  }
}
