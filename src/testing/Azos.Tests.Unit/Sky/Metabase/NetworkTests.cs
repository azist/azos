/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azos.IO.FileSystem.Local;
using Azos.Scripting;

using Azos.Sky.Metabase;

namespace Azos.Tests.Unit.Sky.Metabase
{
  [Runnable]
  public class NetworkTests : BaseTestRigWithSkyApp
  {

      [Run]
      public void NET_Description()
      {
         Aver.AreEqual("NOC gov network", Metabase.GetNetworkDescription("NOCGOV"));
      }

      [Run]
      public void NET_Scope()
      {
          Aver.IsTrue(NetworkScope.NOC == Metabase.GetNetworkScope("NOCGOV"));
      }

      [Run]
      public void NET_List()
      {
          var names = Metabase.NetworkNames;
          names.ForEach(n=>Console.WriteLine(n));
          Aver.IsTrue(names.SequenceEqual(new List<string>{"nocgov","internoc","shard","utesting"}  ));
      }


      [Run]
      public void NET_SvcList()
      {
          var names = Metabase.GetNetworkSvcNames("internoc");
          names.ForEach(n=>Console.WriteLine(n));
          Aver.IsTrue(names.SequenceEqual(new List<string>{"socialgraphtodoqueue", "webman-azgov","webman-ahgov","zgov","gdida"}  ));
      }

      [Run]
      public void NET_GroupList()
      {
          var names = Metabase.GetNetworkGroupNames("nocgov");
          Aver.IsTrue(names.SequenceEqual(new List<string>{"any","any2"}  ));
      }

      [Run]
      public void NET_SvcBindingsList()
      {
          var names = Metabase.GetNetworkSvcBindingNames("internoc", "zgov");
          Aver.IsTrue(names.SequenceEqual(new List<string>{"async","sync"}  ));
      }

      [Run]
      public void NET_SvcBindingNode()
      {
          var mb = Metabase;

          var node1 = mb.GetNetworkSvcBindingConfNode("internoc", "zgov");
          Aver.AreEqual("async", node1.Name);

          var node2 = mb.GetNetworkSvcBindingConfNode("internoc", "zgov", "sync");
          Aver.AreEqual("sync", node2.Name);
      }


      [Run]
      public void NET_Resolve_SameNOC()
      {
          var mb = Metabase;

          Aver.AreEqual("async://localhost:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0001", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));
          Aver.AreEqual("async://192.168.1.2:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0002", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));

          Aver.AreEqual("async://192.168.1.3:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0003", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));
          Aver.AreEqual("async://192.168.1.4:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0004", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));

          Aver.AreEqual("async://192.168.2.1:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/B/wmed0010", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));
          Aver.AreEqual("async://192.168.2.2:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/B/wmed0011", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));
      }


      [Run]
      public void NET_Resolve_FromAnotherNOC_SameRegion()
      {
          var mb = Metabase;

          Aver.AreEqual("async://localhost:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0001", "internoc", "zgov", null, "US/East/JFK/A/wmed1024"));
          Aver.AreEqual("async://wmed0002.us1.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0002", "internoc", "zgov", null, "US/East/JFK/A/wmed1024"));
          Aver.AreEqual("async://wmed0003.us1.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0003", "internoc", "zgov", null, "US/East/JFK/A/wmed1024"));
          Aver.AreEqual("async://wmed0004.us1.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0004", "internoc", "zgov", null, "US/East/JFK/A/wmed1024"));
      }

      [Run]
      public void NET_Resolve_FromAnotherNOC_DifferentRegion()
      {
          var mb = Metabase;

          //AMS->CLE
          Aver.AreEqual("async://localhost:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0001", "internoc", "zgov", null,  "Europe/West/AMS/A/wmed2024"));
          Aver.AreEqual("async://wmed0002.us3.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0002", "internoc", "zgov", null,  "Europe/West/AMS/A/wmed2024"));
          Aver.AreEqual("async://wmed0003.us3.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0003", "internoc", "zgov", null, "Europe/West/AMS/A/wmed2024"));
          Aver.AreEqual("async://wmed0004.us3.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0004", "internoc", "zgov", null, "Europe/West/AMS/A/wmed2024"));

          //CLE->AMS
          Aver.AreEqual("async://wmed2024.eu3.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("Europe/West/AMS/A/wmed2024", "internoc", "zgov", null,  "US/East/CLE/A/I/wmed0001"));
          Aver.AreEqual("async://wmed2025.eu3.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("Europe/West/AMS/A/wmed2025", "internoc", "zgov", null,  "US/East/CLE/A/I/wmed0001"));
      }

      [Run]
      [Aver.Throws(typeof(MetabaseException), Message="parties are in different NOCs", MsgMatch = Aver.ThrowsAttribute.MatchType.Contains)]
      public void NET_Resolve_FromAnotherNOC_ProhibitedByScope()
      {
          var mb = Metabase;

          Aver.AreEqual("sync://192.168.1.1:9201",
             mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0001", "nocgov", "hgov", null, "US/East/JFK/A/wmed1024"));
      }



      [Run]
      public void NET_Resolve_FilterScore()
      {
          var mb = Metabase;

          Aver.AreEqual("async://wmed7000.eu.africa.com:9200",
                          mb.ResolveNetworkServiceToConnectString("Africa/Cairo/A/wmed7000", "internoc", "zgov", null,  "Europe/West/AMS/A/wmed2024"));

          //EU->Africa
          Aver.AreEqual("async://wmed7000.eu.africa.com:9200",
                          mb.ResolveNetworkServiceToConnectString("Africa/Cairo/A/wmed7000", "internoc", "zgov", null,  "Europe/West/AMS/A/wmed2024"));

          //EU->Africa  sync
          Aver.AreEqual("sync://wmed7000.eu.africa.com:9201",
                          mb.ResolveNetworkServiceToConnectString("Africa/Cairo/A/wmed7000", "internoc", "zgov", "sync",  "Europe/West/AMS/A/wmed2024"));

          //US->Africa
          Aver.AreEqual("async://wmed7000.us.africa.com:9200",
                          mb.ResolveNetworkServiceToConnectString("Africa/Cairo/A/wmed7000", "internoc", "zgov", null,  "US/East/CLE/A/I/wmed0001"));
      }


      [Run]
      public void NET_Various_Parallel_AndPerformance()
      {
          var mb = Metabase;

          var sw = System.Diagnostics.Stopwatch.StartNew();

          var CNT = 50000;

          Parallel.For(0, CNT,
             (i)=>
             {

          Aver.AreEqual("async://wmed7000.eu.africa.com:9200",
                          mb.ResolveNetworkServiceToConnectString("Africa/Cairo/A/wmed7000", "internoc", "zgov", null,  "Europe/West/AMS/A/wmed2024"));


          //EU->Africa
          Aver.AreEqual("async://wmed7000.eu.africa.com:9200",
                          mb.ResolveNetworkServiceToConnectString("Africa/Cairo/A/wmed7000", "internoc", "zgov", null,  "Europe/West/AMS/A/wmed2024"));
          //EU->Africa  sync
          Aver.AreEqual("sync://wmed7000.eu.africa.com:9201",
                          mb.ResolveNetworkServiceToConnectString("Africa/Cairo/A/wmed7000", "internoc", "zgov", "sync",  "Europe/West/AMS/A/wmed2024"));
          //US->Africa
          Aver.AreEqual("async://wmed7000.us.africa.com:9200",
                          mb.ResolveNetworkServiceToConnectString("Africa/Cairo/A/wmed7000", "internoc", "zgov", null,  "US/East/CLE/A/I/wmed0001"));


          //AMS->CLE
          Aver.AreEqual("async://localhost:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0001", "internoc", "zgov", null,  "Europe/West/AMS/A/wmed2024"));
          Aver.AreEqual("async://wmed0002.us3.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0002", "internoc", "zgov", null,  "Europe/West/AMS/A/wmed2024"));
          Aver.AreEqual("async://wmed0003.us3.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0003", "internoc", "zgov", null, "Europe/West/AMS/A/wmed2024"));
          Aver.AreEqual("async://wmed0004.us3.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0004", "internoc", "zgov", null, "Europe/West/AMS/A/wmed2024"));


          //CLE->AMS
          Aver.AreEqual("async://wmed2024.eu3.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("Europe/West/AMS/A/wmed2024", "internoc", "zgov", null,  "US/East/CLE/A/I/wmed0001"));
          Aver.AreEqual("async://wmed2025.eu3.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("Europe/West/AMS/A/wmed2025", "internoc", "zgov", null,  "US/East/CLE/A/I/wmed0001"));

          Aver.AreEqual("async://localhost:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0001", "internoc", "zgov", null, "US/East/JFK/A/wmed1024"));
          Aver.AreEqual("async://wmed0002.us1.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0002", "internoc", "zgov", null, "US/East/JFK/A/wmed1024"));
          Aver.AreEqual("async://wmed0003.us1.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0003", "internoc", "zgov", null, "US/East/JFK/A/wmed1024"));
          Aver.AreEqual("async://wmed0004.us1.internoc.zhabis.com:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0004", "internoc", "zgov", null, "US/East/JFK/A/wmed1024"));


          Aver.AreEqual("async://localhost:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0001", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));
          Aver.AreEqual("async://192.168.1.2:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/I/wmed0002", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));


          Aver.AreEqual("async://192.168.1.3:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0003", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));
          Aver.AreEqual("async://192.168.1.4:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/A/II/wmed0004", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));

          Aver.AreEqual("async://192.168.2.1:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/B/wmed0010", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));
          Aver.AreEqual("async://192.168.2.2:9200", mb.ResolveNetworkServiceToConnectString("US/East/CLE/B/wmed0011", "nocgov", "hgov", null, "US/East/CLE/A/I/wmed0001"));

          });

          Console.WriteLine("ResolveNetSvc performance(parallel): {0} ops/sec".Args(CNT / (sw.ElapsedMilliseconds / 1000d)));
      }



  }
}
