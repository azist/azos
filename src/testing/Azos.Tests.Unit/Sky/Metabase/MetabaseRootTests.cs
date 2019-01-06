/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Linq;
using Azos.Apps;
using Azos.IO.FileSystem.Local;
using Azos.Scripting;

using Azos.Sky.Metabase;

namespace Azos.Tests.Unit.Sky.Metabase
{
  [Runnable]
  public class MetabaseRootTests
  {
      [Run]
      public void MR_RootConfigs()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.IsNotNull(mb.CommonLevelConfig);
          Aver.IsNotNull(mb.RootConfig);

          Aver.AreEqual("value1", mb.CommonLevelConfig.Navigate("/common/$var1").Value);

          Aver.AreEqual("value1", mb.RootConfig.Navigate("/common/$var1").Value);
        }
      }

      [Run]
      public void MR_CommonConfigGetsIncludedAtAllLevels()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual("value1", mb.CatalogApp.Applications["WebApp1"].LevelConfig.Navigate("/common/$var1").Value);

          Aver.AreEqual("value1", mb.CatalogReg["us/east/cle/a/i/wmed0001"].LevelConfig.Navigate("/common/$var1").Value);
        }
      }

      [Run]
      public void MR_PlatformNames()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          var platforms = mb.PlatformNames.ToList();

          Aver.AreEqual(3, platforms.Count);
          Aver.AreEqual("Win64",   platforms[0]);
          Aver.AreEqual("Linux64", platforms[1]);
          Aver.AreEqual("Android", platforms[2]);
        }
      }

      [Run]
      public void MR_OSNames()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          var oses = mb.OSNames.ToList();

          Aver.AreEqual(5,          oses.Count);
          Aver.AreEqual("win7",     oses[0]);
          Aver.AreEqual("w2003k",   oses[1]);
          Aver.AreEqual("fedora20", oses[2]);
          Aver.AreEqual("JellyBean4.1", oses[3]);
          Aver.AreEqual("KitKat4.4",    oses[4]);
        }
      }

      [Run]
      public void MR_GetOSPlatformNode()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          var pn = mb.GetOSPlatformNode("fedora20");

          Aver.AreEqual(Metabank.CONFIG_PLATFORM_SECTION,  pn.Name);
          Aver.AreEqual("Linux64",  pn.AttrByName(Metabank.CONFIG_NAME_ATTR).Value);
        }
      }

      [Run]
      public void MR_GetOSPlatformName()
      {
        using(var fs = new LocalFileSystem(NOPApplication.Instance))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        {
          Aver.AreEqual("Android",  mb.GetOSPlatformName("KitKat4.4"));
        }
      }



  }
}
