using System;

using Azos.IO.FileSystem.Local;
using Azos.Scripting;

using Azos.Sky.Apps;
using Azos.Sky.Metabase;

namespace Azos.Tests.Unit.Sky.Metabase
{
  [Runnable]
  public class MetabankIncludeTest
  {
    const string WMED0004 = "us/east/cle/a/ii/wmed0004.h";

    [Run]
    public void MI_Test()
    {
      using (var fs = new LocalFileSystem(null))
      using(var mb = new Metabank(fs, null, TestSources.RPATH))
      using (var session = BootConfLoader.LoadForTest(SystemApplicationType.TestRig, mb, TestSources.THIS_HOST))
      {
        var host = mb.CatalogReg.NavigateHost(WMED0004);

        var conf = host.GetEffectiveAppConfig("WebApp1");

        Aver.AreEqual("value", conf.Navigate("/$var").Value);
      }
    }
  }
}
