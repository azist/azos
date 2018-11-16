using System;
using System.Collections.Generic;
using System.Threading;

using Azos;
using Azos.Apps;
using Azos.Log;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Security;

namespace Azos.Sky.Workers.Server
{
  public class ProcessControllerServer : Contracts.IProcessController
  {
    public void Spawn(ProcessFrame frame)
    {
      ProcessControllerService.Instance.Spawn(frame);
    }

    public ProcessFrame Get(PID pid)
    {
      return ProcessControllerService.Instance.Get(pid);
    }

    public ProcessDescriptor GetDescriptor(PID pid)
    {
      return ProcessControllerService.Instance.GetDescriptor(pid);
    }

    public SignalFrame Dispatch(SignalFrame signal)
    {
      return ProcessControllerService.Instance.Dispatch(signal);
    }

    public IEnumerable<ProcessDescriptor> List(int processorID)
    {
      return ProcessControllerService.Instance.List(processorID);
    }
  }

  public class ProcessControllerService : DaemonWithInstrumentation<object>, Contracts.IProcessController, IProcessHost
  {
    #region CONSTS
    public const string CONFIG_PROCESS_CONTROLLER_SECTION = "process-controller";
    public const string CONFIG_PROCESS_STORE_SECTION = "process-store";

    public const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;
    #endregion

    #region STATIC/.ctor

    private static object s_Lock = new object();
    private static volatile ProcessControllerService s_Instance;

    internal static ProcessControllerService Instance
    {
      get
      {
        var instance = s_Instance;
        if (instance == null) throw new WorkersException("{0} is not allocated".Args(typeof(ProcessControllerService).FullName));
        return instance;
      }
    }

    public ProcessControllerService() : this(null) { }

    public ProcessControllerService(object director) : base(director)
    {
      LogLevel = MessageType.Error;

      lock (s_Lock)
      {
        if (s_Instance != null)
          throw new WorkersException("{0} is already allocated".Args(typeof(ProcessControllerService).FullName));

        s_Instance = this;
      }
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_ProcessStore);
      s_Instance = null;
    }

    #endregion

    private ProcessStore m_ProcessStore;

    [Config]
    [ExternalParameter(SysConsts.EXT_PARAM_GROUP_WORKER, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set; }

    [Config(Default = DEFAULT_LOG_LEVEL)]
    [ExternalParameter(SysConsts.EXT_PARAM_GROUP_WORKER, CoreConsts.EXT_PARAM_GROUP_LOG)]
    public MessageType LogLevel { get; set; }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      if (node == null)
        node = App.ConfigRoot[CONFIG_PROCESS_CONTROLLER_SECTION];

      base.DoConfigure(node);

      if (node == null) return;

      DisposeAndNull(ref m_ProcessStore);
      var queueStoreNode = node[CONFIG_PROCESS_STORE_SECTION];
      if (queueStoreNode.Exists)
        m_ProcessStore = FactoryUtils.Make<ProcessStore>(queueStoreNode, args: new object[] { this, queueStoreNode });
    }

    protected override void DoStart()
    {
      if (m_ProcessStore == null)
        throw new WorkersException("{0} does not have process store injected".Args(GetType().Name));

      if (m_ProcessStore is IDaemon) ((IDaemon)m_ProcessStore).Start();
      base.DoStart();
    }

    protected override void DoSignalStop()
    {
      if (m_ProcessStore is IDaemon) ((IDaemon)m_ProcessStore).SignalStop();
      base.DoSignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      if (m_ProcessStore is IDaemon) ((IDaemon)m_ProcessStore).WaitForCompleteStop();
      base.DoWaitForCompleteStop();
    }

    void IProcessHost.LocalSpawn(Process process, AuthenticationToken? token) { Spawn(new ProcessFrame(process)); }

    public void Spawn(ProcessFrame frame)
    {
      CheckDaemonActive();

      var tx = m_ProcessStore.BeginTransaction();
      try
      {
        spawnCore(frame, tx);
        m_ProcessStore.CommitTransaction(tx);
      }
      catch (Exception error)
      {
        m_ProcessStore.RollbackTransaction(tx);

        Log(MessageType.CatastrophicError, "Spawn", error.ToMessageWithType(), error);

        // TODO: fix exception
        throw new WorkersException(StringConsts.TODO_ENQUEUE_TX_BODY_ERROR.Args(error.ToMessageWithType()), error);
      }
    }

    public ProcessFrame Get(PID pid)
    {
      return m_ProcessStore.GetByPID(pid);
    }

    public ProcessDescriptor GetDescriptor(PID pid)
    {
      ProcessFrame processFrame = m_ProcessStore.GetByPID(pid);
      return processFrame.Descriptor;
    }

    ResultSignal IProcessHost.LocalDispatch(Signal signal) { return dispatch(new SignalFrame(signal)); }

    public SignalFrame Dispatch(SignalFrame signalFrame) { return new SignalFrame(dispatch(signalFrame)); }

    private ResultSignal dispatch(SignalFrame signalFrame)
    {
      lockProcess(signalFrame.PID.ID);
      try
      {
        ProcessFrame processFrame = m_ProcessStore.GetByPID(signalFrame.PID);

        var process = processFrame.Materialize(SkySystem.ProcessManager.ProcessTypeResolver);
        var signal = signalFrame.Materialize(SkySystem.ProcessManager.SignalTypeResolver);
        if (process == null || signal == null)//safeguard
          throw new WorkersException("TODO");

        return process.Accept(this, signal);
      }
      finally
      {
        releaseProcess(signalFrame.PID.ID);
      }
    }

    void IProcessHost.Update(Process process, bool sysOnly) { Update(process, sysOnly); }

    public void Update(Process process, bool sysOnly) { Update(new ProcessFrame(process, sysOnly ? (int?)null : ProcessFrame.SERIALIZER_BSON), sysOnly); }

    public void Update(ProcessFrame frame, bool sysOnly)
    {
      CheckDaemonActive();

      var tx = m_ProcessStore.BeginTransaction();
      try
      {
        update(frame, sysOnly, tx);
        m_ProcessStore.CommitTransaction(tx);
      }
      catch (Exception error)
      {
        m_ProcessStore.RollbackTransaction(tx);

        Log(MessageType.CatastrophicError, "Update", error.ToMessageWithType(), error);

        // TODO fix exception
        throw new WorkersException(StringConsts.TODO_ENQUEUE_TX_BODY_ERROR.Args(error.ToMessageWithType()), error);
      }
    }

    void IProcessHost.Finalize(Process process) { Finalize(process); }

    public void Finalize(Process process) { Finalize(new ProcessFrame(process)); }

    public void Finalize(ProcessFrame frame)
    {
      CheckDaemonActive();

      var tx = m_ProcessStore.BeginTransaction();
      try
      {
        delete(frame, tx);
        m_ProcessStore.CommitTransaction(tx);
      }
      catch (Exception error)
      {
        m_ProcessStore.RollbackTransaction(tx);

        Log(MessageType.CatastrophicError, "Finalize", error.ToMessageWithType(), error);

        // TODO fix exception
        throw new WorkersException(StringConsts.TODO_ENQUEUE_TX_BODY_ERROR.Args(error.ToMessageWithType()), error);
      }
    }

    public IEnumerable<ProcessDescriptor> List(int processorID)
    {
      CheckDaemonActive();

      return m_ProcessStore.List(processorID);
    }

    /// <summary>
    /// Writes to log on behalf of worker service
    /// </summary>
    public Guid Log(MessageType type,
                    Process process,
                    string from,
                    string message,
                    Exception error = null,
                    Guid? relatedMessageID = null,
                    string parameters = null)
    {
      if (type < LogLevel) return Guid.Empty;

      return Log(type, "{0}.{1}".Args(process.GetType().FullName, from), message, error, relatedMessageID, parameters);
    }

    /// <summary>
    /// Writes to log on behalf of worker service
    /// </summary>
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


    private void spawnCore(ProcessFrame frame, object tx)
    {
      var pid = frame.Descriptor.PID;
      if (pid.IsUnique)
      {
        put(frame, tx);
        return;
      }

      lockProcess(pid.ID);
      try
      {
        var utcNow = App.TimeSource.UTCNow;
        ProcessFrame existing;
        if (!m_ProcessStore.TryGetByPID(pid, out existing))
        {
          put(frame, tx);
          return;
        }

        var processExisting = existing.Materialize(SkySystem.ProcessManager.ProcessTypeResolver);
        var processAnother = frame.Materialize(SkySystem.ProcessManager.ProcessTypeResolver);
        if (processExisting == null || processAnother == null)//safeguard
        {
          put(frame, tx);
          return;
        }

        try
        {
          processExisting.Merge(this, utcNow, processAnother);
        }
        catch (Exception error)
        {
          // TODO : fix exception
          throw new WorkersException(StringConsts.TODO_CORRELATED_MERGE_ERROR.Args(processExisting, processAnother, error.ToMessageWithType()), error);
        }
      }
      finally
      {
        releaseProcess(pid.ID);
      }
    }

    private void put(ProcessFrame frame, object transaction = null)
    {
      try
      {
        frame.Descriptor = new ProcessDescriptor(frame.Descriptor, ProcessStatus.Started, "Started", App.TimeSource.UTCNow, "@{0}@{1}".Args(App.Name, SkySystem.HostName));
        m_ProcessStore.Put(frame, transaction);
      }
      catch (Exception e)
      {
        Log(MessageType.Critical, "put", "{0} Leaked: {1}".Args(frame, e.ToMessageWithType()), e);
        throw;
      }
    }

    private void update(ProcessFrame frame, bool sysOnly, object transaction = null)
    {
      try
      {
        m_ProcessStore.Update(frame, sysOnly, transaction);
      }
      catch (Exception e)
      {
        Log(MessageType.Critical, "update", "{0} Leaked: {1}".Args(frame, e.ToMessageWithType()), e);
        throw;
      }
    }

    private void delete(ProcessFrame frame, object transaction = null)
    {
      try
      {
        m_ProcessStore.Delete(frame, transaction);
      }
      catch (Exception e)
      {
        Log(MessageType.Critical, "delete", "{0} Leaked: {1}".Args(frame, e.ToMessageWithType()), e);
        throw;
      }
    }

    private class _lck { public Thread Thread; public int Count; }
    private Dictionary<string, _lck> m_Locker = new Dictionary<string, _lck>(StringComparer.Ordinal);

    private void lockProcess(string key)
    {
      if (key == null) key = string.Empty;

      var ct = Thread.CurrentThread;

      uint spinCount = 0;
      while (true)
      {
        lock (m_Locker)
        {
          _lck lck;
          if (!m_Locker.TryGetValue(key, out lck))
          {
            m_Locker.Add(key, new _lck { Thread = ct, Count = 1 } );
            return;
          }

          if (lck.Thread == ct)//if already acquired by this thread
          {
            lck.Count++;
            return;
          }
        }

        if (spinCount < 100) Thread.SpinWait(500);
        else Thread.Yield();

        unchecked { spinCount++; }
      }
    }

    private bool releaseProcess(string key)
    {
      if (key == null) key = string.Empty;

      lock (m_Locker)
      {
        _lck lck;
        if (!m_Locker.TryGetValue(key, out lck)) return false;

        if (Thread.CurrentThread != lck.Thread) return false;//not locked by this thread

        lck.Count--;

        if (lck.Count == 0)
          m_Locker.Remove(key);

        return true;
      }
    }
  }
}
