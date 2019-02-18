/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Scripting;

namespace Azos.Tests.Unit.Wave
{
  [Runnable(TRUN.BASE)]
  public class BasicTests : ServerTestsBase
  {
    private static readonly string BASE_ADDRESS = BASE_URI.ToString() + "mvc/basic/";

    [Run]
    public void ActionPlainText()
    {
      using (var wc = CreateWebClient())
      {
        var got = wc.DownloadString(BASE_ADDRESS + "actionplaintext");
        Aver.AreEqual("Response in plain text", got);
      }
    }

  }
}
