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
  /// Represents metadata for Network Operations Center (NOC). A NOC name must be globally-unique regardless of it's parent region
  /// </summary>
  public sealed class SectionNOC : SectionRegionBase
  {
      internal SectionNOC(RegCatalog catalog, SectionRegion parentRegion, string name, string path,  FileSystemSession session) : base(catalog, parentRegion, name, path, session)
      {
        m_ParentRegion = parentRegion;
      }

      private SectionRegion m_ParentRegion;


      public override string RootNodeName
      {
        get { return "noc"; }
      }

      /// <summary>
      /// Returns parent region for this NOC
      /// </summary>
      public SectionRegion ParentRegion { get { return m_ParentRegion;} }


      /// <summary>
      /// Returns Zone section in parent NOC that this whole NOC is logically (not phisically) under, or null if
      /// this NOC does not have any NOC above it.
      /// This property is used to detect ZoneGovernor procesees which are higher in hierarchy than any zone in this NOC.
      /// This is important for failover to parent.
      /// The path must point to an existing zone in a NOC which is higher in hierarchy than this one and
      /// it must be of the same root, that is -  have at least one common region with this one
      /// </summary>
      public SectionZone ParentNOCZone
      {
        get
        {
           var path = this.ParentNOCZonePath;
           if (path.IsNullOrWhiteSpace()) return null;
           var zone = Catalog[path] as SectionZone;
           if (zone==null)
             throw new MetabaseException(StringConsts.METABASE_REG_NOC_PARENT_NOC_ZONE_ERROR.Args(Name, path));

           if (Catalog.CountMatchingPathSegments(zone, this)<1)
            throw new MetabaseException(StringConsts.METABASE_REG_NOC_PARENT_NOC_ZONE_NO_ROOT_ERROR.Args(Name, path));

           if (zone.NOC.SectionsOnPath.Count()>=this.SectionsOnPath.Count())
            throw new MetabaseException(StringConsts.METABASE_REG_NOC_PARENT_NOC_ZONE_LEVEL_ERROR.Args(Name, path));

           return zone;
        }
      }


      /// <summary>
      /// Returns Zone section path in parent NOC that this whole NOC is logically (not phisically) under, or null if
      /// this NOC does not have any NOC above it.
      /// This property is used to detect ZoneGovernor procesees which are higher in hierarchy than any zone in this NOC.
      /// This is important for failover to parent.
      /// The path must point to an existing zone in a NOC which is higher in hierarchy than this one and
      /// it must be of the same root, that is -  have at least one common region with this one
      /// </summary>
      public string ParentNOCZonePath { get{ return LevelConfig.AttrByName(CONFIG_PARENT_NOC_ZONE_ATTR).Value;}}


      /// <summary>
      /// Returns names of child zones
      /// </summary>
      public IEnumerable<string> ZoneNames
      {
        get
        {
          string CACHE_KEY = ("NOC.zn"+Path).ToLowerInvariant();
          //1. Try to get from cache
          var result = Metabank.cacheGet(REG_CATALOG, CACHE_KEY) as IEnumerable<string>;
          if (result!=null) return result;

          result =
            Metabank.fsAccess("NOC[{0}].ZoneNames.get".Args(m_Name), Path,
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
      /// Navigates to a named child zone. The search is done using case-insensitive comparison, however
      ///  the underlying file system may be case-sensitive and must be supplied the exact name
      /// </summary>
      public override SectionRegionBase this[string name]
      {
        get
        {
          if (name.IsNullOrWhiteSpace()) return null;

          name = name.Trim();

          if (name.EndsWith(RegCatalog.ZON_EXT))
            name = name.Substring(0, name.LastIndexOf(RegCatalog.ZON_EXT));

          if (ZoneNames.Any(srn=>INVSTRCMP.Equals(srn, name)))
            return GetZone(name);

          return null;
        }
      }



      /// <summary>
      /// Gets zone by name
      /// </summary>
      public SectionZone GetZone(string name)
      {
        string CACHE_KEY = ("NOC.z"+Path+name).ToLowerInvariant();
        //1. Try to get from cache
        var result = Metabank.cacheGet(REG_CATALOG, CACHE_KEY) as SectionZone;
        if (result!=null) return result;

        var rdir = "{0}{1}".Args(name, RegCatalog.ZON_EXT);
        var rpath = Metabank.JoinPaths(Path, rdir);
        result =
            Metabank.fsAccess("NOC[{0}].GetZone({1})".Args(m_Name, name), rpath,
                  (session, dir) => new SectionZone(this, null, name, rpath, session)
            );


        Metabank.cachePut(REG_CATALOG, CACHE_KEY, result);
        return result;
      }

      public override void Validate(ValidationContext ctx)
      {
        var output = ctx.Output;

        base.Validate(ctx);

        SectionZone pnz = null;
        try
        {
          pnz = this.ParentNOCZone;//throws
        }
        catch(Exception error)
        {
          output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this,
                        "Invalid '{0}' specified: {1}".Args(CONFIG_PARENT_NOC_ZONE_ATTR, error.ToMessageWithType())));
        }

        if (pnz==null && ParentSectionsOnPath.Count()>1)
            output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Warning, Catalog, this,
                        "This NOC is not top-level but it does not have any '{0}' specified".Args(CONFIG_PARENT_NOC_ZONE_ATTR)));

        if (pnz!=null && !pnz.ZoneGovernorHosts.Any())
          output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this,
                        "ParenNOCZone '{0}' pointed to by this NOC does not have any hosts that run zone governor application".Args(pnz.RegionPath)));

        foreach(var szone in this.ZoneNames)
            this.GetZone(szone).Validate(ctx);
      }


  }



}}
