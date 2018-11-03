using System;

using Azos.Conf;
using Azos.Log;
using Azos.Log.Sinks;
using Azos.Instrumentation;

using Azos.Sky.Contracts;

namespace Azos.Sky.Log
{
  /// <summary>
  /// Sends log messages to log receiver
  /// </summary>
  public sealed class SkySink : Sink
  {
    #region CONSTS
      public const string CONFIG_HOST_ATTR = "host";

      private const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;
    #endregion

    #region .ctor
      public SkySink() : base() { }
      public SkySink(string name) : base(name) { }
    #endregion

    #region Fields

      private string m_HostName;
      private ILogReceiverClient m_Client;

    #endregion

    #region Properties

      /// <summary>
      /// Specifies the log level for logging operations of this destination
      /// </summary>
      [Config(Default = DEFAULT_LOG_LEVEL)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
      public MessageType LogLevel { get; set; }


      /// <summary>
      /// Specifies the name of the host where the destination sends the data
      /// </summary>
      [Config]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
      public string Host { get; set; }

    #endregion

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      //throws on bad host spec
      SkySystem.Metabase.CatalogReg.NavigateHost(Host);
    }

    public override void Close()
    {
      base.Close();
      DisposeAndNull(ref m_Client);
    }

    protected override void DoSend(Message entry)
    {
      var msg = entry.ThisOrNewSafeWrappedException(false);

      try
      {
        ensureClient();
        m_Client.Async_SendLog(msg);
      }
      catch (Exception error)
      {
        throw new LogArchiveException("{0}.DoSend: {1}".Args(GetType().Name, error.ToMessageWithType()), error);
      }
    }
    #endregion

    #region Private

      private void ensureClient()
      {
        var hn = this.Host;
        if (m_Client == null && !hn.EqualsOrdIgnoreCase(m_HostName))
        {
          m_Client = ServiceClientHub.New<ILogReceiverClient>(hn);
          m_HostName = hn;
        }
      }

    #endregion
  }
}
