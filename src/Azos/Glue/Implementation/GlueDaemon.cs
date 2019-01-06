/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Log;
using Azos.Instrumentation;
using Azos.Glue.Protocol;

namespace Azos.Glue.Implementation
{
  /// <summary>
  /// Provides default implementation for IGlue. This is the root context for all other glue objects
  /// </summary>
  [ConfigMacroContext]
  public sealed class GlueDaemon : DaemonWithInstrumentation<IApplicationComponent>, IGlueImplementation
  {
    #region CONSTS
    public const string CONFIG_PROVIDERS_SECTION = "providers";
    public const string CONFIG_PROVIDER_SECTION = "provider";
    public const string CONFIG_PROVIDER_ATTR = "provider";

    public const string CONFIG_BINDINGS_SECTION = "bindings";
    public const string CONFIG_BINDING_SECTION = "binding";

    public const string CONFIG_SERVERS_SECTION = "servers";
    public const string CONFIG_SERVER_SECTION = "server";

    public const string CONFIG_TRANSPORT_SECTION = "transport";

    public const int DEFAULT_DISPATCH_TIMEOUT_MS = 100;
    public const int DEFAULT_TIMEOUT_MS = 20000;

    public const int DEFAULT_SERVER_INSTANCE_LOCK_TIMEOUT_MS = 10*1000;
    public const int MINIMUM_SERVER_INSTANCE_LOCK_TIMEOUT_MS = 5;

    public const MessageType DEFAULT_CLIENT_LOG_LEVEL = MessageType.Error;
    public const MessageType DEFAULT_SERVER_LOG_LEVEL = MessageType.Error;

    private const string THREAD_NAME = "GlueService.Main";
    private const int THREAD_GRANULARITY_MS = 500;
    private const int MANAGER_VISIT_GRANULARITY_MS = 3750;
    #endregion

    #region .ctors
    public GlueDaemon(IApplication app) : base(app)
    {
    }

    public GlueDaemon(IApplicationComponent director) : base(director)
    {
    }
    #endregion

    #region Fields
    private ServerHandler m_ServerHandler;
    private Calls m_Calls;//sync calls do not get registered in this list
    private CallsWithTasks m_CallsWithTasks;//only calls gotten asTask get registered here

    private Thread          m_Thread;
    private AutoResetEvent  m_Waiter;
    private bool m_InstrumentationEnabled;

    internal readonly Registry<Provider>  m_Providers       = new Registry<Provider>();
    internal readonly Registry<Binding>  m_Bindings         = new Registry<Binding>();
    internal readonly Registry<ServerEndPoint>   m_Servers  = new Registry<ServerEndPoint>();

    [Config("$default-dispatch-timeout-ms", DEFAULT_DISPATCH_TIMEOUT_MS)]
    private int m_DefaultDispatchTimeoutMs;

    [Config("$default-timeout-ms", DEFAULT_TIMEOUT_MS)]
    private int m_DefaultTimeoutMs;

    [Config("$client-log-level", DEFAULT_CLIENT_LOG_LEVEL)]
    private MessageType m_ClientLogLevel = DEFAULT_CLIENT_LOG_LEVEL;

    [Config("$server-log-level", DEFAULT_SERVER_LOG_LEVEL)]
    private MessageType m_ServerLogLevel = DEFAULT_SERVER_LOG_LEVEL;

    [Config("$server-instance-lock-timeout-ms", DEFAULT_SERVER_INSTANCE_LOCK_TIMEOUT_MS)]
    private int m_ServerInstanceLockTimeoutMs = DEFAULT_SERVER_INSTANCE_LOCK_TIMEOUT_MS;


    private readonly OrderedRegistry<IClientMsgInspector> m_ClientMsgInspectors = new OrderedRegistry<IClientMsgInspector>();
    private readonly OrderedRegistry<IServerMsgInspector> m_ServerMsgInspectors = new OrderedRegistry<IServerMsgInspector>();
    #endregion


    #region Properties

    public bool Active => Running && App.Active;

    public override string ComponentCommonName => "glue";

    public override string ComponentLogTopic => CoreConsts.GLUE_TOPIC;

    /// <summary>
    /// Implements IInstrumentable
    /// </summary>
    [Config(Default=false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled;}
      set { m_InstrumentationEnabled = value;}
    }

    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
    public int DefaultDispatchTimeoutMs
    {
      get { return m_DefaultDispatchTimeoutMs; }
      set { m_DefaultDispatchTimeoutMs = value>0 ? value : 0; }
    }

    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
    public int DefaultTimeoutMs
    {
      get { return m_DefaultTimeoutMs; }
      set { m_DefaultTimeoutMs = value>0 ? value : 0; }
    }

    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public MessageType ClientLogLevel
    {
      get { return m_ClientLogLevel; }
      set { m_ClientLogLevel = value; }
    }

    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public MessageType ServerLogLevel
    {
      get { return m_ServerLogLevel; }
      set { m_ServerLogLevel = value; }
    }

    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE)]
    public int ServerInstanceLockTimeoutMs
    {
      get { return m_ServerInstanceLockTimeoutMs; }
      set { m_ServerInstanceLockTimeoutMs = value<MINIMUM_SERVER_INSTANCE_LOCK_TIMEOUT_MS ? MINIMUM_SERVER_INSTANCE_LOCK_TIMEOUT_MS : value; }
    }

    public IRegistry<Provider>       Providers => m_Providers;
    public IRegistry<Binding>        Bindings  => m_Bindings;
    public IRegistry<ServerEndPoint> Servers   => m_Servers;
    public OrderedRegistry<IClientMsgInspector> ClientMsgInspectors => m_ClientMsgInspectors;
    public OrderedRegistry<IServerMsgInspector> ServerMsgInspectors => m_ServerMsgInspectors;

    public IConfigSectionNode GlueConfiguration
    {
      get { return App.ConfigRoot[CommonApplicationLogic.CONFIG_GLUE_SECTION];}
    }

    public IConfigSectionNode ProvidersConfigurationSection
    {
      get { return GlueConfiguration[CONFIG_PROVIDERS_SECTION]; }
    }

    public IEnumerable<IConfigSectionNode> ProviderConfigurations
    {
      get { return ProvidersConfigurationSection.Children.Where(n=> n.IsSameName(CONFIG_PROVIDER_SECTION)); }
    }

    public IConfigSectionNode BindingsConfigurationSection
    {
      get { return GlueConfiguration[CONFIG_BINDINGS_SECTION]; }
    }

    public IEnumerable<IConfigSectionNode> BindingConfigurations
    {
        get { return BindingsConfigurationSection.Children.Where(n=> n.IsSameName(CONFIG_BINDING_SECTION)); }
    }

    public IConfigSectionNode ServersConfigurationSection
    {
      get { return GlueConfiguration[CONFIG_SERVERS_SECTION]; }
    }

    public IEnumerable<IConfigSectionNode> ServerConfigurations
    {
      get { return ServersConfigurationSection.Children.Where(n=> n.IsSameName(CONFIG_SERVER_SECTION)); }
    }
    #endregion



    #region Public


#warning missing check for this Glue affinity
    public void RegisterProvider(Provider p)
    {
        if (!m_Providers.Register(p))
            throw new GlueException(StringConsts.GLUE_DUPLICATE_NAMED_INSTANCE_ERROR +
                "Provider = " + p.Name);
    }

#warning missing check for this Glue affinity
    public void UnregisterProvider(Provider p)
    {
        m_Providers.Unregister(p);
    }

#warning missing check for this Glue affinity
    public void RegisterBinding(Binding b)
    {
        if (!m_Bindings.Register(b))
            throw new GlueException(StringConsts.GLUE_DUPLICATE_NAMED_INSTANCE_ERROR +
                "Binding = " + b.Name);
    }

#warning missing check for this Glue affinity
    public void UnregisterBinding(Binding b)
    {
        m_Bindings.Unregister(b);
    }

#warning missing check for this Glue affinity
    public void RegisterServerEndpoint(ServerEndPoint ep)
    {
        if (!m_Servers.Register(ep))
            throw new GlueException(StringConsts.GLUE_DUPLICATE_NAMED_INSTANCE_ERROR +
                "ServerEndPoint = " + ep.Name);
    }

#warning missing check for this Glue affinity
    public void UnregisterServerEndpoint(ServerEndPoint ep)
    {
        m_Servers.Unregister(ep);
    }

    public Binding GetNodeBinding(Node node)
    {
      var binding = Bindings[node.Binding];
      if (binding==null)
        throw new GlueException(StringConsts.GLUE_NAMED_BINDING_NOT_FOUND_ERROR + node.Binding);

      return binding;
    }

    public Binding GetNodeBinding(string node)
    {
      var nd = new Node(node);
      var binding = Bindings[nd.Binding];
      if (binding==null)
        throw new GlueException(StringConsts.GLUE_NAMED_BINDING_NOT_FOUND_ERROR + nd.Binding);

      return binding;
    }

    public RequestMsg ClientDispatchingRequest(ClientEndPoint client, RequestMsg request)
    {
      //Glue level inspectors
      foreach(var insp in ClientMsgInspectors.OrderedValues)
            request = insp.ClientDispatchCall(client, request);

      return request;
    }

    public void ClientDispatchedRequest(ClientEndPoint client, RequestMsg request, CallSlot callSlot)
    {
      if (client.Binding.OperationFlow == OperationFlow.Asynchronous)
        m_Calls.Put(callSlot);
    }


    public void ClientDeliverAsyncResponse(ResponseMsg response)
    {
      clientDeliverAsyncResponse(response, true);
    }

    private void clientDeliverAsyncResponse(ResponseMsg response, bool first)
    {
      var callSlot =  m_Calls.TryGetAndRemove(response.RequestID);

      if (callSlot!=null)
          callSlot.DeliverResponse(response);
      else
      {
        if (first)
        // If execution paused right after Send and before ClientDispatchedRequest could register callslot
        // we re-try asynchronously to find call slot again
          Task.Delay(1000).ContinueWith( (t, objRes) => clientDeliverAsyncResponse(objRes as ResponseMsg, false), response);
        else
          if (m_InstrumentationEnabled) Instrumentation.CallSlotNotFoundErrorEvent.Happened(App.Instrumentation);
      }
    }

    public void ServerDispatchRequest(RequestMsg request)
    {
        m_ServerHandler.HandleRequestAsynchronously(request);
    }

    public ResponseMsg ServerHandleRequest(RequestMsg request)
    {
        return m_ServerHandler.HandleRequestSynchronously(request);
    }

    public ResponseMsg ServerHandleRequestFailure(FID reqID, bool oneWay, Exception failure, object bindingSpecCtx)
    {
        return m_ServerHandler.HandleRequestFailure(reqID, oneWay, failure, bindingSpecCtx);
    }

    public void SubscribeCallSlotWithTaskReactor(CallSlot call)
    {
      if (!Running) return;
      if (call==null) return;
      m_CallsWithTasks.Put(call);
    }

    #endregion

    #region Protected

    protected override void DoStart()
    {
      const string START = "starting";
      const string STOP  = "stopping";

      base.DoStart();

      try
      {
        m_ServerHandler = new ServerHandler(this);

        m_Calls = new Calls(0);
        m_CallsWithTasks = new CallsWithTasks();

        run(() => m_ServerHandler.Start(), START, "server handler", m_ServerHandler.Name, m_ServerHandler.GetType());

        foreach(var p in m_Providers) run(() => p.Start(), START, "provider", p.Name, p.GetType());
        foreach(var b in m_Bindings)  run(() => b.Start(), START, "binding",  b.Name, b.GetType());
        foreach(var s in m_Servers)   run(() => s.Open(),  START, "server",   s.Name, s.GetType());

        m_Waiter = new AutoResetEvent(false);
        m_Thread = new Thread(threadSpin);
        m_Thread.Name = THREAD_NAME;
        m_Thread.Start();

        WriteLog(MessageType.Trace, nameof(DoStart), "Started OK");
      }
      catch(Exception error)
      {
        AbortStart();

        WriteLog(MessageType.CatastrophicError, nameof(DoStart), "Leaked: " + error.ToMessageWithType(), error);

        if (m_Thread != null)
        {
            m_Thread.Join();
            m_Thread = null;
        }

        foreach (var s in m_Servers)   run(() => s.Close(),               STOP, "server",   s.Name, s.GetType(), false);
        foreach (var b in m_Bindings)  run(() => b.WaitForCompleteStop(), STOP, "binding",  b.Name, b.GetType(), false);
        foreach (var p in m_Providers) run(() => p.WaitForCompleteStop(), STOP, "provider", p.Name, p.GetType(), false);

        if (m_ServerHandler != null)
        {
          run(() => m_ServerHandler.Dispose(), STOP, "server handler", m_ServerHandler.Name, m_ServerHandler.GetType(), false);
          m_ServerHandler = null;
        }

        throw error;
      }
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();

      m_ServerHandler.SignalStop();

      m_Bindings.ForEach( b => b.SignalStop());
      m_Providers.ForEach( p => p.SignalStop());

      m_Waiter.Set();
    }

    protected override void DoWaitForCompleteStop()
    {
      const string STOP = "stopping";

      base.DoWaitForCompleteStop();

      try
      {
        m_Thread.Join();
        m_Thread = null;

        foreach(var s in m_Servers)   run(() => s.Close(), STOP, "server", s.Name, s.GetType(), false);
        foreach(var b in m_Bindings)  run(() => b.WaitForCompleteStop(), STOP, "binding", b.Name, b.GetType(), false);
        foreach(var p in m_Providers) run(() => p.WaitForCompleteStop(), STOP, "provider", p.Name, p.GetType(), false);

        run(() => m_ServerHandler.WaitForCompleteStop(), STOP, "server handler", m_ServerHandler.Name, m_ServerHandler.GetType(), false);

        DisposeAndNull(ref m_ServerHandler);

        m_Calls = null;
        m_CallsWithTasks.ScanAll();
        m_CallsWithTasks = null;
      }
      catch(Exception error)
      {
        try
        {
          DisposeAndNull(ref m_ServerHandler);
        }
        catch(Exception she)
        {
          WriteLog(MessageType.CatastrophicError, nameof(DoWaitForCompleteStop), "DisposeAndNull(ref m_ServerHandler) leaked: " + she.ToMessageWithType(), she);
        }
        WriteLog(MessageType.CatastrophicError, nameof(DoWaitForCompleteStop), "Leaked exception: " + error.ToMessageWithType(), error);
        throw error;
      }
    }


    protected override void DoConfigure(IConfigSectionNode node)
    {
        const string CONFIG = "configuring";

        base.DoConfigure(node);

        foreach (var pnode in node[CONFIG_PROVIDERS_SECTION].Children.Where(n =>  n.IsSameName(CONFIG_PROVIDER_SECTION)))
        {
            var name = pnode.AttrByName(CONFIG_NAME_ATTR).ValueAsString();
            run(() => FactoryUtils.MakeAndConfigureDirectedComponent<Provider>(this, pnode, null, new object[] { name }), CONFIG, "provider", name);
        }

        foreach (var bnode in node[CONFIG_BINDINGS_SECTION].Children.Where(n =>  n.IsSameName(CONFIG_BINDING_SECTION)))
        {
            var name = bnode.AttrByName(CONFIG_NAME_ATTR).ValueAsString();
            run(() => FactoryUtils.MakeAndConfigureDirectedComponent<Binding>(this, bnode, null, new object[] { name, null }), CONFIG, "binding", name);
        }

        foreach (var snode in node[CONFIG_SERVERS_SECTION].Children.Where(n =>  n.IsSameName(CONFIG_SERVER_SECTION)))
        {
            var name = snode.AttrByName(CONFIG_NAME_ATTR).ValueAsString();
            run(() => FactoryUtils.MakeAndConfigure<ServerEndPoint>(snode,
                        defaultType: typeof(ServerEndPoint), args: new object[] { this, name }), CONFIG, "server", name);
        }

        run(() => MsgInspectorConfigurator.ConfigureClientInspectors(App, m_ClientMsgInspectors, node), CONFIG, "ClientInspectors");
        run(() => MsgInspectorConfigurator.ConfigureServerInspectors(App, m_ServerMsgInspectors, node), CONFIG, "ServerInspectors");
    }


    #endregion


    #region .pvt

    private DateTime m_LastManagerVisit;
    private void threadSpin()
    {
      var cmpName = string.Empty;

      while(Running)
      {
        try
        {
          var now = LocalizedTime;

          cmpName = ".Manager";
          if ((now-m_LastManagerVisit).TotalMilliseconds >= MANAGER_VISIT_GRANULARITY_MS)
          {
            m_LastManagerVisit = now;

            cmpName = ".Providers";
            foreach(var component in Providers)
            {
              cmpName = component.Name;
              run(() => component.AcceptManagerVisit(this,  now), nameof(threadSpin), cmpName, rethrow: false);
            }

            cmpName = ".Bindings";
            foreach(var component in Bindings)
            {
              cmpName = component.Name;
              run(() => component.AcceptManagerVisit(this,  now), nameof(threadSpin), cmpName, rethrow: false);
            }

            cmpName = m_ServerHandler.Name;
            run(() => m_ServerHandler.AcceptManagerVisit(this, now), nameof(threadSpin), cmpName, rethrow: false);

            cmpName = nameof(purgeTimedOutCallSlots);
            run(() => purgeTimedOutCallSlots(now), nameof(threadSpin), cmpName, rethrow: false);//calls
          }//at manager visit granularity

          cmpName = nameof(purgeTimedOutCallSlotsWithTasks);
          run( () => purgeTimedOutCallSlotsWithTasks(now), nameof(threadSpin), cmpName, rethrow: false);//callsWithTasks

          cmpName = string.Empty;

          m_Waiter.WaitOne(THREAD_GRANULARITY_MS);
        }
        catch(Exception error)
        {
          WriteLog(MessageType.CatastrophicError,
                   nameof(threadSpin),
                   "GlueService.threadSpin(component: '{0}') leaked: {1}".Args(cmpName, error.ToMessageWithType()),
                   error);
        }
      }//while(Running)
    }



    private void run(Action func, string topic, string what, string name = null, Type type = null, bool rethrow = true)
    {
      try { func(); }
      catch (Exception e)
      {
        WriteLog(MessageType.CatastrophicError,"{0}::{1}|{2}|{3}".Args(topic, what, name, type?.Name ?? "?"), "Leaked: "+e.ToMessageWithType(), e);
        if (rethrow) throw;
      }
    }



    private DateTime lastPurgeTimedOutCallSlots = DateTime.MinValue;
    private void purgeTimedOutCallSlots(DateTime now)
    {
      const int PURGE_EVERY_SEC = 39;

      if ((now - lastPurgeTimedOutCallSlots).TotalSeconds < PURGE_EVERY_SEC) return;
      lastPurgeTimedOutCallSlots = now;

      Task.Factory.StartNew(
      () =>
      {
          var calls = m_Calls;
          if (calls!=null)
          {
            var removed = calls.PurgeTimedOutSlots();
            if (removed>0)
            {
              if (m_InstrumentationEnabled)
              Instrumentation.ClientTimedOutCallSlotsRemoved.Record(App.Instrumentation, removed);

              if (m_ClientLogLevel<=MessageType.Info)
                WriteLog(MessageType.Trace, nameof(purgeTimedOutCallSlots), "Purged {0} timed-out CallSlot objects".Args(removed));

              Platform.RandomGenerator.Instance.FeedExternalEntropySample(removed << ((int)now.Ticks & 0b111));
            }
          }
      });
    }

    private void purgeTimedOutCallSlotsWithTasks(DateTime now)
    {
      var calls = m_CallsWithTasks;
      if (calls != null)
      {
        var removed = calls.ScanAll();
        if (removed > 0)
        {
          if (m_InstrumentationEnabled)
            Instrumentation.ClientTimedOutCallSlotsRemoved.Record(App.Instrumentation, removed);

          if (m_ClientLogLevel <= MessageType.Info)
            WriteLog(MessageType.Trace, nameof(purgeTimedOutCallSlotsWithTasks), "Purged {0} timed-out CallSlot objects".Args(removed));

          Platform.RandomGenerator.Instance.FeedExternalEntropySample(removed << ((int)now.Ticks & 0b111));
        }
      }
    }

    #endregion

  }
}
