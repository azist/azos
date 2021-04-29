/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Scripting;
using Azos.Web;

namespace Azos.Tests.Nub.Web
{
  [Runnable]
  public class WebSettingsTests
  {
    [Run]
    public void ServicePointManagerTest()
    {
      var conf = @"
      app
      {
        web-settings
        {
          service-point-manager
          {

            check-certificate-revocation-list=true //false
            default-connection-limit=4 //2
            dns-refresh-timeout=100000 //120000
            enable-dns-round-robin=true //False
            expect-100-continue=false //true
            max-service-point-idle-time=90000 //100000
            max-service-points=10 //0
            security-protocol=Tls12 //Ssl3, Tls
            use-nagle-algorithm=true

            service-point
            {
              uri='https://footest.com'

              connection-lease-timeout=2300 // -1
              ConnectionLimit=7 //4
              expect-100-continue=false //True
              max-idle-time=103000 //90000
              receive-buffer-size=127000 //-1
              use-nagle-algorithm=false //True
            }

            service-point
            {
              uri='https://footest_a.com'
            }

            policy
            {
              default-certificate-validation
              {
                case { uri='https://footest.com' trusted=true}
                case { uri='https://footest_a.com'}
              }
            }
          }
        }
      }".AsLaconicConfig();

      using (var app = new Azos.Apps.AzosApplication(new string[] {}, conf))
      {
        var spmc = app.GetServicePointManagerConfigurator();

        Aver.IsTrue(System.Net.ServicePointManager.CheckCertificateRevocationList);
        Aver.AreEqual( 4, System.Net.ServicePointManager.DefaultConnectionLimit);
        Aver.AreEqual( 100000, System.Net.ServicePointManager.DnsRefreshTimeout);
        Aver.IsTrue(System.Net.ServicePointManager.EnableDnsRoundRobin);
        Aver.IsFalse(System.Net.ServicePointManager.Expect100Continue);
        Aver.AreEqual( 90000, System.Net.ServicePointManager.MaxServicePointIdleTime);
        Aver.AreEqual( 10, System.Net.ServicePointManager.MaxServicePoints);
        Aver.IsTrue( System.Net.SecurityProtocolType.Tls12 == System.Net.ServicePointManager.SecurityProtocol);
        Aver.IsTrue(System.Net.ServicePointManager.UseNagleAlgorithm);

        var sp_footest_com = System.Net.ServicePointManager.FindServicePoint(new Uri("https://footest.com"));
        Aver.IsFalse(sp_footest_com.Expect100Continue);
        var sp_footest_1 = System.Net.ServicePointManager.FindServicePoint(new Uri("https://footest_1.com"));
        Aver.IsFalse(sp_footest_1.Expect100Continue);
        var sp_footest_a = System.Net.ServicePointManager.FindServicePoint(new Uri("https://footest_a.com"));
        Aver.AreEqual(System.Net.ServicePointManager.Expect100Continue, sp_footest_a.Expect100Continue);

        Aver.AreEqual(2, spmc.ServicePoints.Count());
        var spc_footest_com = spmc.ServicePoints.First();

        Aver.AreEqual( 2300, spc_footest_com.ConnectionLeaseTimeout);
        Aver.AreEqual( 7, spc_footest_com.ConnectionLimit);
        Aver.IsFalse( spc_footest_com.Expect100Continue);
        Aver.AreEqual( 103000, spc_footest_com.MaxIdleTime);
        Aver.AreEqual( 127000, spc_footest_com.ReceiveBufferSize);
        Aver.IsFalse( spc_footest_com.UseNagleAlgorithm);
      }
    }
  }
}
