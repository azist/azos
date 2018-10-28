using System;
using System.Collections.Generic;
using System.Linq;


using Azos;
using Azos.Conf;
using Azos.IO.FileSystem.Packaging;

using Azos.Sky.Apps.HostGovernor;

namespace Azos.Sky.Metabase{ public sealed partial class Metabank{

    /// <summary>
    /// Represents a system catalog of binary resources
    /// </summary>
    public sealed class BinCatalog : SystemCatalog
    {
      #region Inner

              /// <summary>
              /// Provides information about binary package that includes: name, platform, os, version
              /// </summary>
              public sealed class PackageInfo
              {
                public const string ANY = "any";

                internal PackageInfo(string fullName)
                {
                  if (fullName.IsNullOrWhiteSpace())
                    throw new SkyException(StringConsts.ARGUMENT_ERROR + "PackageInfo.ctor(fullName=null|empty)");

                  var segs = fullName.Split('.');
                  if (segs.Length<4)
                    throw new SkyException(StringConsts.ARGUMENT_ERROR + "PackageInfo.ctor(!name.plat.os.ver)");

                  Name     = string.Join(".", segs, 0, segs.Length-3);
                  Version  = segs[segs.Length-1];
                  OS       = segs[segs.Length-2];
                  Platform = segs[segs.Length-3];

                  if (Name.IsNullOrWhiteSpace() ||
                      Platform.IsNullOrWhiteSpace() ||
                      OS.IsNullOrWhiteSpace() ||
                      Version.IsNullOrWhiteSpace())
                    throw new SkyException(StringConsts.ARGUMENT_ERROR + "PackageInfo.ctor(!name.plat.os.ver)");
                }

                public PackageInfo(string name, string version, string platform = ANY, string os = ANY)
                {
                  if (name.IsNullOrWhiteSpace() || version.IsNullOrWhiteSpace())
                    throw new SkyException(StringConsts.ARGUMENT_ERROR + "PackageInfo.ctor(name|ver=null|empty)");

                  Name = name;
                  Version = version;

                  if (platform.IsNullOrWhiteSpace()) platform = ANY;
                  if (os.IsNullOrWhiteSpace()) os = ANY;

                  Platform = platform;
                  OS = os;
                }

                /// <summary>
                /// Name portion of the package. Note: Name alone DOES NOT uniquely identify package in the catalog
                /// </summary>
                public readonly string Name;
                public readonly string Platform;
                public readonly string OS;
                public readonly string Version;

                public bool IsAnyOS { get { return INVSTRCMP.Equals(OS, ANY);} }
                public bool IsAnyPlatform { get { return INVSTRCMP.Equals(Platform, ANY);} }

                /// <summary>
                /// Returns relative specificity score - the more specific this definition is the higher is the score.
                /// Used for match comparison - finding better matches taking platform and OS into consideration
                /// </summary>
                public int Specificity
                {
                  get
                  {
                    var result = 0;
                    if (!IsAnyPlatform) result+=1000;
                    if (!IsAnyOS) result+=100;
                    return result;
                  }
                }

                /// <summary>
                /// Returns metabase directory full name that this item was constructed from
                /// </summary>
                public string FullName
                {
                  get { return "{0}.{1}.{2}.{3}".Args(Name, Platform, OS, Version);}
                }

                public override string ToString()
                {
 	                 return FullName;
                }

                public override int GetHashCode()
                {
                  return INVSTRCMP.GetHashCode(Name) ^ INVSTRCMP.GetHashCode(Platform) ^ INVSTRCMP.GetHashCode(OS) ^ INVSTRCMP.GetHashCode(Version);
                }

                public override bool Equals(object obj)
                {
                  var other = obj as PackageInfo;
                  if (other==null) return false;

                  return INVSTRCMP.Equals(this.Name,     other.Name) &&
                         INVSTRCMP.Equals(this.Platform, other.Platform)&&
                         INVSTRCMP.Equals(this.OS,       other.OS)&&
                         INVSTRCMP.Equals(this.Version,  other.Version);
                }
              }


      #endregion

      #region .ctor
          internal BinCatalog(Metabank bank) : base(bank, BIN_CATALOG)
          {

          }

      #endregion

      #region Fields


      #endregion

      #region Properties
          /// <summary>
          /// Returns packages in the catalog
          /// </summary>
          public IEnumerable<PackageInfo> Packages
          {
            get
            {
              const string CACHE_KEY = "pnames";
              //1. Try to get from cache
              var result = Metabank.cacheGet(BIN_CATALOG, CACHE_KEY) as IEnumerable<PackageInfo>;
              if (result!=null) return result;

              result =
                Metabank.fsAccess("BinCatalog.Packages.get", BIN_CATALOG,
                      (session, dir) =>
                      {
                        var list = new List<PackageInfo>();
                        foreach(var sdir in dir.SubDirectoryNames)
                        {
                          list.Add( new PackageInfo( sdir ) );
                        }
                        return list;
                      }
                );

              foreach(var pi in result)
              {
                if(!pi.IsAnyPlatform)
                  if (!Metabank.PlatformNames.Any(pn=> Metabank.INVSTRCMP.Equals(pn, pi.Platform)))
                   throw new MetabaseException(StringConsts.METABASE_BIN_PACKAGE_INVALID_PLATFORM_ERROR.Args(pi));

                if(!pi.IsAnyOS)
                  if (!Metabank.OSNames.Any(on=> Metabank.INVSTRCMP.Equals(on, pi.OS)))
                   throw new MetabaseException(StringConsts.METABASE_BIN_PACKAGE_INVALID_OS_ERROR.Args(pi));
              }


              Metabank.cachePut(BIN_CATALOG, CACHE_KEY, result);
              return result;
            }
          }


      #endregion

      #region Public

        public override void Validate(ValidationContext ctx)
        {
           var output = ctx.Output;

           try
           {
             var packages = Packages;
           }
           catch(Exception error)
           {
             output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, this, null, error.ToMessageWithType(), error) );
             return;
           }

           try
           {
                Metabank.fsAccess("BinCatalog.Validate", BIN_CATALOG,
                      (session, dir) =>
                      {
                        foreach(var sdn in dir.SubDirectoryNames)
                          using(var sdir = dir.GetSubDirectory(sdn))
                            using(var mf = sdir.GetFile(ManifestUtils.MANIFEST_FILE_NAME))
                            {
                              if (mf==null)
                              {
                                output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, this, null,
                                                                      StringConsts.METABASE_BIN_PACKAGE_MISSING_MANIFEST_ERROR
                                                                      .Args(sdn, ManifestUtils.MANIFEST_FILE_NAME), null)
                                          );
                                continue;
                              }

                              var manifest = LaconicConfiguration.CreateFromString( mf.ReadAllText()).Root;
                              var computed = ManifestUtils.GeneratePackagingManifest(sdir);

                              if (!ManifestUtils.HasTheSameContent(manifest, computed, oneWay: false))
                               output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, this, null,
                                                                      StringConsts.METABASE_BIN_PACKAGE_OUTDATED_MANIFEST_ERROR
                                                                      .Args(sdn, ManifestUtils.MANIFEST_FILE_NAME), null)
                                          );
                            }

                        return true;
                      }
                );
           }
           catch(Exception error)
           {
             output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, this, null, error.ToMessageWithType(), error) );
             return;
           }

        }


        /// <summary>
        /// Checks the local version and performs local software installation on this machine if needed
        /// This process is an integral part og AHGOV/HostGovernorService implementation and should not be called by developers
        /// </summary>
        /// <returns>True if installation was performed</returns>
        internal bool CheckAndPerformLocalSoftwareInstallation(IList<string> progress, bool force = false)
        {
          return
                Metabank.fsAccess("BinCatalog.CheckAndPerformLocalSoftwareInstallation", BIN_CATALOG,
                      (session, dir) =>
                      {
                        var sw = System.Diagnostics.Stopwatch.StartNew();

                        if (progress!=null)  progress.Add("{0} Building install set...".Args(App.LocalizedTime));

                        var installSet = new List<LocalInstallation.PackageInfo>();
                        foreach(var appPackage in HostGovernorService.Instance.AllPackages)
                        {
                           var subDir = appPackage.MatchedPackage.FullName;
                           var packageDir = dir.GetSubDirectory(subDir);

                           if (progress!=null)
                              progress.Add(" + {0}".Args(appPackage.ToString()));

                           if (packageDir==null)
                             throw new MetabaseException(StringConsts.METABASE_INSTALLATION_BIN_PACKAGE_NOT_FOUND_ERROR.Args(appPackage.Name, subDir));

                           installSet.Add(new LocalInstallation.PackageInfo(appPackage.Name, packageDir, appPackage.Path));
                        }

                        if (progress!=null)
                        {
                          progress.Add("Initiating local installation");
                          progress.Add(" Root Path: {0}".Args(HostGovernorService.Instance.UpdatePath));
                          progress.Add(" Manifest Path: {0}".Args(HostGovernorService.Instance.RunPath));
                          progress.Add(" Force: {0}".Args(force));
                        }

                        var anew = false;
                        using(var install = new LocalInstallation(HostGovernorService.Instance.UpdatePath, HostGovernorService.Instance.RunPath))
                        {
                          anew = install.CheckLocalAndInstallIfNeeded(installSet, force);
                        }
                        if (progress!=null)
                        {
                          progress.Add(" Installed anew: {0}".Args(anew));
                          progress.Add("{0} Done. Duration: {1}".Args(App.LocalizedTime, sw.Elapsed));
                        }
                        return anew;
                      }
                );
        }

      #endregion


    }

}}
