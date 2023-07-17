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
  public class VarDecipherTests
  {
    [Run]
    public void Test_01()
    {
      //dcba
      var cfg = "pwd='$(::decipher value=base64:ZGNiYQ string=true)'".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

      using(var flow = new Azos.Security.SecurityFlowScope(Azos.Security.TheSafe.SAFE_ACCESS_FLAG))
      {
        var got = cfg.ValOf("pwd");
        Aver.AreEqual("abcd", got);
      }
    }

    [Run]
    public void Test_02()
    {
      //dcba
      var cfg = "    encoded=base64:ZGNiYQ       pwd='$($encoded::decipher string=true)'    ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

      using (var flow = new Azos.Security.SecurityFlowScope(Azos.Security.TheSafe.SAFE_ACCESS_FLAG))
      {
        var got = cfg.ValOf("pwd");
        Aver.AreEqual("abcd", got);
      }
    }
  }


}



