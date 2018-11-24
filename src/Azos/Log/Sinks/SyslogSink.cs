/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Log.Syslog;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Implements destination that sends messages to UNIX syslog using UDP datagrams
  /// </summary>
  public class SyslogSink : Sink
  {
    /// <summary>
    /// Creates a new instance of destination that sends messages to .nix SYSLOG
    /// </summary>
    public SyslogSink(ISinkOwner owner) : base(owner)
    {
    }

    public SyslogSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    {
      m_Client = new SyslogClient();
    }

    protected override void Destructor()
    {
      m_Client.Dispose();
      base.Destructor();
    }



    private SyslogClient m_Client;



    /// <summary>
    /// References the underlying syslog client instance
    /// </summary>
    public SyslogClient Client => m_Client;

    protected override void DoConfigure(Conf.IConfigSectionNode node)
    {
      base.DoConfigure(node);
      m_Client.Configure(node);
    }

    protected override void DoWaitForCompleteStop()
    {
        m_Client.Close();
        base.DoWaitForCompleteStop();
    }



    protected internal override void DoSend(Message entry) =>  m_Client.Send( new SyslogMessage(this, entry));
  }
}
