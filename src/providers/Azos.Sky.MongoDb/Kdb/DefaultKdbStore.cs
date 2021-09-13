/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Data.Access;
using Azos.Data;
using Azos.Log;
using Azos.Instrumentation;
using Azos.Serialization.BSON;

using Azos.Sky.MongoDb;

namespace Azos.Sky.Kdb
{
  /// <summary>
  /// Provides default implementation of IKdbDataStore
  /// </summary>
  public sealed partial class DefaultKdbStore : DaemonWithInstrumentation<IApplicationComponent>, IKdbDataStoreImplementation
  {
    private static readonly TimeSpan INSTR_INTERVAL = TimeSpan.FromMilliseconds(3250);
    public const string Kdb_TARGET = "kdb";

    #region .ctor
    public DefaultKdbStore(IApplication app) : base(app) => ctor(null);
    public DefaultKdbStore(IApplicationComponent director) : base(director) => ctor(null);
    public DefaultKdbStore(IApplication app, string name) : base(app) => ctor(name);
    public DefaultKdbStore(IApplicationComponent director, string name) : base(director) => ctor(name);

    private void ctor(string name)
    {
      Name = name.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString() : name;
      m_Converter = new DataDocConverter();
    }

    protected override void Destructor()
    {
      DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
      base.Destructor();
    }
    #endregion

    #region Fields
    private ShardSet m_RootShardSet;
    internal DataDocConverter m_Converter;
    private bool m_InstrumentationEnabled;
    private Time.Event m_InstrumentationEvent;
    private int m_DefaultTimeoutMs;

    private NamedInterlocked m_stat_GetHitCount = new NamedInterlocked();
    private NamedInterlocked m_stat_GetFallbackHitCount = new NamedInterlocked();
    private NamedInterlocked m_stat_GetMissCount = new NamedInterlocked();
    private NamedInterlocked m_stat_GetTouchCount = new NamedInterlocked();
    private NamedInterlocked m_stat_PutCount = new NamedInterlocked();
    private NamedInterlocked m_stat_DeleteHitCount = new NamedInterlocked();
    private NamedInterlocked m_stat_DeleteMissCount = new NamedInterlocked();
    private NamedInterlocked m_stat_DeleteFallbackCount = new NamedInterlocked();
    private NamedInterlocked m_stat_ErrorCount = new NamedInterlocked();
    private NamedInterlocked m_stat_MigrationCount = new NamedInterlocked();
    #endregion

    #region Props
    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_KDB;

    /// <summary>
    /// Provides default timeout imposed on execution of commands/calls. Expressed in milliseconds.
    /// A value less or equal to zero indicates no timeout
    /// </summary>
    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int DefaultTimeoutMs
    {
      get => m_DefaultTimeoutMs;
      set => m_DefaultTimeoutMs = value.KeepBetween(0, (15 * 60) * 1000);
    }

    /// <summary>
    /// Implements IInstrumentable
    /// </summary>
    [Config(Default=false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled;}
      set
      {
        m_InstrumentationEnabled = value;

        if (m_InstrumentationEvent==null)
        {
          if (!value) return;

          resetStats();
          m_InstrumentationEvent = new Time.Event(App.EventTimer, null, e => AcceptManagerVisit(this, e.LocalizedTime), INSTR_INTERVAL);
        }
        else
        {
          if (value) return;
          DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
        }
      }
    }

    public string TargetName{ get { return Kdb_TARGET; } }

    StoreLogLevel IDataStoreImplementation.DataLogLevel
    {
      get { return this.ComponentEffectiveLogLevel >= MessageType.Trace ? StoreLogLevel.Trace : StoreLogLevel.Debug; }
      set {}
    }

    public ShardSet RootShardSet { get { return m_RootShardSet; } }
    #endregion

    #region Public
      public void TestConnection()
      {

      }


      public Doc Get(string table, byte[] key)
      {
        var result = this.Get<Doc>(table, key, false);
        return result.IsAssigned ? result.Value : null;
      }

      public KdbRecord<TRow> Get<TRow>(string table, byte[] key, bool dontToch = false) where TRow : Doc
      {
        Kdb.KdbConstraints.CheckTableName(table, "Get");
        Kdb.KdbConstraints.CheckKey(key, "Get");

        var set = m_RootShardSet;
        while(set != null)
        {
          var shard = set.GetShardForKey(key);
          var result = shard.Get<TRow>(table, key);
          if (result.IsAssigned)
          {
            if (set.FallbackLevel == 0) incCtr(m_stat_GetHitCount, table);

            if (set.FallbackLevel > 0)
            {
              incCtr(m_stat_GetFallbackHitCount, table);
              try
              {
                incCtr(m_stat_MigrationCount, table);
                this.Put(table, key, result.Value, result.SlidingExpirationDays, result.AbsoluteExpirationDateUTC);
              }
              catch (Exception error)
              {
                incCtr(m_stat_ErrorCount, table);
                Log(MessageType.Error, "Get().Put(RootShardSet)", error.ToMessageWithType(), error);
              }
            }
            else if (!dontToch && ((App.TimeSource.UTCNow - result.LastUseDate).TotalDays > 1))
            {
              touchOnGet(shard, table, key);
            }
            return result;
          }

          set = set.Fallback;
        }
        incCtr(m_stat_GetMissCount, table);

        return KdbRecord<TRow>.Unassigned;
      }

      public KdbRecord<byte[]> GetRaw(string table, byte[] key, bool dontToch = false)
      {
        Kdb.KdbConstraints.CheckTableName(table, "Get");
        Kdb.KdbConstraints.CheckKey(key, "Get");

        var set = m_RootShardSet;
        while(set != null)
        {
          var shard = set.GetShardForKey(key);
          var result = shard.GetRaw(table, key);
          if (result.IsAssigned)
          {
            if (set.FallbackLevel == 0) incCtr(m_stat_GetHitCount, table);

            if (set.FallbackLevel > 0)
            {
              incCtr(m_stat_GetFallbackHitCount, table);
              try
              {
                incCtr(m_stat_MigrationCount, table);
                this.PutRaw(table, key, result.Value, result.SlidingExpirationDays, result.AbsoluteExpirationDateUTC);
              }
              catch (Exception error)
              {
                incCtr(m_stat_ErrorCount, table);
                Log(MessageType.Error, "Get().PutRaw(RootShardSet)", error.ToMessageWithType(), error);
              }
            }
            else if (!dontToch && ((App.TimeSource.UTCNow - result.LastUseDate).TotalDays > 1))
            {
              touchOnGet(shard, table, key);
            }
            return result;
          }

          set = set.Fallback;
        }
        incCtr(m_stat_GetMissCount, table);

        return KdbRecord<byte[]>.Unassigned;
      }

      public void Put(string table, byte[] key, Doc value, int slidingExpirationDays = -1, DateTime? absoluteExpirationDateUtc = null)
      {
        if (value == null) throw new KdbException(StringConsts.ARGUMENT_ERROR + "DefaultKdbStore.Put(value=null)");
        Kdb.KdbConstraints.CheckTableName(table, "Put");
        Kdb.KdbConstraints.CheckKey(key, "Put");

        var shard = m_RootShardSet.GetShardForKey(key);
        shard.Put(table, key, value, slidingExpirationDays, absoluteExpirationDateUtc);

        incCtr(m_stat_PutCount, table);
      }

      public void PutRaw(string table, byte[] key, byte[] value, int slidingExpirationDays = -1, DateTime? absoluteExpirationDateUtc = null)
      {
        if (value == null) throw new KdbException(StringConsts.ARGUMENT_ERROR + "DefaultKdbStore.PutRaw(value=null)");
        Kdb.KdbConstraints.CheckTableName(table, "Put");
        Kdb.KdbConstraints.CheckKey(key, "Put");

        var shard = m_RootShardSet.GetShardForKey(key);
        shard.PutRaw(table, key, value, slidingExpirationDays, absoluteExpirationDateUtc);

        incCtr(m_stat_PutCount, table);
      }

      public bool Delete(string table, byte[] key)
      {
        Kdb.KdbConstraints.CheckTableName(table, "Delete");
        Kdb.KdbConstraints.CheckKey(key, "Delete");

        var result = false;
        var set = RootShardSet;
        while(set != null)
        {
          var shard = set.GetShardForKey(key);
          if (shard.Delete(table, key))
          {
            result = true;
            if (set.FallbackLevel == 0) incCtr(m_stat_DeleteHitCount, table);
            else incCtr(m_stat_DeleteFallbackCount, table);
          }
          set = set.Fallback;
        }
        if (!result) incCtr(m_stat_DeleteMissCount, table);

        return result;
      }
    #endregion

    #region Protected
      protected override void DoConfigure(IConfigSectionNode node)
      {
        base.DoConfigure(node);
        if (node == null || !node.Exists) return;

        m_RootShardSet = new ShardSet(this, node);
      }

      protected override void DoStart()
      {
        if (m_RootShardSet == null)
          throw new KdbException(StringConsts.KDB_STORE_ROOT_SHARDSET_NOT_CONFIGURED);
      }

      protected override void DoWaitForCompleteStop()
      {
        base.DoWaitForCompleteStop();
      }

      protected override void DoAcceptManagerVisit(Object manager,DateTime managerNow)
      {
        base.DoAcceptManagerVisit(manager,managerNow);
        if (!m_InstrumentationEnabled) return ;

        var instr = App.Instrumentation;
        if (!instr.Enabled) return;

        foreach(var elm in m_stat_GetHitCount.SnapshotAllLongs(0))         instr.Record(new Instrumentation.GetHitCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_GetFallbackHitCount.SnapshotAllLongs(0)) instr.Record(new Instrumentation.GetFallbackHitCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_GetMissCount.SnapshotAllLongs(0))        instr.Record(new Instrumentation.GetMissCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_GetTouchCount.SnapshotAllLongs(0))       instr.Record(new Instrumentation.GetTouchCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_PutCount.SnapshotAllLongs(0))            instr.Record(new Instrumentation.PutCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_DeleteHitCount.SnapshotAllLongs(0))      instr.Record(new Instrumentation.DeleteHitCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_DeleteFallbackCount.SnapshotAllLongs(0)) instr.Record(new Instrumentation.DeleteFallbackCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_DeleteMissCount.SnapshotAllLongs(0))     instr.Record(new Instrumentation.DeleteMissCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_ErrorCount.SnapshotAllLongs(0))          instr.Record(new Instrumentation.ErrorCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_MigrationCount.SnapshotAllLongs(0))      instr.Record(new Instrumentation.MigrationCount(elm.Key, elm.Value));
      }

      internal void Log(MessageType tp, string from, string text, Exception error = null, Guid? related = null)
      {
        if (tp < ComponentEffectiveLogLevel) return;
        App.Log.Write(new Message
        {
          Type = tp,
          Topic = SysConsts.LOG_TOPIC_KDB,
          From = "{0}.{1}".Args(GetType().Name, from),
          Text = text,
          Exception = error,
          RelatedTo = related ?? Guid.Empty
        });
      }
    #endregion

    #region .pvt
      private int m_TouchCount;
      private void touchOnGet(Shard shard, string table, byte[] key)
      {
        var cnt = Interlocked.Increment(ref m_TouchCount);
        if (cnt > 64)
          touchOnGetCore(shard, table, key);
        else
          Task.Run(()=> { touchOnGetCore(shard, table, key); });
      }

      private void touchOnGetCore(Shard shard, string table, byte[] key)
      {
        try
        {
          incCtr(m_stat_GetTouchCount, table);
          shard.Touch(table, key);
        }
        catch(Exception error)
        {
          incCtr(m_stat_ErrorCount, table);
          Log(MessageType.Error, "touchOnGetCore()", error.ToMessageWithType(), error);
        }
        finally
        {
          Interlocked.Decrement(ref m_TouchCount);
        }
      }

      private void incCtr(NamedInterlocked counter, string table)
      {
        if (!m_InstrumentationEnabled) return;

        counter.IncrementLong(Datum.UNSPECIFIED_SOURCE);
        if (table != null)
          counter.IncrementLong(table);
      }

      private void resetStats()
      {
        m_stat_GetHitCount.Clear();
        m_stat_GetFallbackHitCount.Clear();
        m_stat_GetMissCount.Clear();
        m_stat_GetTouchCount.Clear();
        m_stat_PutCount.Clear();
        m_stat_DeleteHitCount.Clear();
        m_stat_DeleteMissCount.Clear();
        m_stat_DeleteFallbackCount.Clear();
        m_stat_ErrorCount.Clear();
        m_stat_MigrationCount.Clear();
      }
    #endregion
  }
}
