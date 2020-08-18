/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Instrumentation;

namespace Azos.Data.Access.MongoDb.Connector
{
  /// <summary>
  /// The central facade for working with MongoDB. The technology was tested against Mongo DB 3.+
  /// The Azos MongoDB connector is purposely created for specific needs such as using MongoDB as a device yielding 4x-8x better throughput than the official driver.
  /// It does not support: Mongo security, sharding and replication
  /// </summary>
  public sealed class MongoClient : ApplicationComponent, INamed, IConfigurable, IInstrumentable, IExternallyCallable
  {
    #region CONSTS

    public const string CONFIG_MONGO_CLIENT_SECTION = "mongo-db-client";

    public const string CONFIG_CS_ROOT_SECTION = "mongo";
    public const string CONFIG_CS_SERVER_ATTR = "server";
    public const string CONFIG_CS_DB_ATTR = "db";

    private static readonly TimeSpan MANAGEMENT_INTERVAL = TimeSpan.FromMilliseconds(4795);
    #endregion

    #region .ctor

    /// <summary>
    /// Creates a new instance of client.
    /// For most applications it is sufficient to use the default singleton instance App.GetDefaultMongoClient()
    /// </summary>
    public MongoClient(IApplication app) : base(app) => ctor(null);
    /// <summary>
    /// Creates a new instance of client.
    /// For most applications it is sufficient to use the default singleton instance App.GetDefaultMongoClient()
    /// </summary>
    public MongoClient(IApplication app, string name) : base(app) => ctor(name);

    /// <summary>
    /// Creates a new instance of client.
    /// For most applications it is sufficient to use the default singleton instance App.GetDefaultMongoClient()
    /// </summary>
    public MongoClient(IApplicationComponent director) : base(director) => ctor(null);
    /// <summary>
    /// Creates a new instance of client.
    /// For most applications it is sufficient to use the default singleton instance App.GetDefaultMongoClient()
    /// </summary>
    public MongoClient(IApplicationComponent director, string name) : base(director) => ctor(name);

    private void ctor(string name)
    {
      if (name.IsNullOrWhiteSpace())
        name = Guid.NewGuid().ToString();

      m_Name = name;
      m_ManagementEvent = new Time.Event(App.EventTimer,
                                          "MongoClient('{0}'::{1})".Args(m_Name, Guid.NewGuid().ToString()),
                                          e => managementEventBody(),
                                          MANAGEMENT_INTERVAL);

      m_ExternalCallHandler = new ExternalCallHandler<MongoClient>(App, this, null,
          typeof(Instrumentation.DirectDb)
        );
    }

    protected override void Destructor()
    {
      DisposableObject.DisposeAndNull(ref m_ManagementEvent);

      foreach(var server in m_Servers)
        server.Dispose();

      base.Destructor();
    }

    #endregion

    #region Fields
    private IConfigSectionNode m_ConfigRoot;

    private string m_Name;
    private bool m_InstrumentationEnabled;

    private Time.Event m_ManagementEvent;
    internal Registry<ServerNode> m_Servers = new Registry<ServerNode>();

    private ExternalCallHandler<MongoClient> m_ExternalCallHandler;
    #endregion


    #region Properties

    public override string ComponentLogTopic => MongoConsts.MONGO_TOPIC;

    public string Name{ get{ return m_Name;}}

    [Config]
    public bool InstrumentationEnabled { get{ return m_InstrumentationEnabled; } set { m_InstrumentationEnabled = value;} }

    /// <summary>
    /// Returns the config root of the client that was set by the last call to Configure()
    /// or App.CONFIG_MONGO_CLIENT_SECTION (which may be non-existent)
    /// </summary>
    public IConfigSectionNode ConfigRoot
    {
      get { return m_ConfigRoot ?? App.ConfigRoot[CONFIG_MONGO_CLIENT_SECTION];}
    }

    /// <summary>
    /// Returns all connected servers
    /// </summary>
    public IRegistry<ServerNode> Servers { get{ return m_Servers;} }

    /// <summary>
    /// Returns an existing server node or creates a new one
    /// </summary>
    public ServerNode this[Glue.Node node]
    {
      get
      {
          EnsureObjectNotDisposed();
          return m_Servers.GetOrRegister(node.Name, (n) => new ServerNode(this, n), node);
      }
    }

    /// <summary>
    /// Returns ServerNode for local server on a default port
    /// </summary>
    public ServerNode DefaultLocalServer
    {
      get { return this[Connection.DEFAUL_LOCAL_NODE];}
    }


    /// <summary>
    /// Returns a handler which processes external administration calls, such as the ones originating from
    /// the application terminal
    /// </summary>
    public IExternalCallHandler GetExternalCallHandler() => m_ExternalCallHandler;

    #endregion

    #region Public

    /// <summary>
    /// Sets config root. If this method is never called then configuration is done of the App.CONFIG_MONGO_CLIENT_SECTION section
    /// </summary>
    public void Configure(IConfigSectionNode node)
    {
      EnsureObjectNotDisposed();

      if (node==null || !node.Exists)
        node = App.ConfigRoot[CONFIG_MONGO_CLIENT_SECTION];

      m_ConfigRoot = node;
      ConfigAttribute.Apply(this, ConfigRoot);
    }

    public override string ToString()
    {
      return "Client('{0}')".Args(m_Name);
    }

    #endregion

    #region IInstrumentable
    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      return ExternalParameterAttribute.GetParameters(this, groups);
    }

    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
        return ExternalParameterAttribute.GetParameter(App, this, name, out value, groups);
    }

    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      return ExternalParameterAttribute.SetParameter(App, this, name, value, groups);
    }
    #endregion

    #region .pvt

    private void managementEventBody()
    {
      foreach(var server in m_Servers)
        server.ManagerVisit();

      //todo Dump statistics
    }

    #endregion
  }
}
