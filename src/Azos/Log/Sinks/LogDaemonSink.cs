/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Implements a sink that is based on another instance of LogDaemon, which provides asynchronous buffering and failover capabilities
  /// </summary>
  public class LogDaemonSink : Sink
  {
    public LogDaemonSink(ISinkOwner owner) : base(owner)
    {
      m_Daemon = new LogDaemon(this);
    }

    public LogDaemonSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    {
      m_Daemon = new LogDaemon(this);
    }

    private LogDaemon  m_Daemon;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      m_Daemon.Configure(node);
    }

    protected override void DoStart()
    {
      base.DoStart();
      m_Daemon.Start();
    }

    protected override void DoWaitForCompleteStop()
    {
      m_Daemon.WaitForCompleteStop();
      base.DoWaitForCompleteStop();
    }

    protected internal override void DoSend(Message msg)
    {
      m_Daemon.Write(msg);
    }
  }
}
