using System;
using System.Collections.Generic;

using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;

using Azos.Sky.Metabase;

namespace Azos.Sky.Instrumentation
{
  /// <summary>
  /// Reduces instrumentation data stream and uploads it to the higher-standing zone governor
  /// </summary>
  public class SkyInstrumentationProvider : InstrumentationProvider
  {
    #region CONSTS
    public const string CONFIG_HOST_ATTR = "host";
    private const string LOG_TOPIC = "Log.SkyInstr";
    private const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;
    #endregion

    #region .ctor
    public SkyInstrumentationProvider() : base(null) {}
    public SkyInstrumentationProvider(InstrumentationDaemon director) : base(director) {}
    #endregion

    #region Fields
    private Metabank.SectionHost m_Host;
    #endregion

    #region Properties
    /// <summary>
    /// Specifies the log level for operations performed by Pay System.
    /// </summary>
    [Config(Default = DEFAULT_LOG_LEVEL)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
    public MessageType LogLevel { get; set; }
    #endregion

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      m_Host = App.GetMetabase().CatalogReg.NavigateHost(node.AttrByName(CONFIG_HOST_ATTR).Value);
    }

    protected internal override object BeforeBatch()
    {
      return new List<Datum>();
    }

    protected internal override void AfterBatch(object batchContext)
    {
      var datumList = batchContext as List<Datum>;
      if (datumList != null)
        send(datumList.ToArray());

    }
    protected internal override void Write(Datum aggregatedDatum, object batchContext, object typeContext)
    {
      var datumList = batchContext as List<Datum>;
      if (datumList != null)
      {
        datumList.Add(aggregatedDatum);
        if (datumList.Count>100)
        {
          send(datumList.ToArray());
          datumList.Clear();
        }
      }
      else
        send(aggregatedDatum);
    }
    #endregion

    #region Private
    private void send(params Datum[] data)
    {
      try
      {
        //TODO  Cache the client instance, do not create client on every call
        using (var client = App.GetServiceClientHub().MakeNew<Contracts.ITelemetryReceiverClient>(m_Host))
          client.Async_SendDatums(data);
      }
      catch (Exception error)
      {
        throw new TelemetryArchiveException("{0}.Write".Args(GetType().Name), error);
      }
    }

    private Guid log(
      MessageType type,
      string from,
      string message,
      Exception error = null,
      Guid? relatedMessageID = null,
      string parameters = null)
    {
      if (type < LogLevel) return Guid.Empty;

      var logMessage = new Message
      {
        Type = type,
        Topic = LOG_TOPIC,
        From = "{0}.{1}".Args(GetType().FullName, from),
        Text = message,
        Exception = error,
        Parameters = parameters
      };

      if (relatedMessageID.HasValue) logMessage.RelatedTo = relatedMessageID.Value;

      App.Log.Write(logMessage);

      return logMessage.Guid;
    }
    #endregion
  }
}
