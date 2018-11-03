/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Geometry;
using Azos.IO.FileSystem;

namespace Azos.Sky.Metabase{ public sealed partial class Metabank{

  /// <summary>
  /// Base class for all sections that are in region catalog
  /// </summary>
  public abstract class SectionRegionBase : SectionWithNamedAppConfigs<RegCatalog>
  {
      internal SectionRegionBase(RegCatalog catalog, SectionRegionBase parentSection, string name, string path,  FileSystemSession session) : base(catalog, name, path, session)
      {
        ParentSection = parentSection;

        var gc = this.LevelConfig.AttrByName(Metabank.CONFIG_GEO_CENTER_ATTR).Value;

        if (gc.IsNotNullOrWhiteSpace())
         try
         {
           GeoCenter = new LatLng(gc);
         }
         catch(Exception error)
         {
           throw new MetabaseException(StringConsts.METABASE_REG_GEO_CENTER_ERROR.Args(GetType().Name, name, gc, error.ToMessageWithType()));
         }
      }

      /// <summary>
      /// A section which is a parent of this one
      /// </summary>
      public readonly SectionRegionBase ParentSection;


      /// <summary>
      /// Returns geo center coordinates for this level or null. Use EffectiveGeoCenter to get coordinates defaulted from parent if not set on this level
      /// </summary>
      public readonly LatLng? GeoCenter;


      /// <summary>
      /// Returns the effective geo center of this entity, that is - if the coordinates are not specified on this level then
      ///  coordinates of parent are taken
      /// </summary>
      public LatLng EffectiveGeoCenter
      {
        get
        {
          if (this.GeoCenter.HasValue) return this.GeoCenter.Value;
          if (ParentSection==null)
            return CoreConsts.DEFAULT_GEO_LOCATION;//very default

          return ParentSection.EffectiveGeoCenter;
        }
      }

      /// <summary>
      /// Returns region path
      /// </summary>
      public virtual string RegionPath
      {
        get
        {
          return "{0}/{1}".Args( ParentSection==null ? string.Empty : ParentSection.RegionPath, Name);
        }
      }

      /// <summary>
      /// Enumerates parent sections on the path to this section in the order of their descent - from root down into the region tree
      /// </summary>
      public IEnumerable<SectionRegionBase> ParentSectionsOnPath
      {
        get
        {
           var parents = new List<SectionRegionBase>();
           var level = this.ParentSection;
           while(level!=null)
           {
              parents.Add(level);
              level = level.ParentSection;
           }
           parents.Reverse();// zone:noc:region -> region:noc:zone
           return parents;
        }
      }

      /// <summary>
      /// Enumerates parent sections on the path to this section in the order of their descent - from root down into the region tree including this section.
      /// See also ParentSectionsOnPath which only returns parent sections without this one
      /// </summary>
      public IEnumerable<SectionRegionBase> SectionsOnPath
      {
        get
        {
           var chain = ParentSectionsOnPath.ToList();
           chain.Add(this);
           return chain;
        }
      }

      /// <summary>
      /// Provides short mnemonic description of this section type (i.e. "reg", "noc", "zone", "host")
      /// </summary>
      public string SectionMnemonicType
      {
        get
        { // do not localize!
          return this is Metabank.SectionRegion ? SysConsts.REGION_MNEMONIC_REGION :
                 this is Metabank.SectionNOC ? SysConsts.REGION_MNEMONIC_NOC :
                 this is Metabank.SectionZone ? SysConsts.REGION_MNEMONIC_ZONE : SysConsts.REGION_MNEMONIC_HOST;
        }
      }

      /// <summary>
      /// Navigates to a named child section
      /// </summary>
      public abstract SectionRegionBase this[string name] { get;}

      public override string ToString()
      {
        return "{0}({1})".Args(GetType().Name, this.RegionPath);
      }

      /// <summary>
      /// Returns true if another instance represents logically the same regional entity: has the same type and path, regardless of path extensions ('.r','.noc',...).
      /// Use this as a test as Equals() is not overriden by this class and does instance-based comparison by default
      /// </summary>
      public bool IsLogicallyTheSame(SectionRegionBase other)
      {
        if (other==null) return false;
        if (GetType()!=other.GetType())return false;
        if (!this.RegionPath.IsSameRegionPath(other.RegionPath)) return false;
        return true;
      }


      /// <summary>
      /// Matches the requested parameters to the most appropriate network route specified in the 'networks' level config subsection
      /// </summary>
      /// <param name="net">Network</param>
      /// <param name="svc">Service</param>
      /// <param name="binding">Binding name</param>
      /// <param name="from">Calling party - region path to host i.e. '/US/East/CLE/A/I/wmed0001'</param>
      /// <returns>Best matched NetSvcPeer descriptor which may be blank. Check NetSvcPeer.Blank</returns>
      public NetSvcPeer MatchNetworkRoute(string net, string svc, string binding, string from)
      {
        var routing = this.LevelConfig[CONFIG_NETWORK_ROUTING_SECTION];
        if (!routing.Exists)
          return new NetSvcPeer();

        var routeNodes = routing.Children.Where(n => n.IsSameName(CONFIG_NETWORK_ROUTING_ROUTE_SECTION));

        var match = routeNodes.Where(n =>
                                       ( n.AttrByName(CONFIG_NETWORK_ROUTING_NETWORK_ATTR).Value.IsNullOrWhiteSpace() ||
                                         Metabank.INVSTRCMP.Equals(n.AttrByName(CONFIG_NETWORK_ROUTING_NETWORK_ATTR).Value, net)
                                       ) &&
                                       ( n.AttrByName(CONFIG_NETWORK_ROUTING_SERVICE_ATTR).Value.IsNullOrWhiteSpace() ||
                                         Metabank.INVSTRCMP.Equals(n.AttrByName(CONFIG_NETWORK_ROUTING_SERVICE_ATTR).Value, svc)
                                       ) &&
                                       ( n.AttrByName(CONFIG_NETWORK_ROUTING_BINDING_ATTR).Value.IsNullOrWhiteSpace() ||
                                         Metabank.INVSTRCMP.Equals(n.AttrByName(CONFIG_NETWORK_ROUTING_BINDING_ATTR).Value, binding)
                                       ) &&
                                       ( n.AttrByName(CONFIG_NETWORK_ROUTING_FROM_ATTR).Value.IsNullOrWhiteSpace() ||
                                         matchRegPath(n.AttrByName(CONFIG_NETWORK_ROUTING_FROM_ATTR).Value, from)>0
                                       )
                                      ).Select(n => new
                                                    {
                                                      ToAddress = n.AttrByName(CONFIG_NETWORK_ROUTING_TO_ADDRESS_ATTR).Value,
                                                      ToPort    = n.AttrByName(CONFIG_NETWORK_ROUTING_TO_PORT_ATTR)   .Value,
                                                      ToGroup   = n.AttrByName(CONFIG_NETWORK_ROUTING_TO_GROUP_ATTR)  .Value,
                                                      Score =
                                                         (n.AttrByName(CONFIG_NETWORK_ROUTING_NETWORK_ATTR).Value.IsNullOrWhiteSpace() ? 0 : 1000000000)
                                                       + (n.AttrByName(CONFIG_NETWORK_ROUTING_SERVICE_ATTR).Value.IsNullOrWhiteSpace() ? 0 :    1000000)
                                                       + (n.AttrByName(CONFIG_NETWORK_ROUTING_BINDING_ATTR).Value.IsNullOrWhiteSpace() ? 0 :       1000)
                                                       + (n.AttrByName(CONFIG_NETWORK_ROUTING_FROM_ATTR)   .Value.IsNullOrWhiteSpace() ? 0 :
                                                                 1+matchRegPath(n.AttrByName(CONFIG_NETWORK_ROUTING_FROM_ATTR).Value, from))
                                                    }
                                              ).OrderBy(m => -m.Score).FirstOrDefault();
        if (match==null)
          return new NetSvcPeer();

        return new NetSvcPeer(match.ToAddress, match.ToPort, match.ToGroup);
      }

      public override void Validate(ValidationContext ctx)
      {
        base.Validate(ctx);

        validateRouting(ctx);
      }


      private int matchRegPath(string path, string pattern)
      {
        return Metabank.CatalogReg.CountMatchingPathSegments(path, pattern);
      }

      private void validateRouting(ValidationContext ctx)
      {
        var routing = this.LevelConfig[CONFIG_NETWORK_ROUTING_SECTION];
        if (!routing.Exists) return;
        var routeNodes = routing.Children.Where(n => n.IsSameName(CONFIG_NETWORK_ROUTING_ROUTE_SECTION));

        foreach(var rnode in routeNodes)
        {
          foreach(var anode in rnode.Attributes)
            if (!anode.IsSameName(CONFIG_NETWORK_ROUTING_NETWORK_ATTR) &&
                !anode.IsSameName(CONFIG_NETWORK_ROUTING_SERVICE_ATTR) &&
                !anode.IsSameName(CONFIG_NETWORK_ROUTING_BINDING_ATTR) &&
                !anode.IsSameName(CONFIG_NETWORK_ROUTING_FROM_ATTR) &&
                !anode.IsSameName(CONFIG_NETWORK_ROUTING_TO_ADDRESS_ATTR) &&
                !anode.IsSameName(CONFIG_NETWORK_ROUTING_TO_PORT_ATTR) &&
                !anode.IsSameName(CONFIG_NETWORK_ROUTING_TO_GROUP_ATTR)
               )
           ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Warning, Catalog, this,
                          StringConsts.METABASE_NETWORK_REGION_ROUTING_ATTR_UNRECOGNIZED_WARNING.Args(RegionPath, anode.Name) ) );




          var net     = rnode.AttrByName( CONFIG_NETWORK_ROUTING_NETWORK_ATTR ).Value;
          var svc     = rnode.AttrByName( CONFIG_NETWORK_ROUTING_SERVICE_ATTR ).Value;
          var binding = rnode.AttrByName( CONFIG_NETWORK_ROUTING_BINDING_ATTR ).Value;
          var from    = rnode.AttrByName( CONFIG_NETWORK_ROUTING_FROM_ATTR ).Value;

          var toaddr   = rnode.AttrByName( CONFIG_NETWORK_ROUTING_TO_ADDRESS_ATTR ).Value;
          var toport   = rnode.AttrByName( CONFIG_NETWORK_ROUTING_TO_PORT_ATTR ).Value;
          var togroup  = rnode.AttrByName( CONFIG_NETWORK_ROUTING_TO_GROUP_ATTR ).Value;

          if (toaddr.IsNullOrWhiteSpace() &&
              toport.IsNullOrWhiteSpace() &&
              togroup.IsNullOrWhiteSpace())
           ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, StringConsts.METABASE_NETWORK_REGION_ROUTING_EMPTY_ROUTE_ASSIGNMENT_ERROR.Args(RegionPath) ) );

          if (net.IsNotNullOrEmpty())
          {
            try
            {
              var netNode = Metabank.GetNetworkConfNode(net);
            }
            catch(Exception error)
            {
              ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, error.ToMessageWithType() ) );
            }

            if (svc.IsNotNullOrWhiteSpace())
            {
              try
              {
                var svcNode = Metabank.GetNetworkSvcConfNode(net, svc);
              }
              catch(Exception error)
              {
                ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, error.ToMessageWithType() ) );
              }

              if (binding.IsNotNullOrWhiteSpace())
                try
                {
                  var svcNode = Metabank.GetNetworkSvcBindingConfNode(net, svc, binding);
                }
                catch(Exception error)
                {
                  ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, error.ToMessageWithType() ) );
                }

            }
          }

          if (from.IsNotNullOrWhiteSpace())
          {
            var reg = Metabank.CatalogReg[from];
            if (reg==null)
             ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, StringConsts.METABASE_NETWORK_REGION_ROUTING_FROM_PATH_ERROR.Args(RegionPath, from) ) );
          }


        }
      }

  }




  /// <summary>
  /// Represents metadata for Region
  /// </summary>
  public sealed class SectionRegion : SectionRegionBase
  {
      internal SectionRegion(RegCatalog catalog, SectionRegion parentRegion, string name, string path,  FileSystemSession session) : base(catalog, parentRegion, name, path, session)
      {
        m_ParentRegion = parentRegion;
      }

      private SectionRegion m_ParentRegion;


      public override string RootNodeName
      {
        get { return "region"; }
      }

      /// <summary>
      /// Returns parent region for this region or null if this region is top-level
      /// </summary>
      public SectionRegion ParentRegion { get { return m_ParentRegion;} }


      /// <summary>
      /// Returns names of child regions
      /// </summary>
      public IEnumerable<string> SubRegionNames
      {
        get
        {
          string CACHE_KEY = ("Reg.srn"+Path).ToLowerInvariant();
          //1. Try to get from cache
          var result = Metabank.cacheGet(REG_CATALOG, CACHE_KEY) as IEnumerable<string>;
          if (result!=null) return result;

          result =
            Metabank.fsAccess("Region[{0}].SubRegionNames.get".Args(m_Name), Path,
                  (session, dir) => dir.SubDirectoryNames
                                       .Where(dn=>dn.EndsWith(RegCatalog.REG_EXT))
                                       .Select(dn=>Metabank.chopExt(dn))
                                       .ToList()
            );

          Metabank.cachePut(REG_CATALOG, CACHE_KEY, result);
          return result;
        }
      }


      /// <summary>
      /// Returns all NOC sections that this region and all sub-regions contain.
      /// This property may fetch much data for large cloud if enumerated to the end
      /// </summary>
      public IEnumerable<SectionNOC> AllNOCs
      {
        get
        {
          foreach(var nn in this.NOCNames)
            yield return this.GetNOC(nn);

          foreach(var srn in this.SubRegionNames)
          {
           var sr = this.GetSubRegion(srn);
           foreach(var srnoc in sr.AllNOCs)
            yield return srnoc;
          }
        }
      }


      /// <summary>
      /// Gets names of network op centers in this region
      /// </summary>
      public IEnumerable<string> NOCNames
      {
        get
        {
          string CACHE_KEY = ("Reg.nn"+Path).ToLowerInvariant();
          //1. Try to get from cache
          var result = Metabank.cacheGet(REG_CATALOG, CACHE_KEY) as IEnumerable<string>;
          if (result!=null) return result;

          result =
            Metabank.fsAccess("Region[{0}].NOCNames.get".Args(m_Name), Path,
                  (session, dir) => dir.SubDirectoryNames
                                       .Where(dn=>dn.EndsWith(RegCatalog.NOC_EXT))
                                       .Select(dn=>Metabank.chopExt(dn))
                                       .ToList()
            );

          Metabank.cachePut(REG_CATALOG, CACHE_KEY, result);
          return result;
        }
      }

      /// <summary>
      /// Navigates to a named child region or NOC. The search is done using case-insensitive comparison, however
      ///  the underlying file system may be case-sensitive and must be supplied the exact name
      /// </summary>
      public override SectionRegionBase this[string name]
      {
        get
        {
          if (name.IsNullOrWhiteSpace()) return null;

          name = name.Trim();

          if (name.EndsWith(RegCatalog.REG_EXT))
          {
            name = name.Substring(0, name.LastIndexOf(RegCatalog.REG_EXT));
            if (SubRegionNames.Any(srn=>INVSTRCMP.Equals(srn, name)))
              return GetSubRegion(name);
            return null;
          }


          if (name.EndsWith(RegCatalog.NOC_EXT))
          {
            name = name.Substring(0, name.LastIndexOf(RegCatalog.NOC_EXT));
            if (NOCNames.Any(srn=>INVSTRCMP.Equals(srn, name)))
              return GetNOC(name);
            return null;
          }


          if (SubRegionNames.Any(srn=>INVSTRCMP.Equals(srn, name)))
            return GetSubRegion(name);

          if (NOCNames.Any(srn=>INVSTRCMP.Equals(srn, name)))
            return GetNOC(name);

          return null;
        }
      }


      /// <summary>
      /// Gets sub-region by name
      /// </summary>
      public SectionRegion GetSubRegion(string name)
      {
        string CACHE_KEY = ("Reg.sr"+Path+name).ToLowerInvariant();
        //1. Try to get from cache
        var result = Metabank.cacheGet(REG_CATALOG, CACHE_KEY) as SectionRegion;
        if (result!=null) return result;

        var rdir = "{0}{1}".Args(name, RegCatalog.REG_EXT);
        var rpath = Metabank.JoinPaths(Path, rdir);
        result =
            Metabank.fsAccess("Region[{0}].GetSubRegion({1})".Args(m_Name, name), rpath,
                  (session, dir) => new SectionRegion(Catalog, this, name, rpath, session)
            );


        Metabank.cachePut(REG_CATALOG, CACHE_KEY, result);
        return result;
      }

      /// <summary>
      /// Gets child network operation center by name
      /// </summary>
      public SectionNOC GetNOC(string name)
      {
        string CACHE_KEY = ("Reg.cn"+Path+name).ToLowerInvariant();
        //1. Try to get from cache
        var result = Metabank.cacheGet(REG_CATALOG, CACHE_KEY) as SectionNOC;
        if (result!=null) return result;

        var rdir = "{0}{1}".Args(name, RegCatalog.NOC_EXT);
        var rpath = Metabank.JoinPaths(Path, rdir);
        result =
            Metabank.fsAccess("Region[{0}].GetNOC({1})".Args(m_Name, name), rpath,
                  (session, dir) => new SectionNOC(Catalog, this, name, rpath, session)
            );


        Metabank.cachePut(REG_CATALOG, CACHE_KEY, result);
        return result;
      }

      public override void Validate(ValidationContext ctx)
      {
        base.Validate(ctx);

        foreach(var sreg in this.SubRegionNames)
            this.GetSubRegion(sreg).Validate(ctx);

        foreach(var snoc in this.NOCNames)
            this.GetNOC(snoc).Validate(ctx);
      }




  }



}}
