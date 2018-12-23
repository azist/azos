using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;
using Azos.Instrumentation;

namespace Azos.Sky.Instrumentation.Server
{
  /// <summary>
  /// Represents a base for entities that archive instrumentation data
  /// </summary>
  public abstract class TelemetryArchiveStore : ApplicationComponent
  {
    protected TelemetryArchiveStore(TelemetryReceiverService director, IConfigSectionNode node) : base(director)
    {
      ConfigAttribute.Apply(this, node);
    }

    /// <summary>
    /// References service that this store is under
    /// </summary>
    public TelemetryReceiverService ArchiveService { get { return (TelemetryReceiverService)ComponentDirector;} }


    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_INSTRUMENTATION;


    public abstract object BeginTransaction();

    public abstract void CommitTransaction(object transaction);

    public abstract void RollbackTransaction(object transaction);

    public abstract void Put(Datum[] data, object transaction);
  }
}
