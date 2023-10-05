/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Apps
{
  /// <summary>
  /// Models a distributed call flow executed by multiple hosts.
  /// Maintains a list of steps of a distributed call flow, having the first element with index zero
  /// representing the very entry point and all subsequent elements representing a logical call chain - a flow of
  /// a distributed activity. The steps are similar conceptually to call stack frames which are created
  /// by the host/process ports - boundaries such as API controllers, network servers, business facades etc.
  /// </summary>
  public sealed class DistributedCallFlow : ICallFlow, IJsonReadable, IJsonWritable
  {
    /// <summary>
    /// Represents as step in a distributed call flow.
    /// Steps are created by the app ports - boundaries of application host/process (e.g. server handler)
    /// </summary>
    public sealed class Step : ICallFlow, IJsonReadable, IJsonWritable
    {
      /// <summary> FX internal method not to be used by developers </summary>
      internal void __SetSession(ISession session)
      {
        string thisSession;

        if (session == null)
        {
          thisSession = "null";
        } else if (session is NOPSession)
        {
          thisSession = "nop";
        } else if (session.User.Status > Security.UserStatus.Invalid)
        {
          thisSession = $"{session.GetType().Name}({session.ID.ToString().TakeFirstChars(4)},<{session.User.ToString().TakeFirstChars(48, "..")}>,`{session.DataContextName.TakeLastChars(32, "..")}`)";
        } else thisSession = $"{session.GetType().Name}({session.ID.ToString().TakeFirstChars(4)},INVAL,`{session.DataContextName}`)";

        Session = (Session.IsNullOrWhiteSpace() ? thisSession : $"{Session}; {thisSession}").TakeLastChars(200, "...");
      }

      internal Step(IApplication app, ISession session, ICallFlow flow)
      {
        Utc = app.TimeSource.UTCNow;
        __SetSession(session);
        App = app.AppId;
        AppInstance = app.InstanceId;
        Origin = app.CloudOrigin;
        Host = Platform.Computer.HostName;
        Type = flow.GetType().Name;

        ID = flow.ID;
        DirectorName = flow.DirectorName;
        CallerAddress = flow.CallerAddress;
        CallerAgent = flow.CallerAgent;
        CallerPort = flow.CallerPort;

        if (flow.Items.Any())
        {
          m_Items = new ConcurrentDictionary<string, object>(flow.Items);
        }
      }

      internal Step(JsonDataMap map) => ReadAsJson(map, true, null);

      private object m_Lock = new object();
      private volatile ConcurrentDictionary<string, object> m_Items;

      public IEnumerable<KeyValuePair<string, object>> Items => m_Items != null ? m_Items : Enumerable.Empty<KeyValuePair<string, object>>();

      public object this[string key]
      {
        get
        {
          key.NonNull(nameof(key));
          if (m_Items == null) return null;
          if (m_Items.TryGetValue(key, out var result)) return result;
          return null;
        }
        set
        {
          key.NonNull(nameof(key));
          if (m_Items == null)
          {
            lock (m_Lock)
            {
              if (m_Items == null)
              {
                m_Items = new ConcurrentDictionary<string, object>();
              }
            }
          }
          m_Items[key] = value;
        }//set
      }

      public DateTime Utc         { get; private set; }
      public string   Session     { get; private set; }
      public Atom     App         { get; private set; }
      public Guid     AppInstance { get; private set; }
      public Atom     Origin      { get; private set; }
      public string   Host        { get; private set; }
      public string   Type        { get; private set; }

      public Guid ID              { get; private set; }
      public string DirectorName  { get; private set; }
      public string CallerAddress { get; private set; }
      public string CallerAgent   { get; private set; }
      public string CallerPort    { get; private set; }

      public void SetDirectorName(string name) => DirectorName = name;

      public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
      {
        if (data is JsonDataMap map)
        {
          Utc = map["utc"].AsLong(0).FromMillisecondsSinceUnixEpochStart();
          Session = map["ssn"].AsString();
          App = map["app"].AsAtom(Atom.ZERO);
          AppInstance = map["ain"].AsGUID(Guid.Empty);
          Origin = map["org"].AsAtom(Atom.ZERO);
          Host = map["h"].AsString();
          Type = map["t"].AsString();
          ID = map["id"].AsGUID(Guid.Empty);
          DirectorName = map["dir"].AsString();
          CallerAddress = map["cad"].AsString();
          CallerAgent = map["cag"].AsString();
          CallerPort = map["cpr"].AsString();

          if (map["items"] is JsonDataMap items)
          {
            m_Items = new ConcurrentDictionary<string, object>(items);
          }

          return (true, this);
        }
        return (false, this);
      }

      public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
        => JsonWriter.WriteMap(wri, RepresentAsJson(), nestingLevel + 1, options);

      public JsonDataMap RepresentAsJson()
      {
        var map = new JsonDataMap
        {
          {"id", ID.ToString("N")},
          {"utc", Utc.ToMillisecondsSinceUnixEpochStart()},
          {"ain", AppInstance.ToString("N")},
        };

        if (Session.IsNotNullOrWhiteSpace()) map["ssn"] = Session;
        if (!App.IsZero) map["app"] = App;
        if (!Origin.IsZero) map["org"] = Origin;
        if (Host.IsNotNullOrWhiteSpace()) map["h"] = Host;
        if (Type.IsNotNullOrWhiteSpace()) map["t"] = Type;
        if (DirectorName.IsNotNullOrWhiteSpace()) map["dir"] = DirectorName;
        if (CallerAddress.IsNotNullOrWhiteSpace()) map["cad"] = CallerAddress;
        if (CallerAgent.IsNotNullOrWhiteSpace()) map["cag"] = CallerAgent;
        if (CallerPort.IsNotNullOrWhiteSpace()) map["cpr"] = CallerPort;


        var items = m_Items;
        if (items != null)
        {
          map["items"] = items;
        }

        return map;
      }
    }


    internal DistributedCallFlow(){ }

    /// <summary>
    /// Establishes a distributed call flow for this LOGICAL thread (asynchronous chain).
    /// If the current logical thread is already set with distributed flow then throws exception.
    /// Otherwise, sets the existing code flow step as the first step of the distributed one
    /// </summary>
    public static DistributedCallFlow Start(IApplication app, string description, Guid? guid = null, string directorName = null, string callerAgent = null, string callerPort = null)
    {
      app.NonNull(nameof(app));

      var current = ExecutionContext.CallFlow;

      var result = current as DistributedCallFlow;
      if (result == null)
      {
        result = new DistributedCallFlow();
        result.m_Description = description;

        if (current == null)
        {
          callerAgent = callerAgent.Default(System.Reflection.Assembly.GetCallingAssembly().GetName().Name);
          callerPort = callerPort.Default(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name);
          current = new CodeCallFlow(guid ?? Guid.NewGuid(), directorName, callerAgent, callerPort);
        }

        result.m_List.Add(new Step(app, ExecutionContext.Session, current));
        ExecutionContext.__SetThreadLevelCallContext(result);
      }
      else throw new AzosException("Distributed flow error: The context is already injected with DistributedCallFlow");

      return result;
    }

    /// <summary>
    /// Continues a distributed call flow on this LOGICAL thread (asynchronous call chain) from the supplied json header state.
    /// If the current logical thread is already set with distributed flow then throws exception.
    /// Otherwise, sets the existing code flow step as the first step of the distributed one
    /// </summary>
    public static DistributedCallFlow Continue(IApplication app, string hdrValue, Guid? guid = null, string directorName = null, string callerAgent = null, string callerPort = null)
    {
      hdrValue.NonBlank(nameof(hdrValue));
      JsonDataMap existing;
      try { existing = (hdrValue.JsonToDataObject() as JsonDataMap).IsTrue(v => v != null && v.Count > 0, nameof(existing)); }
      catch { throw new AzosException("Bad distributed call flow header"); }

      return Continue(app, existing, guid, directorName, callerAgent, callerPort);
    }

    /// <summary>
    /// Continues a distributed call flow on this LOGICAL thread (asynchronous call chain) from the supplied state (JsonDataMap).
    /// If the current logical thread is already set with distributed flow then throws exception.
    /// Otherwise, sets the existing code flow step as the first step of the distributed one
    /// </summary>
    public static DistributedCallFlow Continue(IApplication app, JsonDataMap existing, Guid? guid = null, string directorName = null, string callerAgent = null, string callerPort = null)
    {
      app.NonNull(nameof(app));
      existing.IsTrue(v => v!=null && v.Count > 0, nameof(existing));

      var current = ExecutionContext.CallFlow;

      var result = current as DistributedCallFlow;
      if (result == null)
      {
        result = new DistributedCallFlow();

        result.ReadAsJson(existing, false, null);

        if (current == null)
        {
          callerAgent = callerAgent.Default(System.Reflection.Assembly.GetCallingAssembly().GetName().Name);
          callerPort = callerPort.Default(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name);
          current = new CodeCallFlow(guid ?? Guid.NewGuid(), directorName, callerAgent, callerPort);
        }
        result.m_List.Add(new Step(app, ExecutionContext.Session, current));
        ExecutionContext.__SetThreadLevelCallContext(result);
      }
      else throw new AzosException("Distributed flow error: The context is already injected with DistributedCallFlow");

      return result;
    }

    /// <summary>
    /// Executes an asynchronous code block in a `DistributedCallFlow` scope.
    /// The block captures the current call flow at entry and guarantees to revert it back regardless of `body()` call outcome (e.g. if it fails with exception).
    /// If the current flow is already distributed, then it is continued as-if via a passing through process port/boundary adding a `CodeCallFlow` step
    /// with the supplied properties.
    /// If the current flow is not distributed, then a new distributed call flow is started with the existing flow as its first step.
    /// At the block exit the flow object is restored to the instance captured at the block entrance.
    /// </summary>
    public static async Task<TResult> ExecuteBlockAsync<TResult>(IApplication app, Func<DistributedCallFlow, Task<TResult>> body, string description, Guid? guid = null, string directorName = null, string callerAgent = null, string callerPort = null)
    {
      app.NonNull(nameof(app));
      body.NonNull(nameof(app));
      callerAgent = callerAgent.Default(System.Reflection.Assembly.GetCallingAssembly().GetName().Name);

      var original = ExecutionContext.CallFlow;
      try
      {
        DistributedCallFlow newFlow;

        if (original is DistributedCallFlow dcf)
        {
          var hdr = dcf.ToHeaderValue();
          ExecutionContext.__SetThreadLevelCallContext(null);
          newFlow = Continue(app, hdr, guid, directorName, callerAgent, callerPort);
        }
        else
        {
          newFlow = Start(app, description, guid, directorName, callerAgent, callerPort);
        }

        return await body(newFlow);//do not .ConfigureAwait(false);
      }
      finally
      {
        ExecutionContext.__SetThreadLevelCallContext(original);
      }
    }

    private bool m_WasLogged;
    private string m_Description;
    private List<Step> m_List = new List<Step>();

    /// <summary>
    /// Provides short description for the whole flow.
    /// It can only be set at the call flow start
    /// </summary>
    public string Description => m_Description;


    /// <summary>
    /// True if this call was already logged and <see cref="SetWasLogged(bool)"/> was called
    /// </summary>
    public bool WasLogged => m_WasLogged;

    /// <summary>
    /// Sets WasLogged to prevent duplicated logging
    /// </summary>
    public void SetWasLogged(bool v = true) => m_WasLogged = v;

    /// <summary>
    /// Returns the very first call flow frame which represents the entry point into a distributed call flow
    /// </summary>
    public Step EntryPoint => m_List[0];


    /// <summary>
    /// Returns the last call flow frame which represents the most current execution point in a flow
    /// </summary>
    public Step Current => m_List[m_List.Count - 1];

    /// <summary>
    /// Returns the frames (steps) of a distributed call flow, having the first element with index zero
    /// representing the very entry point and all subsequent elements representing a logical call chain - a flow of
    /// distributed activity. Returns null when a frame can not be gotten by the specified index
    /// </summary>
    public Step this[int idx]
    {
      get
      {
        if (idx < 0 || idx >= m_List.Count) return null;
        return m_List[idx];
      }
    }

    /// <summary>
    /// Total flow frame count
    /// </summary>
    public int Count => m_List.Count;

    /// <summary>
    /// Returns a flow ID at the entry point of this distributed flow, this way
    /// you can correlated various distributed activities using the same guid id
    /// as soon as it is generated at the very entry point
    /// </summary>
    public Guid ID => EntryPoint.ID;

    /// <summary>
    /// Returns a logical name of the primary flow originator/controller which GOVERNS the execution of various
    /// steps of the flow. The name is returned from the first set (non null) property in the distributed call chain or null
    /// </summary>
    public string DirectorName => m_List.FirstOrDefault(f => f.DirectorName.IsNotNullOrWhiteSpace())?.DirectorName;

    public string CallerAddress => EntryPoint.CallerAddress;

    public string CallerAgent => EntryPoint.CallerAgent;

    public string CallerPort => EntryPoint.CallerPort;

    public IEnumerable<KeyValuePair<string, object>> Items => Current.Items;

    public object this[string key] { get => Current[key]; set => Current[key] = value; }

    public void SetDirectorName(string name)
      => Current.SetDirectorName(name);


    /// <summary>
    /// Returns header value which is a terse json representation of the instance which can be used to
    /// re-construct the value on the other host
    /// </summary>
    public string ToHeaderValue() => this.ToJson(JsonWritingOptions.CompactASCII);


    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        m_Description = map["d"].AsString();

        var steps = map["steps"] as JsonDataArray;
        if (steps != null) m_List = steps.OfType<JsonDataMap>().Select(im => new Step(im)).ToList();

        return (true, this);
      }
      return (false, this);
    }

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
      => JsonWriter.WriteMap(wri, nestingLevel + 1, options,
                    new System.Collections.DictionaryEntry("d", m_Description),
                    new System.Collections.DictionaryEntry("steps", m_List));

    public JsonDataMap RepresentAsJson() => new JsonDataMap{ {"tp", "dcf"}, {"d", m_Description}, {"steps", m_List} };
  }
}
