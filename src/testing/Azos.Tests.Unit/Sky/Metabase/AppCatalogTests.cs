/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azos.Scripting;
using Azos.Sky.Metabase;

namespace Azos.Tests.Unit.Sky.Metabase
{
  [Runnable]
  public class AppCatalogTests : BaseTestRigWithSkyApp
  {

      [Run]
      public void AppCat_ApplicationList()
      {
        var mb = Metabase;
        Aver.IsNotNull(Metabase.CatalogApp);

        Aver.IsTrue( Metabase.CatalogApp.IsSystem );
        Aver.IsNotNull(Metabase.CatalogApp.Applications);
        Aver.AreEqual(8, Metabase.CatalogApp.Applications.Count());

        Aver.AreEqual("WebApp1", Metabase.CatalogApp.Applications["WebApp1"].Name);
        Aver.AreEqual("AHGov", Metabase.CatalogApp.Applications["AHGov"].Name);
        Aver.AreEqual("AZGov", Metabase.CatalogApp.Applications["AZGov"].Name);
        Aver.AreEqual("AGDIDA", Metabase.CatalogApp.Applications["AGDIDA"].Name);
        Aver.AreEqual("TestApp", Metabase.CatalogApp.Applications["TestApp"].Name);
        Aver.AreEqual("WinFormsTest", Metabase.CatalogApp.Applications["WinFormsTest"].Name);
      }

      [Run]
      public void AppCat_ApplicationList_Parallel()
      {
        Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
            (i)=>
            {
              Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(100, 10000));

              Aver.IsNotNull(Metabase.CatalogApp);

              Aver.IsTrue( Metabase.CatalogApp.IsSystem );
              Aver.IsNotNull(Metabase.CatalogApp.Applications);
              Aver.AreEqual(8, Metabase.CatalogApp.Applications.Count());

              Aver.AreEqual("WebApp1", Metabase.CatalogApp.Applications["WebApp1"].Name);
              Aver.AreEqual("AHGov", Metabase.CatalogApp.Applications["AHGov"].Name);
              Aver.AreEqual("AZGov", Metabase.CatalogApp.Applications["AZGov"].Name);
              Aver.AreEqual("AGDIDA", Metabase.CatalogApp.Applications["AGDIDA"].Name);
              Aver.AreEqual("TestApp", Metabase.CatalogApp.Applications["TestApp"].Name);
              Aver.AreEqual("WinFormsTest", Metabase.CatalogApp.Applications["WinFormsTest"].Name);
            });
      }


      [Run]
      public void AppCat_RoleList()
      {
        Aver.IsNotNull(Metabase.CatalogApp);

        Aver.IsTrue( Metabase.CatalogApp.IsSystem );
        Aver.IsNotNull(Metabase.CatalogApp.Roles);
        Aver.AreEqual(6, Metabase.CatalogApp.Roles.Count());

        Aver.AreEqual("MixedServer",  Metabase.CatalogApp.Roles["MixedServer"].Name);
        Aver.AreEqual("Single",  Metabase.CatalogApp.Roles["Single"].Name);
        Aver.AreEqual("ZoneGovernor",  Metabase.CatalogApp.Roles["ZoneGovernor"].Name);
        Aver.AreEqual("WWWServer",    Metabase.CatalogApp.Roles["WWWServer"].Name);
        Aver.AreEqual("TestServer",   Metabase.CatalogApp.Roles["TestServer"].Name);
        Aver.AreEqual("Agdida",   Metabase.CatalogApp.Roles["Agdida"].Name);
      }

      [Run]
      public void AppCat_RoleList_Parallel()
      {
        Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
            (i)=>
            {
              Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(100, 10000));

              Aver.IsNotNull(Metabase.CatalogApp);

              Aver.IsTrue( Metabase.CatalogApp.IsSystem );
              Aver.IsNotNull(Metabase.CatalogApp.Roles);
              Aver.AreEqual(6, Metabase.CatalogApp.Roles.Count());

              Aver.AreEqual("MixedServer",  Metabase.CatalogApp.Roles["MixedServer"].Name);
              Aver.AreEqual("Single",  Metabase.CatalogApp.Roles["Single"].Name);
              Aver.AreEqual("ZoneGovernor", Metabase.CatalogApp.Roles["ZoneGovernor"].Name);
              Aver.AreEqual("WWWServer",    Metabase.CatalogApp.Roles["WWWServer"].Name);
              Aver.AreEqual("TestServer",   Metabase.CatalogApp.Roles["TestServer"].Name);
              Aver.AreEqual("Agdida",   Metabase.CatalogApp.Roles["Agdida"].Name);
            });
      }




      [Run]
      public void AppCat_HostAppConfig()
      {
        var host = Metabase.CatalogReg["us/east/cle/a/ii/wmed0004.h"];

        Aver.AreEqual("Popov", host.AnyAppConfig.AttrByName("clown").Value);
        Aver.AreEqual("Nikulin", host.GetAppConfig("WinFormsTest").AttrByName("clown").Value);
      }

      [Run]
      public void AppCat_HostAppConfig_Parallel()
      {
        Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
            (i)=>
            {
              Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(100, 10000));

              var host = Metabase.CatalogReg["us/east/cle/a/ii/wmed0004.h"];

              Aver.AreEqual("Popov", host.AnyAppConfig.AttrByName("clown").Value);
              Aver.AreEqual("Nikulin", host.GetAppConfig("WinFormsTest").AttrByName("clown").Value);
            });
      }




      [Run]
      public void AppCat_Packages()
      {
        var app = Metabase.CatalogApp.Applications["TestApp"];

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


      [Run]
      public void AppCat_Packages_Parallel()
      {
        Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
            (i)=>
            {
              Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(100, 10000));

              var app = Metabase.CatalogApp.Applications["TestApp"];

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




      [Run]
      public void AppCat_MatchPackageBinaries()
      {
        var app = Metabase.CatalogApp.Applications["TestApp"];

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

      [Run]
      public void AppCat_MatchPackageBinaries_Parallel()
      {
        Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
            (i)=>
            {
            Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(100000, 10000));

            var app = Metabase.CatalogApp.Applications["TestApp"];

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
