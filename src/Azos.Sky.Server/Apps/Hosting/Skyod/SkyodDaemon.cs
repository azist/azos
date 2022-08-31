/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Serialization.JSON;

namespace Azos.Sky.Server.Apps.Hosting.Skyod
{
  /// <summary>
  /// Provides services for managing subordinate nodes and Governor processes on nodes
  /// </summary>
  public sealed class SkyodDaemon : DaemonWithInstrumentation<IApplicationComponent>
  {
    public SkyodDaemon(IApplication app) : base(app)
    {
      m_Sets = new Registry<SoftwareSet>();
    }

    protected override void Destructor()
    {
      base.Destructor();
 //     DisposeAndNull(ref m_Chain);
    }

    private Registry<SoftwareSet> m_Sets;
    private Thread m_Thread;
    private AutoResetEvent m_Wait;

    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_SKYOD;
    public override bool InstrumentationEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

  }
}
