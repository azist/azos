/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Azos.Log;
using Azos.Web;

using Azos.Serialization.JSON;
using Azos.Web.GeoLookup;
using Azos.Platform;

namespace Azos.Wave
{
  /// <summary>
  /// Represents a context for request/response server processing in WAVE framework
  /// </summary>
  [Serialization.Slim.SlimSerializationProhibited]
  public class WorkContext : DisposableObject, Apps.ICallFlow
  {
    #region .ctor/.dctor
    private static AsyncFlowMutableLocal<WorkContext> ats_Current = new AsyncFlowMutableLocal<WorkContext>();

    /// <summary>
    /// Returns the current call context/thread/async flow instance
    /// </summary>
    public static WorkContext Current => ats_Current.Value;

    internal WorkContext(WaveServer server, HttpContext httpContext)
    {
      m_ID = Guid.NewGuid();
      m_Server = server;
      m_HttpContext = httpContext;
      m_Request = new Request(httpContext.Request);
      m_Response = new Response(this, httpContext.Response);

      ats_Current.Value = this;
      Apps.ExecutionContext.__SetThreadLevelCallContext(this);
      Interlocked.Increment(ref m_Server.m_stat_WorkContextCtor);

      var flowHdr = m_Server.CallFlowHeader;
      if (flowHdr.IsNotNullOrWhiteSpace())
        m_HttpContext.Response.AddHeader(flowHdr, m_ID.ToString());
    }

    /// <summary>
    /// Warning: if overridden, must call base otherwise semaphore will not get released
    /// </summary>
    protected override void Destructor()
    {
      if (m_Server.m_InstrumentationEnabled)
      {
        Interlocked.Increment(ref m_Server.m_stat_WorkContextDctor);
        if (m_Aborted) Interlocked.Increment(ref m_Server.m_stat_WorkContextAborted);
        if (m_Handled) Interlocked.Increment(ref m_Server.m_stat_WorkContextHandled);
        if (m_NoDefaultAutoClose) Interlocked.Increment(ref m_Server.m_stat_WorkContextNoDefaultClose);
      }

      ats_Current.Value = null;
      Apps.ExecutionContext.__SetThreadLevelCallContext(null);
      m_Response.Dispose();
    }
    #endregion

    #region Fields
    private Guid m_ID;
    private WaveServer m_Server;

    private HttpContext m_HttpContext;
    internal IPEndPoint m_EffectiveCallerIPEndPoint;//set by filters
    private Request m_Request;
    private Response m_Response;

    internal Filters.SessionFilter m_SessionFilter;
    internal WaveSession m_Session;

    internal Filters.PortalFilter m_PortalFilter;
    internal Portal m_Portal;
    internal Theme m_PortalTheme;
    internal WorkMatch m_PortalMatch;
    internal JsonDataMap m_PortalMatchedVars;


    private object m_ItemsLock = new object();
    private volatile ConcurrentDictionary<object, object> m_Items;

    internal WorkHandler m_Handler;

    private WorkMatch m_Match;
    private JsonDataMap m_MatchedVars;
    /// <summary>
    /// Internal method. Developers do not call
    /// </summary>
    internal void ___SetWorkMatch(WorkMatch match, JsonDataMap vars){m_Match = match; m_MatchedVars = vars;}

    private bool m_HasParsedRequestBody;
    private JsonDataMap m_RequestBodyAsJSONDataMap;
    private JsonDataMap m_WholeRequestAsJSONDataMap;

    internal bool m_Handled;
    private bool m_Aborted;

    private bool m_NoDefaultAutoClose;

    private GeoEntity m_GeoEntity;

    private bool m_IsAuthenticated;

    private string m_CallFlowDirectorName;
    private volatile ConcurrentDictionary<string, object> m_CallFlowValues;
    #endregion

    #region Properties


    /// <summary>
    /// Uniquely identifies the request
    /// </summary>
    public Guid ID => m_ID;

    string Apps.ICallFlow.DirectorName => m_CallFlowDirectorName;

    void Apps.ICallFlow.SetDirectorName(string name) => m_CallFlowDirectorName = name;

    string Apps.ICallFlow.CallerAddress => EffectiveCallerIPEndPoint.ToString();
    string Apps.ICallFlow.CallerAgent   => Request.UserAgent.TakeFirstChars(96, "..");
    string Apps.ICallFlow.CallerPort    => Request.Method + "  " + Request.Url.TakeFirstChars(96, "..");

    object Apps.ICallFlow.this[string key]
    {
      get
      {
        key.NonNull(nameof(key));
        if (m_CallFlowValues != null && m_CallFlowValues.TryGetValue(key, out var existing)) return existing;
        return null;
      }
      set
      {
        key.NonNull(nameof(key));
        if (m_CallFlowValues == null)
        {
          lock(m_ItemsLock)
          {
            if (m_CallFlowValues == null)
              m_CallFlowValues = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);
          }
        }
        m_CallFlowValues[key] = value;
      }
    }

    IEnumerable<KeyValuePair<string, object>> Apps.ICallFlow.Items
      => m_CallFlowValues==null ? Enumerable.Empty<KeyValuePair<string, object>>() : m_CallFlowValues;

    /// <summary>
    /// Returns the application that this context is under
    /// </summary>
    public IApplication App => m_Server.App;

    /// <summary>
    /// Returns the server that this context is under
    /// </summary>
    public WaveServer Server => m_Server;


    /// <summary>
    /// Returns ASP context
    /// </summary>
    public HttpContext HttpContext => m_HttpContext;

    /// <summary>
    /// Returns Request object for this context
    /// </summary>
    public Request Request => m_Request;


    /// <summary>
    /// Returns the effective caller endpoint- that is, if the real caller filter is set it will inject the real IP
    /// as seen before any proxy devices. By default this property returns the Request.RemoteEndPoint
    /// </summary>
    public IPEndPoint EffectiveCallerIPEndPoint
      => m_EffectiveCallerIPEndPoint ?? new IPEndPoint(m_HttpContext.Connection.RemoteIpAddress, m_HttpContext.Connection.RemotePort);


    /// <summary>
    /// Returns Response object for this context
    /// </summary>
    public Response Response => m_Response;

    /// <summary>
    /// Returns session that this context is linked with or null
    /// </summary>
    public WaveSession Session => m_Session;

    /// <summary>
    /// Returns the first session filter which was injected in the processing line.
    /// It is the filter that manages the session state for this context
    /// </summary>
    public Filters.SessionFilter SessionFilter => m_SessionFilter;

    /// <summary>
    /// Returns true when the context was configured to support SessionFilter so Session can be injected
    /// </summary>
    public bool SupportsSession => m_SessionFilter != null;

    /// <summary>
    /// Returns portal object for this request or null if no portal was injected
    /// </summary>
    public Portal Portal => m_Portal;

              /// <summary>
              /// DEVELOPERS do not use!
              /// A hack method needed in some VERY RARE cases, like serving an error page form the filter which is out of portal scope.
              /// </summary>
              public void ___InternalInjectPortal(Portal portal = null,
                                                  Theme theme = null,
                                                  WorkMatch match = null,
                                                  JsonDataMap matchedVars = null)
                                                  {
                                                    m_Portal = portal;
                                                    m_PortalTheme = theme;
                                                    m_PortalMatch = match;
                                                    m_PortalMatchedVars = matchedVars;
                                                  }

    /// <summary>
    /// Returns the first portal filter which was injected in the processing line.
    /// It is the filter that manages the portals for this context
    /// </summary>
    public Filters.PortalFilter PortalFilter => m_PortalFilter;

    /// <summary>
    /// Returns matched that was made by portal filter or null
    /// </summary>
    public WorkMatch PortalMatch => m_PortalMatch;

    /// <summary>
    /// Gets/sets portal theme. This may be null as this is just a holder variable
    /// </summary>
    public Theme PortalTheme
    {
      get{ return m_PortalTheme ?? (m_Portal!=null ? m_Portal.DefaultTheme :  null);}
      set{ m_PortalTheme = value;}
    }


    /// <summary>
    /// Returns variables that have been extracted by WorkMatch when PortalFilter assigned portal.
    /// Returns null if no portal was matched
    /// </summary>
    public JsonDataMap PortalMatchedVars => m_PortalMatchedVars;


    /// <summary>
    /// Returns the work match instances that was made for this requested work or null if nothing was matched yet
    /// </summary>
    public WorkMatch Match => m_Match;

    /// <summary>
    /// Returns variables that have been extracted by WorkMatch when dispatcher assigned request to WorkHandler.
    /// If variables have not been assigned yet returns empty object
    /// </summary>
    public JsonDataMap MatchedVars
    {
      get
      {
        if (m_MatchedVars==null)
          m_MatchedVars = new JsonDataMap(false);

        return m_MatchedVars;
      }
    }

    /// <summary>
    /// Returns dynamic object that contains variables that have been extracted by WorkMatch when dispatcher assigned request to WorkHandler.
    /// If variables have not been assigned yet returns empty object
    /// </summary>
    public dynamic Matched => new JsonDynamicObject(MatchedVars);



    /// <summary>
    /// Fetches request body: multi-part content, URL encoded content, or JSON body into one JSONDataMap bag,
    /// or null if there is no body. The method performs value caching
    /// </summary>
    public async ValueTask<JsonDataMap> GetRequestBodyAsJsonDataMapAsync()
    {
      if (!m_HasParsedRequestBody)
      {
        m_RequestBodyAsJSONDataMap = await DoGetRequestBodyAsJsonDataMapAsync();
        m_HasParsedRequestBody = true;
      }
      return m_RequestBodyAsJSONDataMap;
    }

    /// <summary>
    /// Fetches matched vars, multi-part content, URL encoded content, or JSON body into one JSONDataMap bag.
    /// The method performs value caching
    /// </summary>
    public async ValueTask<JsonDataMap> GetWholeRequestAsJsonDataMapAsync()
    {
      if (m_WholeRequestAsJSONDataMap == null)
      {
        m_WholeRequestAsJSONDataMap = await DoGetWholeRequestAsJsonDataMapAsync();
      }
      return m_WholeRequestAsJSONDataMap;
    }


    /// <summary>
    /// Provides a thread-safe dictionary of items. The underlying collection is lazily allocated
    /// </summary>
    public ConcurrentDictionary<object, object> Items
    {
        get
        {
          if (m_Items==null)
              lock(m_ItemsLock)
              {
                if (m_Items==null)
                  m_Items = new ConcurrentDictionary<object,object>(4, 16);
              }
          return m_Items;
        }
    }

    /// <summary>
    /// Returns the work handler instance that was matched to perform work on this context or null if the match has not been made yet
    /// </summary>
    public WorkHandler Handler => m_Handler;


    /// <summary>
    /// Returns true when the work has been executed by the WorkHandler instance
    /// </summary>
    public bool Handled => m_Handled;

    /// <summary>
    /// Indicates whether the work context is logically finished and its nested processing (i.e. through Filters/Handlers) should stop.
    /// For example, when some filter detects a special condition (judging by the request) and generates the response
    ///  and needs to abort the work request so it does no get filtered/processed anymore, it can set this property to true.
    /// This mechanism performs much better than throwing exceptions
    /// </summary>
    public bool Aborted
    {
      get {return m_Aborted;}
      set {m_Aborted = value;}
    }


    /// <summary>
    /// Generates short context description
    /// </summary>
    public string About
      =>"Work('{0}'@'{1}' -> {2} '{3}')".Args(Request.UserAgent, EffectiveCallerIPEndPoint, Request.Method, Request.Url);

    /// <summary>
    /// Indicates whether the default dispatcher should close the WorkContext upon completion of async processing.
    /// This property may ONLY be set to TRUE IF Response.Buffered = false (chunked transfer) and Response has already been written to.
    /// When this property is set to true the WorkDispatcher will not auto dispose this WorkContext instance.
    /// This may be needed for a server that streams chat messages and some other thread manages the lifetime of this WorkContext.
    /// Keep in mind that alternative implementations of WorkDispatcher (derived classes that implement alternative threading/lifecycle)
    ///  may disregard this flag altogether
    /// </summary>
    public bool NoDefaultAutoClose
    {
      get { return m_NoDefaultAutoClose;}
      set
      {
        if ( value && (Response.Buffered==true || !Response.WasWrittenTo))
          throw new WaveException(StringConsts.WORK_NO_DEFAULT_AUTO_CLOSE_ERROR);

        m_NoDefaultAutoClose = value;
      }
    }

    /// <summary>
    /// Captures last error
    /// </summary>
    public Exception LastError { get; set; }

    /// <summary>
    /// Gets sets geo location information as detected by GeoLookupHandler.
    /// If Session context is injected then get/set passes through into session object
    /// </summary>
    public GeoEntity GeoEntity
    {
      get { return m_Session==null? m_GeoEntity : m_Session.GeoEntity;}
      set { if (m_Session==null)  m_GeoEntity = value; else  m_Session.GeoEntity = value;}
    }


    private bool? m_RequestedJson;
    /// <summary>
    /// Returns true if client indicated in response that "application/json" is accepted
    /// </summary>
    public bool RequestedJson
    {
      get
      {
        if (!m_RequestedJson.HasValue)
          m_RequestedJson = Request.RequestedJson;
        return m_RequestedJson.Value;
      }
    }

    /// <summary>
    /// Indicates that request method is POST
    /// </summary>
    public bool IsPOST => Request.Method.EqualsOrdIgnoreCase(WebConsts.HTTP_POST);

    /// <summary>
    /// Indicates that request method is GET
    /// </summary>
    public bool IsGET => Request.Method.EqualsOrdIgnoreCase(WebConsts.HTTP_GET);

    /// <summary>
    /// Indicates that request method is PUT
    /// </summary>
    public bool IsPUT => Request.Method.EqualsOrdIgnoreCase(WebConsts.HTTP_PUT);

    /// <summary>
    /// Indicates that request method is DELETE
    /// </summary>
    public bool IsDELETE => Request.Method.EqualsOrdIgnoreCase(WebConsts.HTTP_DELETE);

    /// <summary>
    /// Indicates that request method is PATCH
    /// </summary>
    public bool IsPATCH => Request.Method.EqualsOrdIgnoreCase(WebConsts.HTTP_PATCH);

    /// <summary>
    /// Indicates that request method is OPTIONS
    /// </summary>
    public bool IsOPTIONS => Request.Method.EqualsOrdIgnoreCase(WebConsts.HTTP_OPTIONS);

    /// <summary>
    /// Returns true to indicate that this context is/was authenticated.
    /// Used to not redirect users to login page on authorization exception
    /// </summary>
    public bool IsAuthenticated => m_IsAuthenticated;
    #endregion

    #region Public

    /// <summary>
    /// Ensures that session is injected if session filter is present in processing chain.
    /// If session is already available (Session!=null) then does nothing, otherwise
    /// fills Session property with either NEW session (if onlyExisting=false(default)) if user supplied no session token,
    /// OR gets session from session store as defined by the first SessionFilter in the chain
    /// </summary>
    public WaveSession NeedsSession(bool onlyExisting = false)
    {
      if (m_Session!=null) return m_Session;

      Interlocked.Increment(ref m_Server.m_stat_WorkContextNeedsSession);

      if (m_SessionFilter!=null)
        m_SessionFilter.FetchExistingOrMakeNewSession(this, onlyExisting);
      else
        throw new WaveException(StringConsts.SESSION_NOT_AVAILABLE_ERROR.Args(About));

      return m_Session;
    }


    /// <summary>
    /// Facilitates context-aware logging
    /// </summary>
    public void Log(MessageType type, string text, string from = null, Exception error = null, string pars = null, Guid? related = null)
    {
      var msg = new Message
      {
        Type = type,
        Topic = SysConsts.WAVE_LOG_TOPIC,
        From = from.IsNotNullOrWhiteSpace() ? from : About,
        Text = text,
        Exception = error ?? LastError,
        Parameters = pars
      };

      if (related.HasValue)
        msg.RelatedTo = related.Value;
      else
        msg.RelatedTo = this.m_ID;

      App.Log.Write(msg);
    }

    /// <summary>
    /// Returns true if the whole request (body or matched vars) contains any names matching any field names of the specified document
    /// </summary>
    public bool HasAnyVarsMatchingFieldNames(Data.Doc doc)
    {
      if (doc == null) return false;

      //this is rarely use method
      var whole = GetWholeRequestAsJsonDataMapAsync().AsTask().SyncRun();

      foreach (var fdef in doc.Schema)
        if (whole.ContainsKey(fdef.Name)) return true;

      return false;
    }

    public override string ToString()
    {
      return About;
    }

    /// <summary>
    /// Invoked by applications to signify the presence of authentication
    /// </summary>
    public void SetAuthenticated(bool value)
    {
      m_IsAuthenticated = value;
    }

    /// <summary>
    /// Tries to increase server network Gate named variable for incoming traffic for this caller's effective ip.
    /// Returns true if gate is enabled and variable was increased
    /// </summary>
    public bool IncreaseGateVar(string varName, int value = 1)
    {
      varName.NonBlank(nameof(varName));
      var gate = Server.Gate;
      if (gate == null || !gate.Enabled) return false;
      var ip = EffectiveCallerIPEndPoint.Address.ToString();
      gate.IncreaseVariable(IO.Net.Gate.TrafficDirection.Incoming, ip, varName, value);
      return true;
    }

    #endregion


    #region Protected

    /// <summary>
    /// Converts request body and MatchedVars into a single JSONDataMap. Users should call WholeRequestAsJSONDataMap.get() as it caches the result
    /// </summary>
    protected virtual async ValueTask<JsonDataMap> DoGetWholeRequestAsJsonDataMapAsync()
    {
      var body = await this.GetRequestBodyAsJsonDataMapAsync();

      if (body == null) return MatchedVars;

      var result = new JsonDataMap(false);
      result.Append(MatchedVars)
            .Append(body);
      return result;
    }

      /// <summary>
      /// This method is called only once as it touches the input streams
      /// </summary>
      //todo: Add async json parsing
      protected virtual async ValueTask<JsonDataMap> DoGetRequestBodyAsJsonDataMapAsync()
      {
        //request content decompression
        //https://stackoverflow.com/questions/42792099/request-content-decompression-in-asp-net-core
        //https://markb.uk/asp-net-core-read-raw-request-body-as-string.html
        // Request.AspRequest.ContentLength

        var ctp = Request.ContentType;

        //Has body by no content type
        if (ctp == null) return null; //no body

        JsonDataMap result = null;
        try
        {
          //Multi-part
          if (ctp.IndexOf(ContentType.FORM_MULTIPART_ENCODED)>=0)
          {
            var boundary = Multipart.ParseContentType(ctp);
            var mp = Multipart.ReadFromStream(Request.BodyStream, ref boundary);
            result =  mp.ToJSONDataMap();
          }
          else //Form URL encoded
          if (ctp.IndexOf(ContentType.FORM_URL_ENCODED)>=0)
            result = JsonDataMap.FromURLEncodedStream(new Azos.IO.NonClosingStreamWrap(Request.BodyStream));
          else//JSON
          if (ctp.IndexOf(ContentType.JSON)>=0)
            result = JsonReader.DeserializeDataObject(new Azos.IO.NonClosingStreamWrap(Request.BodyStream)) as JsonDataMap;

          return result;
        }
        catch(Exception error)
        {
          throw new HTTPStatusException(WebConsts.STATUS_400,
                                        WebConsts.STATUS_400_DESCRIPTION + " body",
                                        error.ToMessageWithType(),
                                        error);
        }
      }
    #endregion

  }

}
