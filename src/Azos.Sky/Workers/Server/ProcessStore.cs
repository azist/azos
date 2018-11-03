using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;

namespace Azos.Sky.Workers.Server
{
  /// <summary>
  /// Represents a base for entities that store queue data
  /// </summary>
  public abstract class ProcessStore : ApplicationComponent
  {
    protected ProcessStore(ProcessControllerService director, IConfigSectionNode node) : base(director)
    {
      ConfigAttribute.Apply(this, node);
    }

    /// <summary>
    /// References service that this store is under
    /// </summary>
    public ProcessControllerService ProcessController { get { return (ProcessControllerService)ComponentDirector;} }

    public abstract IEnumerable<ProcessDescriptor> List(int processorID);

    public abstract bool TryGetByPID(PID pid, out ProcessFrame frame);

    public virtual ProcessFrame GetByPID(PID pid)
    {
      ProcessFrame result;
      if (!TryGetByPID(pid, out result))
        throw new WorkersException("TODO");
      return result;
    }

    public abstract void Put(ProcessFrame frame, object transaction);

    public abstract void Update(ProcessFrame frame, bool sysOnly, object transaction);

    public abstract void Delete(ProcessFrame frame, object transaction);

    public abstract object BeginTransaction();

    public abstract void CommitTransaction(object transaction);

    public abstract void RollbackTransaction(object transaction);

    protected Guid Log(MessageType type,
                       string from,
                       string message,
                       Exception error = null,
                       Guid? relatedMessageID = null,
                       string parameters = null)
    {
      if (type < ProcessController.LogLevel) return Guid.Empty;

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
  }
}
