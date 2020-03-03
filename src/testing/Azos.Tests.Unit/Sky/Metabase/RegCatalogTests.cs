/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.IO.FileSystem.Local;
using Azos.Scripting;

using Azos.Sky;
using Azos.Sky.Metabase;

namespace Azos.Tests.Unit.Sky.Metabase
{
  [Runnable]
  public class RegCatalogTests : BaseTestRigWithSkyApp
  {
      [Run]
      public void RC_Region_Top_List()
      {
        {
          Aver.IsTrue( Metabase.CatalogReg.IsSystem );

          Aver.IsNotNull(Metabase.CatalogReg.Regions["US"]);
          Aver.IsNotNull(Metabase.CatalogReg.Regions["Europe"]);

          Aver.AreEqual("US", Metabase.CatalogReg.Regions["US"].Name);
          Aver.AreEqual("Europe", Metabase.CatalogReg.Regions["Europe"].Name);
        }
      }


      [Run]
      public void RC_Region_Top_Navigation()
      {
        {
          Aver.AreEqual("US",     Metabase.CatalogReg["/US"].Name);
          Aver.AreEqual("Europe", Metabase.CatalogReg["/Europe"].Name);
        }
      }


      [Run]
      public void RC_Region_Sub_Navigation()
      {
        {
          Aver.AreEqual("East",     Metabase.CatalogReg["/US/East"].Name);
          Aver.AreEqual("East",     Metabase.CatalogReg["/US/East.r"].Name);
          Aver.AreEqual("East",     Metabase.CatalogReg["/US.r/East"].Name);
        }
      }


      [Run]
      public void RC_Region_NOC_Navigation()
      {
        {
          Aver.AreEqual("CLE",     Metabase.CatalogReg["/US/East/CLE"].Name);
          Aver.AreEqual("CLE",     Metabase.CatalogReg["/US/East/CLE.noc"].Name);
          Aver.AreEqual("CLE",     Metabase.CatalogReg["/US.r/East/CLE.noc"].Name);
          Aver.AreEqual("CLE",     Metabase.CatalogReg["/US.r/East.r/CLE.noc"].Name);
        }
      }


      [Run]
      public void RC_Region_Zone_Navigation()
      {
        {
          Aver.AreEqual("I",     Metabase.CatalogReg["/US/East/CLE/A/I"].Name);
          Aver.AreEqual("A",     Metabase.CatalogReg["/US/East/CLE.noc/A.z"].Name);
        }
      }


      [Run]
      public void RC_Region_Host_Navigation()
      {
        {
          Aver.AreEqual("wmed0001", Metabase.CatalogReg["/US/East/CLE/A/I/wmed0001"].Name);
          Aver.AreEqual("wmed0001", Metabase.CatalogReg["/US/East/CLE.noc/A/I/wmed0001"].Name);
        }
      }

      [Run]
      public void RC_Region_PathWithSpaces()
      {
        {
          Aver.AreEqual("wmed0001", Metabase.CatalogReg["/US/East/CLE/A/I/wmed0001"].Name);
          Aver.AreEqual("wmed0001", Metabase.CatalogReg["/US /East/ CLE /A/I/wmed0001 "].Name);
          Aver.AreEqual("wmed0001", Metabase.CatalogReg["/US/    East.r  /CLE.noc  /A/I/wmed0001.h    "].Name);
          Aver.AreEqual("wmed0001", Metabase.CatalogReg["/            US.r /    East.r  /CLE.noc  / A.z / I.z /  wmed0001.h    "].Name);
        }
      }


      [Run]
      public void RC_Region_DynamicHost_Navigation()
      {
        {
          Aver.AreEqual("wlgdyn0001",     Metabase.CatalogReg["/US/East/CLE/A/I/wlgdyn0001"].Name);
          Aver.AreEqual("wlgdyn0001",     Metabase.CatalogReg["/US/East/CLE.noc/A/I/wlgdyn0001.h"].Name);
          Aver.AreEqual("wlgdyn0001",     Metabase.CatalogReg["/US/East/CLE/A/I/wlgdyn0001~5-672C21E7-2950C499BF8DFA68"].Name);
          Aver.AreEqual("wlgdyn0001",     Metabase.CatalogReg["/US/East/CLE.noc/A/I/wlgdyn0001.h~5-672C21E7-2950C499BF8DFA68"].Name);

          Aver.AreEqual("wlgdyn0001",     Metabase.CatalogReg["/US/ East /CLE/A /I /  wlgdyn0001  ~  5-672C21E7-2950C499BF8DFA68"].Name);
        }
      }


      [Run]
      public void RC_Region_Host_RegionPath()
      {
        {
          Aver.AreEqual("/US/East/CLE/A/I/wmed0001", Metabase.CatalogReg["/US/East/CLE/A/I/wmed0001"].RegionPath);
          Aver.AreEqual("/US/East/CLE/A/I/wmed0001", Metabase.CatalogReg["/US.r/East/CLE.noc/a/i/wmed0001.h"].RegionPath);
        }
      }

      [Run]
      public void RC_Region_Host_Caching()
      {
        {
          var h1 =  Metabase.CatalogReg["/US/East/CLE/A/I/wmed0001"];
          var h2 =  Metabase.CatalogReg["/US.r/East/CLE.noc/a/i/wmed0001.h"];

          Aver.IsTrue( object.ReferenceEquals(h1, h2) );
        }
      }


      [Run]
      public void RC_Navigate_Methods()
      {
        {
          Aver.AreEqual("US", Metabase.CatalogReg.NavigateRegion("/US/East").ParentRegion.Name);
          Aver.AreEqual("US", Metabase.CatalogReg.NavigateRegion("/US/East.r").ParentRegion.Name);
          Aver.AreEqual("US", Metabase.CatalogReg.NavigateRegion("/US.r/East.r").ParentRegion.Name);

          Aver.AreEqual("East", Metabase.CatalogReg.NavigateNOC("/US/East/CLE").ParentRegion.Name);
          Aver.AreEqual("CLE",  Metabase.CatalogReg.NavigateNOC("/US/East/CLE.noc").Name);

          Aver.AreEqual("A", Metabase.CatalogReg.NavigateZone("/US/East/CLE/A/I").ParentZone.Name);
          Aver.AreEqual("A", Metabase.CatalogReg.NavigateZone("/US/East/CLE/A.z/I").ParentZone.Name);
          Aver.AreEqual("A", Metabase.CatalogReg.NavigateZone("/US/East/CLE/A/I.z").ParentZone.Name);

          Aver.AreEqual("CLE", Metabase.CatalogReg.NavigateHost("/US/East/CLE/A/I/wmed0001").ParentZone.NOC.Name);
          Aver.AreEqual("CLE", Metabase.CatalogReg.NavigateHost("/US/East/CLE.noc/A.z/I.z/wmed0001.h").ParentZone.NOC.Name);
          Aver.AreEqual("CLE", Metabase.CatalogReg.NavigateHost("/US.r/East/CLE.noc/A/I.z/wmed0001.h").ParentZone.NOC.Name);

        }
      }

      [Run]
      public void RC_ParentSectionsPath()
      {
        {
          var host = Metabase.CatalogReg.NavigateHost("/US.r/East/CLE.noc/A/I.z/wmed0001.h");

          var list = host.ParentSectionsOnPath.ToList();

          Aver.AreEqual(5, list.Count);
          Aver.AreEqual("US",       list[0].Name);
          Aver.AreEqual("East",     list[1].Name);
          Aver.AreEqual("CLE",      list[2].Name);
          Aver.AreEqual("A",        list[3].Name);
          Aver.AreEqual("I",        list[4].Name);
        }
      }


      [Run]
      public void RC_CountMatchingPathSegments()
      {
        {
          Aver.AreEqual(0, Metabase.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I", "Africa/Cairo"));
          Aver.AreEqual(2, Metabase.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I", "US/East/JFK"));

          Aver.AreEqual(4, Metabase.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I/wmed0001", "US/East/CLE/A/II/wmed0005"));
        }
      }


      [Run]
      public void RC_GetMatchingPathSegmentRatio()
      {
        {
          Aver.AreEqual(0.5d, Metabase.CatalogReg.GetMatchingPathSegmentRatio("/US/East/CLE/A", "/US/East/JFK/A"));

          Aver.AreEqual(0d, Metabase.CatalogReg.GetMatchingPathSegmentRatio("Us", "Europe"));
          Aver.AreEqual(1d, Metabase.CatalogReg.GetMatchingPathSegmentRatio("EUROPE", "Europe.r"));
        }
      }


       [Run]
      public void RC_Various_Parallel()
      {
        {
          Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
          (i)=>
          {
                Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(10, 1000));

                Aver.IsTrue( Metabase.CatalogReg.IsSystem );
                Aver.IsNotNull(Metabase.CatalogReg.Regions["US"]);
                Aver.IsNotNull(Metabase.CatalogReg.Regions["Europe"]);
                Aver.AreEqual("US", Metabase.CatalogReg.Regions["US"].Name);
                Aver.AreEqual("Europe", Metabase.CatalogReg.Regions["Europe"].Name);
                Aver.AreEqual("US",     Metabase.CatalogReg["/US"].Name);
                Aver.AreEqual("Europe", Metabase.CatalogReg["/Europe"].Name);
                Aver.AreEqual("East",     Metabase.CatalogReg["/US/East"].Name);
                Aver.AreEqual("East",     Metabase.CatalogReg["/US/East.r"].Name);
                Aver.AreEqual("East",     Metabase.CatalogReg["/US.r/East"].Name);
                Aver.AreEqual("CLE",     Metabase.CatalogReg["/US/East/CLE"].Name);
                Aver.AreEqual("CLE",     Metabase.CatalogReg["/US/East/CLE.noc"].Name);
                Aver.AreEqual("CLE",     Metabase.CatalogReg["/US.r/East/CLE.noc"].Name);
                Aver.AreEqual("CLE",     Metabase.CatalogReg["/US.r/East.r/CLE.noc"].Name);
                Aver.AreEqual("I",     Metabase.CatalogReg["/US/East/CLE/A/I"].Name);
                Aver.AreEqual("A",     Metabase.CatalogReg["/US/East/CLE.noc/A.z"].Name);
                Aver.AreEqual("wmed0001",     Metabase.CatalogReg["/US/East/CLE/A/I/wmed0001"].Name);
                Aver.AreEqual("wmed0001",     Metabase.CatalogReg["/US/East/CLE.noc/A/I/wmed0001"].Name);
                Aver.AreEqual("/US/East/CLE/A/I/wmed0001", Metabase.CatalogReg["/US/East/CLE/A/I/wmed0001"].RegionPath);
                Aver.AreEqual("/US/East/CLE/A/I/wmed0001", Metabase.CatalogReg["/US.r/East/CLE.noc/a/i/wmed0001.h"].RegionPath);
                Aver.AreEqual("US", Metabase.CatalogReg.NavigateRegion("/US/East").ParentRegion.Name);
                Aver.AreEqual("US", Metabase.CatalogReg.NavigateRegion("/US/East.r").ParentRegion.Name);
                Aver.AreEqual("US", Metabase.CatalogReg.NavigateRegion("/US.r/East.r").ParentRegion.Name);

                Aver.AreEqual("East", Metabase.CatalogReg.NavigateNOC("/US/East/CLE").ParentRegion.Name);
                Aver.AreEqual("CLE",  Metabase.CatalogReg.NavigateNOC("/US/East/CLE.noc").Name);

                Aver.AreEqual("A", Metabase.CatalogReg.NavigateZone("/US/East/CLE/A/I").ParentZone.Name);
                Aver.AreEqual("A", Metabase.CatalogReg.NavigateZone("/US/East/CLE/A.z/I").ParentZone.Name);
                Aver.AreEqual("A", Metabase.CatalogReg.NavigateZone("/US/East/CLE/A/I.z").ParentZone.Name);

                Aver.AreEqual("CLE", Metabase.CatalogReg.NavigateHost("/US/East/CLE/A/I/wmed0001").ParentZone.NOC.Name);
                Aver.AreEqual("CLE", Metabase.CatalogReg.NavigateHost("/US/East/CLE.noc/A.z/I.z/wmed0001.h").ParentZone.NOC.Name);
                Aver.AreEqual("CLE", Metabase.CatalogReg.NavigateHost("/US.r/East/CLE.noc/A/I.z/wmed0001.h").ParentZone.NOC.Name);

                var host = Metabase.CatalogReg.NavigateHost("/US.r/East/CLE.noc/A/I.z/wmed0001.h");

                var list = host.ParentSectionsOnPath.ToList();

                Aver.AreEqual(5, list.Count);
                Aver.AreEqual("US",       list[0].Name);
                Aver.AreEqual("East",     list[1].Name);
                Aver.AreEqual("CLE",      list[2].Name);
                Aver.AreEqual("A",        list[3].Name);
                Aver.AreEqual("I",        list[4].Name);

                Aver.AreEqual(0, Metabase.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I", "Africa/Cairo"));
                Aver.AreEqual(2, Metabase.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I", "US/East/JFK"));
                Aver.AreEqual(4, Metabase.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I/wmed0001", "US/East/CLE/A/II/wmed0005"));

                Aver.AreEqual(0.5d, Metabase.CatalogReg.GetMatchingPathSegmentRatio("/US/East/CLE/A", "/US/East/JFK/A"));
                Aver.AreEqual(0d, Metabase.CatalogReg.GetMatchingPathSegmentRatio("Us", "Europe"));
                Aver.AreEqual(1d, Metabase.CatalogReg.GetMatchingPathSegmentRatio("EUROPE", "Europe.r"));

           });
        }
      }




      [Run]
      public void RC_TryNavigateAsFarAsPossible_1()
      {
        {

          int depth;
          var target = Metabase.CatalogReg.TryNavigateAsFarAsPossible("/US/no/nei/nai", out depth);

          Aver.AreEqual("US", target.Name);
          Aver.AreEqual(1, depth);

        }
      }

      [Run]
      public void RC_TryNavigateAsFarAsPossible_2()
      {
        {

          int depth;
          var target = Metabase.CatalogReg.TryNavigateAsFarAsPossible("/us/east/cle/a/i", out depth);

          Aver.AreEqual("i", target.Name);
          Aver.AreEqual("/US/east/cle/a/i", target.RegionPath);
          Aver.AreEqual(5, depth);

        }
      }

      [Run]
      public void RC_TryNavigateAsFarAsPossible_3()
      {
        {

          int depth;
          var target = Metabase.CatalogReg.TryNavigateAsFarAsPossible("US/ East/CLE/A.z/ I.z /wlgdyn0001~5-672C21E7-2950C499BF8DFA68", out depth);

          Aver.AreEqual("wlgdyn0001", target.Name);
          Aver.AreEqual("/US/East/CLE/A/I/wlgdyn0001", target.RegionPath);
          Aver.AreEqual(6, depth);

        }
      }




      [Run]
      public void RC_AllNOCs()
      {
        {

          var nocs = Metabase.CatalogReg.AllNOCs.ToList();

          Aver.AreEqual(7, nocs.Count);

          Aver.IsTrue( nocs.Any(noc=>noc.Name=="Cairo"));
          Aver.IsTrue( nocs.Any(noc=>noc.Name=="AMS"));
          Aver.IsTrue( nocs.Any(noc=>noc.Name=="CLE"));
          Aver.IsTrue( nocs.Any(noc=>noc.Name=="LAX"));
          Aver.IsTrue( nocs.Any(noc=>noc.Name=="JFK"));
          Aver.IsTrue( nocs.Any(noc=>noc.Name=="USNational"));
        }
      }


      [Run]
      public void RC_GetDistanceBetweenPaths_1()
      {
        {

          var d1 = Metabase.CatalogReg.GetDistanceBetweenPaths("/us/west/lax", "/us/east/cle");
          var d2 = Metabase.CatalogReg.GetDistanceBetweenPaths("/us/west/lax", "/us/east/jfk");

          Console.WriteLine("Logical distance between LAX and CLE is {0}km", d1);
          Console.WriteLine("Logical distance between LAX and JFK is {0}km", d2);
          Aver.IsTrue( d1 < d2);
        }
      }

      [Run]
      public void RC_GetDistanceBetweenPaths_2()
      {
        {

          var d1 = Metabase.CatalogReg.GetDistanceBetweenPaths("/europe/west/ams", "/us/east/cle");
          var d2 = Metabase.CatalogReg.GetDistanceBetweenPaths("/europe/west/ams", "/us/east/jfk");
          var d3 = Metabase.CatalogReg.GetDistanceBetweenPaths("/europe/west/ams", "/africa/cairo");

          Console.WriteLine("Logical distance between AMS and CLE is {0}km", d1);
          Console.WriteLine("Logical distance between AMS and JFK is {0}km", d2);
          Console.WriteLine("Logical distance between AMS and Cairo is {0}km", d3);
          Aver.IsTrue( d2 < d1);
          Aver.IsTrue( d3 < d2);
        }
      }


      [Run]
      public void RC_GetDistanceBetweenPaths_Parallel()
      {
        {

          Parallel.For(0, 100000, (i)=>
            {
              var d1 = Metabase.CatalogReg.GetDistanceBetweenPaths("/us/west/lax", "/us/east/cle");
              var d2 = Metabase.CatalogReg.GetDistanceBetweenPaths("/us/west/lax", "/us/east/jfk");

              Aver.IsTrue( d1 < d2);
            });
        }
      }


      [Run]
      public void RC_GeoDistanceBetweenPaths()
      {
        {

          var d1 = Metabase.CatalogReg.GetGeoDistanceBetweenPaths("/europe/west/ams", "/us/east/cle");
          var d2 = Metabase.CatalogReg.GetGeoDistanceBetweenPaths("/europe/west/ams", "/us/east/jfk");
          var d3 = Metabase.CatalogReg.GetGeoDistanceBetweenPaths("/europe/west/ams", "/africa/cairo");
          var d4 = Metabase.CatalogReg.GetGeoDistanceBetweenPaths( "/africa/cairo", "/europe/west/ams");


          Console.WriteLine("Physical geo distance between AMS and CLE is {0}km", d1);
          Console.WriteLine("Physical geo distance between AMS and JFK is {0}km", d2);
          Console.WriteLine("Physical geo distance between AMS and Cairo is {0}km", d3);
          Aver.IsTrue( d2 < d1);
          Aver.IsTrue( d3 < d2);

          Aver.AreEqual( d3, d4);
        }
      }



      [Run]
      public void RC_GetNOCOfPath_1()
      {
        {

          var noc = Metabase.CatalogReg.GetNOCofPath("/us/east/cle/BDB/Alex/UserData/s0/srv0.h");

          Aver.IsTrue( "/us/east/CLE.noc".IsSameRegionPath(noc.RegionPath) );
        }
      }

      [Run]
      public void RC_GetNOCOfPath_2()
      {
        {

          var noc = Metabase.CatalogReg.GetNOCofPath("/us/east/");

          Aver.IsNull(noc);
        }
      }


      [Run]
      public void RC_ArePathsInSameNOC_1()
      {
        {

          Aver.IsNotNull( Metabase.CatalogReg.NavigateHost("/us/east/cle/A/I/wmed0001") );
          Aver.IsNotNull( Metabase.CatalogReg.NavigateHost("/us/east/cle/A/I/wmed0002") );

          Aver.IsTrue( Metabase.CatalogReg.ArePathsInSameNOC("/us/east/cle/A/I/wmed0001", "/us/east/cle/A/I/wmed0002")  );
        }
      }

      [Run]
      public void RC_ArePathsInSameNOC_2()
      {
        {

          Aver.IsNotNull( Metabase.CatalogReg.NavigateHost("/us/east/cle/A/II/wmed0005") );
          Aver.IsNotNull( Metabase.CatalogReg.NavigateHost("/us/east/cle/A/II/wmed0003") );

          Aver.IsTrue( Metabase.CatalogReg.ArePathsInSameNOC("/us/east/cle/A/II/wmed0005", "/us/east/cle/A/II/wmed0003")  );
        }
      }

      [Run]
      public void RC_ArePathsInSameNOC_3()
      {
        {

          Aver.IsNotNull( Metabase.CatalogReg.NavigateHost("/us/east/jfk/a/wmed1024") );
          Aver.IsNotNull( Metabase.CatalogReg.NavigateHost("/us/east/cle/a/i/wmed0001") );

          Aver.IsFalse( Metabase.CatalogReg.ArePathsInSameNOC("/us/east/jfk/a/wmed1024", "/us/east/cle/ai/wmed0001")  );
        }
      }



      [Run]
      public void RC_ParentZoneGov_1()
      {
        {

          var host =  Metabase.CatalogReg.NavigateHost("/US/East/CLE/A/I/wmed0001");
          var zgov = host.ParentZoneGovernorPrimaryHost();

          Aver.AreEqual("zgov0001",  zgov.Name);
          Aver.AreEqual(1,  zgov.ParentZone.ZoneGovernorHosts.Count());
          Aver.AreEqual("zgov0001",  zgov.ParentZone.ZoneGovernorHosts.FirstOrDefault().Name);
          Aver.AreEqual("zgov0001",  zgov.ParentZone.ZoneGovernorPrimaryHost.Name);

          Aver.AreEqual("/US/East/CLE/A/I/zgov0001",  zgov.RegionPath);
          Aver.AreEqual("/US/East/CLE/A/I/zgov0001",  zgov.ParentZone.ZoneGovernorPrimaryHost.RegionPath);
        }
      }

      [Run]
      public void RC_ParentZoneGov_2()
      {
        {
          var host =  Metabase.CatalogReg.NavigateHost("/US/East/CLE/A/I/zgov0001");
          var zgov = host.ParentZoneGovernorPrimaryHost(); //My zone gov is the same level I am in because my process is NOT ZGOV

          Aver.AreEqual("/US/East/CLE/A/I/zgov0001",  zgov.RegionPath);
        }
      }

      [Run]
      public void RC_ParentZoneGov_3()
      {
      {
          var host =  Metabase.CatalogReg.NavigateHost("/US/East/CLE/A/I/zgov0001");
          var zgov = host.ParentZoneGovernorPrimaryHost(); //My zone gov is level higher because my process is ZGOV

          Aver.AreEqual("/US/East/CLE/A/zgov0456",  zgov.RegionPath);
        }
      }

      [Run]
      public void RC_ParentZoneGov_4_TranscendNOC()
      {
      {
          var host =  Metabase.CatalogReg.NavigateHost("/US/East/CLE/A/zgov0456");

          var zgov = host.ParentZoneGovernorPrimaryHost(false); //Do not leave this NOC
          Aver.IsNull( zgov );

          zgov = host.ParentZoneGovernorPrimaryHost(/* default */); //DEFAULT=FALSE Do not leave this NOC
          Aver.IsNull( zgov );

          zgov = host.ParentZoneGovernorPrimaryHost(true); //Transcend this NOC
          Aver.IsNotNull( zgov );
          Aver.AreEqual("/US/USNational/Controller/zgov0891",  zgov.RegionPath);
        }
      }

      [Run]
      public void RC_HasDirectOrIndirectParentZoneGovernor_1()
      {
      {
          var host =  Metabase.CatalogReg.NavigateHost("/US/East/CLE/A/zgov0456");

          Aver.IsFalse( host.HasDirectOrIndirectParentZoneGovernor("/us/usnational/controller/zgov0891") );

          Aver.IsTrue( host.HasDirectOrIndirectParentZoneGovernor("/us/usnational/controller/zgov0891", transcendNOC: true) );//transcend NOC
        }
      }


      [Run]
      public void RC_IsZoneGov()
      {
        {
          Aver.IsFalse(Metabase.CatalogReg.NavigateHost("/US/East/CLE/A/I/wmed0001").IsZGov);
          Aver.IsFalse(Metabase.CatalogReg.NavigateHost("/US/East/CLE/A/I/wmed0002").IsZGov);
          Aver.IsTrue(Metabase.CatalogReg.NavigateHost("/US/East/CLE/A/I/zgov0001").IsZGov);
        }
      }


      [Run]
      public void RC_ParentNOCZone()
      {
        {
          var nocNATIONAL = Metabase.CatalogReg.NavigateNOC("/US/USNational.noc");
          var nocCLE = Metabase.CatalogReg.NavigateNOC("/US/East/Cle.noc");
          var nocLAX = Metabase.CatalogReg.NavigateNOC("/US/West/LAX.noc");
          var nocJFK = Metabase.CatalogReg.NavigateNOC("/US/East/JFK.noc");
          var nocAMS = Metabase.CatalogReg.NavigateNOC("/Europe/West/AMS.noc");

          Aver.IsTrue( nocCLE.ParentNOCZone.NOC.IsLogicallyTheSame( nocNATIONAL ) );
          Aver.IsTrue( nocLAX.ParentNOCZone.NOC.IsLogicallyTheSame( nocNATIONAL ) );
          Aver.IsTrue( nocJFK.ParentNOCZone.NOC.IsLogicallyTheSame( nocNATIONAL ) );

          Aver.IsNull( nocAMS.ParentNOCZone );
          Aver.IsNull( nocAMS.ParentNOCZonePath );

        }
      }


    [Run]
    public void RC_JoinPathSegments()
    {
      Aver.AreEqual("b/c", Metabank.RegCatalog.JoinPathSegments(null, "b", "c"));
      Aver.AreEqual("b/c", Metabank.RegCatalog.JoinPathSegments(string.Empty, "b", "c"));
      Aver.AreEqual("/b/c", Metabank.RegCatalog.JoinPathSegments("/", "b", "c"));
      Aver.AreEqual("/a/b/c", Metabank.RegCatalog.JoinPathSegments("/a", "b", "c"));
      Aver.AreEqual("a/b/c", Metabank.RegCatalog.JoinPathSegments("a", "b", "c"));
      Aver.AreEqual("a/b/c", Metabank.RegCatalog.JoinPathSegments("a", "/b", " c"));
      Aver.AreEqual("a/b/c", Metabank.RegCatalog.JoinPathSegments("a", "b/", "c "));
      Aver.AreEqual("a/b/c", Metabank.RegCatalog.JoinPathSegments("a ", "b/", "/c"));
      Aver.AreEqual("a/b/c", Metabank.RegCatalog.JoinPathSegments("a", "b/", "  /c/   "));
      Aver.AreEqual("a/b/c/d", Metabank.RegCatalog.JoinPathSegments("a", "b/", "  /c/d/   "));
      Aver.AreEqual("/a/b/c/d", Metabank.RegCatalog.JoinPathSegments("/a", "b/", "  /c/d/   "));
      Aver.AreEqual("/a/b/c/d", Metabank.RegCatalog.JoinPathSegments(" /a", "b/", "  /c/d/   "));
      Aver.AreEqual("/a/b/c/d", Metabank.RegCatalog.JoinPathSegments(string.Empty, " / a", "b/", "  /c/d/   "));
    }
  }
}
