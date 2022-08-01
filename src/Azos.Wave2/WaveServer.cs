/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Log;
using Azos.IO.Net.Gate;
using Azos.Instrumentation;
using Azos.Serialization.JSON;

using Azos.Wave.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Azos.Wave
{
  /// <summary>
  /// Represents "(W)eb(A)pp(V)iew(E)nhanced" web server which provides DYNAMIC web site services.
  /// This server is not meant to be exposed directly to the public Internet, rather it should be used as an application server
  /// behind the reverse proxy, such as NGINX. This server is designed to serve dynamic data-driven requests/APIs and not meant to be used
  /// for serving static content files (although it can).
  /// The implementation is based on a lightweight HttpListener that processes incoming Http requests via an injectable WorkDispatcher
  /// which governs the threading and WorkContext lifecycle.
  /// The server processing pipeline is purposely designed to be synchronous-blocking (thread per call) which does not mean that the
  /// server is inefficient, to the contrary - this server design is specifically targeting short-called methods relying on a classical thread call stack.
  /// This approach obviates the need to create surrogate message loops/synchro contexts, tasks and other objects that introduce extra GC load.
  /// The server easily support "dangling"(left open indefinitely) WorkContext instances that can stream events (i.e. SSE/Server Push) and WebSockets from
  ///  specially-purposed asynchronous notification threads.
  /// </summary>
  /// <remarks>
  /// The common belief that asynchronous non-thread-based web servers always work faster (i.e. Node.js) is not true in the data-oriented systems of high scale because
  ///  eventually multiple web server machines still block on common data access resources, so it is much more important to design the database backend
  /// in an asynchronous fashion, as it is the real bottle neck of the system. Even if most of the available threads are not physically paused by IO,
  ///  they are paused logically as the logical units of work are waiting for IO and the fact that server can accept more socket requests does not mean that they
  ///  will not timeout.  The downsides of asynchronous web layers are:
  ///   (a) much increased implementation/maintenance complexity
  ///   (b) many additional task/thread context switches and extra objects that facilitate the event loops/messages/tasks etc...
  /// </remarks>
  public class WaveServer : DaemonWithInstrumentation<IApplicationComponent>
  {
    #region CONSTS

      public const string CONFIG_SERVER_SECTION = "server";

      public const string CONFIG_PREFIX_SECTION = "prefix";

      public const string CONFIG_GATE_SECTION = "gate";

      public const string CONFIG_DEFAULT_ERROR_HANDLER_SECTION = "default-error-handler";


      public const int ACCEPT_THREAD_GRANULARITY_MS = 250;

      public const int INSTRUMENTATION_DUMP_PERIOD_MS = 3377;

    #endregion

    #region Static

    private static Registry<WaveServer> s_Servers = new Registry<WaveServer>();

    /// <summary>
    /// Returns the global registry of all server instances that are active in this process
    /// </summary>
    public static IRegistry<WaveServer> Servers
    {
      get{ return s_Servers; }
    }


    #endregion


    #region .ctor
    public WaveServer(IApplication app) : base(app) => ctor();
    public WaveServer(IApplicationComponent director) : base(director) => ctor();
    public WaveServer(IApplication app, string name) : this(app) => Name = name;
    public WaveServer(IApplicationComponent director, string name) : this(director) => Name = name;

    private void ctor()
    {
      m_Prefixes = new EventedList<string, WaveServer>(this, true);
      m_Prefixes.GetReadOnlyEvent = (l) => Status != DaemonStatus.Inactive;
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeIfDisposableAndNull(ref m_Gate);
      DisposeIfDisposableAndNull(ref m_RootHandler);
    }
    #endregion

    #region Fields

    private bool m_LogHandleExceptionErrors;
    private EventedList<string, WaveServer> m_Prefixes;

    private Thread m_InstrumentationThread;
    private AutoResetEvent m_InstrumentationThreadWaiter;


    private INetGate m_Gate;
    private CompositeWorkHandler m_RootHandler;


    private OrderedRegistry<WorkMatch> m_ErrorShowDumpMatches = new OrderedRegistry<WorkMatch>();
    private OrderedRegistry<WorkMatch> m_ErrorLogMatches = new OrderedRegistry<WorkMatch>();


    //*Instrumentation Statistics*//
    internal bool m_InstrumentationEnabled;

    internal long m_stat_ServerRequest;
    internal long m_stat_ServerGateDenial;
    internal long m_stat_ServerHandleException;
    internal long m_stat_FilterHandleException;

    internal long m_stat_WorkContextWrittenResponse;
    internal long m_stat_WorkContextBufferedResponse;
    internal long m_stat_WorkContextBufferedResponseBytes;
    internal long m_stat_WorkContextCtor;
    internal long m_stat_WorkContextDctor;
    internal long m_stat_WorkContextAborted;
    internal long m_stat_WorkContextHandled;
    internal long m_stat_WorkContextNoDefaultClose;
    internal long m_stat_WorkContextNeedsSession;

    internal long m_stat_SessionNew;
    internal long m_stat_SessionExisting;
    internal long m_stat_SessionEnd;
    internal long m_stat_SessionInvalidID;

    internal long m_stat_GeoLookup;
    internal long m_stat_GeoLookupHit;

    internal NamedInterlocked m_stat_PortalRequest = new NamedInterlocked();

    #endregion

    #region Properties

    public override string ComponentLogTopic => CoreConsts.WAVE_TOPIC;

    public override string ComponentCommonName =>  "ws-" + Name;

    /// <summary>
    /// Provides a list of served endpoints
    /// </summary>
    public override string ServiceDescription => Prefixes.Aggregate(string.Empty, (s, p) => s + "  " + p);


    /// <summary>
    /// Optional name of header used for disclosure of WorkContext.ID. If set to null, suppresses the header
    /// </summary>
    [Config(Default = CoreConsts.HTTP_HDR_DEFAULT_CALL_FLOW)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB)]
    public string CallFlowHeader { get; set;} = CoreConsts.HTTP_HDR_DEFAULT_CALL_FLOW;


    /// <summary>
    /// When true, emits instrumentation messages
    /// </summary>
    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled
    {
        get { return m_InstrumentationEnabled;}
        set { m_InstrumentationEnabled = value;}
    }


    /// <summary>
    /// When true writes errors that get thrown in server catch-all HandleException methods
    /// </summary>
    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public bool LogHandleExceptionErrors
    {
      get { return m_LogHandleExceptionErrors;}
      set { m_LogHandleExceptionErrors = value;}
    }

    /// <summary>
    /// Returns HttpListener prefixes such as "http://+:8080/"
    /// </summary>
    public IList<string> Prefixes => m_Prefixes;


    /// <summary>
    /// Gets/sets network gate
    /// </summary>
    public INetGate Gate
    {
      get { return m_Gate;}
      set
      {
        CheckDaemonInactive();
        m_Gate = value;
      }
    }

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB)]
    public string GateCallerRealIpAddressHeader  {  get; set;  }


    /// <summary>
    /// Gets/sets work dispatcher
    /// </summary>
    public CompositeWorkHandler RootHandler
    {
      get { return m_RootHandler;}
      set
      {
        CheckDaemonInactive();
        if (value!=null && value.ComponentDirector!=this)
          throw new WaveException(StringConsts.DISPATCHER_NOT_THIS_SERVER_ERROR);
        m_RootHandler = value;
      }
    }

    /// <summary>
    /// Returns matches used by the server's default error handler to determine whether exception details should be shown
    /// </summary>
    public OrderedRegistry<WorkMatch> ShowDumpMatches => m_ErrorShowDumpMatches;

    /// <summary>
    /// Returns matches used by the server's default error handler to determine whether exception details should be logged
    /// </summary>
    public OrderedRegistry<WorkMatch> LogMatches => m_ErrorLogMatches;

    #endregion

    #region Public
    /// <summary>
    /// Handles processing exception by calling ErrorFilter.HandleException(work, error).
    /// All parameters except ERROR can be null - which indicates error that happened during WorkContext dispose
    /// </summary>
    public virtual async Task HandleExceptionAsync(WorkContext work, Exception error)
    {
      try
      {
        if (m_InstrumentationEnabled) Interlocked.Increment(ref m_stat_ServerHandleException);

        //work may be null (when WorkContext is already disposed)
        if (work != null)
          await ErrorFilter.HandleExceptionAsync(work, error, m_ErrorShowDumpMatches, m_ErrorLogMatches).ConfigureAwait(false);
        else
          WriteLog(MessageType.Error,
              nameof(HandleExceptionAsync),
              StringConsts.SERVER_DEFAULT_ERROR_WC_NULL_ERROR + error.ToMessageWithType(),
              error);
      }
      catch(Exception error2)
      {
        if (m_LogHandleExceptionErrors)
          try
          {
            WriteLog(MessageType.Error,
                  nameof(HandleExceptionAsync),
                  StringConsts.SERVER_DEFAULT_ERROR_HANDLER_ERROR + error2.ToMessageWithType(),
                  error2,
                  pars: new
                  {
                    OriginalError = error.ToMessageWithType()
                  }.ToJson()
                  );
          }
          catch{}
      }
    }
    #endregion


    #region Protected

      protected override void DoConfigure(IConfigSectionNode node)
      {
        if (node==null || !node.Exists)
        {
          //0 get very root
          node = App.ConfigRoot[SysConsts.CONFIG_WAVE_SECTION];
          if (!node.Exists) return;

          //1 try to find the server with the same name as this instance
          var snode = node.Children.FirstOrDefault(cn=>cn.IsSameName(CONFIG_SERVER_SECTION) && cn.IsSameNameAttr(Name));

          //2 try to find a server without a name
          if (snode==null)
            snode = node.Children.FirstOrDefault(cn=>cn.IsSameName(CONFIG_SERVER_SECTION) && cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value.IsNullOrWhiteSpace());

          if (snode==null) return;
          node = snode;
        }


        ConfigAttribute.Apply(this, node);

        m_Prefixes.Clear();
        foreach(var name in node.Children
                             .Where(c=>c.IsSameName(CONFIG_PREFIX_SECTION))
                             .Select(c=>c.AttrByName(Configuration.CONFIG_NAME_ATTR).Value)
                             .Where(n=>n.IsNotNullOrWhiteSpace()))
           m_Prefixes.Add(name);

        var nGate = node[CONFIG_GATE_SECTION];
        if (nGate.Exists)
        {
          DisposeIfDisposableAndNull(ref m_Gate);
          m_Gate = FactoryUtils.MakeAndConfigure<INetGateImplementation>(nGate, typeof(NetGate), args: new object[]{this});
        }


        var nRootHandler = node[WorkHandler.CONFIG_HANDLER_SECTION];
        m_RootHandler = FactoryUtils.MakeDirectedComponent<CompositeWorkHandler>(this,
                                           nRootHandler,
                                           typeof(CompositeWorkHandler),
                                           new object[]{ nRootHandler });

        ErrorFilter.ConfigureMatches(node[CONFIG_DEFAULT_ERROR_HANDLER_SECTION], m_ErrorShowDumpMatches, m_ErrorLogMatches, null, GetType().FullName);
      }

      protected override void DoStart()
      {
        if (m_Prefixes.Count==0)
          throw new WaveException(StringConsts.SERVER_NO_PREFIXES_ERROR.Args(Name));

        if (!s_Servers.Register(this))
          throw new WaveException(StringConsts.SERVER_COULD_NOT_GET_REGISTERED_ERROR.Args(Name));

        try
        {
           if (m_Gate!=null)
             if (m_Gate is Daemon)
               ((Daemon)m_Gate).Start();


           m_RootHandler.NonNull(nameof(RootHandler));

           m_InstrumentationThread = new Thread(instrumentationThreadSpin);
           m_InstrumentationThread.Name = "{0}-InstrumentationThread".Args(Name);
           m_InstrumentationThreadWaiter = new AutoResetEvent(false);


           foreach(var prefix in m_Prefixes)
             m_Listener.Prefixes.Add(prefix);


        }
        catch
        {

          if (m_Gate!=null && m_Gate is Daemon)
            ((Daemon)m_Gate).WaitForCompleteStop();

          s_Servers.Unregister(this);

          throw;
        }

        m_InstrumentationThread.Start();
      }

      protected override void DoSignalStop()
      {
        if (m_InstrumentationThreadWaiter!=null)
              m_InstrumentationThreadWaiter.Set();

        if (m_Gate!=null)
          if (m_Gate is Daemon)
             ((Daemon)m_Gate).SignalStop();
      }

      protected override void DoWaitForCompleteStop()
      {
        s_Servers.Unregister(this);

        if (m_InstrumentationThread!=null)
        {
          m_InstrumentationThread.Join();
          m_InstrumentationThread = null;
          m_InstrumentationThreadWaiter.Close();
        }

         if (m_Gate!=null)
            if (m_Gate is Daemon)
                ((Daemon)m_Gate).WaitForCompleteStop();
      }


    /// <summary>
    /// Called by the Asp.Net middleware, an entry point for server request processing
    /// </summary>
    public async Task ProcessAsync(HttpContext httpContext)
    {
      using var work = MakeContext(httpContext);
      try
      {
        if (m_InstrumentationEnabled) Interlocked.Increment(ref m_stat_ServerRequest);

        var gate = m_Gate;
        if (gate != null)
        {
          try
          {
            var action = gate.CheckTraffic(new HTTPIncomingTraffic(httpContext, GateCallerRealIpAddressHeader));
            if (action != GateAction.Allow)
            {
              //access denied
              httpContext.Response.StatusCode = WebConsts.STATUS_429;
              //await httpContext.Response.WriteAsync(WebConsts.STATUS_429_DESCRIPTION).ConfigureAwait(false);
              if (m_InstrumentationEnabled) Interlocked.Increment(ref m_stat_ServerGateDenial);
              return;
            }
          }
          catch (Exception denyError)
          {
            WriteLog(MessageType.Error, nameof(ProcessAsync), denyError.ToMessageWithType(), denyError);
          }
        }

        await m_RootHandler.Process(work).ConfigureAwait(false);
      }
      catch(Exception unhandled)
      {
        await this.HandleExceptionAsync(work, unhandled).ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Factory method to make WorkContext
    /// </summary>
    protected virtual WorkContext MakeContext(HttpContext httpContext)
      =>  new WorkContext(this, httpContext);

    #endregion

    #region .pvt


    private void instrumentationThreadSpin()
    {
      var pe = m_InstrumentationEnabled;
      while(Running)
      {
        if (pe!=m_InstrumentationEnabled)
        {
          resetStats();
          pe = m_InstrumentationEnabled;
        }

        if (m_InstrumentationEnabled &&
            App.Instrumentation.Enabled)
        {
            dumpStats();
            resetStats();
        }

        m_InstrumentationThreadWaiter.WaitOne(INSTRUMENTATION_DUMP_PERIOD_MS);
      }
    }

     private void resetStats()
     {
        m_stat_ServerRequest                        = 0;
        m_stat_ServerGateDenial                     = 0;
        m_stat_ServerHandleException                = 0;
        m_stat_FilterHandleException                = 0;


        m_stat_WorkContextWrittenResponse           = 0;
        m_stat_WorkContextBufferedResponse          = 0;
        m_stat_WorkContextBufferedResponseBytes     = 0;
        m_stat_WorkContextCtor                      = 0;
        m_stat_WorkContextDctor                     = 0;
        m_stat_WorkContextAborted                   = 0;
        m_stat_WorkContextHandled                   = 0;
        m_stat_WorkContextNoDefaultClose            = 0;
        m_stat_WorkContextNeedsSession              = 0;

        m_stat_SessionNew                           = 0;
        m_stat_SessionExisting                      = 0;
        m_stat_SessionEnd                           = 0;
        m_stat_SessionInvalidID                     = 0;

        m_stat_GeoLookup                            = 0;
        m_stat_GeoLookupHit                         = 0;

        m_stat_PortalRequest.Clear();
     }

     private void dumpStats()
     {
        var i = App.Instrumentation;

        i.Record( new Instrumentation.ServerRequest                      (Name, m_stat_ServerRequest                      ));
        i.Record( new Instrumentation.ServerGateDenial                   (Name, m_stat_ServerGateDenial                   ));
        i.Record( new Instrumentation.ServerHandleException              (Name, m_stat_ServerHandleException              ));
        i.Record( new Instrumentation.FilterHandleException              (Name, m_stat_FilterHandleException              ));


        i.Record( new Instrumentation.WorkContextWrittenResponse         (Name, m_stat_WorkContextWrittenResponse         ));
        i.Record( new Instrumentation.WorkContextBufferedResponse        (Name, m_stat_WorkContextBufferedResponse        ));
        i.Record( new Instrumentation.WorkContextBufferedResponseBytes   (Name, m_stat_WorkContextBufferedResponseBytes   ));
        i.Record( new Instrumentation.WorkContextCtor                    (Name, m_stat_WorkContextCtor                    ));
        i.Record( new Instrumentation.WorkContextDctor                   (Name, m_stat_WorkContextDctor                   ));
        i.Record( new Instrumentation.WorkContextAborted                 (Name, m_stat_WorkContextAborted                 ));
        i.Record( new Instrumentation.WorkContextHandled                 (Name, m_stat_WorkContextHandled                 ));
        i.Record( new Instrumentation.WorkContextNoDefaultClose          (Name, m_stat_WorkContextNoDefaultClose          ));
        i.Record( new Instrumentation.WorkContextNeedsSession            (Name, m_stat_WorkContextNeedsSession            ));

        i.Record( new Instrumentation.SessionNew                         (Name, m_stat_SessionNew                         ));
        i.Record( new Instrumentation.SessionExisting                    (Name, m_stat_SessionExisting                    ));
        i.Record( new Instrumentation.SessionEnd                         (Name, m_stat_SessionEnd                         ));
        i.Record( new Instrumentation.SessionInvalidID                   (Name, m_stat_SessionInvalidID                   ));

        i.Record( new Instrumentation.GeoLookup                          (Name, m_stat_GeoLookup                          ));
        i.Record( new Instrumentation.GeoLookupHit                       (Name, m_stat_GeoLookupHit                       ));

        foreach(var kvp in m_stat_PortalRequest.SnapshotAllLongs(0))
            i.Record( new Instrumentation.ServerPortalRequest(Name+"."+kvp.Key, kvp.Value) );

        var sample = (int)m_stat_WorkContextBufferedResponseBytes;
        if (sample!=0) Platform.RandomGenerator.Instance.FeedExternalEntropySample(sample);
     }

    #endregion

  }

}
