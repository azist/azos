/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.IO.FileSystem;

namespace Azos.Sky.Metabase{ public sealed partial class Metabank{

  /// <summary>
  /// Represents metadata for Zone
  /// </summary>
  public sealed class SectionZone : SectionRegionBase
  {
      internal SectionZone(SectionNOC noc, SectionZone parentZone, string name, string path,  FileSystemSession session)
               : base(noc.Catalog, (SectionRegionBase)parentZone ?? (SectionRegionBase)noc, name, path, session)
      {
        m_NOC = noc;
        m_ParentZone = parentZone;
      }

      private SectionNOC m_NOC;
      private SectionZone m_ParentZone;
      private Dictionary<int, IEnumerable<SectionHost>> m_ProcessorMap;

      public override string RootNodeName
      {
        get { return "zone"; }
      }

      /// <summary>
      /// Returns the NOC that this zone is under
      /// </summary>
      public SectionNOC NOC { get { return m_NOC;} }

      /// <summary>
      /// Returns parent zone for this zone or null if this zone is top-level under NOC
      /// </summary>
      public SectionZone ParentZone { get { return m_ParentZone;} }


      /// <summary>
      /// Returns region path to this zone
      /// </summary>
      public override string RegionPath
      {
        get
        {
          var parent = (m_ParentZone==null) ? (SectionRegionBase)m_NOC : (SectionRegionBase)m_ParentZone;
          return "{0}/{1}".Args( parent.RegionPath, Name);
        }
      }

      /// <summary>
      /// Returns names of child zones
      /// </summary>
      public IEnumerable<string> SubZoneNames
      {
        get
        {
          string CACHE_KEY = ("ZN.szn"+Path).ToLowerInvariant();
          //1. Try to get from cache
          var result = Metabank.cacheGet(REG_CATALOG, CACHE_KEY) as IEnumerable<string>;
          if (result!=null) return result;

          result =
            Metabank.fsAccess("Zone[{0}].SubZoneNames.get".Args(m_Name), Path,
                  (session, dir) => dir.SubDirectoryNames
                                       .Where(dn=>dn.EndsWith(RegCatalog.ZON_EXT))
                                       .Select(dn=>Metabank.chopExt(dn))
                                       .ToList()
            );

          Metabank.cachePut(REG_CATALOG, CACHE_KEY, result);
          return result;
        }
      }


      /// <summary>
      /// Gets names of hosts
      /// </summary>
      public IEnumerable<string> HostNames
      {
        get
        {
          string CACHE_KEY = ("ZN.hns"+Path).ToLowerInvariant();
          //1. Try to get from cache
          var result = Metabank.cacheGet(REG_CATALOG, CACHE_KEY) as IEnumerable<string>;
          if (result!=null) return result;

          result =
            Metabank.fsAccess("Zone[{0}].HostNames.get".Args(m_Name), Path,
                  (session, dir) => dir.SubDirectoryNames
                                       .Where(dn=>dn.EndsWith(RegCatalog.HST_EXT))
                                       .Select(dn=>Metabank.chopExt(dn))
                                       .ToList()
            );

          Metabank.cachePut(REG_CATALOG, CACHE_KEY, result);
          return result;
        }
      }


      /// <summary>
      /// Navigates to a named child zone or host. The search is done using case-insensitive comparison, however
      ///  the underlying file system may be case-sensitive and must be supplied the exact name
      /// </summary>
      public override SectionRegionBase this[string name]
      {
        get
        {
          if (name.IsNullOrWhiteSpace()) return null;

          name = name.Trim();

          //ignore the dynamic host name suffix (if any) at the end
          var ic = name.LastIndexOf(HOST_DYNAMIC_SUFFIX_SEPARATOR);
          if (ic>0) name = name.Substring(0, ic).Trim();



          if (name.EndsWith(RegCatalog.ZON_EXT))
          {
            name = name.Substring(0, name.LastIndexOf(RegCatalog.ZON_EXT));
            if (SubZoneNames.Any(srn=>INVSTRCMP.Equals(srn, name)))
              return GetSubZone(name);
            return null;
          }


          if (name.EndsWith(RegCatalog.HST_EXT))
          {
            name = name.Substring(0, name.LastIndexOf(RegCatalog.HST_EXT));
            if (HostNames.Any(srn=>INVSTRCMP.Equals(srn, name)))
              return GetHost(name);
            return null;
          }


          if (SubZoneNames.Any(srn=>INVSTRCMP.Equals(srn, name)))
            return GetSubZone(name);

          if (HostNames.Any(srn=>INVSTRCMP.Equals(srn, name)))
            return GetHost(name);

          return null;
        }
      }

      /// <summary>
      /// Returns the first host that has Zone governor in its role, or null if there are no governors in the zone
      /// </summary>
      public SectionHost ZoneGovernorPrimaryHost
      {
        get { return this.ZoneGovernorHosts.FirstOrDefault(); }
      }

      /// <summary>
      /// Returns zone governor host/s in order of their names, primary host returned first.
      /// Returns empty enumeration if there are no zgov hosts
      /// </summary>
      public IEnumerable<SectionHost> ZoneGovernorHosts
      {
        get
        {
          return this.HostNames
                     .Select(hn => this.GetHost(hn))
                     .Where(host => host.IsZGov)
                     .OrderBy( host => host.Name);
        }
      }

      /// <summary>
      /// Returns a mapping of ProcessorID to process controller host pair {Primary, Secondary}
      /// </summary>
      public IDictionary<int, IEnumerable<SectionHost>> ProcessorMap
      {
        get
        {
          if (m_ProcessorMap == null)
          {
            var hostNodes = LevelConfig[CONFIG_PROCESSOR_SET_SECTION].Children.Where(cn => cn.IsSameName(CONFIG_PROCESSOR_HOST_SECTION));

            m_ProcessorMap = new Dictionary<int, IEnumerable<SectionHost>>(hostNodes.Count());

            foreach (var hn in hostNodes)
            {
              var id = hn.AttrByName(CONFIG_PROCESSOR_HOST_ID_ATTR).ValueAsNullableInt(null);
              if (!id.HasValue)
                throw new MetabaseException(StringConsts.METABASE_PROCESSOR_SET_MISSING_ATTRIBUTE_ERROR.Args(CONFIG_PROCESSOR_HOST_ID_ATTR));

              var primaryHostPath = RegCatalog.JoinPathSegments(RegionPath, hn.AttrByName(CONFIG_PROCESSOR_HOST_PRIMARY_PATH_ATTR).Value);
              var primaryHost = Catalog.NavigateHost(primaryHostPath);
              if (!primaryHost.IsProcessHost)
                throw new MetabaseException(StringConsts.METABASE_PROCESSOR_SET_HOST_IS_NOT_PROCESSOR_HOST_ERROR.Args(id.Value));

              var seondaryHostPath = RegCatalog.JoinPathSegments(RegionPath, hn.AttrByName(CONFIG_PROCESSOR_HOST_SECONDARY_PATH_ATTR).Value);
              var secondaryHost = Catalog.NavigateHost(seondaryHostPath);
              if (!secondaryHost.IsProcessHost)
                throw new MetabaseException(StringConsts.METABASE_PROCESSOR_SET_HOST_IS_NOT_PROCESSOR_HOST_ERROR.Args(id.Value));

              if (m_ProcessorMap.ContainsKey(id.Value))
                throw new MetabaseException(StringConsts.METABASE_PROCESSOR_SET_DUPLICATE_ATTRIBUTE_ERROR.Args(CONFIG_PROCESSOR_HOST_ID_ATTR));
              m_ProcessorMap.Add(id.Value, new[] { primaryHost, secondaryHost });
            }
          }
          return m_ProcessorMap;
        }
      }

      /// <summary>
      /// Maps sharding ID to processor ID. This method is used to map process unqiue mutex id, or GDID to processor ID - which denotes a
      /// set of actual executor hosts
      /// </summary>
      public int MapShardingKeyToProcessorID(string shardingKey)
      {
        var ids = ProcessorMap.Keys.ToArray();
        if (ids.Length == 0) throw new MetabaseException("TODO: MapShardingKeyToProcessorID(ProcessorMap is empty)");
        var hash = Mdb.ShardingUtils.StringToShardingID(shardingKey);
        var idx = hash % (ulong)ids.Length;
        return ids[idx];
      }

      /// <summary>
      /// Tries to map processor id to {Primary, Secondary} host pair or null if there is no mapping
      /// </summary>
      public IEnumerable<SectionHost> TryGetProcessorHostsByID(int id)
      {
        var dict = ProcessorMap;
        IEnumerable<SectionHost> result;
        if (dict.TryGetValue(id, out result)) return result;
        return null;
      }

      /// <summary>
      /// Maps map processor id to {Primary, Secondary} host pair or throws
      /// </summary>
      public IEnumerable<SectionHost> GetProcessorHostsByID(int id)
      {
        var hosts = TryGetProcessorHostsByID(id);

        if (hosts == null)
          throw new MetabaseException(StringConsts.METABASE_ZONE_COULD_NOT_FIND_PROCESSOR_HOST_ERROR.Args(RegionPath, id));

        return hosts;
      }

      /// <summary>
      /// Returns true if this zone has direct or indirect parent zone governor above it, optionally examining higher-level NOCs.
      /// If iAmZoneGovernor is true then this zone is skipped as the zone gov of this zone gov is not itself.
      /// </summary>
      public bool HasDirectOrIndirectParentZoneGovernor(string zgovHost, bool? iAmZoneGovernor = null, bool transcendNOC = false)
      {
        if (zgovHost.IsNullOrWhiteSpace()) return false;
        return FindNearestParentZoneGovernors(iAmZoneGovernor, (sh) => SkyExtensions.IsSameRegionPath( sh.RegionPath, zgovHost), transcendNOC).Any();
      }

      /// <summary>
      /// Returns true if this zone has direct or indirect parent zone governor above it, optionally examining higher-level NOCs.
      /// If iAmZoneGovernor is true then this zone is skipped as the zone gov of this zone gov is not itself.
      /// </summary>
      public bool HasDirectOrIndirectParentZoneGovernor(SectionHost zgovHost, bool? iAmZoneGovernor = null, bool transcendNOC = false)
      {
        if (zgovHost==null) return false;
        return FindNearestParentZoneGovernors(iAmZoneGovernor, (sh) => sh.IsLogicallyTheSame( zgovHost ), transcendNOC).Any();
      }


      /// <summary>
      /// Tries to find hosts that run zone governors in this zone or parent zone chain, optionally looking in the higher-level NOCs.
      /// If iAmZoneGovernor is true then this zone is skipped as the zone gov of this zone gov is not itself.
      /// Pass filter lambda to filter-out-unneeded hosts in the chain. Returns empty enumeration for no matches
      /// </summary>
      public IEnumerable<SectionHost> FindNearestParentZoneGovernors(bool? iAmZoneGovernor = null, Func<SectionHost, bool> filter = null,  bool transcendNOC = false)
      {
          if (!iAmZoneGovernor.HasValue)
           iAmZoneGovernor = Apps.BootConfLoader.SystemApplicationType == Apps.SystemApplicationType.ZoneGovernor;

          var zone = this;

          if (iAmZoneGovernor.Value)
          {
            zone = zone.ParentZone;
          }

          while(zone!=null)
          {
             var zgovs = zone.ZoneGovernorHosts;
             if (filter==null ? zgovs.Any() : zgovs.Any(filter)) return zgovs;
             zone = zone.ParentZone;
          }

          if (!transcendNOC) return Enumerable.Empty<SectionHost>(); //not found in this NOC

          SectionNOC noc = this.NOC;

          while(true)
          {
            zone = noc.ParentNOCZone;
            if (zone==null) return Enumerable.Empty<SectionHost>();//no parent NOC or no NOC higher (already at the top)

            noc = zone.NOC;

            while(zone!=null)
            {
               var zgovs = zone.ZoneGovernorHosts;
               if (filter==null ? zgovs.Any() : zgovs.Any(filter)) return zgovs;
               zone = zone.ParentZone;
            }
          }
      }

      /// <summary>
      /// Gets sub-zone by name
      /// </summary>
      public SectionZone GetSubZone(string name)
      {
        string CACHE_KEY = ("ZN.sz"+Path+name).ToLowerInvariant();
        //1. Try to get from cache
        var result = Metabank.cacheGet(REG_CATALOG, CACHE_KEY) as SectionZone;
        if (result!=null) return result;

        var rdir = "{0}{1}".Args(name, RegCatalog.ZON_EXT);
        var rpath = Metabank.JoinPaths(Path, rdir);
        result =
            Metabank.fsAccess("Zone[{0}].GetSubZone({1})".Args(m_Name, name), rpath,
                  (session, dir) => new SectionZone(m_NOC, this, name, rpath, session)
            );


        Metabank.cachePut(REG_CATALOG, CACHE_KEY, result);
        return result;
      }

      /// <summary>
      /// Gets host by name
      /// </summary>
      public SectionHost GetHost(string name)
      {
        string CACHE_KEY = ("ZN.h"+Path+name).ToLowerInvariant();
        //1. Try to get from cache
        var result = Metabank.cacheGet(REG_CATALOG, CACHE_KEY) as SectionHost;
        if (result!=null) return result;

        var rdir = "{0}{1}".Args(name, RegCatalog.HST_EXT);
        var rpath = Metabank.JoinPaths(Path, rdir);
        result =
            Metabank.fsAccess("Zone[{0}].GetHost({1})".Args(m_Name, name), rpath,
                  (session, dir) => new SectionHost(this, name, rpath, session)
            );


        Metabank.cachePut(REG_CATALOG, CACHE_KEY, result);
        return result;
      }


      public override void Validate(ValidationContext ctx)
      {
        var output = ctx.Output;

        base.Validate(ctx);

        foreach(var szone in this.SubZoneNames)
            this.GetSubZone(szone).Validate(ctx);

        foreach(var shost in this.HostNames)
            this.GetHost(shost).Validate(ctx);

        var processorMap = ProcessorMap;

        //check Zone governor locking
        var zgovs = this
                      .ZoneGovernorHosts
                      .OrderBy( host => host.Name)
                      .ToArray();

        if (zgovs.Length>0)
        {

            var primaryLockZgovs = this
                              .ZoneGovernorHosts.Where(host => !host.IsZGovLockFailover)
                              .OrderBy( host => host.Name)
                              .ToArray();

            if (primaryLockZgovs.Length<1)
              output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, "No primary ZGov locking nodes defined") );

            var failoverLockZgovs = this
                                .ZoneGovernorHosts.Where(host => host.IsZGovLockFailover)
                                .OrderBy( host => host.Name)
                                .ToArray();

            if (failoverLockZgovs.Length>0 && primaryLockZgovs.Length!=failoverLockZgovs.Length)
              output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, "The number of primary ZGov locking nodes does not equal to the number of failover nodes") );
        }
      }



  }



}}
