using System;
using System.Collections.Generic;
using System.Linq;

using Azos.IO.FileSystem;

namespace Azos.Sky.Metabase{ public sealed partial class Metabank{

  /// <summary>
  /// Represents metadata for Application Role
  /// </summary>
  public sealed class SectionRole : SectionWithAnyAppConfig<AppCatalog>
  {

            public struct AppInfo
            {
              internal AppInfo(string name, int? autoRun, string exeFile, string exeArgs)
              {
                Name = name;
                AutoRun = autoRun;
                ExeFile = exeFile;
                ExeArgs = exeArgs;
              }

              /// <summary>
              /// Metabase application name
              /// </summary>
              public readonly string Name;

              /// <summary>
              /// Specifies the relative order of start if application should be auto-started by host governor, null otherwise
              /// </summary>
              public readonly int? AutoRun;

              /// <summary>
              /// Specifies the execurtable file
              /// </summary>
              public readonly string ExeFile;

              /// <summary>
              /// Specifies the execurtable file arguments
              /// </summary>
              public readonly string ExeArgs;


              public override string ToString()
              {
                return "AppInfo('{0}', autos: {1}, exe: '{2}'/'{3}')".Args(Name, AutoRun, ExeFile, ExeArgs);
              }

            }




      internal SectionRole(AppCatalog catalog, string name, string path,  FileSystemSession session) : base(catalog, name, path, session)
      {

      }

      public override string RootNodeName
      {
        get { return "role"; }
      }

      /// <summary>
      /// Enumerates all application names that this role has
      /// </summary>
      public IEnumerable<string> AppNames
      {
        get { return Applications.Select(ai=>ai.Name); }
      }

      /// <summary>
      /// Enumerates all application infos declared for this role
      /// </summary>
      public IEnumerable<AppInfo> Applications
      {
        get
        {
           return LevelConfig.Children
                       .Where(n=>n.IsSameName(AppCatalog.CONFIG_APPLICATION_SECTION))
                       .Select(n=>
                              new AppInfo( n.AttrByName(Metabank.CONFIG_NAME_ATTR).ValueAsString(string.Empty).Trim(),
                                           n.AttrByName(Metabank.CONFIG_AUTO_RUN_ATTR).ValueAsNullableInt(),
                                           n.AttrByName(Metabank.CONFIG_EXE_FILE_ATTR).Value,
                                           n.AttrByName(Metabank.CONFIG_EXE_ARGS_ATTR).Value
                                           ));
        }
      }

      public override void Validate(ValidationContext ctx)
      {
         base.Validate(ctx);

         foreach(var appInfo in Applications)
          try
          {
            var appName = appInfo.Name;
            var app = Metabank.CatalogApp.Applications[appName];
            if (app==null)
            {
              ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this,
                                                    StringConsts.METABASE_VALIDATION_ROLE_APP_ERROR.Args(Name, appName)) );
              continue;
            }

            if (appInfo.ExeFile.IsNullOrWhiteSpace() && app.ExeFile.IsNullOrWhiteSpace())
              ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this,
                                                    StringConsts.METABASE_VALIDATION_ROLE_APP_EXE_MISSING_ERROR.Args(Name, appName, Metabank.CONFIG_EXE_FILE_ATTR)) );
          }
          catch(Exception error)
          {
            ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, error.ToMessageWithType(), error) );
          }
      }


  }


}}
