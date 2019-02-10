/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using Azos.Apps;
using Azos.IO.FileSystem.Local;
using Azos.Scripting;

using Azos.Sky.Apps;
using Azos.Sky.Metabase;

namespace Azos.Tests.Unit.Sky.Metabase
{
  [Runnable]
  public class MetabankIncludeTest : BaseTestRigWithSkyApp
  {
    const string WMED0004 = "us/east/cle/a/ii/wmed0004.h";

    [Run]
    public void MI_Test()
    {
      var host = Metabase.CatalogReg.NavigateHost(WMED0004);
      var conf = host.GetEffectiveAppConfig("WebApp1");
      Aver.AreEqual("value", conf.Navigate("/$var").Value);
    }
  }
}
