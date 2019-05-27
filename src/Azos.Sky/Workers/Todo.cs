/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Access;

namespace Azos.Sky.Workers
{
  /// <summary>
  /// Represents a unit of abstract work that is dispatched to a remote worker in an asynchronous fashion.
  /// Todos are essentially a form of a queueable asynchronous one-way command object (Execute() does not return business object).
  /// Todos are dequeued in the order of submission and SysStartDate constraint, processed sequentially or in-parallel depending on a SysParallelKey
  /// </summary>
  [Serializable]
  public abstract class Todo : AmorphousTypedDoc, IDistributedStableHashProvider
  {
    /// <summary>
    /// Denotes states of Todo execution state machine
    /// </summary>
    public struct ExecuteState
    {
      /// <summary>
      /// The todo will reexecute depending on ReexecuteAfterErrorInMs() having updated in-place
      /// </summary>
      public static readonly ExecuteState ReexecuteUpdatedAfterError = new ExecuteState(-6, true);

      /// <summary>
      /// The todo will reexecute depending on ReexecuteAfterErrorInMs() having sys fields updated in-place
      /// </summary>
      public static readonly ExecuteState ReexecuteAfterError = new ExecuteState(-5, true);

      /// <summary>
      /// The todo must be updated in-place and reexecuted when due
      /// </summary>
      public static readonly ExecuteState ReexecuteUpdated = new ExecuteState(-4, true);

      /// <summary>
      /// The todo sys fields must be updated in-place and reexecuted when due
      /// </summary>
      public static readonly ExecuteState ReexecuteSysUpdated = new ExecuteState(-3, true);

      /// <summary>
      /// The todo must be re-executed as-is again
      /// </summary>
      public static readonly ExecuteState Reexecute = new ExecuteState(-2, true);

      /// <summary>
      /// The execution is completed and todo should be discarded from the queue
      /// </summary>
      public static readonly ExecuteState Complete = new ExecuteState(-1, true);

      /// <summary>
      /// Initial state
      /// </summary>
      public static readonly ExecuteState Initial = new ExecuteState(0, true);

      internal ExecuteState(int state, bool sys) { State = state; }
      public ExecuteState(int state)
      {
        if (state <= 0) throw new WorkersException(StringConsts.ARGUMENT_ERROR + "state < 0");
        State = state;
      }

      public readonly int State;

      public static bool operator ==(ExecuteState a, ExecuteState b) { return a.State == b.State; }
      public static bool operator !=(ExecuteState a, ExecuteState b) { return a.State != b.State; }

      public override int GetHashCode() { return State; }
      public override bool Equals(object obj) { return this == (ExecuteState)obj; }

      public override string ToString() { return "ExecuteState({0})".Args(State); }
    }

    /// <summary>
    /// Factory method that creates new Todos assigning them new GDID
    /// </summary>
    public static TTodo MakeNew<TTodo>(IApplication app) where TTodo : Todo, new() { return makeDefault(app.AsSky(), new TTodo()); }

    /// <summary>
    /// Factory method that creates new Todos from Type and Configuration assigning them new GDID
    /// </summary>
    public static Todo MakeNew(IApplication app, Type type, IConfigSectionNode args) { return makeDefault(app.AsSky(), FactoryUtils.MakeAndConfigure<Todo>(args, type)); }

    private static TTodo makeDefault<TTodo>(ISkyApplication app, TTodo todo) where TTodo : Todo
    {
      app.DependencyInjector.InjectInto(todo);
      //warning: Todo IDs must be cross-type unique (should not depend on queue)
      todo.m_SysID = app.GdidProvider.GenerateOneGdid(SysConsts.GDID_NS_WORKER, SysConsts.GDID_NAME_WORKER_TODO);
      todo.m_SysCreateTimestampUTC = app.TimeSource.UTCNow;
      return todo;
    }

    protected Todo() { }

#pragma warning disable 649
    [Inject] ISkyApplication  m_App;
#pragma warning restore 649

    public ISkyApplication App => m_App.NonNull(nameof(m_App));

    private GDID m_SysID;
    private DateTime m_SysCreateTimestampUTC;
    /// <summary>
    /// Infrustructure method, developers do not call
    /// </summary>
    public void ____Deserialize(GDID id, DateTime ts) { m_SysID = id; m_SysCreateTimestampUTC = ts;}

    /// <summary>
    /// Globally-unique ID of the TODO
    /// </summary>
    public GDID SysID { get { return m_SysID; } }

    /// <summary>
    /// When was created
    /// </summary>
    public DateTime SysCreateTimeStampUTC { get { return m_SysCreateTimestampUTC; } }

    /// <summary>
    /// Provides the sharding key which is used for dispatching items into HostSets
    /// </summary>
    public string SysShardingKey { get; set; }

    /// <summary>
    /// Provides the key which is used for parallel processing: items with the same key
    /// get executed sequentially
    /// </summary>
    public string SysParallelKey { get; set;}

    /// <summary>
    /// Provides relative processing priority of processing
    /// </summary>
    public int SysPriority { get; set; }

    /// <summary>
    /// When set, tells the system when (UTC) should the item be considered for processing
    /// </summary>
    public DateTime SysStartDate { get; set; }

    /// <summary>
    /// Provides current state machine execution state
    /// </summary>
    public ExecuteState SysState { get; internal set; }

    /// <summary>
    /// Provides current machine execution retry state
    /// </summary>
    public int SysTries { get; internal set; }

    /// <summary>
    /// Executes the todo. Override to perform actual logic.
    /// You have to handle all exceptions, otherwise the leaked exception will
    /// complete the todo with error. Return the result that describes whether the item completed or should be reexecuted again.
    /// Keep in mind: Todos are not designed to execute long-running(tens+ of seconds) processes, launch other async workers instead
    /// </summary>
    protected internal abstract ExecuteState Execute(ITodoHost host, DateTime utcBatchNow);

    /// <summary>
    /// Invoked to determine when should the next reexecution takes place after an error.
    /// Throw exception if your buisiness case has exhausted all allowed retries as judjed by SysTries.
    /// Return -1 to indicate the immediate execution without consideration of SysTries (default)
    /// </summary>
    protected internal virtual int RetryAfterErrorInMs(DateTime utcBatchNow) { return -1; }

    public override string ToString() { return "{0}('{1}')".Args(GetType().FullName, SysID); }

    public override int GetHashCode() { return m_SysID.GetHashCode(); }

    public override bool Equals(Doc other)
    {
      var otodo = other as Todo;
      if (otodo==null) return false;
      return this.m_SysID == otodo.m_SysID;
    }

    public ulong GetDistributedStableHash()
    {
      return m_SysID.GetDistributedStableHash();
    }

    public void ValidateAndPrepareForEnqueue(string targetName)
    {
      DoPrepareForEnqueuePreValidate(targetName);

        var ve = this.Validate(targetName);
        if (ve != null)
          throw new WorkersException(StringConsts.ARGUMENT_ERROR + "Todo.ValidateAndPrepareForEnqueue(todo).validate: " + ve.ToMessageWithType(), ve);

      DoPrepareForEnqueuePostValidate(targetName);
    }

    public override Exception Validate(string targetName)
    {
      var ve = base.Validate(targetName);
      if (ve != null) return ve;

      if (SysID.IsZero)
        return new FieldValidationException(this, "SysID", "SysID.IsZero, use NewTodo<>() to make new instances");

      return null;
    }

    protected virtual void DoPrepareForEnqueuePreValidate(string targetName) { }
    protected virtual void DoPrepareForEnqueuePostValidate(string targetName) { }
  }
}
