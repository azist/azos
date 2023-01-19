/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using Azos.Collections;

namespace Azos.Sky.Metabase{ public sealed partial class Metabank{



    /// <summary>
    /// Represents a system catalog of region objects
    /// </summary>
    public sealed class RegCatalog : SystemCatalog
    {
      #region CONSTS
          public const string REG_EXT = ".r";
          public const string NOC_EXT = ".noc";
          public const string ZON_EXT = ".z";
          public const string HST_EXT = ".h";
      #endregion

      #region .ctor
          internal RegCatalog(Metabank bank) : base(bank, REG_CATALOG)
          {

          }

      #endregion

      #region Fields


      #endregion

      #region Properties
          /// <summary>
          /// Returns registry of top-level regions
          /// </summary>
          public IRegistry<SectionRegion> Regions
          {
            get
            {
              const string CACHE_KEY = "topregs";
              //1. Try to get from cache
              var result = Metabank.cacheGet(REG_CATALOG, CACHE_KEY) as IRegistry<SectionRegion>;
              if (result!=null) return result;

              result =
                Metabank.fsAccess("RegionCatalog.Regions.get", REG_CATALOG,
                      (session, dir) =>
                      {
                        var reg = new Registry<SectionRegion>();
                        foreach(var sdir in dir.SubDirectoryNames.Where(dn=>dn.EndsWith(RegCatalog.REG_EXT)))
                        {
                          var spath = Metabank.JoinPaths(REG_CATALOG, sdir);
                          var region = new SectionRegion(this, null, Metabank.chopExt(sdir), spath, session);
                          reg.Register(region);
                        }
                        return reg;
                      }
                );

              Metabank.cachePut(REG_CATALOG, CACHE_KEY, result);
              return result;
            }
          }

          /// <summary>
          /// Navigates to region path and returns the corresponding section or null.
          /// The path root starts at region catalog level i.e.: '/Us/East/Clev.noc/A/IV/wlarge0001' -
          ///   a path to a host called 'wlarge0001' located in zone A-IV in Cleveland network center etc.
          /// NOTE: may omit the '.r', '.noc', and '.z' region/noc and zone designators.
          /// The navigation is done using case-insensitive name comparison, however
          ///  the underlying file system may be case-sensitive and must be supplied the exact name
          /// </summary>
          public SectionRegionBase this[string path]
          {
            get
            {
              if (path==null) return null;
              var segs = path.Split('/').Where(s=>s.IsNotNullOrWhiteSpace()).Select(s=>s.Trim());
              SectionRegionBase result = null;
              var first = true;
              foreach(var seg in (segs))
              {
                if (first)
                {
                  first = false;
                  if (seg.EndsWith(RegCatalog.REG_EXT))
                    result = Regions[ seg.Substring(0, seg.LastIndexOf(RegCatalog.REG_EXT)) ];
                  else
                    result = Regions[seg];
                }
                else
                  result = result[seg];

                if (result==null) break;
              }
              return result;
            }
          }

          /// <summary>
          /// Enumerates all NOCs. This may fetch much data if enumerated to the end
          /// </summary>
          public IEnumerable<SectionNOC> AllNOCs
          {
            get
            {
              foreach(var region in Regions)
              {
               foreach(var noc in region.AllNOCs)
                yield return noc;
              }
            }
          }


      #endregion

      #region Public


          /// <summary>
          /// Tries to navigate to region path as far as possible and returns the deepest section or null.
          /// This method is needed to obtain root paths from detailed paths with wild cards.
          /// Method also returns how deep it could navigate(how many path levels resolved).
          /// For example:
          ///  '/Us/East/Clev.noc/{1}/{2}' - Clev.noc with depth=3 will be returned.
          /// </summary>
          public SectionRegionBase TryNavigateAsFarAsPossible(string path, out int depth)
          {
              depth = 0;
              if (path==null) return null;
              var segs = path.Split('/').Where(s=>s.IsNotNullOrWhiteSpace()).Select(s=>s.Trim());
              SectionRegionBase existing = null;
              SectionRegionBase current = null;
              var first = true;
              foreach(var seg in (segs))
              {
                if (first)
                {
                  first = false;
                  if (seg.EndsWith(RegCatalog.REG_EXT))
                    current = Regions[ seg.Substring(0, seg.LastIndexOf(RegCatalog.REG_EXT)) ];
                  else
                    current = Regions[seg];
                }
                else
                  current = current[seg];

                if (current==null) break;
                depth++;
                existing = current;
              }
              return existing;
          }



         /// <summary>
         /// Navigates to region section or throws. NOTE: may omit the '.r' suffix
         /// </summary>
         public SectionRegion NavigateRegion(string path)
         {
            var result = this[path] as SectionRegion;
            if (result==null)
              throw new MetabaseException(ServerStringConsts.METABASE_REG_CATALOG_NAV_ERROR.Args("NavigateRegion()", path ?? SysConsts.NULL));
            return result;
         }

         /// <summary>
         /// Navigates to NOC section or throws. NOTE: may omit the '.r' and '.noc' suffixes
         /// </summary>
         public SectionNOC NavigateNOC(string path)
         {
            var result = this[path] as SectionNOC;
            if (result==null)
              throw new MetabaseException(ServerStringConsts.METABASE_REG_CATALOG_NAV_ERROR.Args("NavigateNOC()", path ?? SysConsts.NULL));
            return result;
         }

         /// <summary>
         /// Navigates to zone section or throws. NOTE: may omit the '.r','.noc', and '.z' suffixes
         /// </summary>
         public SectionZone NavigateZone(string path)
         {
            var result = this[path] as SectionZone;
            if (result==null)
              throw new MetabaseException(ServerStringConsts.METABASE_REG_CATALOG_NAV_ERROR.Args("NavigateZone()", path ?? SysConsts.NULL));
            return result;
         }

         /// <summary>
         /// Navigates to host section or throws. NOTE: may omit the '.r','.noc','.z', and '.h' suffixes
         /// </summary>
         public SectionHost NavigateHost(string path)
         {
            var result =  this[path] as SectionHost;
            if (result==null)
              throw new MetabaseException(ServerStringConsts.METABASE_REG_CATALOG_NAV_ERROR.Args("NavigateHost()", path ?? SysConsts.NULL));
            return result;
         }


         public override void Validate(ValidationContext ctx)
         {
           var output = ctx.Output;

           foreach(var sreg in Regions)
            sreg.Validate(ctx);

           var here = App.GetThisHostMetabaseSection().EffectiveGeoCenter;
           var allNOCs = AllNOCs.ToList();
           foreach(var noc in allNOCs)
           {
             if (noc.EffectiveGeoCenter.Equals(CoreConsts.DEFAULT_GEO_LOCATION))
               output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Warning, this, noc,
                                     ServerStringConsts.METABASE_NOC_DEFAULT_GEO_CENTER_WARNING.Args(noc.Name, noc.EffectiveGeoCenter)) );

             output.Add(new MetabaseValidationMsg(MetabaseValidationMessageType.Info, this, noc,
                                     "Distance to '{0}' is {1}km".Args(App.GetThisHostMetabaseSection().RegionPath,
                                                                      (int)here.HaversineEarthDistanceKm(noc.EffectiveGeoCenter))));

           }

           if (allNOCs.Select(n=>n.Name).Count()!=allNOCs.Select(n=>n.Name).Distinct(StringComparer.InvariantCultureIgnoreCase).Count())
             output.Add(new MetabaseValidationMsg(MetabaseValidationMessageType.Error, this, null,
                                     "There are duplicate NOCs defined. NOC names should be globally unique regardless of their region"));

         }


         /// <summary>
         /// Counts the number of matching region path segments that resolve to the same section in the region catalog.
         /// Example:  /US/East/CLE/A/I and /US/East/JFK/A/I returns 2
         /// </summary>
         public int CountMatchingPathSegments(string path1, string path2)
         {
            if (path1.IsNullOrWhiteSpace() && path2.IsNullOrWhiteSpace()) return 1;
            if (path1.IsNullOrWhiteSpace() || path2.IsNullOrWhiteSpace()) return 0;

            var sect1 = this[path1];
            var sect2 = this[path2];
            return CountMatchingPathSegments(sect1, sect2);
         }


         /// <summary>
         /// Counts the number of matching region path segments that resolve to the same section in the region catalog.
         /// Example:  /US/East/CLE/A/I and /US/East/JFK/A/I returns 2
         /// </summary>
         public int CountMatchingPathSegments(SectionRegionBase sect1, SectionRegionBase sect2)
         {
            if (sect1==null || sect2==null) return 0;

            var chain1 = sect1.SectionsOnPath.ToList();
            var chain2 = sect2.SectionsOnPath.ToList();

            var i = 0;//count number of matching segments
            for(; i<chain1.Count && i<chain2.Count; i++)
             if (!chain1[i].IsLogicallyTheSame(chain2[i])) break;

            return i;
         }

         /// <summary>
         /// Returns the ratio of matching region path segments that resolve to the same section in the region catalog, to maximum path length.
         /// Example:  /US/East/CLE/A and /US/East/JFK/A returns 0.5d = 50% match
         /// </summary>
         public double GetMatchingPathSegmentRatio(string path1, string path2)
         {
            if (path1.IsNullOrWhiteSpace() && path2.IsNullOrWhiteSpace()) return 1d;
            if (path1.IsNullOrWhiteSpace() || path2.IsNullOrWhiteSpace()) return 0d;

            var sect1 = this[path1];
            var sect2 = this[path2];
            if (sect1==null || sect2==null) return 0;

            var chain1 = sect1.SectionsOnPath.ToList();
            var chain2 = sect2.SectionsOnPath.ToList();

            double max = chain1.Count > chain2.Count ? chain1.Count : chain2.Count;

            var i = 0;//count number of matching segments
            for(; i<chain1.Count && i<chain2.Count; i++)
             if (!chain1[i].IsLogicallyTheSame(chain2[i])) break;

            return i / max;
         }



             private ConcurrentDictionary<string, double> m_CacheGetDistanceBetweenPaths = new ConcurrentDictionary<string, double>(StringComparer.Ordinal);

         /// <summary>
         /// Calculates the logical distance between two entities in the cloud.
         /// Although the distance is measured in kilometers it is really a logical distance which is somewhat related to physical.
         /// At first, the two paths are compared in terms of matching segment count, if paths match 100% (point to the same entity) then
         ///  the distance is zero, otherwise the path match ratio is prorated against the earth circumference to get distance value.
         /// The second comparison is performed against physical geo centers that use haversine formula to compute the distance 'as the crow flies'.
         /// The second step is necessary for the comparison of various non-matching paths. i.e.: '/world/us/east/cle...' and '/world/us/east/ny...' both yield
         ///  the same distance as far as paths comparison however, 'cle' is closer while compared with 'los angeles' than 'ny'
         /// </summary>
         /// <remarks>This function does caching of results based on case-sensitive paths keys</remarks>
         public double GetDistanceBetweenPaths(string path1, string path2)
         {
            const int MAX_DISTANCE_KM = 20000;//Earth equator 40,000km /2

            if (path1.IsNullOrWhiteSpace() || path2.IsNullOrWhiteSpace()) return double.MaxValue;

            var cacheKey = path1 + "  ///////  " + path2;
            double result = 0;
            if (m_CacheGetDistanceBetweenPaths.TryGetValue(cacheKey, out result)) return result;

            var sect1 = this[path1];
            var sect2 = this[path2];
            if (sect1!=null && sect2!=null)
            {
              if (sect1.IsLogicallyTheSame(sect2))
               result = 0d;
              else
              {
                  var matchRatio = GetMatchingPathSegmentRatio(path1, path2);
                  var matchDistanceKm = MAX_DISTANCE_KM * (1.0d - matchRatio);

                  var geoDistanceKm = sect1.EffectiveGeoCenter.HaversineEarthDistanceKm(sect2.EffectiveGeoCenter);

                  result =  matchDistanceKm + geoDistanceKm;
              }
            }
            else result = double.MaxValue;

            m_CacheGetDistanceBetweenPaths.TryAdd(cacheKey, result);

            return result;
         }


         /// <summary>
         /// Calculates the physical distance between two entities in the cloud.
         /// The distance is based on paths geo centers and maybe inaccurate if geo coordinates are not properly set
         /// </summary>
         public double GetGeoDistanceBetweenPaths(string path1, string path2)
         {
            var sect1 = this[path1];
            var sect2 = this[path2];
            if (sect1==null || sect2==null) return double.MaxValue;

            if (sect1.IsLogicallyTheSame(sect2)) return 0;

            return sect1.EffectiveGeoCenter.HaversineEarthDistanceKm(sect2.EffectiveGeoCenter);
         }


         /// <summary>
         /// Tries to navigate to NOC section in the specified absolute path and returns it or null if path is invalid/does not lead to NOC.
         /// Ignores anything after the NOC section (zones, hosts etc..). For example '/world/us/east/CLE.noc/a/ii/x' will return 'CLE.noc' section,
         ///  but '/world/us/east' will return null as there is no NOC in the path
         /// </summary>
         public SectionNOC GetNOCofPath(string path)
         {
              if (path==null) return null;
              var segs = path.Split('/').Where(s=>s.IsNotNullOrWhiteSpace());
              SectionRegionBase current = null;
              var first = true;
              foreach(var seg in (segs))
              {
                if (first)
                {
                  first = false;
                  if (seg.EndsWith(RegCatalog.REG_EXT))
                    current = Regions[ seg.Substring(0, seg.LastIndexOf(RegCatalog.REG_EXT)) ];
                  else
                    current = Regions[seg];
                }
                else
                {
                  if (current==null) return null;
                  current = current[seg];
                }
                if (current is SectionNOC) return (SectionNOC)current;
              }
              return null;
         }

         /// <summary>
         /// Returns true when all of the supplied paths are in the same NOC
         /// </summary>
         public bool ArePathsInSameNOC(params string[] paths)
         {
             if (paths==null || paths.Length==0) return false;
             if (paths.Any(p=>p.IsNullOrWhiteSpace())) return false;

             if (paths.Length==1) return true;


             var noc =  GetNOCofPath(paths[0]);
             if (noc==null) return false;

             return paths.All(p => noc.IsLogicallyTheSame(GetNOCofPath(p)));
         }

        /// <summary>
        /// Combines region paths using '/' where needed
        /// </summary>
        public static string JoinPathSegments(params string[] segments)
        {
          if (segments == null || segments.Length == 0) return string.Empty;

          var source = segments.Where(s => s.IsNotNullOrWhiteSpace());

          if (!source.Any()) return string.Empty;

          var prefix = source.First().Trim();
          var leading = prefix.Length > 0 && prefix[0] == '/';

          var result = string.Join("/", source.Select(s => s.Trim('/', ' ')).Where(s => s.IsNotNullOrWhiteSpace()));

          if (leading) return '/' + result;

          return result;
        }

      #endregion


    }



}}
