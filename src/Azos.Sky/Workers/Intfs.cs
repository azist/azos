using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Security;
using Azos.Sky.Coordination;

namespace Azos.Sky.Workers
{
  /// <summary>
  /// Represents the Todo hosting entity
  /// </summary>
  public interface ITodoHost
  {
    /// <summary>
    /// WARNING: this method should not be used by a typical business code as it enqueues messages
    /// locally bypassing all routing and networking stack. This method is used for performance optimization
    /// for some limited Todo instances that do not rely on sequencing and sharding and are guaranteed to have
    /// a local queue capable of processing this message
    /// </summary>
    void LocalEnqueue(Todo todos);

    /// <summary>
    /// WARNING: this method should not be used by a typical business code as it enqueues messages
    /// locally bypassing all routing and networking stack. This method is used for performance optimization
    /// for some limited Todo instances that do not rely on sequencing and sharding and are guaranteed to have
    /// a local queue capable of processing this message
    /// </summary>
    void LocalEnqueue(IEnumerable<Todo> todos);

    /// <summary>
    /// WARNING: this method should not be used by a typical business code as it enqueues messages
    /// locally bypassing all routing and networking stack. This method is used for performance optimization
    /// for some limited Todo instances that do not rely on sequencing and sharding and are guaranteed to have
    /// a local queue capable of processing this message
    /// </summary>
    Task LocalEnqueueAsync(IEnumerable<Todo> todos);

    /// <summary>
    /// Returns true if host is instrumented
    /// </summary>
    bool InstrumentationEnabled { get;}

    /// <summary>
    /// Emits a local log message based on host's logging policy
    /// </summary>
    Guid Log(MessageType type,
             Todo todo,
             string from,
             string message,
             Exception error = null,
             Guid? relatedMessageID = null,
             string parameters = null);

    /// <summary>
    /// Emits a local log message based on host's logging policy
    /// </summary>
    Guid Log(MessageType type,
             string from,
             string message,
             Exception error = null,
             Guid? relatedMessageID = null,
             string parameters = null);
  }

  /// <summary>
  /// Represents the Todo hosting entity
  /// </summary>
  public interface IProcessHost
  {
    /// <summary>
    /// WARNING: this method should not be used by a typical business code as it enqueues messages
    /// locally bypassing all routing and networking stack. This method is used for performance optimization
    /// for some limited Todo instances that do not rely on sequencing and sharding and are guaranteed to have
    /// a local queue capable of processing this message
    /// </summary>
    void LocalSpawn(Process process, AuthenticationToken? token = null);

    ResultSignal LocalDispatch(Signal signal);

    void Update(Process process, bool sysOnly = false);

    void Finalize(Process process);

    /// <summary>
    /// Returns true if host is instrumented
    /// </summary>
    bool InstrumentationEnabled { get; }

    /// <summary>
    /// Emits a local log message based on host's logging policy
    /// </summary>
    Guid Log(MessageType type,
             Process todo,
             string from,
             string message,
             Exception error = null,
             Guid? relatedMessageID = null,
             string parameters = null);

    /// <summary>
    /// Emits a local log message based on host's logging policy
    /// </summary>
    Guid Log(MessageType type,
             string from,
             string message,
             Exception error = null,
             Guid? relatedMessageID = null,
             string parameters = null);
  }

  public interface IProcessManager
  {
    /// <summary>
    /// Allocates process identifier (PID) in the specified zone
    /// </summary>
    PID Allocate(string zonePath);

    /// <summary>
    /// Allocates process identifier (PID) in the specified zone based on a mutualy exclusive ID (mutex).
    /// Mutexes are case-insensitive
    /// </summary>
    PID AllocateMutex(string zonePath, string mutex);

    /// <summary>
    /// Starts the process at the host specified by PID
    /// </summary>
    void Spawn<TProcess>(TProcess process) where TProcess : Process;
    void Spawn(PID pid, IConfigSectionNode args, Type type = null);
    void Spawn(PID pid, IConfigSectionNode args, Guid type);

    Task Async_Spawn<TProcess>(TProcess process) where TProcess : Process;
    Task Async_Spawn(PID pid, IConfigSectionNode args, Type type = null);
    Task Async_Spawn(PID pid, IConfigSectionNode args, Guid type);

    ResultSignal Dispatch<TSignal>(TSignal signal) where TSignal : Signal;
    ResultSignal Dispatch(PID pid, IConfigSectionNode args, Type type = null);
    ResultSignal Dispatch(PID pid, IConfigSectionNode args, Guid type);

    Task<ResultSignal> Async_Dispatch<TSignal>(TSignal signal) where TSignal : Signal;
    Task<ResultSignal> Async_Dispatch(PID pid, IConfigSectionNode args, Type type = null);
    Task<ResultSignal> Async_Dispatch(PID pid, IConfigSectionNode args, Guid type);

    int Enqueue<TTodo>(IEnumerable<TTodo> todos, string hostSetName, string svcName) where TTodo : Todo;
    void Enqueue<TTodo>(TTodo todo, string hostSetName, string svcName) where TTodo : Todo;
    void Enqueue(string hostSetName, string svcName, IConfigSectionNode args, Type type = null);
    void Enqueue(string hostSetName, string svcName, IConfigSectionNode args, Guid type);

    Task<int> Async_Enqueue<TTodo>(IEnumerable<TTodo> todos, string hostSetName, string svcName) where TTodo : Todo;
    Task Async_Enqueue<TTodo>(TTodo todo, string hostSetName, string svcName) where TTodo : Todo;
    Task Async_Enqueue(string hostSetName, string svcName, IConfigSectionNode args, Type type = null);
    Task Async_Enqueue(string hostSetName, string svcName, IConfigSectionNode args, Guid type);

    TProcess Get<TProcess>(PID pid) where TProcess : Process;

    ProcessDescriptor GetDescriptor(PID pid);

    IEnumerable<ProcessDescriptor> List(string zonePath, IConfigSectionNode filter = null);

    IGuidTypeResolver ProcessTypeResolver { get; }
    IGuidTypeResolver SignalTypeResolver { get; }
    IGuidTypeResolver TodoTypeResolver { get; }

    IRegistry<HostSet> HostSets { get; }
  }


  public interface IProcessManagerImplementation : IProcessManager, IApplicationComponent, IDisposable, IConfigurable, IInstrumentable
  {
  }
}
