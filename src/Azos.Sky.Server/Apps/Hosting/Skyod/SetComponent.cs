/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Collections;
using Azos.Conf;
using Azos.Apps.Hosting.Skyod.Adapters;
using Azos.Serialization.Bix;

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
      ConfigAttribute.Apply(this, cfg);

      var nadapter = cfg[CONFIG_INSTALLATION_SECTION].NonEmpty($"Attribute ${CONFIG_INSTALLATION_SECTION}");
      m_Installation = FactoryUtils.MakeDirectedComponent<InstallationAdapter>(this, nadapter, typeof(DefaultAzosPackageInstaller), new []{ nadapter });

      nadapter = cfg[CONFIG_ACTIVATION_SECTION].NonEmpty($"Attribute ${CONFIG_ACTIVATION_SECTION}");
      m_Activation = FactoryUtils.MakeDirectedComponent<ActivationAdapter>(this, nadapter, typeof(DefaultHgovOsProcessActivator), new[] { nadapter });

      var nrset = cfg[CONFIG_REQUESTSET_SECTION];
      m_SetPersistence = new GuidSetFilePersistenceHandler(this, nrset);
      m_ProcessedRequestIds = new CappedSet<Guid>(this, null, m_SetPersistence);
      ConfigAttribute.Apply(m_SetPersistence, nrset);
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Installation);
      DisposeAndNull(ref m_Activation);
      DisposeAndNull(ref m_ProcessedRequestIds);
      DisposeAndNull(ref m_SetPersistence);
    }

    private readonly string m_Name;
    private readonly int m_Order;

    private InstallationAdapter m_Installation;
    private ActivationAdapter m_Activation;
    private GuidSetFilePersistenceHandler m_SetPersistence;
    private CappedSet<Guid> m_ProcessedRequestIds;

    [Config]
    private bool m_IsManagedInstall;

    [Config]
    private bool m_IsManagedActivation;

    [Config]
    private bool m_IsManagedStatus;


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
    /// True when component can be (re)installed
    /// </summary>
    public bool IsManagedInstall => m_IsManagedInstall;

    /// <summary>
    /// True when component can be started/stopped
    /// </summary>
    public bool IsManagedActivation => m_IsManagedActivation;

    /// <summary>
    /// True when component status gets scanned
    /// </summary>
    public bool IsManagedStatus => m_IsManagedStatus;

    /// <summary>
    /// Handles installation functionality
    /// </summary>
    public InstallationAdapter Installation => this.NonDisposed(nameof(SetComponent)).m_Installation;


    /// <summary>
    /// Handles activation functionality
    /// </summary>
    public ActivationAdapter Activation => this.NonDisposed(nameof(SetComponent)).m_Activation;

    public bool TryRegisterNewAdapterRequest(Guid id) => m_ProcessedRequestIds.Put(id);
  }



}
