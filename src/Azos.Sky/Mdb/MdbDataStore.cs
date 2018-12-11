using System;
using System.Linq;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Data;
using Azos.Data.Access;
using Azos.Instrumentation;
using Azos.Pile;

using Azos.Sky.Identification;

namespace Azos.Sky.Mdb
{
  /// <summary>
  /// Represents a MDB data store - consisting of areas with partitions
  /// </summary>
  public class MdbDataStore : DaemonWithInstrumentation<IApplicationComponent>, IMdbDataStore, IDataStoreImplementation
  {
    #region .ctor
    public MdbDataStore(IApplication app) : base(app) { }
    public MdbDataStore(IApplicationComponent director, string name) : base(director)
    {
      Name = name.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString() : name;
    }
    #endregion

    #region Private Fields

    public string m_SchemaName;
    public string m_BankName;

    private bool m_InstrumentationEnabled;
    private StoreLogLevel m_LogLevel;
    private string m_TargetName;


    private GdidGenerator m_GdidGenerator;

    private MDBCentralArea m_CentralArea;
    private Registry<MdbArea> m_Areas = new Registry<MdbArea>();

    private LocalCache m_Cache;

    #endregion

    #region Properties

    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_MDB;

    [Config]
    public string SchemaName
    {
      get{ return m_SchemaName;}
      set
      {
        CheckDaemonInactive();
        m_SchemaName = value;
      }
    }

    [Config]
    public string BankName
    {
      get{ return m_BankName;}
      set
      {
        CheckDaemonInactive();
        m_BankName = value;
      }
    }

    /// <summary>
    /// Generates GDIDs
    /// </summary>
    public IGdidProvider GdidGenerator { get {return m_GdidGenerator;} }

    [Config(Default=false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled{ get{return m_InstrumentationEnabled;} set{m_InstrumentationEnabled = value;}}


    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public StoreLogLevel LogLevel
    {
      get { return m_LogLevel; }
      set { m_LogLevel = value;}
    }

    [Config]
    public string TargetName
    {
      get { return m_TargetName; }
      set
      {
        CheckDaemonInactive();
        m_TargetName = value;
      }
    }

    /// <summary>
    /// Returns the central area - the one that does not partition data
    /// </summary>
    public MDBCentralArea  CentralArea { get{ return m_CentralArea;}}

    /// <summary>
    /// Returns the areas (including the central area)
    /// </summary>
    public IRegistry<MdbArea> Areas { get{ return m_Areas;}}


    /// <summary>
    /// Pile big memory cache. The store does not use this by itself, the implementors/derived stores should
    ///  use the cache to store business-specific data
    /// </summary>
    public ICache Cache
    {
      get{ return m_Cache;}
    }

    #endregion

    #region Public

    public void TestConnection()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Returns CRUDOperations facade connected to the appropriate database server within the named area
    ///  which services the shard computed from the briefcase GDID
    /// </summary>
    public CRUDOperations PartitionedOperationsFor(string areaName, GDID idBriefcase)
    {
      CheckDaemonActive();
      var area = m_Areas[areaName] as MdbPartitionedArea;
      if (area==null)
        throw new MdbException(StringConsts.MDB_PARTITIONED_AREA_MISSING_ERROR.Args(areaName));

      return area.PartitionedOperationsFor(idBriefcase);
    }

      ///// <summary>
      ///// Returns CRUDOperations facade connected to the appropriate database server within the named area
      /////  which services the shard computed from the briefcase GDID
      ///// </summary>
      //public CRUDOperations PartitionedOperationsFor(string areaName, object shardingID, int volumeOffset = -1)
      //{
      //  CheckServiceActive();
      //  var area = m_Areas[areaName] as MDBPartitionedArea;
      //  if (area == null)
      //    throw new MDBException(StringConsts.MDB_PARTITIONED_AREA_MISSING_ERROR.Args(areaName));

      //  #warning TODO
      //  var parts = area.AllPartitions;
      //  var count = parts.Count();
      //  var volume = volumeOffset % count;
      //  if (volume < 0)
      //    volume = count - volume;
      //  return parts.Skip(volume).Single().ShardedOperationsFor(shardingID);
      //}

    /// <summary>
    /// Returns CRUDOperations facade connected to the appropriate shard within the central area as
    /// determined by the shardingID
    /// </summary>
    public CRUDOperations CentralOperationsFor(object shardingID)
    {
      CheckDaemonActive();
      return m_CentralArea.ShardedOperationsFor(shardingID);
    }
    #endregion

    #region Protected

    public static string GetGdidScopePrefix(string schemaName, string bankName)
    {
        return "{0}.{1}.".Args(schemaName, bankName);
    }


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node==null || !node.Exists) return;


      m_CentralArea = null;
      m_Areas.Clear();

      var nodes = node.Children.Where( cn => cn.IsSameName(MdbArea.CONFIG_AREA_SECTION) && (cn.IsSameNameAttr(MdbArea.CENTRAL_AREA_NAME)));
      if (!nodes.Any() || nodes.Count()>1)
        throw new MdbException(StringConsts.MDB_STORE_CONFIG_MANY_CENTRAL_ERROR);

      var centralNode = nodes.First();
      m_CentralArea = new MDBCentralArea(this, centralNode);

      m_Areas.Register( m_CentralArea );//central area gets registered anyway

      //non-central - they are not required
      nodes = node.Children.Where( cn => cn.IsSameName(MdbArea.CONFIG_AREA_SECTION) && (!cn.IsSameNameAttr(MdbArea.CENTRAL_AREA_NAME)));
      foreach(var anode in nodes)
      {
        var area = new MdbPartitionedArea(this, anode);
        m_Areas.Register( area );
      }
    }

    protected override void DoStart()
    {
      if (m_TargetName.IsNullOrWhiteSpace())
        throw new MdbException(StringConsts.MDB_STORE_CONFIG_NO_TARGET_NAME_ERROR);

      if (m_Areas.Count==0)
        throw new MdbException(StringConsts.MDB_STORE_CONFIG_NO_AREAS_ERROR);


      try
      {
        GdidAuthorityService.CheckNameValidity(m_SchemaName);
        GdidAuthorityService.CheckNameValidity(m_BankName);

        var gdidScope = GetGdidScopePrefix(m_SchemaName, m_BankName);
        m_GdidGenerator = new GdidGenerator(this, "GdidGen({0})".Args(gdidScope), gdidScope, null);
        if (SkySystem.IsMetabase)
        {
            foreach(var ah in SkySystem.Metabase.GDIDAuthorities)
            {
              m_GdidGenerator.AuthorityHosts.Register(ah);
              App.Log.Write( new Azos.Log.Message
              {
                  Type = Azos.Log.MessageType.InfoD,
                  Topic = SysConsts.LOG_TOPIC_MDB,
                  From = GetType().FullName+".makeGDIDGen()",
                  Text = "Registered GDID authority host: "+ah.ToString()
              });
            }
        }

      }
      catch(Exception error)
      {
        throw new MdbException(StringConsts.MDB_STORE_CONFIG_GDID_ERROR + error.ToMessageWithType());
      }

      try
      {
        m_Cache = new LocalCache(this, "MDBDataStore::"+Name);
        m_Cache.Pile = new DefaultPile(m_Cache, "MDBDataStore::Pile::"+Name);
        m_Cache.Configure(null);
        m_Cache.Start();
      }
      catch
      {
        try { DisposableObject.DisposeAndNull(ref m_GdidGenerator);} catch{}
        try { DisposableObject.DisposeAndNull(ref m_Cache); } catch {}
        throw;
      }
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      DisposableObject.DisposeAndNull(ref m_GdidGenerator);
      DisposableObject.DisposeAndNull(ref m_Cache);
    }

    #endregion

  }
}
