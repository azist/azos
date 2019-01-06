/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps;
using Azos.Apps.Injection;
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
    [Inject] IApplication m_App;
    public void SendDatums(params Datum[] data) => m_App.NonNull(nameof(m_App))
                                                        .Singletons
                                                        .Get<TelemetryReceiverService>().NonNull(nameof(TelemetryReceiverService))
                                                        .SendDatums(data);
  }


  /// <summary>
  /// Provides singleton server implementation of Contracts.ITelemetryReceiver.
  /// The server is usually hosted by composite ASH process
  /// </summary>
  public sealed class TelemetryReceiverService : DaemonWithInstrumentation<IApplicationComponent>, Contracts.ITelemetryReceiver
  {
    #region CONSTS
    public const string CONFIG_ARCHIVE_STORE_SECTION = "archive-store";

    public const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;
    #endregion

    #region STATIC/.ctor

    public TelemetryReceiverService(IApplicationComponent director) : base(director)
    {
      if (!App.Singletons.GetOrCreate(() => this).created)
        throw new TelemetryArchiveException("{0} is already allocated".Args(typeof(TelemetryReceiverService).FullName));
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_ArchiveStore);
      App.Singletons.Remove<TelemetryReceiverService>();
    }
    #endregion

    #region Fields
    private TelemetryArchiveStore m_ArchiveStore;
    #endregion

    #region Properties
    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_INSTRUMENTATION;

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set; }

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

        WriteLog(MessageType.CatastrophicError, "put('{0}')".Args(data.Length), error.ToMessageWithType(), error);

        throw new TelemetryArchiveException(StringConsts.TELEMETRY_ARCHIVE_PUT_TX_BODY_ERROR.Args(m_ArchiveStore.GetType().Name, error.ToMessageWithType()), error);
      }
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
