/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class ExtensionsTests
  {
    [Run]
    public void Of_1()
    {
      var cfg = "a=-1 b=-2".AsLaconicConfig();
      Aver.IsTrue(cfg.Of("a").Exists);
      Aver.IsTrue(cfg.Of("b").Exists);
      Aver.IsFalse(cfg.Of("dont-exist").Exists);

      Aver.AreSameRef(cfg.AttrByName("a"), cfg.Of("a"));
      Aver.AreSameRef(cfg.AttrByName("b"), cfg.Of("b"));

      Aver.AreEqual(-1, cfg.Of("A").ValueAsInt());
      Aver.AreEqual(-2, cfg.Of("B").ValueAsInt());
    }

    [Run]
    public void Of_2()
    {
      var cfg = "a=-1 b=-2".AsLaconicConfig();
      Aver.IsTrue(cfg.Of("a", "z").Exists);
      Aver.IsTrue(cfg.Of("z", "a").Exists);
      Aver.IsTrue(cfg.Of("b", "z").Exists);
      Aver.IsTrue(cfg.Of("z", "b").Exists);
      Aver.IsFalse(cfg.Of("dont-exist", "z").Exists);

      Aver.AreSameRef(cfg.AttrByName("a"), cfg.Of("a", "z"));
      Aver.AreSameRef(cfg.AttrByName("a"), cfg.Of("z", "a"));
      Aver.AreSameRef(cfg.AttrByName("b"), cfg.Of("b", "z"));
      Aver.AreSameRef(cfg.AttrByName("b"), cfg.Of("z", "b"));

      Aver.AreEqual(-1, cfg.Of("z", "A").ValueAsInt());
      Aver.AreEqual(-1, cfg.Of("A", "z").ValueAsInt());
      Aver.AreEqual(-2, cfg.Of("z", "B").ValueAsInt());
      Aver.AreEqual(-2, cfg.Of("B", "z").ValueAsInt());
    }

    [Run]
    public void Of_3()
    {
      var cfg = "a=-1 b=-2".AsLaconicConfig();
      Aver.IsTrue(cfg.Of("a", "z", "_").Exists);
      Aver.IsTrue(cfg.Of("z", "_", "a").Exists);
      Aver.IsTrue(cfg.Of("z", "a", "_").Exists);
      Aver.IsTrue(cfg.Of("b", "z", "_").Exists);
      Aver.IsTrue(cfg.Of("z", "_", "b").Exists);
      Aver.IsTrue(cfg.Of("z", "b", "_").Exists);
      Aver.IsFalse(cfg.Of("dont-exist", "z", "_").Exists);
      Aver.IsFalse(cfg.Of("dont-exist", "_", "z").Exists);

      Aver.AreSameRef(cfg.AttrByName("a"), cfg.Of("a", "z", "_"));
      Aver.AreSameRef(cfg.AttrByName("a"), cfg.Of("z", "_", "a"));
      Aver.AreSameRef(cfg.AttrByName("b"), cfg.Of("b", "z", "_"));
      Aver.AreSameRef(cfg.AttrByName("b"), cfg.Of("z", "b", "_"));
      Aver.AreSameRef(cfg.AttrByName("b"), cfg.Of("z", "_", "b"));

      Aver.AreEqual(-1, cfg.Of("z", "A", "_").ValueAsInt());
      Aver.AreEqual(-1, cfg.Of("A", "_", "z").ValueAsInt());
      Aver.AreEqual(-2, cfg.Of("z", "_", "B").ValueAsInt());
      Aver.AreEqual(-2, cfg.Of("B", "z", "_").ValueAsInt());
    }

    [Run]
    public void Val_1()
    {
      var cfg = "a = -1    b = -2".AsLaconicConfig();
      Aver.AreEqual("-1", cfg.ValOf("a"));
      Aver.AreEqual("-2", cfg.ValOf("b"));
      Aver.AreEqual(null, cfg.ValOf("z"));
    }

    [Run]
    public void Val_2()
    {
      var cfg = "a = -1    b = -2".AsLaconicConfig();
      Aver.AreEqual("-1", cfg.ValOf("z", "a"));
      Aver.AreEqual("-1", cfg.ValOf("a", "z"));
      Aver.AreEqual("-2", cfg.ValOf("b", "z"));
      Aver.AreEqual("-2", cfg.ValOf("z", "b"));
      Aver.AreEqual(null, cfg.ValOf("z", "_"));
    }

    [Run]
    public void Val_3()
    {
      var cfg = "a = -1    b = -2".AsLaconicConfig();
      Aver.AreEqual("-1", cfg.ValOf("z", "a", "_"));
      Aver.AreEqual("-1", cfg.ValOf("_", "a", "z"));
      Aver.AreEqual("-2", cfg.ValOf("b", "z", "_"));
      Aver.AreEqual("-2", cfg.ValOf("_", "z", "b"));
      Aver.AreEqual(null, cfg.ValOf("z", "_", "gabradge"));
    }

    [Run]
    public void BuildJsonObjectFromConfigSnippet_1()
    {
      var c = @"struct{
 a=1
 b=-23
 c=true
      }".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

      var obj = new JsonDataMap();
      c.BuildJsonObjectFromConfigSnippet(obj, v => v);

      Aver.AreEqual(3, obj.Count);
      Aver.AreEqual(1, obj["a"].AsInt());
      Aver.AreEqual(-23, obj["b"].AsInt());
      Aver.AreEqual(true, obj["c"].AsBool());
    }

    [Run]
    public void BuildJsonObjectFromConfigSnippet_2()
    {
      var c = @"struct{
 [a]=1
 [a]=-23
 [a]=true
      }".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

      var obj = new JsonDataMap();
      c.BuildJsonObjectFromConfigSnippet(obj, v => v);

      Aver.AreEqual(1, obj.Count);
      var arr = obj["a"] as JsonDataArray;
      Aver.IsNotNull(arr);
      Aver.AreEqual(3, arr.Count);

      Aver.AreEqual(1, arr[0].AsInt());
      Aver.AreEqual(-23, arr[1].AsInt());
      Aver.AreEqual(true, arr[2].AsBool());
    }

    [Run]
    public void BuildJsonObjectFromConfigSnippet_3()
    {
    //notice the use of {} to place all array elements as sections if you have array mix of attributes/sections
      var c = @"struct{
 [a]=1{}
 [a]{q = -23}
 [a]=true{}
      }".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      var obj = new JsonDataMap();
      c.BuildJsonObjectFromConfigSnippet(obj, v => v);
 obj.See();

      Aver.AreEqual(1, obj.Count);
      var arr = obj["a"] as JsonDataArray;
      Aver.IsNotNull(arr);
      Aver.AreEqual(3, arr.Count);

      Aver.AreEqual(1, arr[0].AsInt());

      var sub = arr[1] as JsonDataMap;
      Aver.IsNotNull(sub);
      Aver.AreEqual(-23, sub["q"].AsInt());
      Aver.AreEqual(true, arr[2].AsBool());
    }
  }
}
