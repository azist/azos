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
        //This test requires OS var AGNI_MESSAGE_UTEST = "Hello AGNI!".
        //Please set it up on your machine

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
