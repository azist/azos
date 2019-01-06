/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;


using Azos.Conf;
using Azos.Scripting;
using Azos.Sky;

namespace Azos.Tests.Unit.Sky
{
  [Runnable]
  public class SysConstsTests : IRunnableHook
  {
      public void Prologue(Runner runner, FID id) => SystemVarResolver.Bind();
      public bool Epilogue(Runner runner, FID id, Exception error) => false;


      [Run]
      public void GlobalVarsInNewConf()
      {
        var conf = Configuration.ProviderLoadFromString(
        @"
           sky
           {
              name = '$(~SysConsts.APP_NAME_HGOV).1'
              port = $(~SysConsts.DEFAULT_ZONE_GOV_APPTERM_PORT)
           }
         ","laconf"
        );


        Aver.AreEqual("ahgov.1", conf.Root.AttrByIndex(0).Value);
        Aver.AreEqual(SysConsts.DEFAULT_ZONE_GOV_APPTERM_PORT, conf.Root.AttrByIndex(1).ValueAsInt());
      }


      [Run]
      public void GlobalVarsPassThroughToOS()
      {
        var conf = Configuration.ProviderLoadFromString(
        @"
           sky
           {
              port = $(~SysConsts.DEFAULT_ZONE_GOV_APPTERM_PORT)
              tezt = $(~SKY_MESSAGE_UTEST)
           }
         ","laconf"
        );

        Aver.AreEqual(SysConsts.DEFAULT_ZONE_GOV_APPTERM_PORT, conf.Root.AttrByName("port").ValueAsInt());
        Aver.AreEqual("Hello SKY!", conf.Root.AttrByName("tezt").Value, "Did you forget to set SKY_MESSAGE_UTEST='Hello SKY!'?");
      }
  }
}
