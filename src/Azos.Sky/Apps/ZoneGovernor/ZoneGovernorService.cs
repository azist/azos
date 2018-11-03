/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;
using Azos.Collections;
using Azos.Instrumentation;

using Azos.Sky.Metabase;

namespace Azos.Sky.Apps.ZoneGovernor
{

  /// <summary>
  /// Provides Zone Governor Services - this is a singleton class
  /// </summary>
  public sealed class ZoneGovernorService : Service
  {
    #region CONSTS
       public const string THREAD_NAME = "ZoneGovernorService";
       public const int THREAD_GRANULARITY_MS = 3120;

       public const string CONFIG_ZONE_GOVERNOR_SECTION = "zone-governor";

       public const string CONFIG_SUB_INSTRUMENTATION_SECTION = "sub-instrumentation";
       public const string CONFIG_SUB_LOG_SECTION = "sub-log";

       public const string CONFIG_REDUCE_DETAIL_SECTION = "reduce-detail";
       public const string CONFIG_TYPE_SECTION = "type";
       public const string CONFIG_LEVEL_ATTR = "level";

       public const int SUB_HOST_MAX_AGE_SEC = 7/*min*/ * 60;

    #endregion

    #region Static
      private static object s_InstanceLock = new object();
      private static volatile ZoneGovernorService s_Instance;


      /// <summary>
      /// Returns true to indicate that this process has zone governor instance
      /// </summary>
      public static bool IsZoneGovernor
      {
        get { return s_Instance!=null; }
      }

      /// <summary>
      /// Returns singleton instance or throws if service has not been allocated yet
      /// </summary>
      public static ZoneGovernorService Instance
      {
        get
        {
          var instance = s_Instance;
          if (instance==null)
            throw new AZGOVException(StringConsts.AZGOV_INSTANCE_NOT_ALLOCATED_ERROR);

          return instance;
        }
      }
    #endregion

    #region .ctor/.dctor
    /// <summary>
    /// Creates a singleton instance or throws if instance is already created
    /// </summary>
    public ZoneGovernorService() : base(null)
      {
        if (!SkySystem.IsMetabase)
           throw new AZGOVException(StringConsts.METABASE_NOT_AVAILABLE_ERROR.Args(GetType().FullName+".ctor()"));

        lock(s_InstanceLock)
        {
          if (s_Instance!=null)
            throw new AZGOVException(StringConsts.AZGOV_INSTANCE_ALREADY_ALLOCATED_ERROR);

          m_SubInstr = new InstrumentationService(this);
          m_SubInstrReductionLevels = new Dictionary<string,int>();
          m_SubInstrCallers = new ConcurrentDictionary<string, DateTime>();

          m_SubLog = new LogService(this);

          m_SubHosts = new Registry<Contracts.HostInfo>();
          m_DynamicHostSlots = new Registry<Contracts.DynamicHostInfo>();

          m_Locker = new Locking.Server.LockServerService( this );

          s_Instance = this;
        }
      }

      protected override void Destructor()
      {
        lock(s_InstanceLock)
        {
          base.Destructor();
          s_Instance = null;

          m_Locker.Dispose();

          var sis = m_SubInstr;
          if (sis!=null)
          {
            m_SubInstr = null;
            sis.Dispose();
          }

          var slg = m_SubLog;
          if (slg != null)
          {
            m_SubLog = null;
            slg.Dispose();
          }
        }
      }

    #endregion

    #region Fields

      private InstrumentationService m_SubInstr;
      private Dictionary<string, int> m_SubInstrReductionLevels;
      private ConcurrentDictionary<string, DateTime> m_SubInstrCallers;
      private int m_SubInstrCallerCount;

      private LogService m_SubLog;

      private Registry<Contracts.HostInfo> m_SubHosts;
      private Registry<Contracts.DynamicHostInfo> m_DynamicHostSlots;

      private double m_CPULoadFactor = 1d;

      private Locking.Server.LockServerService m_Locker;

      private Thread m_Thread;
      private AutoResetEvent m_WaitEvent;

    #endregion

    #region Properties

      public override string ComponentCommonName { get { return "zgov"; }}

      /// <summary>
      /// A form of throttling.
      /// Returns the coefficient 0.0 .. 1.0 that stipulates how much more work/load the node is ready to take.
      /// The higher the CPU usage, the lower this number gets (closer to 0.0)
      /// </summary>
      public double CPULoadFactor { get { return m_CPULoadFactor;}}

      /// <summary>
      /// Returns the number of active subordinate telemetry uploaders -
      /// nodes that upload telemetry to this Zone Governor
      /// </summary>
      public int SubordinateInstrumentationCallerCount { get{ return m_SubInstrCallerCount; }}


      /// <summary>
      /// Returns instrumentation as reported by subordinate telemetry callers
      /// </summary>
      public IInstrumentation SubordinateInstrumentation   {   get { return m_SubInstr; }  }

      /// <summary>
      /// Returns locking server that this zgov hosts
      /// </summary>
      public Contracts.ILocker Locker { get { return m_Locker; } }
    #endregion

    #region Public
      /// <summary>
      /// Called by subordinate nodes to report telemetry
      /// </summary>
      public int SendTelemetry(string host, Azos.Instrumentation.Datum[] data)
      {
        if (!Running || data==null) return 0;
        var sis = m_SubInstr;
        if (sis==null) return 0;

        for(var i=0; i<data.Length; i++)
        {
          var datum = data[i];
          var tpn = datum.GetType().FullName;
          int reductionLevel;

          if (!m_SubInstrReductionLevels.TryGetValue(tpn, out reductionLevel)) reductionLevel = -1;

          datum.ReduceSourceDetail(reductionLevel);
          sis.Record(datum);
        }


        if (host==null) host = string.Empty;

        m_SubInstrCallers[host] = App.TimeSource.UTCNow;

        //How many slots are available in memory
        var freeSpace = sis.MaxRecordCount - sis.RecordCount;
        var freePerClient = freeSpace / m_SubInstrCallerCount;

        var canTake = freePerClient * m_CPULoadFactor;

        return (int)canTake;
      }

      /// <summary>
      /// Called by subordinate nodes to report log
      /// </summary>
      public int SendLog(string host, string appName, Azos.Log.Message[] data)
      {
        if (!Running || data == null) return 0;
        var slg = m_SubLog;
        if (slg == null) return 0;

        foreach (var msg in data)
        {
          if (appName.IsNotNullOrWhiteSpace())
            msg.From = appName + "::" + msg.From;
          slg.Write(msg);
        }

        return (int)(Log.SkyZoneSink.MAX_BUF_SIZE * m_CPULoadFactor);
      }

      /// <summary>
      /// Returns log messages from cyclical subordinate instrumentation buffer.
      /// Please note that in order to use this property subordinate instrumentation service
      /// must have its InstrumentationEnabled property set to true
      /// </summary>
      public IEnumerable<Message> GetSubordinateInstrumentationLogBuffer(bool asc)
      {
        return m_SubLog.GetInstrumentationBuffer(asc);
      }

      /// <summary>
      /// Registers /updates existing subordinate host information. This method implements IZoneHostRegistry contract
      /// </summary>
      public void RegisterSubordinateHost(Contracts.HostInfo host, Contracts.DynamicHostID? hid)
      {
        if (!Running || m_SubHosts==null || host==null) return;

        registerSubordinateHost(host, hid);

        var zHosts = SkySystem.HostMetabaseSection.ParentZone.ZoneGovernorHosts.Where(hh => !SkySystem.HostName.IsSameRegionPath(hh.RegionPath));
        foreach (var z in zHosts)
          using (var cl = Contracts.ServiceClientHub.New<Contracts.IZoneHostReplicatorClient>(z))
            cl.Async_PostHostInfo(host, hid);
      }

      /// <summary>
      /// Returns registred subordinate hosts, optionally taking search pattern. This method implements IZoneHostRegistry contract.
      /// Match pattern can contain up to one * wildcard and multiple ? wildcards
      /// </summary>
      public IEnumerable<Contracts.HostInfo> GetSubordinateHosts(string hostNameSearchPattern)
      {
        if (!Running || m_SubHosts==null) return Enumerable.Empty<Contracts.HostInfo>();

        var matches = hostNameSearchPattern.IsNullOrWhiteSpace()
                    ? m_SubHosts
                    : m_SubHosts.Where(h => Text.Utils.MatchPattern(h.Name, hostNameSearchPattern, senseCase: false));

        return matches.ToArray();
      }

      /// <summary>
      /// Returns information for specified subordinate host or null
      /// </summary>
      public Contracts.HostInfo GetSubordinateHost(string hostName)
      {
        if (!Running || m_SubHosts==null || hostName.IsNullOrWhiteSpace()) return null;

        //safeguard trim around dynamic name
        var i = hostName.IndexOf(Metabank.HOST_DYNAMIC_SUFFIX_SEPARATOR);
        if (i>0 && i<hostName.Length-1)
        {
          hostName = hostName.Substring(0, i).Trim()
                   + Metabank.HOST_DYNAMIC_SUFFIX_SEPARATOR
                   + hostName.Substring(i+1).Trim();
        }

        return m_SubHosts[hostName];
      }

      public Contracts.DynamicHostID Spawn(string hostPath, string id)
      {
        var zone = SkySystem.HostMetabaseSection.ParentZone;
        if (!Running || m_DynamicHostSlots == null) return new Contracts.DynamicHostID(null, zone.RegionPath);

        // TODO: Check zone
        var host = SkySystem.Metabase.CatalogReg.NavigateHost(hostPath) as Metabank.SectionHost;
        if (!host.Dynamic) throw new AZGOVException("TODO: Host '0' is not dynamic".Args(hostPath));

        if (id == null) id = SkySystem.GdidProvider.GenerateOneGdid(SysConsts.GDID_NS_DYNAMIC_HOST, SysConsts.GDID_NAME_DYNAMIC_HOST).ToString();

        var hid = new Contracts.DynamicHostID(id, zone.RegionPath);

        var dhi = m_DynamicHostSlots[id];
        if (dhi == null)
        {
          var stamp = App.TimeSource.UTCNow;

          dhi = new Contracts.DynamicHostInfo(id);
          dhi.Stamp = stamp;
          dhi.Owner = SkySystem.HostName;
          dhi.Votes = 1;
          m_DynamicHostSlots.Register(dhi);

          var hosts = zone.ZoneGovernorHosts.Where(hh => !SkySystem.HostName.IsSameRegionPath(hh.RegionPath));
          foreach (var h in hosts)
            using (var cl = Contracts.ServiceClientHub.New<Contracts.IZoneHostReplicatorClient>(h))
              cl.Async_PostDynamicHostInfo(hid, dhi.Stamp, dhi.Owner, dhi.Votes);
        }
        return hid;
      }

      public void PostDynamicHostInfo(Contracts.DynamicHostID hid, DateTime stamp, string owner, int votes)
      {
        if (!Running || m_DynamicHostSlots == null) return;
        var zone = SkySystem.HostMetabaseSection.ParentZone;

        // TODO: Check zone
        var dhi = m_DynamicHostSlots[hid.ID];
        var post = false;
        if (dhi == null)
        {
          dhi = new Contracts.DynamicHostInfo(hid.ID);
          post = true;
          m_DynamicHostSlots.Register(dhi);
        }

        if (dhi.Stamp > stamp || post)
        {
          dhi.Stamp = stamp;
          dhi.Owner = owner;
          dhi.Votes = votes;
          post = true;
        }

        if (dhi.Stamp == stamp)
          dhi.Votes += 1;

        if (post)
        {
          var hosts = zone.ZoneGovernorHosts.Where(hh => !SkySystem.HostName.IsSameRegionPath(hh.RegionPath));
          foreach (var h in hosts)
            using (var cl = Contracts.ServiceClientHub.New<Contracts.IZoneHostReplicatorClient>(h))
              cl.Async_PostDynamicHostInfo(hid, dhi.Stamp, dhi.Owner, dhi.Votes);
        }
      }

      public void PostHostInfo(Contracts.HostInfo host, Contracts.DynamicHostID? hid)
      {
        if (!Running || m_SubHosts==null || host==null) return;

        registerSubordinateHost(host, hid);
      }

      public Contracts.DynamicHostInfo GetDynamicHostInfo(Contracts.DynamicHostID hid)
      {
        if (!Running || m_DynamicHostSlots == null) return null;

        var zone = SkySystem.HostMetabaseSection.ParentZone;

        // TODO: Check zone
        var dhi = m_DynamicHostSlots[hid.ID];
        if (dhi == null)
        {
          throw new NotImplementedException();
        }

        return dhi;
      }
    #endregion

    #region Protected

    protected override void DoConfigure(IConfigSectionNode node)
      {
        string ip = "--";
        try
        {
          if (node == null)
            node = App.ConfigRoot[CONFIG_ZONE_GOVERNOR_SECTION];

          base.DoConfigure(node);

          ip = "A";

          var siNode = node[CONFIG_SUB_INSTRUMENTATION_SECTION];
          m_SubInstr.Configure(siNode);

          ip = "B";

          m_SubInstrReductionLevels.Clear();
          foreach (var tn in siNode[CONFIG_REDUCE_DETAIL_SECTION].Children.Where(cn => cn.IsSameName(CONFIG_TYPE_SECTION)))
          {
            var tname = tn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
            if (tname.IsNullOrWhiteSpace()) continue;
            m_SubInstrReductionLevels[tname] = tn.AttrByName(CONFIG_LEVEL_ATTR).ValueAsInt();
          }

          ip = "C";

          var slNode = node[CONFIG_SUB_LOG_SECTION];
          m_SubLog.Configure(slNode);

          ip = "D";

          m_Locker.Configure(node[Locking.Server.LockServerService.CONFIG_LOCK_SERVER_SECTION]);

          ip = "E";

          log(MessageType.Info, ".DoConfigure()", "Configured OK. ip=" + ip);
        }
        catch (Exception error)
        {
          var msg = "Error after '{0}' during ZoneGovernorService configuration: {1}".Args(ip, error.ToMessageWithType());
          log(MessageType.CatastrophicError, ".DoConfigure()", msg, error);
          throw new AZGOVException(msg, error);
        }
      }

      protected override void DoStart()
      {
        try
        {
          m_CPULoadFactor = 1d;
          m_SubInstrCallerCount = 0;
          m_SubInstr.Start();

          m_SubLog.Start();

          m_Locker.Start();

          m_WaitEvent = new AutoResetEvent(false);

          m_Thread = new Thread(threadSpin);
          m_Thread.Name = THREAD_NAME;
          m_Thread.Start();
        }
        catch
        {
          if (m_Locker.Running) try { m_Locker.WaitForCompleteStop(); } catch { }
          if (m_SubLog.Running) try { m_SubLog.WaitForCompleteStop(); } catch { }
          if (m_SubInstr.Running) try { m_SubInstr.WaitForCompleteStop(); } catch { }
          AbortStart();
          throw;
        }
      }

      protected override void DoSignalStop()
      {
        m_SubInstr.SignalStop();
        m_SubLog.SignalStop();
        m_Locker.SignalStop();
      }

      protected override void DoWaitForCompleteStop()
      {
        m_WaitEvent.Set();

        m_Thread.Join();
        m_Thread = null;

        m_WaitEvent.Close();
        m_WaitEvent = null;

        m_SubInstr.WaitForCompleteStop();
        m_SubInstrCallers.Clear();
        m_SubInstrCallerCount = 0;

        m_SubLog.WaitForCompleteStop();

        m_Locker.WaitForCompleteStop();

        base.DoWaitForCompleteStop();
      }

    #endregion


    #region .pvt .impl

        private void threadSpin()
        {
            const string FROM = "threadSpin()";

            while (Running)
            {
                try
                {
                  var now = App.TimeSource.UTCNow;

                  updateCPULoadFactor();
                  updateTelemetryCallers(now);
                  purgeOldSubHosts(now);

                  m_WaitEvent.WaitOne(THREAD_GRANULARITY_MS);
                }
                catch(Exception error)
                {                                  //TODO   restart loop??????
                  log(MessageType.CatastrophicError, FROM, error.ToMessageWithType(), error);
                }
            }

        }

        private int m_CPU_1;
        private int m_CPU_2;
        private int m_CPU_3;

        private void updateCPULoadFactor()
        {
           var cpu = (Platform.Computer.CurrentProcessorUsagePct + m_CPU_1 + m_CPU_2 + m_CPU_3) / 4;
           m_CPU_3 = m_CPU_2;
           m_CPU_2 = m_CPU_1;
           m_CPU_1 = cpu;

           if (cpu<20)
             m_CPULoadFactor = 1.0d;
           else if (cpu<80)
             m_CPULoadFactor = 1.0d - ((double)cpu / 100.0d);
           else if (cpu<90)
             m_CPULoadFactor = 0.075;
           else
             m_CPULoadFactor = 0.0d;
        }

        private void purgeOldSubHosts(DateTime utcNow)
        {

          var hosts = m_SubHosts.Where(h => (utcNow - h.UTCTimeStamp).TotalSeconds > SUB_HOST_MAX_AGE_SEC ).ToArray();
          foreach(var host in hosts)
           m_SubHosts.Unregister( host );//expired
        }

        private void updateTelemetryCallers(DateTime utcNow)
        {
           const int MAX_ACTIVE_AGE_MSEC = 40000;

           var delete = new List<string>();

           foreach(var kvp in m_SubInstrCallers)
             if ((utcNow - kvp.Value).TotalMilliseconds > MAX_ACTIVE_AGE_MSEC) delete.Add(kvp.Key);

           DateTime dummy;
           foreach(var key in delete)
             m_SubInstrCallers.TryRemove(key, out dummy);

           m_SubInstrCallerCount = m_SubInstrCallers.Count();
        }

        private void registerSubordinateHost(Contracts.HostInfo host, Contracts.DynamicHostID? hid)
        {
          try
          {
            var shost = SkySystem.Metabase.CatalogReg.NavigateHost(host.Name);
            if (!shost.HasDirectOrIndirectParentZoneGovernor(SkySystem.HostMetabaseSection, iAmZoneGovernor: false, transcendNOC: false))
              throw new AZGOVException(StringConsts.AZGOV_REGISTER_SUBORDINATE_HOST_PARENT_ERROR.Args(SkySystem.HostName, host.Name));
          }
          catch (Exception error)
          {
            throw new AZGOVException(StringConsts.AZGOV_REGISTER_SUBORDINATE_HOST_ERROR.Args(host.Name, error.ToMessageWithType()), error);
          }

          m_SubHosts.RegisterOrReplace(host);
          if (hid.HasValue && m_DynamicHostSlots != null)
          {
            var slot = m_DynamicHostSlots[hid.Value.ID];
            if (slot != null) slot.Host = host.Name;
          }

          if (host.RandomSample.HasValue)
            Platform.RandomGenerator.Instance.FeedExternalEntropySample(host.RandomSample.Value);
        }

        internal void log(MessageType type, string from, string text, Exception error = null, Guid? related = null)
        {
           var msg = new Azos.Log.Message
              {
                 Type = type,
                 Topic = SysConsts.LOG_TOPIC_ZONE_MANAGEMENT,
                 From = "{0}.{1}".Args(GetType().FullName, from),
                 Text = text,
                 Exception = error
              };

              if (related.HasValue) msg.RelatedTo = related.Value;

           App.Log.Write( msg );
        }

    #endregion


  }
}
