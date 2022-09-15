/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Collections;
using Azos.Conf;
using Azos.Apps.Hosting.Skyod.Adapters;
using Azos.Instrumentation;

namespace Azos.Apps.Hosting.Skyod
{
  /// <summary>
  /// Provides logical isolation for sets of software, as the daemon
  /// performs operations (such as install) within the software system
  /// </summary>
  public sealed class SubordinateHost : ApplicationComponent<SetComponent>, INamed, IOrdered
  {
    public const string CONFIG_SUBORDINATE_HOST_SECTION = "subordinate-host";

    internal SubordinateHost(SetComponent director, IConfigSectionNode cfg) : base(director)
    {
      cfg.NonEmpty("SubordinateHost cfg");
      m_Name = cfg.ValOf(Configuration.CONFIG_NAME_ATTR).NonBlank($"{nameof(SetComponent)}.{Configuration.CONFIG_NAME_ATTR}");
      m_Order = cfg.Of(Configuration.CONFIG_ORDER_ATTR).NonEmpty($"{nameof(SetComponent)}.{Configuration.CONFIG_ORDER_ATTR}").ValueAsInt();
      ConfigAttribute.Apply(this, cfg);

    }

    protected override void Destructor()
    {
      base.Destructor();
    }

    private readonly string m_Name;
    private readonly int m_Order;

    //private HttpClient m_Client;

    public override string ComponentLogTopic => Sky.SysConsts.LOG_TOPIC_SKYOD;

    /// <summary>
    /// Unique logical host name
    /// </summary>
    public string Name => m_Name;

    /// <summary>
    /// Order of the component within its parent set.
    /// The components get installed/activated in order
    /// </summary>
    public int Order => m_Order;

    /// <summary>
    /// The URI root component of the remote host e.g. http://host123.mysystem.local:9102.
    /// This is where master connects to subordinate
    /// </summary>
    [Config, ExternalParameter]
    public string SkyodRootUri { get; set; }


    /// <summary>
    /// How often should the hot be pinged. When set to leq to zero, disables ping
    /// </summary>
    [Config, ExternalParameter]
    public int PingIntervalSec { get; set; }


    /// <summary>
    /// When true, starts logging panic messages
    /// </summary>
    [Config, ExternalParameter]
    public bool CausesPanic { get; set; }

  }
}
