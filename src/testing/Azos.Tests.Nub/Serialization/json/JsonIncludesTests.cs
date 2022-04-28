/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Reflection;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonIncludesTests
  {
    [Run]
    public void Test_Map_1()
    {
      var input = "{a: 1, b: 2, c: '@{ x=900 }'}".JsonToDataObject() as JsonDataMap;

      var got = input.ProcessJsonIncludes(cfg => cfg.ValOf("x")) as JsonDataMap;

      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(1, got["a"].AsInt());
      Aver.AreEqual(2, got["b"].AsInt());
      Aver.AreEqual(900, got["c"].AsInt());
    }

    [Run]
    public void Test_Map_2()
    {
      var input = "{a: 1, b: { x:'Appleseed', y:'@{ x=600 }' } , c: '@{ x=-900 }'}".JsonToDataObject() as JsonDataMap;

      var got = input.ProcessJsonIncludes(cfg => cfg.ValOf("x")) as JsonDataMap;

      var gotMap = got["b"] as JsonDataMap;

      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(1, got["a"].AsInt());
      Aver.AreEqual(600, gotMap["y"].AsInt());
      Aver.AreEqual("Appleseed", gotMap["x"].AsString());
      Aver.AreEqual(-900, got["c"].AsInt());
    }

    [Run]
    public void Test_Array_1()
    {
      var input = "[1,2, '@{ x=900 }']".JsonToDataObject() as JsonDataArray;

      var got = input.ProcessJsonIncludes(cfg => cfg.ValOf("x")) as JsonDataArray;

      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(1, got[0].AsInt());
      Aver.AreEqual(2, got[1].AsInt());
      Aver.AreEqual(900, got[2].AsInt());
    }
  }
}
