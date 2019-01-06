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
using Azos.Sky.Apps;
using Azos.Sky.Metabase;

namespace Azos.Tests.Unit.Sky.Metabase
{
  [Runnable]
  public class RegCatalogTests
  {
      [Run]
      public void RC_Region_Top_List()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.IsTrue( mb.CatalogReg.IsSystem );

          Aver.IsNotNull(mb.CatalogReg.Regions["US"]);
          Aver.IsNotNull(mb.CatalogReg.Regions["Europe"]);

          Aver.AreEqual("US", mb.CatalogReg.Regions["US"].Name);
          Aver.AreEqual("Europe", mb.CatalogReg.Regions["Europe"].Name);
        }
      }


      [Run]
      public void RC_Region_Top_Navigation()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual("US",     mb.CatalogReg["/US"].Name);
          Aver.AreEqual("Europe", mb.CatalogReg["/Europe"].Name);
        }
      }


      [Run]
      public void RC_Region_Sub_Navigation()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual("East",     mb.CatalogReg["/US/East"].Name);
          Aver.AreEqual("East",     mb.CatalogReg["/US/East.r"].Name);
          Aver.AreEqual("East",     mb.CatalogReg["/US.r/East"].Name);
        }
      }


      [Run]
      public void RC_Region_NOC_Navigation()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual("CLE",     mb.CatalogReg["/US/East/CLE"].Name);
          Aver.AreEqual("CLE",     mb.CatalogReg["/US/East/CLE.noc"].Name);
          Aver.AreEqual("CLE",     mb.CatalogReg["/US.r/East/CLE.noc"].Name);
          Aver.AreEqual("CLE",     mb.CatalogReg["/US.r/East.r/CLE.noc"].Name);
        }
      }


      [Run]
      public void RC_Region_Zone_Navigation()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual("I",     mb.CatalogReg["/US/East/CLE/A/I"].Name);
          Aver.AreEqual("A",     mb.CatalogReg["/US/East/CLE.noc/A.z"].Name);
        }
      }


      [Run]
      public void RC_Region_Host_Navigation()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual("wmed0001", mb.CatalogReg["/US/East/CLE/A/I/wmed0001"].Name);
          Aver.AreEqual("wmed0001", mb.CatalogReg["/US/East/CLE.noc/A/I/wmed0001"].Name);
        }
      }

      [Run]
      public void RC_Region_PathWithSpaces()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual("wmed0001", mb.CatalogReg["/US/East/CLE/A/I/wmed0001"].Name);
          Aver.AreEqual("wmed0001", mb.CatalogReg["/US /East/ CLE /A/I/wmed0001 "].Name);
          Aver.AreEqual("wmed0001", mb.CatalogReg["/US/    East.r  /CLE.noc  /A/I/wmed0001.h    "].Name);
          Aver.AreEqual("wmed0001", mb.CatalogReg["/            US.r /    East.r  /CLE.noc  / A.z / I.z /  wmed0001.h    "].Name);
        }
      }


      [Run]
      public void RC_Region_DynamicHost_Navigation()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual("wlgdyn0001",     mb.CatalogReg["/US/East/CLE/A/I/wlgdyn0001"].Name);
          Aver.AreEqual("wlgdyn0001",     mb.CatalogReg["/US/East/CLE.noc/A/I/wlgdyn0001.h"].Name);
          Aver.AreEqual("wlgdyn0001",     mb.CatalogReg["/US/East/CLE/A/I/wlgdyn0001~5-672C21E7-2950C499BF8DFA68"].Name);
          Aver.AreEqual("wlgdyn0001",     mb.CatalogReg["/US/East/CLE.noc/A/I/wlgdyn0001.h~5-672C21E7-2950C499BF8DFA68"].Name);

          Aver.AreEqual("wlgdyn0001",     mb.CatalogReg["/US/ East /CLE/A /I /  wlgdyn0001  ~  5-672C21E7-2950C499BF8DFA68"].Name);
        }
      }


      [Run]
      public void RC_Region_Host_RegionPath()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual("/US/East/CLE/A/I/wmed0001", mb.CatalogReg["/US/East/CLE/A/I/wmed0001"].RegionPath);
          Aver.AreEqual("/US/East/CLE/A/I/wmed0001", mb.CatalogReg["/US.r/East/CLE.noc/a/i/wmed0001.h"].RegionPath);
        }
      }

      [Run]
      public void RC_Region_Host_Caching()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          var h1 =  mb.CatalogReg["/US/East/CLE/A/I/wmed0001"];
          var h2 =  mb.CatalogReg["/US.r/East/CLE.noc/a/i/wmed0001.h"];

          Aver.IsTrue( object.ReferenceEquals(h1, h2) );
        }
      }


      [Run]
      public void RC_Navigate_Methods()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual("US", mb.CatalogReg.NavigateRegion("/US/East").ParentRegion.Name);
          Aver.AreEqual("US", mb.CatalogReg.NavigateRegion("/US/East.r").ParentRegion.Name);
          Aver.AreEqual("US", mb.CatalogReg.NavigateRegion("/US.r/East.r").ParentRegion.Name);

          Aver.AreEqual("East", mb.CatalogReg.NavigateNOC("/US/East/CLE").ParentRegion.Name);
          Aver.AreEqual("CLE",  mb.CatalogReg.NavigateNOC("/US/East/CLE.noc").Name);

          Aver.AreEqual("A", mb.CatalogReg.NavigateZone("/US/East/CLE/A/I").ParentZone.Name);
          Aver.AreEqual("A", mb.CatalogReg.NavigateZone("/US/East/CLE/A.z/I").ParentZone.Name);
          Aver.AreEqual("A", mb.CatalogReg.NavigateZone("/US/East/CLE/A/I.z").ParentZone.Name);

          Aver.AreEqual("CLE", mb.CatalogReg.NavigateHost("/US/East/CLE/A/I/wmed0001").ParentZone.NOC.Name);
          Aver.AreEqual("CLE", mb.CatalogReg.NavigateHost("/US/East/CLE.noc/A.z/I.z/wmed0001.h").ParentZone.NOC.Name);
          Aver.AreEqual("CLE", mb.CatalogReg.NavigateHost("/US.r/East/CLE.noc/A/I.z/wmed0001.h").ParentZone.NOC.Name);

        }
      }

      [Run]
      public void RC_ParentSectionsPath()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          var host = mb.CatalogReg.NavigateHost("/US.r/East/CLE.noc/A/I.z/wmed0001.h");

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
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual(0, mb.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I", "Africa/Cairo"));
          Aver.AreEqual(2, mb.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I", "US/East/JFK"));

          Aver.AreEqual(4, mb.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I/wmed0001", "US/East/CLE/A/II/wmed0005"));
        }
      }


      [Run]
      public void RC_GetMatchingPathSegmentRatio()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual(0.5d, mb.CatalogReg.GetMatchingPathSegmentRatio("/US/East/CLE/A", "/US/East/JFK/A"));

          Aver.AreEqual(0d, mb.CatalogReg.GetMatchingPathSegmentRatio("Us", "Europe"));
          Aver.AreEqual(1d, mb.CatalogReg.GetMatchingPathSegmentRatio("EUROPE", "Europe.r"));
        }
      }


       [Run]
      public void RC_Various_Parallel()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Parallel.For(0, TestSources.PARALLEL_LOOP_TO,
          (i)=>
          {
                Thread.SpinWait(Ambient.Random.NextScaledRandomInteger(10, 1000));

                Aver.IsTrue( mb.CatalogReg.IsSystem );
                Aver.IsNotNull(mb.CatalogReg.Regions["US"]);
                Aver.IsNotNull(mb.CatalogReg.Regions["Europe"]);
                Aver.AreEqual("US", mb.CatalogReg.Regions["US"].Name);
                Aver.AreEqual("Europe", mb.CatalogReg.Regions["Europe"].Name);
                Aver.AreEqual("US",     mb.CatalogReg["/US"].Name);
                Aver.AreEqual("Europe", mb.CatalogReg["/Europe"].Name);
                Aver.AreEqual("East",     mb.CatalogReg["/US/East"].Name);
                Aver.AreEqual("East",     mb.CatalogReg["/US/East.r"].Name);
                Aver.AreEqual("East",     mb.CatalogReg["/US.r/East"].Name);
                Aver.AreEqual("CLE",     mb.CatalogReg["/US/East/CLE"].Name);
                Aver.AreEqual("CLE",     mb.CatalogReg["/US/East/CLE.noc"].Name);
                Aver.AreEqual("CLE",     mb.CatalogReg["/US.r/East/CLE.noc"].Name);
                Aver.AreEqual("CLE",     mb.CatalogReg["/US.r/East.r/CLE.noc"].Name);
                Aver.AreEqual("I",     mb.CatalogReg["/US/East/CLE/A/I"].Name);
                Aver.AreEqual("A",     mb.CatalogReg["/US/East/CLE.noc/A.z"].Name);
                Aver.AreEqual("wmed0001",     mb.CatalogReg["/US/East/CLE/A/I/wmed0001"].Name);
                Aver.AreEqual("wmed0001",     mb.CatalogReg["/US/East/CLE.noc/A/I/wmed0001"].Name);
                Aver.AreEqual("/US/East/CLE/A/I/wmed0001", mb.CatalogReg["/US/East/CLE/A/I/wmed0001"].RegionPath);
                Aver.AreEqual("/US/East/CLE/A/I/wmed0001", mb.CatalogReg["/US.r/East/CLE.noc/a/i/wmed0001.h"].RegionPath);
                Aver.AreEqual("US", mb.CatalogReg.NavigateRegion("/US/East").ParentRegion.Name);
                Aver.AreEqual("US", mb.CatalogReg.NavigateRegion("/US/East.r").ParentRegion.Name);
                Aver.AreEqual("US", mb.CatalogReg.NavigateRegion("/US.r/East.r").ParentRegion.Name);

                Aver.AreEqual("East", mb.CatalogReg.NavigateNOC("/US/East/CLE").ParentRegion.Name);
                Aver.AreEqual("CLE",  mb.CatalogReg.NavigateNOC("/US/East/CLE.noc").Name);

                Aver.AreEqual("A", mb.CatalogReg.NavigateZone("/US/East/CLE/A/I").ParentZone.Name);
                Aver.AreEqual("A", mb.CatalogReg.NavigateZone("/US/East/CLE/A.z/I").ParentZone.Name);
                Aver.AreEqual("A", mb.CatalogReg.NavigateZone("/US/East/CLE/A/I.z").ParentZone.Name);

                Aver.AreEqual("CLE", mb.CatalogReg.NavigateHost("/US/East/CLE/A/I/wmed0001").ParentZone.NOC.Name);
                Aver.AreEqual("CLE", mb.CatalogReg.NavigateHost("/US/East/CLE.noc/A.z/I.z/wmed0001.h").ParentZone.NOC.Name);
                Aver.AreEqual("CLE", mb.CatalogReg.NavigateHost("/US.r/East/CLE.noc/A/I.z/wmed0001.h").ParentZone.NOC.Name);

                var host = mb.CatalogReg.NavigateHost("/US.r/East/CLE.noc/A/I.z/wmed0001.h");

                var list = host.ParentSectionsOnPath.ToList();

                Aver.AreEqual(5, list.Count);
                Aver.AreEqual("US",       list[0].Name);
                Aver.AreEqual("East",     list[1].Name);
                Aver.AreEqual("CLE",      list[2].Name);
                Aver.AreEqual("A",        list[3].Name);
                Aver.AreEqual("I",        list[4].Name);

                Aver.AreEqual(0, mb.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I", "Africa/Cairo"));
                Aver.AreEqual(2, mb.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I", "US/East/JFK"));
                Aver.AreEqual(4, mb.CatalogReg.CountMatchingPathSegments("US/East/CLE/A/I/wmed0001", "US/East/CLE/A/II/wmed0005"));

                Aver.AreEqual(0.5d, mb.CatalogReg.GetMatchingPathSegmentRatio("/US/East/CLE/A", "/US/East/JFK/A"));
                Aver.AreEqual(0d, mb.CatalogReg.GetMatchingPathSegmentRatio("Us", "Europe"));
                Aver.AreEqual(1d, mb.CatalogReg.GetMatchingPathSegmentRatio("EUROPE", "Europe.r"));

           });
        }
      }




      [Run]
      public void RC_TryNavigateAsFarAsPossible_1()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          int depth;
          var target = mb.CatalogReg.TryNavigateAsFarAsPossible("/US/no/nei/nai", out depth);

          Aver.AreEqual("US", target.Name);
          Aver.AreEqual(1, depth);

        }
      }

      [Run]
      public void RC_TryNavigateAsFarAsPossible_2()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          int depth;
          var target = mb.CatalogReg.TryNavigateAsFarAsPossible("/us/east/cle/a/i", out depth);

          Aver.AreEqual("i", target.Name);
          Aver.AreEqual("/US/east/cle/a/i", target.RegionPath);
          Aver.AreEqual(5, depth);

        }
      }

      [Run]
      public void RC_TryNavigateAsFarAsPossible_3()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          int depth;
          var target = mb.CatalogReg.TryNavigateAsFarAsPossible("US/ East/CLE/A.z/ I.z /wlgdyn0001~5-672C21E7-2950C499BF8DFA68", out depth);

          Aver.AreEqual("wlgdyn0001", target.Name);
          Aver.AreEqual("/US/East/CLE/A/I/wlgdyn0001", target.RegionPath);
          Aver.AreEqual(6, depth);

        }
      }




      [Run]
      public void RC_AllNOCs()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          var nocs = mb.CatalogReg.AllNOCs.ToList();

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
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          var d1 = mb.CatalogReg.GetDistanceBetweenPaths("/us/west/lax", "/us/east/cle");
          var d2 = mb.CatalogReg.GetDistanceBetweenPaths("/us/west/lax", "/us/east/jfk");

          Console.WriteLine("Logical distance between LAX and CLE is {0}km", d1);
          Console.WriteLine("Logical distance between LAX and JFK is {0}km", d2);
          Aver.IsTrue( d1 < d2);
        }
      }

      [Run]
      public void RC_GetDistanceBetweenPaths_2()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          var d1 = mb.CatalogReg.GetDistanceBetweenPaths("/europe/west/ams", "/us/east/cle");
          var d2 = mb.CatalogReg.GetDistanceBetweenPaths("/europe/west/ams", "/us/east/jfk");
          var d3 = mb.CatalogReg.GetDistanceBetweenPaths("/europe/west/ams", "/africa/cairo");

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
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          Parallel.For(0, 100000, (i)=>
            {
              var d1 = mb.CatalogReg.GetDistanceBetweenPaths("/us/west/lax", "/us/east/cle");
              var d2 = mb.CatalogReg.GetDistanceBetweenPaths("/us/west/lax", "/us/east/jfk");

              Aver.IsTrue( d1 < d2);
            });
        }
      }


      [Run]
      public void RC_GeoDistanceBetweenPaths()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          var d1 = mb.CatalogReg.GetGeoDistanceBetweenPaths("/europe/west/ams", "/us/east/cle");
          var d2 = mb.CatalogReg.GetGeoDistanceBetweenPaths("/europe/west/ams", "/us/east/jfk");
          var d3 = mb.CatalogReg.GetGeoDistanceBetweenPaths("/europe/west/ams", "/africa/cairo");
          var d4 = mb.CatalogReg.GetGeoDistanceBetweenPaths( "/africa/cairo", "/europe/west/ams");


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
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          var noc = mb.CatalogReg.GetNOCofPath("/us/east/cle/BDB/Alex/UserData/s0/srv0.h");

          Aver.IsTrue( "/us/east/CLE.noc".IsSameRegionPath(noc.RegionPath) );
        }
      }

      [Run]
      public void RC_GetNOCOfPath_2()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          var noc = mb.CatalogReg.GetNOCofPath("/us/east/");

          Aver.IsNull(noc);
        }
      }


      [Run]
      public void RC_ArePathsInSameNOC_1()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          Aver.IsNotNull( mb.CatalogReg.NavigateHost("/us/east/cle/A/I/wmed0001") );
          Aver.IsNotNull( mb.CatalogReg.NavigateHost("/us/east/cle/A/I/wmed0002") );

          Aver.IsTrue( mb.CatalogReg.ArePathsInSameNOC("/us/east/cle/A/I/wmed0001", "/us/east/cle/A/I/wmed0002")  );
        }
      }

      [Run]
      public void RC_ArePathsInSameNOC_2()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          Aver.IsNotNull( mb.CatalogReg.NavigateHost("/us/east/cle/A/II/wmed0005") );
          Aver.IsNotNull( mb.CatalogReg.NavigateHost("/us/east/cle/A/II/wmed0003") );

          Aver.IsTrue( mb.CatalogReg.ArePathsInSameNOC("/us/east/cle/A/II/wmed0005", "/us/east/cle/A/II/wmed0003")  );
        }
      }

      [Run]
      public void RC_ArePathsInSameNOC_3()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          Aver.IsNotNull( mb.CatalogReg.NavigateHost("/us/east/jfk/a/wmed1024") );
          Aver.IsNotNull( mb.CatalogReg.NavigateHost("/us/east/cle/a/i/wmed0001") );

          Aver.IsFalse( mb.CatalogReg.ArePathsInSameNOC("/us/east/jfk/a/wmed1024", "/us/east/cle/ai/wmed0001")  );
        }
      }



      [Run]
      public void RC_ParentZoneGov_1()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {

          var host =  mb.CatalogReg.NavigateHost("/US/East/CLE/A/I/wmed0001");
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
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using(new SkyApplication(NOPApplication.Instance, SystemApplicationType.TestRig, mb, "/US/East/CLE/A/I/wmed0001", true, null, null))
        {
          var host =  mb.CatalogReg.NavigateHost("/US/East/CLE/A/I/zgov0001");
          var zgov = host.ParentZoneGovernorPrimaryHost(); //My zone gov is the same level I am in because my process is NOT ZGOV

          Aver.AreEqual("/US/East/CLE/A/I/zgov0001",  zgov.RegionPath);
        }
      }

      [Run]
      public void RC_ParentZoneGov_3()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
      using (new SkyApplication(NOPApplication.Instance, SystemApplicationType.TestRig, mb, "/US/East/CLE/A/I/wmed0001", true, null, null))
      {
          var host =  mb.CatalogReg.NavigateHost("/US/East/CLE/A/I/zgov0001");
          var zgov = host.ParentZoneGovernorPrimaryHost(); //My zone gov is level higher because my process is ZGOV

          Aver.AreEqual("/US/East/CLE/A/zgov0456",  zgov.RegionPath);
        }
      }

      [Run]
      public void RC_ParentZoneGov_4_TranscendNOC()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
      using (new SkyApplication(NOPApplication.Instance, SystemApplicationType.TestRig, mb, "/US/East/CLE/A/I/wmed0001", true, null, null))
      {
          var host =  mb.CatalogReg.NavigateHost("/US/East/CLE/A/zgov0456");

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
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
      using (new SkyApplication(NOPApplication.Instance, SystemApplicationType.TestRig, mb, "/US/East/CLE/A/I/wmed0001", true, null, null))
      {
          var host =  mb.CatalogReg.NavigateHost("/US/East/CLE/A/zgov0456");

          Aver.IsFalse( host.HasDirectOrIndirectParentZoneGovernor("/us/usnational/controller/zgov0891") );

          Aver.IsTrue( host.HasDirectOrIndirectParentZoneGovernor("/us/usnational/controller/zgov0891", transcendNOC: true) );//transcend NOC
        }
      }


      [Run]
      public void RC_IsZoneGov()
      {
        using (var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using (new SkyApplication(NOPApplication.Instance, SystemApplicationType.TestRig, mb, "/US/East/CLE/A/I/wmed0001", true, null, null))
        {
          Aver.IsFalse(mb.CatalogReg.NavigateHost("/US/East/CLE/A/I/wmed0001").IsZGov);
          Aver.IsFalse(mb.CatalogReg.NavigateHost("/US/East/CLE/A/I/wmed0002").IsZGov);
          Aver.IsTrue(mb.CatalogReg.NavigateHost("/US/East/CLE/A/I/zgov0001").IsZGov);
        }
      }


      [Run]
      public void RC_ParentNOCZone()
      {
        using (var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using (new SkyApplication(NOPApplication.Instance, SystemApplicationType.TestRig, mb, "/US/East/CLE/A/I/wmed0001", true, null, null))
        {
          var nocNATIONAL = mb.CatalogReg.NavigateNOC("/US/USNational.noc");
          var nocCLE = mb.CatalogReg.NavigateNOC("/US/East/Cle.noc");
          var nocLAX = mb.CatalogReg.NavigateNOC("/US/West/LAX.noc");
          var nocJFK = mb.CatalogReg.NavigateNOC("/US/East/JFK.noc");
          var nocAMS = mb.CatalogReg.NavigateNOC("/Europe/West/AMS.noc");

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
