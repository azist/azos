/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Time;
using Azos.Scripting;
using Azos.Apps;

namespace Azos.Tests.Nub.Time
{
  [Runnable]
  public class TimeZoneManagerTests
  {
    const string APP = @"
app
{
  modules
  {
    module
    {
      name='sut'
      type='Azos.Time.TimeZoneManager, Azos'

      map{ iana='America/New_York' win='Eastern Standard Time' }
      map{ iana='America/Detroit'  win='Eastern Standard Time' data{ cars=true  distance-to-mars=-987654321.01 } }
      map
      {
        names='cle,cleveland,est'
        iana='America/Louisville'
        win='Eastern Standard Time'
        data{ a=1 b=-210}
      }
      map{ iana='America/Chicago' win='Central Standard Time' }

      zone
      {
        names='z1 ,  kukurbatcha ,utc    '
        utc-offset='00:00'
        display-name='(UTC+00:00) UTC/Kukarbattcha Rajnakhuratt'
        standard-name='(UTC+00:00) UTC/Kukarbattcha Rajnakhuratt STD'

        data{ secret='Marra the lord of avidiya'}
      }
    }
  }
}
    ";


    [Run]
    public void Case001_System()
    {
      using(var app = new AzosApplication(null, APP.AsLaconicConfig()))
      {
        var sut = app.ModuleRoot.Get<TimeZoneManager>();

        var est1 = sut.GetZoneMapping("America/New_York");
        var est2 = sut.GetZoneMapping("America/NEW_YORK");
        var est3 = sut.GetZoneMapping("Eastern Standard Time");
        var est4 = sut.GetZoneMapping("EASTERN STANDARD tiME");
        var est5 = sut.GetZoneMapping("America/Detroit");
        var cst1 = sut.GetZoneMapping("America/Chicago");
        var cst2 = sut.GetZoneMapping("Central Standard Time");

        Aver.AreSameRef(est1, est2);
        Aver.AreNotSameRef(est2, est3);
        Aver.AreSameRef(est3, est4);
        Aver.AreNotSameRef(est5, est1);

        Aver.AreNotSameRef(cst1, cst2);

        Aver.AreSameRef(est1.ZoneInfo, est2.ZoneInfo);
        Aver.AreSameRef(est1.ZoneInfo, est3.ZoneInfo);
        Aver.AreSameRef(est1.ZoneInfo, est4.ZoneInfo);
        Aver.AreSameRef(est1.ZoneInfo, est5.ZoneInfo);

        Aver.AreNotSameRef(est1.ZoneInfo, cst1.ZoneInfo);

        Aver.AreSameRef(cst1.ZoneInfo, cst2.ZoneInfo);

        Aver.IsTrue(est1.MappingType == TimeZoneMappingType.IANA);
        Aver.IsTrue(est2.MappingType == TimeZoneMappingType.IANA);
        Aver.IsTrue(est3.MappingType == TimeZoneMappingType.Windows);
        Aver.IsTrue(est4.MappingType == TimeZoneMappingType.Windows);

        Aver.IsTrue(cst1.MappingType == TimeZoneMappingType.IANA);
        Aver.IsTrue(cst2.MappingType == TimeZoneMappingType.Windows);

        var cle = sut.GetZoneMapping("cle");
        var est = sut.GetZoneMapping("EsT");
        var cleveland = sut.GetZoneMapping("cleveland");
        Aver.AreNotSameRef(cle, cleveland);
        Aver.AreNotSameRef(est, cle);

        Aver.AreSameRef(cle.ZoneInfo, cleveland.ZoneInfo);
        Aver.AreSameRef(est.ZoneInfo, cleveland.ZoneInfo);

        Aver.IsNotNull(cle.Data);
        Aver.AreEqual(1, cle.Data.Of("a").ValueAsInt());
        Aver.AreEqual(-210, cle.Data.Of("b").ValueAsInt());

        Aver.IsNotNull(est5.Data);
        Aver.AreEqual(true, est5.Data.Of("cars").ValueAsBool());
        Aver.AreEqual(-987654321.01, est5.Data.Of("distance-to-mars").ValueAsDouble());

      }
    }

    [Run]
    public void Case002_Custom()
    {
      using (var app = new AzosApplication(null, APP.AsLaconicConfig()))
      {
        var sut = app.ModuleRoot.Get<TimeZoneManager>();

        var z1 = sut.GetZoneMapping("z1");
        var kuk1 = sut.GetZoneMapping("kukurbatcha");
        var utc1 = sut.GetZoneMapping("utc");

        Aver.AreNotSameRef(z1, kuk1);
        Aver.AreNotSameRef(z1, utc1);

        Aver.AreSameRef(z1.ZoneInfo, kuk1.ZoneInfo);
        Aver.AreSameRef(kuk1.ZoneInfo, utc1.ZoneInfo);

        Aver.AreEqual("(UTC+00:00) UTC/Kukarbattcha Rajnakhuratt", kuk1.ZoneInfo.DisplayName);
        Aver.AreEqual("(UTC+00:00) UTC/Kukarbattcha Rajnakhuratt STD", kuk1.ZoneInfo.StandardName);
        Aver.AreEqual(0, utc1.ZoneInfo.BaseUtcOffset.TotalSeconds);

        Aver.IsNotNull(kuk1.Data);
        Aver.AreEqual("Marra the lord of avidiya", kuk1.Data.Of("secret").Value);
      }
    }


  }
}
