using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;


namespace Azos.Sky.Log.Server
{
  /// <summary>
  /// Glue adapter for Contracts.ILogReceiver
  /// </summary>
  public sealed class LogReceiverServer : Contracts.ILogReceiver
  {
    public void SendLog(Message data)
    {
      LogReceiverService.Instance.SendLog(data);
    }

    public Message GetByID(Guid id, string channel = null)
    {
      return LogReceiverService.Instance.GetByID(id, channel);
    }

    public IEnumerable<Message> List(string archiveDimensionsFilter, DateTime startDate, DateTime endDate, MessageType? type = null,
      string host = null, string channel = null, string topic = null,
      Guid? relatedTo = null, int skipCount = 0)
    {
      return LogReceiverService.Instance.List(archiveDimensionsFilter, startDate, endDate, type, host, channel, topic, relatedTo, skipCount);
    }
  }

  /// <summary>
  /// Provides server implementation of Contracts.ILogReceiver
  /// </summary>
  public sealed class LogReceiverService : DaemonWithInstrumentation<object>, Contracts.ILogReceiver
  {
    #region CONSTS
    public const string CONFIG_ARCHIVE_MAPPER_SECTION = "archive-mapper";
    public const string CONFIG_ARCHIVE_STORE_SECTION = "archive-store";

    public const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;
    #endregion

    #region STATIC/.ctor
    private static object s_Lock = new object();
    private static volatile LogReceiverService s_Instance;

    internal static LogReceiverService Instance
    {
      get
      {
        var instance = s_Instance;
        if (instance == null)
          throw new LogArchiveException("{0} is not allocated".Args(typeof(LogReceiverService).FullName));
        return instance;
      }
    }

    public LogReceiverService() : this(null) { }

    public LogReceiverService(object director) : base(director)
    {
      LogLevel = MessageType.Warning;

      lock (s_Lock)
      {
        if (s_Instance != null)
          throw new LogArchiveException("{0} is already allocated".Args(typeof(LogReceiverService).FullName));

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
    private LogArchiveStore m_ArchiveStore;
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
    public void SendLog(Message data)
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

        Log(MessageType.CatastrophicError, "put('{0}', '{1}', '{2}')".Args(data.Host, data.From, data.Guid), error.ToMessageWithType(), error);

        throw new LogArchiveException(StringConsts.LOG_ARCHIVE_PUT_TX_BODY_ERROR.Args(m_ArchiveStore.GetType().Name, error.ToMessageWithType()), error);
      }
    }

    public Message GetByID(Guid id, string channel = null)
    {
      return m_ArchiveStore.GetByID(id, channel);
    }

    public IEnumerable<Message> List(string archiveDimensionsFilter, DateTime startDate, DateTime endDate, MessageType? type = null,
      string host = null, string channel = null, string topic = null,
      Guid? relatedTo = null, int skipCount = 0)
    {
      return m_ArchiveStore.List(archiveDimensionsFilter, startDate, endDate, type, host, channel, topic, relatedTo, skipCount);
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

      LogArchiveDimensionsMapper mapper = null;

      DisposeAndNull(ref m_ArchiveStore);

      var mapperNode = node[CONFIG_ARCHIVE_MAPPER_SECTION];
      mapper = FactoryUtils.Make<LogArchiveDimensionsMapper>(mapperNode, defaultType: typeof(LogArchiveDimensionsMapper), args: new object[] { this, mapperNode });

      var storeNode = node[CONFIG_ARCHIVE_STORE_SECTION];
      if (storeNode.Exists)
        m_ArchiveStore = FactoryUtils.Make<LogArchiveStore>(storeNode, args: new object[] { this, mapper, storeNode });
    }

    protected override void DoStart()
    {
      if (m_ArchiveStore == null)
        throw new LogArchiveException("{0} does not have archive store injected".Args(GetType().Name));

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
