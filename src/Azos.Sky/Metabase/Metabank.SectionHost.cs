/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.IO.FileSystem;
using Azos.Conf;

using Azos.Sky.Apps;

namespace Azos.Sky.Metabase{ public sealed partial class Metabank{

  /// <summary>
  /// Represents metadata for Host
  /// </summary>
  public sealed class SectionHost : SectionRegionBase
  {

      internal SectionHost(SectionZone parentZone, string name, string path,  FileSystemSession session) : base(parentZone.Catalog, parentZone, name, path, session)
      {
        m_ParentZone = parentZone;
      }

      private SectionZone m_ParentZone;


      public override string RootNodeName
      {
        get { return "host"; }
      }

      /// <summary>
      /// Returns the parent zone that this host is in
      /// </summary>
      public SectionZone ParentZone { get { return m_ParentZone;} }

      /// <summary>
      /// Returns the NOC that this host is under
      /// </summary>
      public SectionNOC NOC { get { return m_ParentZone.NOC;}}

      /// <summary>
      /// Navigates to a named child region or NOC. The search is done using case-insensitive comparison, however
      ///  the underlying file system may be case-sensitive and must be supplied the exact name
      /// </summary>
      public override SectionRegionBase this[string name]
      {
        get
        {
          throw new MetabaseException(StringConsts.METABASE_INVALID_OPERTATION_ERROR + "SectionHost.navigate[{0}]".Args(name));
        }
      }

      /// <summary>
      /// Returns a name of the role (as defined by the attribute in host confog file) that this host has in the Sky
      /// </summary>
      public string RoleName { get{ return this.LevelConfig.AttrByName(CONFIG_ROLE_ATTR).ValueAsString(SysConsts.NULL); } }

      /// <summary>
      /// Returns a role app catalog section for the role that this host has in the Sky
      /// </summary>
      public SectionRole Role
      {
         get
         {
            var result = Metabank.CatalogApp.Roles[RoleName];
            if (result==null)
              throw new MetabaseException(StringConsts.METABASE_BAD_HOST_ROLE_ERROR.Args(RegionPath, RoleName));

            return result;
         }
       }

      /// <summary>
      /// Returns the name of the operating system that this host operates on. The value is required or exception is thrown
      /// </summary>
      public string OS
      {
        get
        {
            var result = this.LevelConfig.AttrByName(CONFIG_OS_ATTR).Value;
            if (result.IsNullOrWhiteSpace())
              throw new MetabaseException(StringConsts.METABASE_HOST_MISSING_OS_ATTR_ERROR.Args(RegionPath));

            //check that OS is registered in the list
            Metabank.GetOSConfNode(result);

            return result;
        }
      }

      /// <summary>
      /// Returns true if host is dynamic - that is: a physical representation of host gets created/torn at the runtime of cluter.
      /// The Dynamic host section represents a proptotype of (possibly) many host instances spawn in the Sky.
      /// Every particular instance of dynamic host gets a HOST-unique signature that can be supplied to host resolution service
      ///  to get actual network addresses for particular host
      /// </summary>
      public bool Dynamic { get { return this.LevelConfig.AttrByName(CONFIG_HOST_DYNAMIC_ATTR).ValueAsBool(); } }

      /// <summary>
      /// Returns true when this host has zone governor application in its role
      /// </summary>
      public bool IsZGov { get { return this.Role.Applications.Any(app => Metabank.INVSTRCMP.Equals(app.Name, SysConsts.APP_NAME_ZGOV)); } }

      /// <summary>
      /// Returns true for ZGov nodes that are failover lock service nodes which are used when primary nodes fail
      /// </summary>
      public bool IsZGovLockFailover { get { return this.LevelConfig.AttrByName(CONFIG_HOST_ZGOV_LOCK_FAILOVER_ATTR).ValueAsBool(); } }

      /// <summary>
      /// Returns true for Host nodes that are process host
      /// </summary>
      public bool IsProcessHost { get { return this.LevelConfig.AttrByName(CONFIG_HOST_PROCESS_HOST_ATTR).ValueAsBool(); } }

      /// <summary>
      /// Returns the first/primary host that runs the parent Zone governor for this host.
      /// Null returned for top-level hosts that do not have zone governors higher than themselves in this NOC unless transcendNOC is true
      ///  in which case the governor from higher-level NOC may be returned
      /// </summary>
      public SectionHost ParentZoneGovernorPrimaryHost(bool transcendNOC = false)
      {
          var myZone = m_ParentZone;

          return myZone.FindNearestParentZoneGovernors(transcendNOC: transcendNOC).FirstOrDefault();
      }

      /// <summary>
      /// Returns true if this zone has direct or indirect parent zone governor above it, optionally examining higher-level NOCs.
      /// </summary>
      public bool HasDirectOrIndirectParentZoneGovernor(SectionHost zgovHost, bool? iAmZoneGovernor = null, bool transcendNOC = false)
      {
        var myZone = m_ParentZone;

        return myZone.HasDirectOrIndirectParentZoneGovernor(zgovHost, iAmZoneGovernor, transcendNOC);
      }

      /// <summary>
      /// Returns true if this zone has direct or indirect parent zone governor above it, optionally examining higher-level NOCs.
      /// </summary>
      public bool HasDirectOrIndirectParentZoneGovernor(string zgovHost, bool? iAmZoneGovernor = null, bool transcendNOC = false)
      {
        var myZone = m_ParentZone;

        return myZone.HasDirectOrIndirectParentZoneGovernor(zgovHost, iAmZoneGovernor, transcendNOC);
      }


      /// <summary>
      /// Gets an application configuration tree for the particular named application on this host.
      /// This method traverses the whole region catalog and calculates the effective configuration tree for this host -
      /// the one that will get supplied into the application container process-wide.
      ///
      /// The traversal is done in the following order: MetabaseRoot->Role->App->[Regions]->NOC->[Zones]->Host.
      /// </summary>
      /// <param name="appName">Metabase application name which should resolve in App catalog</param>
      /// <param name="latest">Pass true to bypass the metabase cache on read so the config tree is recalculated</param>
      public IConfigSectionNode GetEffectiveAppConfig(string appName, bool latest = false)
      {
         IConfigSectionNode result = null;
         const string CACHE_TABLE = "EffectiveAppConfigs";
         string CACHE_KEY = (this.RegionPath+"."+appName).ToLowerInvariant();

         //1. Try to get from cache
         if (!latest)
         {
            result = Metabank.cacheGet(CACHE_TABLE, CACHE_KEY) as IConfigSectionNode;
            if (result!=null) return result;
         }

         try
         {
            result = calculateEffectiveAppConfig(appName);
         }
         catch(Exception error)
         {
            throw new MetabaseException(StringConsts.METABASE_EFFECTIVE_APP_CONFIG_ERROR.Args(appName, RegionPath, error.ToMessageWithType()), error);
         }

         Metabank.cachePut(CACHE_TABLE, CACHE_KEY, result);
         return result;
      }

      /// <summary>
      /// Gets the information for the best suitable binary packages on this host for the named app
      /// </summary>
      /// <param name="appName">Metabase application name which should resolve in App catalog</param>
      public IEnumerable<SectionApplication.AppPackage> GetAppPackages(string appName)
      {
         const string CACHE_TABLE = "AppPackages";
         string CACHE_KEY = (this.RegionPath+"."+appName).ToLowerInvariant();
         //1. Try to get from cache
         var result = Metabank.cacheGet(CACHE_TABLE, CACHE_KEY) as IEnumerable<SectionApplication.AppPackage>;
         if (result!=null) return result;

         try
         {
            result = this.calculateAppPackages(appName);
         }
         catch(Exception error)
         {
            throw new MetabaseException(StringConsts.METABASE_APP_PACKAGES_ERROR.Args(appName, RegionPath, error.ToMessageWithType()), error);
         }

         Metabank.cachePut(CACHE_TABLE, CACHE_KEY, result);
         return result;
      }

      public override void Validate(ValidationContext ctx)
      {
         var output = ctx.Output;

         base.Validate(ctx);

         try
         {
           var os = OS;
           var role = Role;
         }
         catch(Exception error)
         {
           output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, error.ToMessageWithType(), error) );
           return;
         }


         foreach(var app in Role.AppNames)
          try
          {
            var appConfig = GetEffectiveAppConfig(app);
            if (Metabank.INVSTRCMP.Equals(app, SysConsts.APP_NAME_GDIDA))
            {
              var authorityIDs = appConfig[Identification.GdidAuthorityService.CONFIG_GDID_AUTHORITY_SECTION]
                               .AttrByName( Identification.GdidAuthorityService.CONFIG_AUTHORITY_IDS_ATTR ).ValueAsByteArray();

              if (authorityIDs==null || authorityIDs.Length<1)
              {
                output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error,
                                                    Catalog,
                                                    this,
                                                    "The app container is improperly configured for '{0}' application. No GDID authorities specified".Args(SysConsts.APP_NAME_GDIDA) ));
                continue;
              }

              var stateKey = "{0}::AuthorityRoles".Args(this.GetType().FullName);
              var authIDs = ctx.StateAs<HashSet<int>>(stateKey);
              if (authIDs==null)
              {
                authIDs = new HashSet<int>();
                ctx.State[stateKey] = authIDs;
              }

              foreach(var aid in authorityIDs)
              {
                if (aid> Data.GDID.AUTHORITY_MAX)
                  output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error,
                                                    Catalog,
                                                    this,
                                                    "The app container is improperly configured for '{0}' application. GDID Authority id is over the limit: {1:X1}".Args(SysConsts.APP_NAME_GDIDA, aid)) );

                if (!authIDs.Add(aid))
                  output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error,
                                                    Catalog,
                                                    this,
                                                    "The app container is improperly configured for '{0}' application. Duplicate GDID authority: {1:X1}".Args(SysConsts.APP_NAME_GDIDA, aid)) );
              }
            }
          }
          catch(Exception error)
          {
            output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, error.ToMessageWithType(), error) );
          }

         foreach(var app in Role.AppNames)
          try
          {
            GetAppPackages(app);
          }
          catch(Exception error)
          {
            output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, error.ToMessageWithType(), error) );
          }

         if (this.Dynamic)
         {

           if (!this.ParentZone.ZoneGovernorHosts.Any())
            output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Warning, Catalog, this,
               "Host is dynamic, but there are no zone governors in this zone") );

           if (this.ParentZoneGovernorPrimaryHost()==null)
            output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this,
               "Host is dynamic, but there are no zone governors in either this zone or any parent zones above in the same NOC. Dynamic hosts must operate under zone governors in the same NOC that they are in") );

           var violations = this.Role.AppNames.Intersect(SysConsts.APP_NAMES_FORBIDDEN_ON_DYNAMIC_HOSTS,  Metabank.INVSTRCMP);
           if (violations.Any() )
            output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this,
                       "Host is dynamic, but the assigned role '{0}' contains system process(es): {1} that can not run on dynamic hosts"
                       .Args( this.RoleName, violations.Aggregate(string.Empty, (a, v) => a += "'{0}', ".Args(v) ))
                      ));

         }//dynamic

         if (this.LevelConfig.AttrByName(CONFIG_HOST_ZGOV_LOCK_FAILOVER_ATTR).Exists && !this.IsZGov)
            output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this,
                       "This host has '{0}' attribute defined, however it is not a zone governor host"
                       .Args(CONFIG_HOST_ZGOV_LOCK_FAILOVER_ATTR) )
                      );

      }






      #region .pvt

                    private struct configLevel
                    {
                      public configLevel(object from, IConfigSectionNode node) { From = from; Node = node;}
                      public readonly object From;
                      public readonly IConfigSectionNode Node;
                    }

      //20150403 DKh added support for include pragmas
      private ConfigSectionNode calculateEffectiveAppConfig(string appName)
      {
         var pragmasDisabled = Metabank.RootConfig.AttrByName(Metabank.CONFIG_APP_CONFIG_INCLUDE_PRAGMAS_DISABLED_ATTR).ValueAsBool(false);
         var includePragma = Metabank.RootConfig.AttrByName(Metabank.CONFIG_APP_CONFIG_INCLUDE_PRAGMA_ATTR).Value;


         var conf = new MemoryConfiguration();
         conf.CreateFromNode( Metabank.RootAppConfig );

         var result = conf.Root;
         if (!pragmasDisabled)
            BootConfLoader.ProcessAllExistingIncludes(result, includePragma, "root");

         var derivation = new List<configLevel>();

         #region build derivation chain --------------------------------------------------------------------------
           //Role level
           var role = this.Role;

           if (!role.AppNames.Any(an => INVSTRCMP.Equals(an, appName)))
             throw new MetabaseException(StringConsts.METABASE_HOST_ROLE_APP_MISMATCH_ERROR.Args(appName, RoleName));

           derivation.Add( new configLevel(role, role.AnyAppConfig) );

           //App level
           var app = Metabank.CatalogApp.Applications[appName];
           if (app==null)
             throw new MetabaseException(StringConsts.METABASE_BAD_HOST_APP_ERROR.Args(appName));

           derivation.Add(new configLevel(app, app.AnyAppConfig));

           //Regional level
           var parents = this.ParentSectionsOnPath;
           foreach(var item in parents)
           {
            derivation.Add( new configLevel(item, item.AnyAppConfig) );
            derivation.Add( new configLevel(item, item.GetAppConfig(appName)) );
           }

           //This Level
           derivation.Add( new configLevel(this, this.AnyAppConfig) );
           derivation.Add( new configLevel(this, this.GetAppConfig(appName)) );
         #endregion

        foreach(var clevel in derivation.Where(cl => cl.Node!=null))
        {
          if (!pragmasDisabled)
            BootConfLoader.ProcessAllExistingIncludes((ConfigSectionNode)clevel.Node, includePragma, clevel.From.ToString());

          try
          {
            result.OverrideBy(clevel.Node);
          }
          catch (Exception error)
          {
            throw new MetabaseException(StringConsts.METABASE_EFFECTIVE_APP_CONFIG_OVERRIDE_ERROR.Args(clevel.From.ToString(), error.ToMessageWithType()), error);
          }
        }

         //OS Include
         var include = result[CONFIG_OS_APP_CONFIG_INCLUDE_SECTION];
         if (include.Exists)
         {
           var osInclude = Metabank.GetOSConfNode(this.OS)[CONFIG_APP_CONFIG_SECTION] as ConfigSectionNode;
           if (osInclude.Exists)
            include.Configuration.Include(include, osInclude);
         }

         result.ResetModified();
         return result;
      }

      private IEnumerable<SectionApplication.AppPackage> calculateAppPackages(string appName)
      {
         var role = this.Role;
         if (!role.AppNames.Any(an => INVSTRCMP.Equals(an, appName)))
             throw new MetabaseException(StringConsts.METABASE_HOST_ROLE_APP_MISMATCH_ERROR.Args(appName, RoleName));
         var app = Metabank.CatalogApp.Applications[appName];
         if (app==null)
             throw new MetabaseException(StringConsts.METABASE_BAD_HOST_APP_ERROR.Args(appName));

         return app.MatchPackageBinaries(this.OS);
      }
      #endregion


  }



}}
