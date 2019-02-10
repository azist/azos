/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Linq;
using Azos.Scripting;

using Azos.Sky.Metabase;

namespace Azos.Tests.Unit.Sky.Metabase
{
  [Runnable]
  public class MetabaseRootTests : BaseTestRigWithSkyApp
  {
      [Run]
      public void MR_RootConfigs()
      {
        {
          Aver.IsNotNull(Metabase.CommonLevelConfig);
          Aver.IsNotNull(Metabase.RootConfig);

          Aver.AreEqual("value1", Metabase.CommonLevelConfig.Navigate("/common/$var1").Value);

          Aver.AreEqual("value1", Metabase.RootConfig.Navigate("/common/$var1").Value);
        }
      }

      [Run]
      public void MR_CommonConfigGetsIncludedAtAllLevels()
      {
        {
          Aver.AreEqual("value1", Metabase.CatalogApp.Applications["WebApp1"].LevelConfig.Navigate("/common/$var1").Value);

          Aver.AreEqual("value1", Metabase.CatalogReg["us/east/cle/a/i/wmed0001"].LevelConfig.Navigate("/common/$var1").Value);
        }
      }

      [Run]
      public void MR_PlatformNames()
      {
        {
          var platforms = Metabase.PlatformNames.ToList();

          Aver.AreEqual(3, platforms.Count);
          Aver.AreEqual("Win64",   platforms[0]);
          Aver.AreEqual("Linux64", platforms[1]);
          Aver.AreEqual("Android", platforms[2]);
        }
      }

      [Run]
      public void MR_OSNames()
      {
        {
          var oses = Metabase.OSNames.ToList();

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
        {
          var pn = Metabase.GetOSPlatformNode("fedora20");

          Aver.AreEqual(Metabank.CONFIG_PLATFORM_SECTION,  pn.Name);
          Aver.AreEqual("Linux64",  pn.AttrByName(Metabank.CONFIG_NAME_ATTR).Value);
        }
      }

      [Run]
      public void MR_GetOSPlatformName()
      {
        {
          Aver.AreEqual("Android",  Metabase.GetOSPlatformName("KitKat4.4"));
        }
      }



  }
}
