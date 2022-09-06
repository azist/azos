/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Collections;
using Azos.Conf;
using Azos.Apps.Hosting.Skyod.Adapters;
using System.IO;

namespace Azos.Apps.Hosting.Skyod
{
  /// <summary>
  /// Provides logical isolation for sets of software, as the daemon
  /// performs operations (such as install) within the software system
  /// </summary>
  public sealed class SetComponent : ApplicationComponent<SoftwareSet>, INamed, IOrdered
  {
    public const string CONFIG_COMPONENT_SECTION = "component";
    public const string CONFIG_INSTALLATION_SECTION = "installation";
    public const string CONFIG_ACTIVATION_SECTION = "activation";
    public const string CONFIG_REQUESTSET_SECTION = "requestset";

    internal SetComponent(SoftwareSet director, IConfigSectionNode cfg) : base(director)
    {
      cfg.NonEmpty("SetComponent cfg");
      m_Name = cfg.ValOf(Configuration.CONFIG_NAME_ATTR).NonBlank($"{nameof(SetComponent)}.{Configuration.CONFIG_NAME_ATTR}");
      m_Order = cfg.Of(Configuration.CONFIG_ORDER_ATTR).NonEmpty($"{nameof(SetComponent)}.{Configuration.CONFIG_ORDER_ATTR}").ValueAsInt();

      Constraints.CheckComponentName(m_Name, "{0}.{1}".Args(nameof(SetComponent), nameof(Name)));

      ConfigAttribute.Apply(this, cfg);

      var nadapter = cfg[CONFIG_INSTALLATION_SECTION];
      if (nadapter.Exists)
      {
        m_Installation = FactoryUtils.MakeDirectedComponent<InstallationAdapter>(this, nadapter, typeof(DefaultAzosPackageInstaller), new []{ nadapter });
      }

      nadapter = cfg[CONFIG_ACTIVATION_SECTION];
      if (nadapter.Exists)
      {
        m_Activation = FactoryUtils.MakeDirectedComponent<ActivationAdapter>(this, nadapter, typeof(DefaultHgovOsProcessActivator), new[] { nadapter });
      }

      var nrset = cfg[CONFIG_REQUESTSET_SECTION];
      m_SetPersistence = new GuidSetFilePersistenceHandler(this, nrset);
      m_ProcessedRequestIds = new CappedSet<Guid>(this, null, m_SetPersistence);
      ConfigAttribute.Apply(m_SetPersistence, nrset);

      m_SubordinateHosts = new OrderedRegistry<SubordinateHost>();

      foreach (var nhost in cfg.ChildrenNamed(SubordinateHost.CONFIG_SUBORDINATE_HOST_SECTION))
      {
        var host = FactoryUtils.MakeDirectedComponent<SubordinateHost>(this, nhost, typeof(SetComponent), new[] { nhost });
        m_SubordinateHosts.Register(host).IsTrue("Unique subordinate host `{0}`".Args(host.Name));
      }

      (!IsLocal && m_SubordinateHosts.Count == 0).IsTrue("Subordinate hosts required when no local footprint");
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Installation);
      DisposeAndNull(ref m_Activation);
      DisposeAndNull(ref m_ProcessedRequestIds);
      DisposeAndNull(ref m_SetPersistence);
      m_SubordinateHosts.ForEach(h => this.DontLeak(() => h.Dispose()));
      m_SubordinateHosts.Clear();
    }

    private readonly string m_Name;
    private readonly int m_Order;

    private InstallationAdapter m_Installation;
    private ActivationAdapter m_Activation;
    private GuidSetFilePersistenceHandler m_SetPersistence;
    private CappedSet<Guid> m_ProcessedRequestIds;
    private OrderedRegistry<SubordinateHost> m_SubordinateHosts;


    /// <summary>
    /// Unique software component name. Package labels must start from the set name e.g. `[x9-biz]-20220801-181200.apar`
    /// </summary>
    public string Name => m_Name;

    /// <summary>
    /// Order of the component within its parent set.
    /// The components get installed/activated in order
    /// </summary>
    public int Order => m_Order;

    public override string ComponentLogTopic => Sky.SysConsts.LOG_TOPIC_SKYOD;

    /// <summary>
    /// References Skyod daemon instance which is a root of this software set containing components
    /// </summary>
    public SkyodDaemon SkyodDaemon => ComponentDirector.SkyodDaemon;


    /// <summary>
    /// Handles installation functionality
    /// </summary>
    public InstallationAdapter Installation => this.NonDisposed(nameof(SetComponent)).m_Installation;

    /// <summary>
    /// Handles activation functionality
    /// </summary>
    public ActivationAdapter Activation => this.NonDisposed(nameof(SetComponent)).m_Activation;


    /// <summary>
    /// True when this component has any kind of local presence, such as installation or activation of software locally.
    /// False when this SetComponent does not have any local footprint and is only listed for the purpose
    /// of subordinate host management
    /// </summary>
    public bool IsLocal => m_Installation != null || m_Activation != null;

    /// <summary>
    /// Root directory for this software set component <seealso cref="EnsureRootDirectory"/>
    /// </summary>
    public string RootDirectory => Path.Combine(ComponentDirector.RootDirectory, Name);

    /// <summary>
    /// Hosts subordinate to this one
    /// </summary>
    public IOrderedRegistry<SubordinateHost> SubordinateHosts => m_SubordinateHosts;

    public bool TryRegisterNewAdapterRequest(Guid id) => m_ProcessedRequestIds.Put(id);

    /// <summary>
    /// Ensures that `RootDirectory` is created locally and returns its DirectoryInfo
    /// </summary>
    public DirectoryInfo EnsureRootDirectory() => Directory.CreateDirectory(RootDirectory);
  }
}
