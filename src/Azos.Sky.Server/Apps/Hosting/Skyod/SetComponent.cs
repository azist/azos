/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;

namespace Azos.Sky.Server.Apps.Hosting.Skyod
{
  /// <summary>
  /// Provides logical isolation for sets of software, as the daemon
  /// performs operations (such as install) within the software system
  /// </summary>
  public sealed class SetComponent : ApplicationComponent<SoftwareSet>, INamed, IOrdered
  {
    internal SetComponent(SoftwareSet director, IConfigSectionNode cfg) : base(director)
    {
    }

    private readonly string m_Name;
    private readonly int m_Order;

    /// <summary>
    /// Unique software component name. Package labels must start from the set name e.g. `[x9-biz]-20220801-181200.apar`
    /// </summary>
    public string Name => m_Name;

    public int Order => m_Order;

    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_SKYOD;


    /// <summary>
    /// Where the set gets installed  e.g. `/opt/sky-sys/` vs `/opt/sky-biz/`
    /// </summary>
    public string RootDirectory { get; set; }

    /// <summary>
    /// By default `/bin`
    /// </summary>
    public string InstallSubdirectory { get; set; }

    //  public InstallationAdapter Install => null;

    //  public ActivationAdapter Activator => null;

  }
}
