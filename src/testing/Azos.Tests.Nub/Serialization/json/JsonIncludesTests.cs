/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Azos.Apps;
using Azos.Conf;
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
    public void Test_Map_3()
    {
      var input = "{a: 1, b: { x:'Appleseed', y:'@{ x=600 }' } , c: '@{ y=-900 }'}".JsonToDataObject() as JsonDataMap;

      var got = input.ProcessJsonIncludes(cfg => cfg.ValOf("x", "y")) as JsonDataMap;

      got.See();

      var gotMap = got["b"] as JsonDataMap;

      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(1, got["a"].AsInt());
      Aver.AreEqual(600, gotMap["y"].AsInt());
      Aver.AreEqual("Appleseed", gotMap["x"].AsString());
      Aver.AreEqual(-900, got["c"].AsInt());
    }



    [Run]
    public void Test_Map_4()
    {
      var input = "{a: 1, b: { b1:'Appleseed', b2:'@{ x=600 y=-444 }' }, c: '@{ x=-900 }'}".JsonToDataObject() as JsonDataMap;

      var got = input.ProcessJsonIncludes(cfg => new { xval = cfg.ValOf("x"), yval = cfg.ValOf("y") }) as JsonDataMap;

      var gotMap = got["b"] as JsonDataMap;
      var gotMapB2 = gotMap["b2"].ToJson().JsonToDataObject() as JsonDataMap;

      var gotMapC = got["c"].ToJson().JsonToDataObject() as JsonDataMap;

      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(1, got["a"].AsInt());
      Aver.AreEqual(600, gotMapB2["xval"].AsInt());
      Aver.AreEqual(-444, gotMapB2["yval"].AsInt());
      Aver.AreEqual("Appleseed", gotMap["b1"].AsString());
      Aver.AreEqual(-900, gotMapC["xval"].AsInt());
    }


    internal abstract class IncludeBase : IConfigurable
    {
      public void Configure(IConfigSectionNode node)
      {
        ConfigAttribute.Apply(this, node);
      }

      public abstract object Run();
    }

    internal class TextInclude : IncludeBase
    {
      public override object Run()
      {
        return "I say: {0}".Args(Text);
      }

      [Config]public string Text { get; set; }
    }

    internal class AddInclude : IncludeBase
    {
      public override object Run()
      {
        return A + B;
      }

      [Config] public int A { get; set; }
      [Config] public int B { get; set; }
    }


    [Run]
    public void Test_Map_5()
    {
      var input = ("{a: 1, b: { b1:'Appleseed', b2:'@{ type=\"Azos.Tests.Nub.Serialization.JsonIncludesTests+TextInclude, Azos.Tests.Nub\" text=Hooray }' }, " +
        "c: '@{ type=\"Azos.Tests.Nub.Serialization.JsonIncludesTests+AddInclude, Azos.Tests.Nub\" a=-5 b=37 }'}").JsonToDataObject() as JsonDataMap;

      var got = input.ProcessJsonIncludes(cfg => FactoryUtils.MakeAndConfigure<IncludeBase>(cfg).Run()) as JsonDataMap;
      got.See();
      var gotMap = got["b"] as JsonDataMap;

      Aver.AreEqual(32, got["c"].AsInt());
      Aver.AreEqual("I say: Hooray", gotMap["b2"].AsString());
    }


    [Run("!ProcessJsonLocalFileIncludes_1", null)]
    public void ProcessJsonLocalFileIncludes_1()
    {
      var path = Path.Combine(Environment.GetEnvironmentVariable("AZIST_HOME"), "azos/src/testing/Azos.Tests.Nub/Serialization/json/sample-text.txt");

      var input = new JsonDataMap
      {
        { "a", 10 },
        { "b", 20 },
        { "c", "@{ file='" + path.Replace("\\", @"\\") + "' }" }
      };

      var got = input.ProcessJsonLocalFileIncludes(ExecutionContext.Application, null) as JsonDataMap;

      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(10, got["a"].AsInt());
      Aver.AreEqual(20, got["b"].AsInt());
      Aver.AreEqual("yellow is not blue", got["c"].AsString());
    }



    [Run("!ProcessJsonLocalFileIncludes_2", null)]
    public void ProcessJsonLocalFileIncludes_2()
    {
      var path = Path.Combine(Environment.GetEnvironmentVariable("AZIST_HOME"), "azos/src/testing/Azos.Tests.Nub/Serialization/json/sample-bin.bin");

      var input = new JsonDataMap
      {
        { "a", 10 },
        { "b", 20 },
        { "c", "@{ file='" + path.Replace("\\", @"\\") + "' }" }
      };

      var got = input.ProcessJsonLocalFileIncludes(ExecutionContext.Application, null) as JsonDataMap;

      Aver.AreEqual(3, got.Count);
      Aver.AreEqual(10, got["a"].AsInt());
      Aver.AreEqual(20, got["b"].AsInt());
      Aver.IsTrue(new byte[] {0x6C,0x75,0x65}.MemBufferEquals(got["c"] as byte[]));
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


    [Run]
    public void Test_Array_2()
    {
      var input = "[1,2, '@{ x=900 }', '@{ x=901 }']".JsonToDataObject() as JsonDataArray;

      var got = input.ProcessJsonIncludes(cfg => cfg.ValOf("x")) as JsonDataArray;

      Aver.AreEqual(4, got.Count);
      Aver.AreEqual(1, got[0].AsInt());
      Aver.AreEqual(2, got[1].AsInt());
      Aver.AreEqual(900, got[2].AsInt());
      Aver.AreEqual(901, got[3].AsInt());
    }

  }
}
