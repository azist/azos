/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Collections;

namespace Azos.Sky.Metabase{ public sealed partial class Metabank{

    /// <summary>
    /// Represents a system catalog of application
    /// </summary>
    public sealed class AppCatalog : SystemCatalog
    {
      #region CONSTS
           public const string CONFIG_APPLICATIONS_SECTION = "applications";
           public const string CONFIG_APPLICATION_SECTION = "application";
           public const string CONFIG_ROLES_SECTION = "roles";
      #endregion

      #region .ctor
          internal AppCatalog(Metabank bank) : base(bank, APP_CATALOG)
          {

          }

      #endregion

      #region Fields


      #endregion

      #region Properties
          /// <summary>
          /// Returns registry of application sections
          /// </summary>
          public IRegistry<SectionApplication> Applications
          {
            get
            {
              const string CACHE_KEY = "apps";
              //1. Try to get from cache
              var result = Metabank.cacheGet(APP_CATALOG, CACHE_KEY) as IRegistry<SectionApplication>;
              if (result!=null) return result;

              result =
                Metabank.fsAccess("AppCatalog.Applications.get", Metabank.FileSystem.CombinePaths(APP_CATALOG, CONFIG_APPLICATIONS_SECTION),
                      (session, dir) =>
                      {
                        var reg = new Registry<SectionApplication>();
                        foreach(var sdir in dir.SubDirectoryNames)
                        {
                          var spath = Metabank.JoinPaths(APP_CATALOG, CONFIG_APPLICATIONS_SECTION, sdir);
                          var region = new SectionApplication(this, sdir, spath, session);
                          reg.Register(region);
                        }
                        return reg;
                      }
                );

              Metabank.cachePut(APP_CATALOG, CACHE_KEY, result);
              return result;
            }
          }

          /// <summary>
          /// Returns registry of role sections
          /// </summary>
          public IRegistry<SectionRole> Roles
          {
            get
            {
              const string CACHE_KEY = "roles";
              //1. Try to get from cache
              var result = Metabank.cacheGet(APP_CATALOG, CACHE_KEY) as IRegistry<SectionRole>;
              if (result!=null) return result;

              result =
                Metabank.fsAccess("AppCatalog.Roles.get", Metabank.FileSystem.CombinePaths(APP_CATALOG, CONFIG_ROLES_SECTION),
                      (session, dir) =>
                      {
                        var reg = new Registry<SectionRole>();
                        foreach(var sdir in dir.SubDirectoryNames)
                        {
                          var spath =  Metabank.JoinPaths(APP_CATALOG, CONFIG_ROLES_SECTION, sdir);
                          var region = new SectionRole(this, sdir, spath, session);
                          reg.Register(region);
                        }
                        return reg;
                      }
                );

              Metabank.cachePut(APP_CATALOG, CACHE_KEY, result);
              return result;
            }
          }

      #endregion

      #region Public
        public override void Validate(ValidationContext ctx)
        {
          foreach(var sapp in Applications)
            sapp.Validate(ctx);

          foreach(var srole in Roles)
            srole.Validate(ctx);

        }

      #endregion


    }

}}
