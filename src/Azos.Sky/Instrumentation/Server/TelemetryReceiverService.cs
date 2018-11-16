using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;

namespace Azos.Sky.Instrumentation.Server
{
  /// <summary>
  /// Glue adapter for Contracts.ITelemetryReceiver
  /// </summary>
  public sealed class TelemetryReceiverServer : Contracts.ITelemetryReceiver
  {
    public void SendDatums(params Datum[] data)
    {
      TelemetryReceiverService.Instance.SendDatums(data);
    }
  }


  /// <summary>
  /// Provides server implementation of Contracts.ITelemetryReceiver
  /// </summary>
  public sealed class TelemetryReceiverService : DaemonWithInstrumentation<object>, Contracts.ITelemetryReceiver
  {
    #region CONSTS
    public const string CONFIG_ARCHIVE_STORE_SECTION = "archive-store";

    public const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;
    #endregion

    #region STATIC/.ctor
    private static object s_Lock = new object();
    private static volatile TelemetryReceiverService s_Instance;

    internal static TelemetryReceiverService Instance
    {
      get
      {
        var instance = s_Instance;
        if (instance == null)
          throw new TelemetryArchiveException("{0} is not allocated".Args(typeof(TelemetryReceiverService).FullName));
        return instance;
      }
    }

    public TelemetryReceiverService() : this(null) { }

    public TelemetryReceiverService(object director) : base(director)
    {
      LogLevel = MessageType.Warning;

      lock (s_Lock)
      {
        if (s_Instance != null)
          throw new TelemetryArchiveException("{0} is already allocated".Args(typeof(TelemetryReceiverService).FullName));

        s_Instance = this;
      }
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_ArchiveStore);
      s_Instance = null;
    }
    #endregion

    #region Fields
    private TelemetryArchiveStore m_ArchiveStore;
    #endregion

    #region Properties
    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set; }

    [Config(Default = DEFAULT_LOG_LEVEL)]
    [ExternalParameter(SysConsts.EXT_PARAM_GROUP_WORKER, CoreConsts.EXT_PARAM_GROUP_LOG)]
    public MessageType LogLevel { get; set; }
    #endregion

    #region Public
    public void SendDatums(params Datum[] data)
    {
      var transaction = m_ArchiveStore.BeginTransaction();
      try
      {
        m_ArchiveStore.Put(data, transaction);
        m_ArchiveStore.CommitTransaction(transaction);
      }
      catch (Exception error)
      {
        m_ArchiveStore.RollbackTransaction(transaction);

        Log(MessageType.CatastrophicError, "put('{0}')".Args(data.Length), error.ToMessageWithType(), error);

        throw new TelemetryArchiveException(StringConsts.TELEMETRY_ARCHIVE_PUT_TX_BODY_ERROR.Args(m_ArchiveStore.GetType().Name, error.ToMessageWithType()), error);
      }
    }

    public Guid Log(MessageType type,
                string from,
                string message,
                Exception error = null,
                Guid? relatedMessageID = null,
                string parameters = null)
    {
      if (type < LogLevel) return Guid.Empty;

      var logMessage = new Message
      {
        Topic = SysConsts.LOG_TOPIC_WORKER,
        Text = message ?? string.Empty,
        Type = type,
        From = "{0}.{1}".Args(this.GetType().Name, from),
        Exception = error,
        Parameters = parameters
      };
      if (relatedMessageID.HasValue) logMessage.RelatedTo = relatedMessageID.Value;

      App.Log.Write(logMessage);

      return logMessage.Guid;
    }
    #endregion

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      DisposeAndNull(ref m_ArchiveStore);

      var storeNode = node[CONFIG_ARCHIVE_STORE_SECTION];
      if (storeNode.Exists)
        m_ArchiveStore = FactoryUtils.Make<TelemetryArchiveStore>(storeNode, args: new object[] { this, storeNode });
    }

    protected override void DoStart()
    {
      if (m_ArchiveStore == null)
        throw new TelemetryArchiveException("{0} does not have archive store injected".Args(GetType().Name));

      if (m_ArchiveStore is IDaemon) ((IDaemon)m_ArchiveStore).Start();
      base.DoStart();
    }

    protected override void DoSignalStop()
    {
      if (m_ArchiveStore is IDaemon) ((IDaemon)m_ArchiveStore).SignalStop();
      base.DoSignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      if (m_ArchiveStore is IDaemon) ((IDaemon)m_ArchiveStore).WaitForCompleteStop();
      base.DoWaitForCompleteStop();
    }
    #endregion
  }
}
