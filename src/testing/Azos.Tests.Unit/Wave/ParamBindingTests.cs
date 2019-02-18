/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Unit.Wave
{
  [Runnable(TRUN.BASE)]
  public class ParamBindingTests : ServerTestsBase
  {
    private static readonly string BASE_ADDRESS = BASE_URI.ToString() + "mvc/parambinding/";

    protected override void DoPrologue(Runner runner, FID id)
    {
      Client.BaseAddress = new Uri(BASE_ADDRESS);
    }

    [Run]
    public async Task EchoParams_GET()
    {
      var response = await Client.GetAsync("echoparams?a=hello&b=2&c=true");
      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual("hello", got["a"].AsString());
      Aver.AreEqual(2, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());
    }

    [Run]
    public async Task EchoParams_POST()
    {
      var content = new FormUrlEncodedContent(new[]{
        new KeyValuePair<string, string>("a", "hello"),
        new KeyValuePair<string, string>("b", "2"),
        new KeyValuePair<string, string>("c", "true"),
      });

      var response = await Client.PostAsync("echoparams", content);
      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual("hello", got["a"].AsString());
      Aver.AreEqual(2, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());
    }


    [Run]
    public async Task EchoParamsWithDefaults_GET()
    {
      var response = await Client.GetAsync("echoparamswithdefaults?a=hello");
      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual("hello", got["a"].AsString());
      Aver.AreEqual(127, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());
    }

    [Run]
    public async Task EchoParamsWithDefaults_POST()
    {
      var content = new FormUrlEncodedContent(new[]{
        new KeyValuePair<string, string>("a", "hello"),
      });

      var response = await Client.PostAsync("echoparamswithdefaults", content);
      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual("hello", got["a"].AsString());
      Aver.AreEqual(127, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());
    }


    [Run]
    public async Task EchoMixMap()
    {
      var response = await Client.GetAsync("echomixmap?a=hello&b=789&c=true&d=ok&e=false");
      var got = (await response.Content.ReadAsStringAsync()).JSONToDataObject() as JSONDataMap;
      Aver.IsNotNull(got);
      Aver.AreEqual(4, got.Count);
      Aver.AreEqual("hello", got["a"].AsString());
      Aver.AreEqual(789, got["b"].AsInt());
      Aver.AreEqual(true, got["c"].AsBool());

      var got2 = got["got"] as JSONDataMap;
      Aver.IsNotNull(got2);
      Console.WriteLine(got2.ToJSON());

      Aver.IsTrue( got2.Count >= 5);
      Aver.AreEqual("hello", got2["a"].AsString());
      Aver.AreEqual(789, got2["b"].AsInt());
      Aver.AreEqual(true, got2["c"].AsBool());
      Aver.AreEqual("ok", got2["d"].AsString());
      Aver.AreEqual("false", got2["e"].AsString());

    }

  }
}
