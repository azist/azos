using System;
using System.Linq;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Instrumentation;

using Azos.Sky.Coordination;
using System.Threading.Tasks;

namespace Azos.Sky.Workers
{
  public abstract class ProcessManagerBase : DaemonWithInstrumentation<IApplicationComponent>, IProcessManagerImplementation
  {
    #region CONSTS
    public const string CONFIG_PROCESS_MANAGER_SECTION = "process-manager";
    public const string CONFIG_PROCESS_TYPE_RESOLVER_SECTION = "process-type-resolver";
    public const string CONFIG_SIGNAL_TYPE_RESOLVER_SECTION = "signal-type-resolver";
    public const string CONFIG_TODO_TYPE_RESOLVER_SECTION = "todo-type-resolver";
    public const string CONFIG_PATH_ATTR = "path";
    public const string CONFIG_SEARCH_PARENT_ATTR = "search-parent";
    public const string CONFIG_TRANSCEND_NOC_ATTR = "transcend-noc";
    public const string CONFIG_TYPE_GUID_ATTR = "type-guid";

    private static readonly TimeSpan INSTRUMENTATION_INTERVAL = TimeSpan.FromMilliseconds(3700);
    #endregion

    #region .ctor
    public ProcessManagerBase(IApplication app) : base(app)
    {
      m_HostSets = new Registry<HostSet>(false);
    }

    protected override void Destructor()
    {
      foreach (var hs in m_HostSets)
        hs.Dispose();

      DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
      base.Destructor();
    }
    #endregion

    #region Fields
    private IGuidTypeResolver m_ProcessTypeResolver;
    private IGuidTypeResolver m_SignalTypeResolver;
    private IGuidTypeResolver m_TodoTypeResolver;
    private Registry<HostSet> m_HostSets;


    private bool m_InstrumentationEnabled;
    private Time.Event m_InstrumentationEvent;

    private NamedInterlocked m_Stats = new NamedInterlocked();
    #endregion

    #region Properties

    public new ISkyApplication App => base.App.AsSky();

    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_PM;

    public IGuidTypeResolver ProcessTypeResolver { get { return m_ProcessTypeResolver; } }

    public IGuidTypeResolver SignalTypeResolver { get { return m_SignalTypeResolver; } }

    public IGuidTypeResolver TodoTypeResolver { get { return m_TodoTypeResolver; } }

    /// <summary>
    /// Registry of all HostSets in the hub
    /// </summary>
    public IRegistry<HostSet> HostSets { get { return m_HostSets; } }

    /// <summary>
    /// Implements IInstrumentable
    /// </summary>
    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOCKING, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled; }
      set
      {
        m_InstrumentationEnabled = value;
        if (m_InstrumentationEvent == null)
        {
          if (!value) return;
          m_Stats.Clear();
          m_InstrumentationEvent = new Time.Event(App.EventTimer, null, e => AcceptManagerVisit(this, e.LocalizedTime), INSTRUMENTATION_INTERVAL);
        }
        else
        {
          if (value) return;
          DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
          m_Stats.Clear();
        }
      }
    }
    #endregion

    #region Public
    public PID Allocate(string zonePath)
    {
      if (zonePath.IsNullOrWhiteSpace())
        throw new WorkersException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".AllocateMutex(zontPath=null|empty)");

      var gdid = App.GdidProvider.GenerateOneGdid(SysConsts.GDID_NS_WORKER, SysConsts.GDID_NAME_WORKER_PROCESS);
      return DoAllocate(zonePath, gdid.ToString(), true);
    }

    /// <summary>
    /// Allocates PID by mutex. Mutexes are case-insensitive
    /// </summary>
    public PID AllocateMutex(string zonePath, string mutex)
    {
      if (zonePath.IsNullOrWhiteSpace() || mutex.IsNullOrWhiteSpace())
        throw new WorkersException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".AllocateMutex((zontPath|mutex)=null|empty)");

      mutex = mutex.ToLowerInvariant();

      return DoAllocate(zonePath, mutex, false);
    }

    public void Spawn(PID pid, IConfigSectionNode args, Guid type) { Spawn(pid, args, ProcessTypeResolver.Resolve(type)); }

    public void Spawn(PID pid, IConfigSectionNode args, Type type = null)
    {
      var guid = args.AttrByName(CONFIG_TYPE_GUID_ATTR).ValueAsGUID(Guid.Empty);
      if (type == null && guid != Guid.Empty)
        type = ProcessTypeResolver.Resolve(guid);
      Spawn(Process.MakeNew(App, type, pid, args));
    }

    public void Spawn<TProcess>(TProcess process) where TProcess : Process
    {
      process.ValidateAndPrepareForSpawn(null);
      DoSpawn(process);
    }

    public Task Async_Spawn(PID pid, IConfigSectionNode args, Guid type) { return Async_Spawn(pid, args, ProcessTypeResolver.Resolve(type)); }

    public Task Async_Spawn(PID pid, IConfigSectionNode args, Type type = null)
    {
      var guid = args.AttrByName(CONFIG_TYPE_GUID_ATTR).ValueAsGUID(Guid.Empty);
      if (type == null && guid != Guid.Empty)
        type = ProcessTypeResolver.Resolve(guid);
      return Async_Spawn(Process.MakeNew(App, type, pid, args));
    }

    public Task Async_Spawn<TProcess>(TProcess process) where TProcess : Process
    {
      process.ValidateAndPrepareForSpawn(null);
      return Async_DoSpawn(process);
    }

    public ResultSignal Dispatch(PID pid, IConfigSectionNode args, Guid type) { return Dispatch(pid, args, SignalTypeResolver.Resolve(type)); }

    public ResultSignal Dispatch(PID pid, IConfigSectionNode args, Type type = null)
    {
      var guid = args.AttrByName(CONFIG_TYPE_GUID_ATTR).ValueAsGUID(Guid.Empty);
      if (type == null && guid != Guid.Empty)
        type = SignalTypeResolver.Resolve(guid);
      return Dispatch(Signal.MakeNew(App, type, pid, args));
    }

    public ResultSignal Dispatch<TSignal>(TSignal signal) where TSignal : Signal
    {
      signal.ValidateAndPrepareForDispatch(null);
      return DoDispatch(signal);
    }

    public Task<ResultSignal> Async_Dispatch(PID pid, IConfigSectionNode args, Guid type){ return Async_Dispatch(pid, args, SignalTypeResolver.Resolve(type)); }

    public Task<ResultSignal> Async_Dispatch(PID pid, IConfigSectionNode args, Type type = null)
    {
      var guid = args.AttrByName(CONFIG_TYPE_GUID_ATTR).ValueAsGUID(Guid.Empty);
      if (type == null && guid != Guid.Empty)
        type = SignalTypeResolver.Resolve(guid);
      return Async_Dispatch(Signal.MakeNew(App, type, pid, args));
    }

    public Task<ResultSignal> Async_Dispatch<TSignal>(TSignal signal) where TSignal : Signal
    {
      signal.ValidateAndPrepareForDispatch(null);
      return Async_DoDispatch(signal);
    }

    public void Enqueue(string hostSetName, string svcName, IConfigSectionNode args, Guid type) { Enqueue(hostSetName, svcName, args, TodoTypeResolver.Resolve(type)); }

    public void Enqueue(string hostSetName, string svcName, IConfigSectionNode args, Type type = null)
    {
      var guid = args.AttrByName(CONFIG_TYPE_GUID_ATTR).ValueAsGUID(Guid.Empty);
      if (type == null && guid != Guid.Empty)
        type = TodoTypeResolver.Resolve(guid);
      Enqueue(Todo.MakeNew(App, type, args), hostSetName, svcName);
    }

    public void Enqueue<TTodo>(TTodo todo, string hostSetName, string svcName) where TTodo : Todo { Enqueue<TTodo>(new[] { todo }, hostSetName, svcName); }

    public int Enqueue<TTodo>(IEnumerable<TTodo> todos, string hostSetName, string svcName) where TTodo : Todo
    {
      if (todos == null || !todos.Any()) return 0;

      todos.ForEach(todo => todo.ValidateAndPrepareForEnqueue(null));

      var hs = getHostSet(hostSetName);
      return DoEnqueue(todos, hs, svcName);
    }

    public Task Async_Enqueue(string hostSetName, string svcName, IConfigSectionNode args, Guid type) { return Async_Enqueue(hostSetName, svcName, args, TodoTypeResolver.Resolve(type)); }

    public Task Async_Enqueue(string hostSetName, string svcName, IConfigSectionNode args, Type type = null)
    {
      var guid = args.AttrByName(CONFIG_TYPE_GUID_ATTR).ValueAsGUID(Guid.Empty);
      if (type == null && guid != Guid.Empty)
        type = TodoTypeResolver.Resolve(guid);
      return Async_Enqueue(Todo.MakeNew(App, type, args), hostSetName, svcName);
    }

    public Task Async_Enqueue<TTodo>(TTodo todo, string hostSetName, string svcName) where TTodo : Todo { return Async_Enqueue<TTodo>(new[] { todo }, hostSetName, svcName); }

    public Task<int> Async_Enqueue<TTodo>(IEnumerable<TTodo> todos, string hostSetName, string svcName) where TTodo : Todo
    {
      if (todos == null || !todos.Any()) return Task.FromResult(0);

      todos.ForEach(todo => todo.ValidateAndPrepareForEnqueue(null));

      var hs = getHostSet(hostSetName);
      return Async_DoEnqueue(todos, hs, svcName);
    }

    public TProcess Get<TProcess>(PID pid) where TProcess : Process { return DoGet<TProcess>(pid); }

    public ProcessDescriptor GetDescriptor(PID pid) { return DoGetDescriptor(pid); }

    public IEnumerable<ProcessDescriptor> List(string zonePath, IConfigSectionNode filter = null) { return DoList(zonePath, filter); }
    #endregion

    #region Protected
    protected abstract PID DoAllocate(string zonePath, string id, bool isUnique);
    protected abstract void DoSpawn<TProcess>(TProcess process) where TProcess : Process;
    protected abstract Task Async_DoSpawn<TProcess>(TProcess process) where TProcess : Process;
    protected abstract ResultSignal DoDispatch<TSignal>(TSignal signal) where TSignal : Signal;
    protected abstract Task<ResultSignal> Async_DoDispatch<TSignal>(TSignal signal) where TSignal : Signal;
    protected abstract int DoEnqueue<TTodo>(IEnumerable<TTodo> todos, HostSet hs, string svcName) where TTodo : Todo;
    protected abstract Task<int> Async_DoEnqueue<TTodo>(IEnumerable<TTodo> todos, HostSet hs, string svcName) where TTodo : Todo;
    protected abstract TProcess DoGet<TProcess>(PID pid) where TProcess : Process;
    protected abstract ProcessDescriptor DoGetDescriptor(PID pid);
    protected abstract IEnumerable<ProcessDescriptor> DoList(string zonePath, IConfigSectionNode filter);

    protected override void DoConfigure(IConfigSectionNode node)
    {

      if (node == null)
        node = App.ConfigRoot[CONFIG_PROCESS_MANAGER_SECTION];

      base.DoConfigure(node);

      var nptr = node[CONFIG_PROCESS_TYPE_RESOLVER_SECTION];
      m_ProcessTypeResolver = FactoryUtils.Make<IGuidTypeResolver>(nptr, typeof(GuidTypeResolver<Process, ProcessAttribute>), new[] { nptr });

      var nstr = node[CONFIG_SIGNAL_TYPE_RESOLVER_SECTION];
      m_SignalTypeResolver = FactoryUtils.Make<IGuidTypeResolver>(nstr, typeof(GuidTypeResolver<Signal, SignalAttribute>), new[] { nstr });

      var nttr = node[CONFIG_TODO_TYPE_RESOLVER_SECTION];
      m_TodoTypeResolver = FactoryUtils.Make<IGuidTypeResolver>(nttr, typeof(GuidTypeResolver<Todo, TodoQueueAttribute>), new[] { nttr });

      if (node == null || !node.Exists) return;

      foreach (var cn in node.Children.Where(cn => cn.IsSameName(Metabase.Metabank.CONFIG_HOST_SET_SECTION)))
      {
        var name = cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
        if (name.IsNullOrWhiteSpace())
          throw new CoordinationException(StringConsts.PM_HOSTSET_CONFIG_MISSING_NAME_ERROR);

        var path = cn.AttrByName(CONFIG_PATH_ATTR).Value;

        if (path.IsNullOrWhiteSpace())
          throw new CoordinationException(StringConsts.PM_HOSTSET_CONFIG_PATH_MISSING_ERROR.Args(name));

        var spar = cn.AttrByName(CONFIG_SEARCH_PARENT_ATTR).ValueAsBool(true);
        var tNoc = cn.AttrByName(CONFIG_TRANSCEND_NOC_ATTR).ValueAsBool(false);
        var hset = HostSet.FindAndBuild<HostSet>(name, path, spar, tNoc);

        var added = m_HostSets.Register(hset);
        if (!added)
          throw new CoordinationException(StringConsts.PM_HOSTSET_CONFIG_DUPLICATE_NAME_ERROR.Args(name));
      }
    }
    #endregion

    #region Private
    private HostSet getHostSet(string hostSetName)
    {
      if (hostSetName == null)
        throw new CoordinationException(StringConsts.ARGUMENT_ERROR + "HostSetHub.EnqueueTodo(hostSetName=null|empty)");

      var result = m_HostSets[hostSetName];

      if (result == null)
        throw new CoordinationException(StringConsts.ARGUMENT_ERROR + "HostSetHub.EnqueueTodo(hostSetName'{0}') not found".Args(hostSetName));

      return result;
    }
    #endregion
  }
}
