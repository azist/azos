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
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Apps
{
  /// <summary>
  /// Models a distributed call flow executed by multiple hosts.
  /// Maintains a list of steps of a distributed call flow, having the first element with index zero
  /// representing the very entry point and all subsequent elements representing a logical call chain - a flow of
  /// a distributed activity. The steps are similar conceptually to call stack frames
  /// </summary>
  public sealed class DistributedCallFlow : ICallFlow, IJsonReadable, IJsonWritable
  {
    /// <summary>
    /// Represents as step in a distributed call flow
    /// </summary>
    public sealed class Step : ICallFlow, IJsonReadable, IJsonWritable
    {
      internal Step(IApplication app, ISession session, ICallFlow flow)
      {
        Utc = app.TimeSource.UTCNow;
        Session = session == null ? "<null>" : "{0}({1})".Args(session.GetType().Name, session.User);
        App = app.AppId;
        AppInstance = app.InstanceId;
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
          Utc = map["utc"].AsDateTime();
          Session = map["ssn"].AsString();
          App = map["app"].AsAtom();
          AppInstance = map["ainst"].AsGUID(Guid.Empty);
          Host = map["h"].AsString();
          Type = map["t"].AsString();
          ID = map["id"].AsGUID(Guid.Empty);
          CallerAddress = map["clr.addr"].AsString();
          CallerAgent = map["clr.agnt"].AsString();
          CallerPort = map["clr.port"].AsString();

          if (map["items"] is JsonDataMap items)
            m_Items = new ConcurrentDictionary<string, object>(items);

          return (true, this);
        }
        return (false, this);
      }

      public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
      {
        var map = new JsonDataMap
        {
          {"utc", Utc},
          {"ssn", Session},
          {"app", App},
          {"ainst", AppInstance},
          {"h", Host},
          {"t", Type},
          {"id", ID},
          {"dir", DirectorName},
          {"clr.addr", CallerAddress},
          {"clr.agnt", CallerAgent},
          {"clr.port", CallerPort}
        };

        var items = m_Items;
        if (items != null)
         map["items"] = items;

        JsonWriter.WriteMap(wri, map, nestingLevel+1, options);
      }
    }


    internal DistributedCallFlow(){ }

    /// <summary>
    /// Establishes a distributed call flow for this LOGICAL thread (asynchronous chain).
    /// If the current logical thread is already set with distributed flow then does nothing.
    /// Otherwise, sets the existing code flow step as the first step of the distributed one
    /// </summary>
    public static DistributedCallFlow Start(IApplication app, string description, Guid? guid = null, string directorName = null, string callerAgent = null, string callerPort = null)
    {
      app.NonNull(nameof(app));

      var current = ExecutionContext.CallFlow;

      if (current == null)
      {
        callerAgent = callerAgent.Default(System.Reflection.Assembly.GetCallingAssembly().GetName().Name);
        callerPort = callerPort.Default(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name);
        current = new CodeCallFlow(guid ?? Guid.NewGuid(), directorName, callerAgent, callerPort);
      }

      var result = current as DistributedCallFlow;
      if (result == null)
      {
        result = new DistributedCallFlow();
        result.m_Description = description;
        result.m_List.Add(new Step(app, ExecutionContext.Session, current));
        ExecutionContext.__SetThreadLevelCallContext(result);
      }

      return result;
    }

    public static DistributedCallFlow Continue(IApplication app, JsonDataMap existing, Guid? guid = null, string directorName = null, string callerAgent = null, string callerPort = null)
    {
      app.NonNull(nameof(app));
      existing.IsTrue(v => v!=null && v.Count > 0, nameof(existing));

      var current = ExecutionContext.CallFlow;

      if (current == null)
      {
        callerAgent = callerAgent.Default(System.Reflection.Assembly.GetCallingAssembly().GetName().Name);
        callerPort = callerPort.Default(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name);
        current = new CodeCallFlow(guid ?? Guid.NewGuid(), directorName, callerAgent, callerPort);
      }

      var result = current as DistributedCallFlow;
      if (result == null)
      {
        result = new DistributedCallFlow();
        result.ReadAsJson(existing, false, null);
        result.m_List.Add(new Step(app, ExecutionContext.Session, current));
        ExecutionContext.__SetThreadLevelCallContext(result);
      }
      else throw new AzosException("Distributed flow error: The context is already injected with DistributedCallFlow");

      return result;
    }

    private string m_Description;
    private List<Step> m_List = new List<Step>();



    /// <summary>
    /// Provides short description for the whole flow.
    /// It can only be set at the call flow start
    /// </summary>
    public string Description => m_Description;

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
    /// Returns a flow ID at the entry point of this distributed flow
    /// </summary>
    public Guid ID => EntryPoint.ID;

    /// <summary>
    /// Returns a logical name of the primary flow originator/controller which GOVERNS the execution of various
    /// steps of the flow. The name is returned from the first set property in the distributed call chain
    /// </summary>
    public string DirectorName => m_List.FirstOrDefault(f => f.DirectorName.IsNotNullOrWhiteSpace()).DirectorName;

    public string CallerAddress => EntryPoint.CallerAddress;

    public string CallerAgent => EntryPoint.CallerAgent;

    public string CallerPort => EntryPoint.CallerPort;

    public IEnumerable<KeyValuePair<string, object>> Items => Current.Items;

    public object this[string key] { get => Current[key]; set => Current[key] = value; }

    public void SetDirectorName(string name)
      => Current.SetDirectorName(name);


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
  }
}
