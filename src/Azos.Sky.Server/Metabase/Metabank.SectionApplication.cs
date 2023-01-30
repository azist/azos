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
  /// Represents metadata for Application
  /// </summary>
  public sealed class SectionApplication : SectionWithAnyAppConfig<AppCatalog>
  {

            public sealed class AppPackage : IEquatable<AppPackage>
            {
              internal AppPackage(string name, string version, string path)
              {
                Name = name.IsNullOrWhiteSpace() ? SysConsts.NULL : name;
                Version = version.IsNullOrWhiteSpace() ? Metabank.DEFAULT_PACKAGE_VERSION : version;
                Path = path ?? string.Empty;
              }

              internal AppPackage(AppPackage other, BinCatalog.PackageInfo matchedPackage)
              {
                Name = other.Name;
                Version = other.Version;
                Path = other.Path;
                MatchedPackage = matchedPackage;
              }

              public readonly string Name;
              public readonly string Version;
              public readonly string Path;

              /// <summary>
              /// The best matching binary package, or null
              /// </summary>
              public readonly BinCatalog.PackageInfo MatchedPackage;


              public override bool Equals(object obj)
              {
                return this.Equals((AppPackage)obj);
              }

              public bool Equals(AppPackage other)
              {
                return INVSTRCMP.Equals(Name, other.Name);
              }

              public override int GetHashCode()
              {
                return INVSTRCMP.GetHashCode(Name);
              }

              public override string ToString()
              {
                return "{0} {1} {2} {3}".Args(Name, Version, Path, MatchedPackage);
              }
            }



      internal SectionApplication(AppCatalog catalog, string name, string path,  FileSystemSession session) : base(catalog, name, path, session)
      {

      }



      public override string RootNodeName
      {
        get { return "application"; }
      }

      /// <summary>
      /// Enumerates all packages that this application is comprised of
      /// </summary>
      public IEnumerable<AppPackage> Packages
      {
         get
         {
           var npackages = this.LevelConfig[CONFIG_PACKAGES_SECTION];

           return npackages.Children.Where(c=>c.IsSameName(CONFIG_PACKAGE_SECTION))
                                    .Select(c => new AppPackage( c.AttrByName(CONFIG_NAME_ATTR).ValueAsString(string.Empty).Trim(),
                                                                 c.AttrByName(CONFIG_VERSION_ATTR).ValueAsString(Metabank.DEFAULT_PACKAGE_VERSION).Trim(),
                                                                 c.AttrByName(CONFIG_PATH_ATTR).Value)
                                           );
         }
      }


      /// <summary>
      /// Returns application executable command used for app start
      /// </summary>
      public string ExeFile
      {
        get
        {
          return this.LevelConfig.AttrByName(Metabank.CONFIG_EXE_FILE_ATTR).ValueAsString(string.Empty);
        }
      }

      /// <summary>
      /// Returns application executable command arguments used for app start
      /// </summary>
      public string ExeArgs
      {
        get
        {
          return this.LevelConfig.AttrByName(Metabank.CONFIG_EXE_ARGS_ATTR).ValueAsString(string.Empty);
        }
      }



      public override void Validate(ValidationContext ctx)
      {
         base.Validate(ctx);

         if (!Packages.Any())
           ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Warning, Catalog, this, ServerStringConsts.METABASE_NO_APP_PACKAGES_WARNING.Args(Name) ) );

         if (Packages.Count() != Packages.Distinct().Count())
           ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, ServerStringConsts.METABASE_APP_PACKAGE_REDECLARED_ERROR.Args(Name) ) );

         foreach(var ap in Packages)
         {
            if (ap.Name.IsNullOrEmpty())
            {
             ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, ServerStringConsts.METABASE_APP_PACKAGE_BLANK_NAME_ERROR.Args(Name) ) );
             continue;
            }

            var refed = Metabank.CatalogBin.Packages.Where(pi=> Metabank.INVSTRCMP.Equals(pi.Name, ap.Name)).Select(pi=>pi.Name).FirstOrDefault();

            if (refed==null)
             ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this, ServerStringConsts.METABASE_APP_PACKAGE_NOT_FOUND_IN_BIN_CATALOG_ERROR.Args(Name, ap.Name) ) );
         }
      }

      /// <summary>
      /// Returns the best suitable package binaries for this application - packages that match the specified OS
      /// </summary>
      /// <param name="os">OS to match</param>
      /// <returns>Best suitable packages</returns>
      public IEnumerable<AppPackage> MatchPackageBinaries(string os)
      {
        var platform = Metabank.GetOSPlatformName(os);

        var result = new List<AppPackage>();
        foreach(var ap in Packages)
        {
            var match = Metabank.CatalogBin.Packages.Where(pi => Metabank.INVSTRCMP.Equals(pi.Name, ap.Name) &&
                                                                 Metabank.INVSTRCMP.Equals(pi.Version, ap.Version) &&
                                                                (pi.IsAnyPlatform || Metabank.INVSTRCMP.Equals(pi.Platform, platform)) &&
                                                                (pi.IsAnyOS || Metabank.INVSTRCMP.Equals(pi.OS, os))
                                                         ).OrderBy(pi => -pi.Specificity).FirstOrDefault();//highest first
            if (match==null)
             throw new MetabaseException(ServerStringConsts.METABASE_APP_DOES_NOT_HAVE_MATCHING_BIN_ERROR.Args(Name, ap.Name, ap.Version, os));

            result.Add( new AppPackage(ap, match));
        }
        return result;
      }

  }


}}
