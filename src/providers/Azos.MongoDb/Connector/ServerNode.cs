/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Glue;
using Azos.Instrumentation;

namespace Azos.Data.Access.MongoDb.Connector
{
  /// <summary>
  /// Manages connections per Mongo Db server
  /// </summary>
  public sealed class ServerNode : ApplicationComponent<MongoClient>, INamed, IInstrumentable
  {
    #region CONSTS
    public const string CONFIG_SERVER_SECTION = "server";

    public const int DEFAULT_RCV_TIMEOUT = 12_100;
    public const int DEFAULT_SND_TIMEOUT =  7_530;

    public const int DEFAULT_RCV_BUFFER_SIZE    = 128 * 1024;
    public const int DEFAULT_SND_BUFFER_SIZE    = 128 * 1024;

    public const int MIN_IDLE_TIMEOUT_SEC  = 5;
    public const int DEFAULT_IDLE_TIMEOUT_SEC  = 2 * 60;

    public const int MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MIN = 573;
    public const int MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MAX = 90_000;
    public const int MAX_EXISTING_ACQUISITION_TIMEOUT_MS_DEFAULT = 7530;
    #endregion

    #region .ctor
    internal ServerNode(MongoClient client, Node node, Node applianceNode) : base(client)
    {
      m_Node = node;
      m_ApplianceNode = applianceNode;

      var cfg = client.ConfigRoot//1. Try to find the SERVER section with name equals this node
                      .Children
                      .FirstOrDefault(c => c.IsSameName(CONFIG_SERVER_SECTION) && c.IsSameNameAttr(node.ConnectString));
      if (cfg==null)
        cfg = client.ConfigRoot //2. If not found, try to find SERVER section without name attr
                    .Children
                    .FirstOrDefault(c => c.IsSameName(CONFIG_SERVER_SECTION) && !c.AttrByName(Configuration.CONFIG_NAME_ATTR).Exists);

      if (cfg!=null)
        ConfigAttribute.Apply(this, client.ConfigRoot);
    }

    protected override void Destructor()
    {
      if (!Client.Disposed)
        Client.m_Servers.Unregister( this );

      var isLocalApplianceShutdown = !App.Active && m_ApplianceNode.Assigned;
      var eText = isLocalApplianceShutdown ? "Mongo appliance chassis is shutting down: " : "";
      var eType = isLocalApplianceShutdown ? Log.MessageType.InfoD : Log.MessageType.Error;

      this.DontLeak(() => killCursors(true),  eText, ".dctor(killCursors)", eType);

      //must be the last thing after killCursors()
      this.DontLeak(() => CloseAllConnections(true), eText, ".dctor(CloseAllConnections)", eType);

      base.Destructor();
    }
    #endregion

    #region Fields
    private int m_ID_SEED;
    private bool m_InstrumentationEnabled;
    private Node m_Node;
    private Node m_ApplianceNode;
    private object m_NewConnectionSync = new object();
    private object m_ListSync = new object();
    private volatile List<Connection> m_List = new List<Connection>();

    internal Registry<Database> m_Databases = new Registry<Database>(true);

    private WriteConcern m_WriteConcern;

    private List<Cursor> m_Cursors = new List<Cursor>(256);


    private int m_MaxConnections;
    private int m_MaxExistingAcquisitionTimeoutMs = MAX_EXISTING_ACQUISITION_TIMEOUT_MS_DEFAULT;
    //Timeouts and socket buffers setup
    private int m_IdleConnectionTimeoutSec = DEFAULT_IDLE_TIMEOUT_SEC;

    private int m_SocketReceiveBufferSize = DEFAULT_RCV_BUFFER_SIZE;
    private int m_SocketSendBufferSize    = DEFAULT_SND_BUFFER_SIZE;

    private int m_SocketReceiveTimeout = DEFAULT_RCV_TIMEOUT;
    private int m_SocketSendTimeout = DEFAULT_SND_TIMEOUT;
    #endregion

    #region Properties
    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public bool InstrumentationEnabled { get { return m_InstrumentationEnabled; } set { m_InstrumentationEnabled = value; } }

    public override string ComponentLogTopic => MongoConsts.MONGO_TOPIC;

    /// <summary>
    /// References client that this node is under
    /// </summary>
    public MongoClient Client => this.ComponentDirector;

    public Node Node { get{ return m_Node;} }

    /// <summary>
    /// Returns if this node was made from appliance connection
    /// </summary>
    public Node Appliance { get { return m_ApplianceNode; } }

    public string Name { get{ return m_Node.ConnectString;} }


    /// <summary>
    /// Generates request ID unique per server node
    /// </summary>
    internal int NextRequestID
    {
      get { return System.Threading.Interlocked.Increment(ref m_ID_SEED); } //overflows are ok
    }

    /// <summary>
    /// Returns mounted databases
    /// </summary>
    public IRegistry<Database> Databases { get {return m_Databases;} }

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public WriteConcern WriteConcern
    {
        get { return m_WriteConcern; }
        set { m_WriteConcern = value;}
    }

    /// <summary>
    /// When greater than zero, imposes a limit on the open connection count
    /// </summary>
    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int MaxConnections
    {
        get { return m_MaxConnections; }
        set { m_MaxConnections = value <0 ? 0 : value; }
    }

    /// <summary>
    /// Imposes a timeout for system trying to get an existing connection instance per remote address.
    /// </summary>
    [Config(Default = MAX_EXISTING_ACQUISITION_TIMEOUT_MS_DEFAULT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int MaxExistingAcquisitionTimeoutMs
    {
        get { return m_MaxExistingAcquisitionTimeoutMs; }
        set
        {
          m_MaxExistingAcquisitionTimeoutMs =
                value <MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MIN ? MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MIN :
                value >MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MAX ? MAX_EXISTING_ACQUISITION_TIMEOUT_MS_MAX :
                value;
        }
    }

    [Config(Default = DEFAULT_IDLE_TIMEOUT_SEC), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int IdleConnectionTimeoutSec
    {
        get { return m_IdleConnectionTimeoutSec; }
        set { m_IdleConnectionTimeoutSec = value <MIN_IDLE_TIMEOUT_SEC ? MIN_IDLE_TIMEOUT_SEC : value;}
    }

    [Config(Default = DEFAULT_RCV_BUFFER_SIZE), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int SocketReceiveBufferSize
    {
        get { return m_SocketReceiveBufferSize; }
        set { m_SocketReceiveBufferSize = value <=0 ? DEFAULT_RCV_BUFFER_SIZE : value;}
    }

    [Config(Default = DEFAULT_SND_BUFFER_SIZE), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int SocketSendBufferSize
    {
        get { return m_SocketSendBufferSize; }
        set { m_SocketSendBufferSize = value <=0 ? DEFAULT_SND_BUFFER_SIZE : value;}
    }

    [Config(Default = DEFAULT_RCV_TIMEOUT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int SocketReceiveTimeout
    {
        get { return m_SocketReceiveTimeout; }
        set { m_SocketReceiveTimeout = value <=0 ? DEFAULT_RCV_TIMEOUT : value;}
    }

    [Config(Default = DEFAULT_SND_TIMEOUT), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int SocketSendTimeout
    {
        get { return m_SocketSendTimeout; }
        set { m_SocketSendTimeout = value <=0 ? DEFAULT_SND_TIMEOUT : value;}
    }

    /// <summary>
    /// Connection information
    /// </summary>
    public IEnumerable<(FID id, bool busy, DateTime sd, DateTime? ed)> ConnectionInfos
    {
      get
      {
        var conns = m_List;
        foreach(var c in conns)
          yield return (c.Id, c.IsAcquired, c.StartDateUTC, c.ExpirationStartUTC);
      }
    }

    /// <summary>
    /// Returns an existing database or creates a new one
    /// </summary>
    public Database this[string name]
    {
      get
      {
          EnsureObjectNotDisposed();

          if (name.IsNullOrWhiteSpace())
            throw new MongoDbConnectorException(StringConsts.ARGUMENT_ERROR+"ServerNode[name==null|empty]");

          return m_Databases.GetOrRegister(name, (n) => new Database(this, n), name);
      }
    }

    #endregion

    #region Public

    /// <summary>
    /// Closes all connections. Waits until all closed if wait==true, otherwise tries to close what it can
    /// </summary>
    public void CloseAllConnections(bool wait)
    {
      bool allClosed = true;
      do
      {
        allClosed = true;
        var lst = m_List;

        foreach(var cnn in lst)
          if (cnn.TryAcquire())
            cnn.Dispose();
          else
            allClosed = false;

      } while(wait && !allClosed);
    }

    #endregion

    #region IInstrumentable
    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters => ExternalParameterAttribute.GetParameters(this);

    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
      => ExternalParameterAttribute.GetParameters(this, groups);

    public bool ExternalGetParameter(string name, out object value, params string[] groups)
      => ExternalParameterAttribute.GetParameter(App, this, name, out value, groups);

    public bool ExternalSetParameter(string name, object value, params string[] groups)
      => ExternalParameterAttribute.SetParameter(App, this, name, value, groups);
    #endregion

    #region Protected/Int


    internal Connection AcquireConnection() => acquireConnection(true);
    private Connection acquireConnection(bool checkDisposed)
    {
      if (checkDisposed) EnsureObjectNotDisposed();

      var result = tryAcquireExistingConnection();
      if (result != null) return result;

      //open new connection
      lock(m_NewConnectionSync)
      {
        result = tryAcquireExistingConnection();
        if (result!=null) return result;

        result = openNewConnection();
        return result;
      }
    }


    /// <summary>
    /// Periodically invoked by the client to do management work, like closing expired connections
    /// </summary>
    internal void ManagerVisit()
     => this.DontLeak(() => managerVisitUnsafe(), errorFrom: nameof(ManagerVisit));


    private bool m_Managing = false;
    private void managerVisitUnsafe()
    {
      if (m_Managing || Disposed) return;
      try
      {
        m_Managing = true;
        killCursors(false);
        closeInactiveConnections();
      }
      finally
      {
        m_Managing = false;
      }
    }


    internal void RegisterCursor(Cursor cursor)
    {
      if (Disposed) return;

      lock(m_Cursors)
        m_Cursors.Add(cursor);
    }

    #endregion

    #region .pvt
    private Connection openNewConnection()
    {
      var result = new Connection( this );

      lock(m_ListSync)
      {
        var lst = new List<Connection>(m_List);
        lst.Add(result);
        System.Threading.Thread.MemoryBarrier();
        m_List = lst;//atomic
      }
      return result;
    }


    internal void closeExistingConnection(Connection cnn)
    {
      if (Disposed) return;

      lock(m_ListSync)
      {
        var lst = new List<Connection>(m_List);
        lst.Remove(cnn);
        System.Threading.Thread.MemoryBarrier();
        m_List = lst;//atomic
      }
    }

    private Connection tryAcquireExistingConnection()
    {
      int GRANULARITY_MS = 5 + ((System.Threading.Thread.CurrentThread.GetHashCode() & CoreConsts.ABS_HASH_MASK) % 15);

      var wasActive = App.Active;//remember whether app was active during start
      var elapsed = 0;
      while ((App.Active | !wasActive) && !Disposed)
      {
        var lst = m_List;//atomic
        var got = lst.FirstOrDefault( c => c.TryAcquire() );
        if (got!=null) return got;

        if (m_MaxConnections<=0) return null;//could not acquire, lets allocate
        if (lst.Count<m_MaxConnections) return null;//limit has not been reached

        System.Threading.Thread.Sleep(GRANULARITY_MS);
        elapsed+=GRANULARITY_MS;

        if (elapsed>m_MaxExistingAcquisitionTimeoutMs)
          throw new MongoDbConnectorException(
                                        StringConsts.CONNECTION_EXISTING_ACQUISITION_ERROR
                                        .Args(this.Node, m_MaxConnections, m_MaxExistingAcquisitionTimeoutMs)
                                        );
      }

      return null; //the new connection may need to opened EVEN when the dispose is in progress so cursors may be closed etc.
    }

    private void closeInactiveConnections()
    {
      var now = App.TimeSource.UTCNow;
      var lst = m_List;
      for(var i=0; i<lst.Count; i++)
      {
          var cnn = lst[i];
          if (cnn.TryAcquire())
          try
          {
            if (cnn.m_ExpirationStartUTC.HasValue)
            {
              if ((now - cnn.m_ExpirationStartUTC.Value).TotalSeconds > m_IdleConnectionTimeoutSec)
              {
                cnn.Dispose();
                continue;
              }
            }
            else cnn.m_ExpirationStartUTC = now;
          }
          finally
          {
            cnn.Release(true);//not to reset use date
          }

      }
    }

    private void killCursors(bool killAll)
    {
      const int BATCH = 256;
      var toKill = new List<Cursor>();

      do
      {
        lock(m_Cursors)
        {
          for(var i=m_Cursors.Count-1; i>=0 && m_Cursors.Count>0 && toKill.Count<=BATCH; i--)
          {
            var cursor = m_Cursors[i];
            if (killAll || cursor.Disposed)
            {
              toKill.Add(cursor);
              m_Cursors.RemoveAt(i);
            }
          }
        }

        if (toKill.Count>0)
        {
          var connection = acquireConnection(false);
          try
          {
            var reqId = NextRequestID;
            connection.KillCursor(reqId, toKill.ToArray());
          }
          finally
          {
            connection.Release();
          }

          toKill.Clear();
        }
        else break;
      }
      while(killAll);
    }

    #endregion

  }

}
