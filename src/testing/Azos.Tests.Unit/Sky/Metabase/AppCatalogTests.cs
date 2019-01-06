/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.IO.FileSystem.Local;
using Azos.Scripting;

using Azos.Sky.Metabase;

namespace Azos.Tests.Unit.Sky.Metabase
{
  [Runnable]
  public class AppCatalogTests
  {

      [Run]
      public void AppCat_ApplicationList()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.IsNotNull(mb.CatalogApp);

          Aver.IsTrue( mb.CatalogApp.IsSystem );
          Aver.IsNotNull(mb.CatalogApp.Applications);
          Aver.AreEqual(8, mb.CatalogApp.Applications.Count());

          Aver.AreEqual("WebApp1", mb.CatalogApp.Applications["WebApp1"].Name);
          Aver.AreEqual("AHGov", mb.CatalogApp.Applications["AHGov"].Name);
          Aver.AreEqual("AZGov", mb.CatalogApp.Applications["AZGov"].Name);
          Aver.AreEqual("AGDIDA", mb.CatalogApp.Applications["AGDIDA"].Name);
          Aver.AreEqual("TestApp", mb.CatalogApp.Applications["TestApp"].Name);
          Aver.AreEqual("WinFormsTest", mb.CatalogApp.Applications["WinFormsTest"].Name);
        }
      }

      [Run]
      public void AppCat_ApplicationList_Parallel()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
             (i)=>
             {
                Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(100, 10000));

                Aver.IsNotNull(mb.CatalogApp);

                Aver.IsTrue( mb.CatalogApp.IsSystem );
                Aver.IsNotNull(mb.CatalogApp.Applications);
                Aver.AreEqual(8, mb.CatalogApp.Applications.Count());

                Aver.AreEqual("WebApp1", mb.CatalogApp.Applications["WebApp1"].Name);
                Aver.AreEqual("AHGov", mb.CatalogApp.Applications["AHGov"].Name);
                Aver.AreEqual("AZGov", mb.CatalogApp.Applications["AZGov"].Name);
                Aver.AreEqual("AGDIDA", mb.CatalogApp.Applications["AGDIDA"].Name);
                Aver.AreEqual("TestApp", mb.CatalogApp.Applications["TestApp"].Name);
                Aver.AreEqual("WinFormsTest", mb.CatalogApp.Applications["WinFormsTest"].Name);
             });
        }
      }


      [Run]
      public void AppCat_RoleList()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.IsNotNull(mb.CatalogApp);

          Aver.IsTrue( mb.CatalogApp.IsSystem );
          Aver.IsNotNull(mb.CatalogApp.Roles);
          Aver.AreEqual(6, mb.CatalogApp.Roles.Count());

          Aver.AreEqual("MixedServer",  mb.CatalogApp.Roles["MixedServer"].Name);
          Aver.AreEqual("Single",  mb.CatalogApp.Roles["Single"].Name);
          Aver.AreEqual("ZoneGovernor",  mb.CatalogApp.Roles["ZoneGovernor"].Name);
          Aver.AreEqual("WWWServer",    mb.CatalogApp.Roles["WWWServer"].Name);
          Aver.AreEqual("TestServer",   mb.CatalogApp.Roles["TestServer"].Name);
          Aver.AreEqual("Agdida",   mb.CatalogApp.Roles["Agdida"].Name);
        }
      }

      [Run]
      public void AppCat_RoleList_Parallel()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
             (i)=>
             {
                Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(100, 10000));

                Aver.IsNotNull(mb.CatalogApp);

                Aver.IsTrue( mb.CatalogApp.IsSystem );
                Aver.IsNotNull(mb.CatalogApp.Roles);
                Aver.AreEqual(6, mb.CatalogApp.Roles.Count());

                Aver.AreEqual("MixedServer",  mb.CatalogApp.Roles["MixedServer"].Name);
                Aver.AreEqual("Single",  mb.CatalogApp.Roles["Single"].Name);
                Aver.AreEqual("ZoneGovernor", mb.CatalogApp.Roles["ZoneGovernor"].Name);
                Aver.AreEqual("WWWServer",    mb.CatalogApp.Roles["WWWServer"].Name);
                Aver.AreEqual("TestServer",   mb.CatalogApp.Roles["TestServer"].Name);
                Aver.AreEqual("Agdida",   mb.CatalogApp.Roles["Agdida"].Name);
             });
        }
      }




      [Run]
      public void AppCat_HostAppConfig()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          var host = mb.CatalogReg["us/east/cle/a/ii/wmed0004.h"];

          Aver.AreEqual("Popov", host.AnyAppConfig.AttrByName("clown").Value);
          Aver.AreEqual("Nikulin", host.GetAppConfig("WinFormsTest").AttrByName("clown").Value);
        }
      }

      [Run]
      public void AppCat_HostAppConfig_Parallel()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
             (i)=>
             {
                Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(100, 10000));

                var host = mb.CatalogReg["us/east/cle/a/ii/wmed0004.h"];

                Aver.AreEqual("Popov", host.AnyAppConfig.AttrByName("clown").Value);
                Aver.AreEqual("Nikulin", host.GetAppConfig("WinFormsTest").AttrByName("clown").Value);
             });
        }
      }




      [Run]
      public void AppCat_Packages()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          var app = mb.CatalogApp.Applications["TestApp"];

          var packages = app.Packages.ToList();

          Aver.AreEqual("TestApp", app.Name);
          Aver.AreEqual("App for unit testing", app.Description);

          Aver.AreEqual(3, packages.Count);
          Aver.AreEqual("TestPackage1", packages[0].Name);
          Aver.AreEqual(Metabank.DEFAULT_PACKAGE_VERSION, packages[0].Version);

          Aver.AreEqual("TestPackage2", packages[1].Name);
          Aver.AreEqual("older", packages[1].Version);

          Aver.AreEqual("TestPackage3", packages[2].Name);
          Aver.AreEqual(Metabank.DEFAULT_PACKAGE_VERSION, packages[2].Version);
        }
      }


      [Run]
      public void AppCat_Packages_Parallel()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
             (i)=>
             {
                Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(100, 10000));

                var app = mb.CatalogApp.Applications["TestApp"];

                var packages = app.Packages.ToList();

                Aver.AreEqual("TestApp", app.Name);
                Aver.AreEqual("App for unit testing", app.Description);

                Aver.AreEqual(3, packages.Count);
                Aver.AreEqual("TestPackage1", packages[0].Name);
                Aver.AreEqual(Metabank.DEFAULT_PACKAGE_VERSION, packages[0].Version);

                Aver.AreEqual("TestPackage2", packages[1].Name);
                Aver.AreEqual("older", packages[1].Version);

                Aver.AreEqual("TestPackage3", packages[2].Name);
                Aver.AreEqual(Metabank.DEFAULT_PACKAGE_VERSION, packages[2].Version);
             });
        }
      }




      [Run]
      public void AppCat_MatchPackageBinaries()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          var app = mb.CatalogApp.Applications["TestApp"];

          var bins = app.MatchPackageBinaries("fedora20").ToList();

          Aver.AreEqual(3, bins.Count);

          Aver.AreEqual("TestPackage1", bins[0].Name);
          Aver.AreEqual(Metabank.DEFAULT_PACKAGE_VERSION, bins[0].Version);
          Aver.IsFalse(bins[0].MatchedPackage.IsAnyPlatform);
          Aver.AreEqual("linux64", bins[0].MatchedPackage.Platform);
          Aver.IsTrue(bins[0].MatchedPackage.IsAnyOS);

          Aver.AreEqual("TestPackage2", bins[1].Name);
          Aver.AreEqual("older", bins[1].Version);
          Aver.IsTrue(bins[1].MatchedPackage.IsAnyPlatform);
          Aver.IsTrue(bins[1].MatchedPackage.IsAnyOS);

          Aver.AreEqual("TestPackage3", bins[2].Name);
          Aver.AreEqual(Metabank.DEFAULT_PACKAGE_VERSION, bins[2].Version);
          Aver.IsTrue(bins[2].MatchedPackage.IsAnyPlatform);
          Aver.IsTrue(bins[2].MatchedPackage.IsAnyOS);

        }
      }

      [Run]
      public void AppCat_MatchPackageBinaries_Parallel()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
             (i)=>
             {
              Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(100000, 10000));

              var app = mb.CatalogApp.Applications["TestApp"];

              var bins = app.MatchPackageBinaries("fedora20").ToList();

              Aver.AreEqual(3, bins.Count);

              Aver.AreEqual("TestPackage1", bins[0].Name);
              Aver.AreEqual(Metabank.DEFAULT_PACKAGE_VERSION, bins[0].Version);
              Aver.IsFalse(bins[0].MatchedPackage.IsAnyPlatform);
              Aver.AreEqual("linux64", bins[0].MatchedPackage.Platform);
              Aver.IsTrue(bins[0].MatchedPackage.IsAnyOS);

              Aver.AreEqual("TestPackage2", bins[1].Name);
              Aver.AreEqual("older", bins[1].Version);
              Aver.IsTrue(bins[1].MatchedPackage.IsAnyPlatform);
              Aver.IsTrue(bins[1].MatchedPackage.IsAnyOS);

              Aver.AreEqual("TestPackage3", bins[2].Name);
              Aver.AreEqual(Metabank.DEFAULT_PACKAGE_VERSION, bins[2].Version);
              Aver.IsTrue(bins[2].MatchedPackage.IsAnyPlatform);
              Aver.IsTrue(bins[2].MatchedPackage.IsAnyOS);
             });
        }
      }


  }
}
