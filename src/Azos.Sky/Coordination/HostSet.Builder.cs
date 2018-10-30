using System.Linq;

using Azos.Conf;
using Azos.Sky.Metabase;

namespace Azos.Sky.Coordination
{
  public partial class HostSet
  {
    /// <summary>
    /// Provides the default HostSet.Builder that finds and makes instance of the matching HostSet
    /// </summary>
    public class Builder
    {

      private static object s_InstanceLock = new object();
      private static volatile Builder s_Instance;

      /// <summary>
      /// Returns a singleton builder instance as configured in the root metabase config,
      /// hence the instance is injectable
      /// </summary>
      public static Builder Instance
      {
        get
        {
          if(s_Instance!=null) return s_Instance;
          lock(s_InstanceLock)
          {
            if(s_Instance!=null) return s_Instance;
            var node = SkySystem.Metabase.RootConfig[Metabank.CONFIG_HOST_SET_BUILDER_SECTION];
            s_Instance = FactoryUtils.Make<Builder>(node, typeof(Builder), new [] {node});
          }

          return s_Instance;
        }
      }

      protected Builder(IConfigSectionNode config) { }


      /// <summary>
      /// Tries to find a named host set starting at the requested cluster level.
      /// Throws if not found.
      /// </summary>
      public THostSet FindAndBuild<THostSet>(string setName, string clusterPath, bool searchParent = true, bool transcendNoc = false)
        where THostSet : HostSet
      {
        var result = TryFindAndBuild<THostSet>(setName, clusterPath, searchParent, transcendNoc);

        if (result==null)
          throw new CoordinationException(StringConsts.HOST_SET_BUILDER_CONFIG_FIND_ERROR
                                         .Args(GetType().Name, setName, clusterPath, searchParent, transcendNoc));
        return result;
      }

      /// <summary>
      /// Tries to find a named host set starting at the requested cluster level.
      /// Returns null if not found.
      /// </summary>
      public THostSet TryFindAndBuild<THostSet>(string setName, string clusterPath, bool searchParent = true, bool transcendNoc = false)
        where THostSet : HostSet
      {
        if (setName.IsNullOrWhiteSpace())
          throw new CoordinationException(StringConsts.ARGUMENT_ERROR+GetType().Name+".ctor(setName==null|empty)");

        if (clusterPath.IsNullOrWhiteSpace())
          throw new CoordinationException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".ctor(clusterPath==null|empty)");

        var result = DoTryFindAndBuild<THostSet>(setName, clusterPath, searchParent, transcendNoc);
        return result;
      }

      protected virtual THostSet DoTryFindAndBuild<THostSet>(string setName, string clusterPath, bool searchParent, bool transcendNoc)
        where THostSet : HostSet
      {
        THostSet result = null;

        string actualPath;
        var cnode = DoTryFindConfig(setName, clusterPath, searchParent, transcendNoc, out actualPath);

        if (cnode!=null)
         result = FactoryUtils.Make<THostSet>(cnode, typeof(THostSet), new object[]{ setName, clusterPath, actualPath, cnode });

        return result;
      }

      protected virtual IConfigSectionNode DoTryFindConfig(string setName, string clusterPath, bool searchParent, bool transcendNoc, out string actualPath)
      {
        actualPath = null;

        Metabank.SectionRegionBase level = SkySystem.Metabase.CatalogReg[clusterPath];
        if (!(level is Metabank.SectionZone || level is Metabank.SectionNOC)) return null;
        IConfigSectionNode result = null;

        while(level!=null)
        {
          result = level.LevelConfig
                      .Children
                      .Where( c => c.IsSameName(Metabank.CONFIG_HOST_SET_SECTION) && c.IsSameNameAttr(setName) )
                      .FirstOrDefault();

          if (result!=null) break;
          if (!searchParent) break;

          var zone = level as Metabank.SectionZone;
          if (zone != null)
          {
            var zparent = zone.ParentZone;
            if (zparent != null)
            {
              level = zparent;
              continue;
            }

            level = zone.NOC;
            continue;
          }

          if (!transcendNoc) break;

          var noc = level as Metabank.SectionNOC;
          if (noc != null)
            level = noc.ParentNOCZone;
        }

        if (result!=null)
           actualPath = level.RegionPath;

        return result;
      }
    }
  }
}
