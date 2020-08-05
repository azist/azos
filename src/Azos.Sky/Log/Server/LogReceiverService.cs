/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;


namespace Azos.Sky.Log.Server
{
  /// <summary>
  /// Glue trampoline for Contracts.ILogReceiver
  /// </summary>
  public sealed class LogReceiverServer : Contracts.ILogReceiver
  {
    [Inject] IApplication m_App;

    public LogReceiverService Service => m_App.NonNull(nameof(m_App))
                                              .Singletons
                                              .Get<LogReceiverService>()
                                              .NonNull(nameof(LogReceiverService));


    public void SendLog(Message data)
      => Service.SendLog(data);

    public Message GetByID(Guid id, Atom channel)
      => Service.GetByID(id, channel);

    public IEnumerable<Message> List(Atom channel, string archiveDimensionsFilter, DateTime startDate, DateTime endDate, MessageType? type = null,
      string host = null,string topic = null,
      Guid? relatedTo = null, int skipCount = 0)
      => Service.List(channel, archiveDimensionsFilter, startDate, endDate, type, host, topic, relatedTo, skipCount);
  }

  /// <summary>
  /// Provides singleton server implementation of Contracts.ILogReceiver
  /// </summary>
  public sealed class LogReceiverService : DaemonWithInstrumentation<IApplicationComponent>, Contracts.ILogReceiver
  {
    #region CONSTS
    public const string CONFIG_ARCHIVE_MAPPER_SECTION = "archive-mapper";
    public const string CONFIG_ARCHIVE_STORE_SECTION = "archive-store";

    #endregion

    #region .ctor
    public LogReceiverService(IApplication app) : base(app)
    {
      if (!App.Singletons.GetOrCreate(() => this).created)
        throw new LogArchiveException("{0} is already allocated".Args(typeof(LogReceiverService).FullName));
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_ArchiveStore);
      App.Singletons.Remove<LogReceiverService>();
    }
    #endregion

    #region Fields
    private LogArchiveStore m_ArchiveStore;
    #endregion

    public override string ComponentLogTopic => CoreConsts.LOG_TOPIC;

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set; }


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

        WriteLog(MessageType.CatastrophicError, "put('{0}', '{1}', '{2}')".Args(data.Host, data.From, data.Guid), error.ToMessageWithType(), error);

        throw new LogArchiveException(StringConsts.LOG_ARCHIVE_PUT_TX_BODY_ERROR.Args(m_ArchiveStore.GetType().Name, error.ToMessageWithType()), error);
      }
    }

    public Message GetByID(Guid id, Atom channel)
    {
      return m_ArchiveStore.GetByID(id, channel);
    }

    public IEnumerable<Message> List(Atom channel, string archiveDimensionsFilter, DateTime startDate, DateTime endDate, MessageType? type = null,
      string host = null, string topic = null,
      Guid? relatedTo = null, int skipCount = 0)
    {
      return m_ArchiveStore.List(channel, archiveDimensionsFilter, startDate, endDate, type, host, topic, relatedTo, skipCount);
    }

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
  }
}
