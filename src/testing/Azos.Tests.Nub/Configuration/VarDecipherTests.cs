/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class VarDecipherTests : IRunnableHook
  {
    public static readonly IConfigSectionNode CFG = @"
    safe
    {
      algorithm{ type='Azos.Security.TheSafe+PwdAlgorithm, Azos' name='nubpwd1' password='12345' }

      algorithm
      {
        name='nubaes'
        default=true
        type='Azos.Security.TheSafe+AesAlgorithm, Azos'
        hmac{key='base64:oIhKI32LPR9Lb--Uk6UPMvBrvSO0XNJoz5ZUi2tvGesrqVFCZNVYoHS-0sxCRI13UHC_lU9BL7EpmjSvz_4o0Q'}
        hmac{key='base64:y_SE2HQI2njLm3DJlC6grgMY1lZALsro8I5CHXQcCKOhFVXj9b7w3m2hiu-6MX_umLm__1h3I4wolc4j4PDj2A'}
        aes{ key = 'base64:mc0IgpVATyjSPXzsH5SD7JyxDFIq1V_07jkFD8Wtu8I'}
        aes{ key = 'base64:gyYjrv3nY4LE3sFjhlzYTaMzk43sfaddgJIZucHDZKo'}
      }
    }

    ".AsLaconicConfig();

    public void Prologue(Runner runner, FID id)
    {
      Azos.Security.TheSafe.Init(CFG, false, false);
    }

    public bool Epilogue(Runner runner, FID id, Exception error) => false;


    [Run]
    public void NopValue()
    {
      //dcba
      var cfg = "pwd='$(::decipher value=base64:YWJjZA string=true algo=nop)'".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

      using(var flow = new Azos.Security.SecurityFlowScope(Azos.Security.TheSafe.SAFE_ACCESS_FLAG))
      {
        var got = cfg.ValOf("pwd");
        Aver.AreEqual("abcd", got);
      }
    }

    [Run]
    public void NopDerefValue()
    {
      //dcba
      var cfg = "    encoded=base64:YWJjZA       pwd='$($encoded::decipher string=true algo=nop)'    ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

      using (var flow = new Azos.Security.SecurityFlowScope(Azos.Security.TheSafe.SAFE_ACCESS_FLAG))
      {
        var got = cfg.ValOf("pwd");
        Aver.AreEqual("abcd", got);
      }
    }

    [Run]
    public void AlgoPassword()
    {
      //dcba
      var cfg = @"
        encoded='base64:LlNBRkUgIAD7UapfEiyL3qmJL5pg-ZjkLHff8_MMVj7shsuo4RXusjxLZu7tyoTmhGRongHqHTlTpRCgIwxdP9-Qx25ca_JkPN4v1T5sW0e1XfUHVqA7BrsAKOpIqDMiX4XmhRQKv06-wethnpJzDU6SkqERl6_BTYWbWjnxd1Fr-nrSIPBGamq5-0xelXrxHLbIrDfi7g7rTDKYmVRuo98M5HSKgAUv_O0yKFYdsV_67OH0w2F8LfZ9BgTCx_2F1av81bd3RlXO-XPKGHYhzKcW_aIqKc1RPyDTWQFECUrr16GErnqw4HlgCAMyDpfbaBKTbLip_5ezljQR25p-TwWyMyS8dMBTfhHvyWpaN1J-F1jdY5kkh6I2esV2NbBuaU7FN3dxPgPBvXVJ0PlrP20UxyVeqsO9lW3URqDZV9rhvokOliihOaU2qfMPleRMtCm3e2tLiq-gOhKbj5RWvr7TrFRiVg7cNWLfhWIKieNwlVSdKtGi9_ekEJuv0cW2lLv6rnB3uochT_PXPuQbgKI2ZoI_nZ0M'
        pwd='$($encoded::decipher algo=nubpwd1)'
      ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

      using (var flow = new Azos.Security.SecurityFlowScope(Azos.Security.TheSafe.SAFE_ACCESS_FLAG))
      {
        var got = cfg.ValOf("pwd");
        Aver.AreEqual("base64:zlCzhEqj-OHyflXqJuCwU2jjCxxbJRfBMgN295ZDdOSO5ZLnagoJ_LolArVIp0B_PTeyBaxWqhG_aqKGuBWTnw", got);
      }
    }

    [Run]
    public void AlgoAes()
    {
      //dcba
      var cfg = @"
        encoded='base64:cpzSW8Cldod1DajW9CNRQ0tXW5dNfFWhGsHsasvkmF-Wlv3ioJvt-ogmkPkRO1ByW09bWwwImxWVk2zGo9eiAa2TGpMr-Zvctp-kFxZaVoNF0Rmj_AX0pOX2NDAg1U9o-G-LmxIRbdAsw_4mu-C3zDoUGADzrX6w5cxQoIwOJVU'
        pwd='$($encoded::decipher algo=nubaes)'
      ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

      using (var flow = new Azos.Security.SecurityFlowScope(Azos.Security.TheSafe.SAFE_ACCESS_FLAG))
      {
        var got = cfg.ValOf("pwd");
        Aver.AreEqual("base64:lZ0xSDTZ0alz1XVMikxhu0qKePXudDTx1KrtRf2N-kSA1VYy8izuuy3HlwpboQ-sMZndryVlZsdv0Jqy87qOMA", got);
      }
    }


  }


}



