/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.IO;
using System.Linq;
using System.Net;

using Azos.Apps;
using Azos.Data;
using Azos.Serialization.CSV;
using Azos.Scripting;
using Azos.Sky.GeoLookup;

namespace Azos.Tests.Integration.GeoLookup
{
  [Runnable]
  public class GeoLookupTest : IRunnableHook
  {
    public const string LACONF = @"
      app
      {
        geo-lookup
        {
          data-path=$(~NFX_GEODATA)
          resolution=city
        }
      }";

    private AzosApplication m_App;

    public IGeoLookup Service { get; set; }

    public string DataPath { get { return m_App.ConfigRoot.Navigate("!geo-lookup/$data-path").Value; } }

    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      var config = LACONF.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
      m_App = new AzosApplication(null, config);

      var service = new GeoLookupService(m_App);
      service.Configure(config["geo-lookup"]);
      service.Start();
      Service = service;
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_App);
      return false;
    }

    [Run]
    public void LookupAllIPv4()
    {
      using (var reader = new StreamReader(Path.Combine(DataPath, "GeoLite2-{0}-Blocks-IPv4.csv".Args("city"))))
      {
        foreach (var row in reader.AsCharEnumerable().ParseCSV(skipHeader: true, columns: 2, skipIfMore: true))
        {
          var record = row.ToArray();
          var subnet = record[0];
          var geoname = record[1];
          var parts = subnet.Split('/');
          var address = IPAddress.Parse(parts[0]);
          var geoentity = Service.Lookup(address);
          Aver.AreEqual(geoname, geoentity.Block.LocationID.Value);
        }
      }
    }

    [Run]
    public void LookupAllIPv6()
    {
      using (var reader = new StreamReader(Path.Combine(DataPath, "GeoLite2-{0}-Blocks-IPv6.csv".Args("city"))))
      {
        foreach (var row in reader.AsCharEnumerable().ParseCSV(skipHeader: true, columns: 2, skipIfMore: true))
        {
          var record = row.ToArray();
          var subnet = record[0];
          var geoname = record[1];
          var parts = subnet.Split('/');
          var address = IPAddress.Parse(parts[0]);
          var geoentity = Service.Lookup(address);
          Aver.AreEqual(geoname, geoentity.Block.LocationID.Value);
        }
      }
    }
  }
}
