/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Unit.Wave
{
  [Runnable(TRUN.BASE)]
  public class AsynchronousTests : ServerTestsBase
  {
    private static readonly string BASE_ADDRESS = BASE_URI.ToString() + "mvc/asynchronous/";

    protected override void DoPrologue(Runner runner, FID id)
    {
      Client.BaseAddress = new Uri(BASE_ADDRESS);
    }

    [Run]
    public async Task ActionPlainText()
    {
      var got = await Client.GetStringAsync("actionplaintext");
      Aver.AreEqual("Response in plain text", got);
    }

    [Run]
    public async Task ActionObjectLiteral()
    {
      var got = (await Client.GetStringAsync("actionobjectliteral")).JSONToDataObject() as JSONDataMap;
      Aver.AreEqual(1, got["a"].AsInt());
      Aver.AreEqual(true, got["b"].AsBool());
      Aver.AreEqual(1980, got["d"].AsDateTime().Year);
    }

  }
}
