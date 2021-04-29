/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using Azos.Scripting;
using Azos.Security;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Security
{
  [Runnable]
  public class SysAuthTokenTests
  {
    [Run]
    public void Assigned()
    {
      var t = new SysAuthToken();
      Aver.IsFalse(t.Assigned);

      t = new SysAuthToken("a", "b");
      Aver.IsTrue(t.Assigned);
    }

    [Run]
    public void UnassignedValues()
    {
      var t = new SysAuthToken();
      Aver.AreEqual("<null>::<null>", t.ToString());
    }

    [Run]
    public void ToStringTest()
    {
      var t = new SysAuthToken("a", "b");
      Aver.AreEqual("a::b", t.ToString());
    }

    [Run]
    public void SerDeser()
    {
      var t = new SysAuthToken("a", "b");
      var s = t.ToString();
      var t2 = SysAuthToken.Parse(s);
      Aver.AreEqual("a", t2.Realm);
      Aver.AreEqual("b", t2.Data);
    }

    [Run]
    public void TryParse()
    {
      Aver.IsTrue(SysAuthToken.TryParse(null, out var got));
      Aver.IsFalse(got.Assigned);

      Aver.IsTrue(SysAuthToken.TryParse("", out got));
      Aver.IsFalse(got.Assigned);

      Aver.IsTrue(SysAuthToken.TryParse("frog::toad", out got));
      Aver.AreEqual("frog", got.Realm);
      Aver.AreEqual("toad", got.Data);

      Aver.IsFalse(SysAuthToken.TryParse("::toad", out got));
      Aver.IsFalse(got.Assigned);

      Aver.IsFalse(SysAuthToken.TryParse("frog::", out got));
      Aver.IsFalse(got.Assigned);

      Aver.IsFalse(SysAuthToken.TryParse("frogtoad", out got));
      Aver.IsFalse(got.Assigned);

      Aver.IsFalse(SysAuthToken.TryParse("sys:lll", out got));
      Aver.IsFalse(got.Assigned);
    }

    [Run]
    public void Parse()
    {
      Aver.IsFalse(SysAuthToken.Parse(null).Assigned);
      Aver.IsFalse(SysAuthToken.Parse("").Assigned);


      var v = SysAuthToken.Parse("frog::toad");
      Aver.AreEqual("frog", v.Realm);
      Aver.AreEqual("toad", v.Data);

      Aver.Throws<SecurityException>( () => SysAuthToken.Parse("::toad"));
      Aver.Throws<SecurityException>(() => SysAuthToken.Parse("frog::"));
      Aver.Throws<SecurityException>(() => SysAuthToken.Parse("frogtoad"));
      Aver.Throws<SecurityException>(() => SysAuthToken.Parse("frog:toad"));
    }

    [Run]
    public void Eq()
    {
      Aver.AreEqual(new SysAuthToken("a","b"), new SysAuthToken("a", "b"));
      Aver.AreNotEqual(new SysAuthToken("a", "b"), new SysAuthToken("aaaa", "b"));
      Aver.AreNotEqual(new SysAuthToken("a", "b"), new SysAuthToken("a", "bbbb"));
    }

    [Run]
    public void EqOp()
    {
      Aver.IsTrue(new SysAuthToken("a", "b") == new SysAuthToken("a", "b"));
      Aver.IsFalse(new SysAuthToken("a", "b") != new SysAuthToken("a", "b"));

      Aver.IsFalse(new SysAuthToken("a", "b") == new SysAuthToken("aaaa", "b"));
      Aver.IsTrue(new SysAuthToken("a", "b") != new SysAuthToken("aaaa", "b"));

      Aver.IsFalse(new SysAuthToken("a", "b") == new SysAuthToken("a", "bbbb"));
      Aver.IsTrue(new SysAuthToken("a", "b") != new SysAuthToken("a", "bbbb"));
    }

    [Run]
    public void Bin()
    {
      var bin = new byte[]{1,2,3,4,5};
      var t = new SysAuthToken("sys", bin);
      t.See();
      var bin2 = t.BinData;
      Aver.IsTrue(bin.MemBufferEquals(bin2));
    }

    [Run]
    public void JsonSer()
    {
      var d = new {a = 199, t = new SysAuthToken("sys","1234")};
      var json = d.ToJson();
      json.See();

      var got = json.JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(got);

      Aver.AreEqual(199, got["a"].AsInt());
      Aver.AreEqual("sys::1234", got["t"].AsString());
    }


    public class Data : TypedDoc
    {
      [Field] public SysAuthToken Token { get;set;}
    }


    [Run]
    public void JsonSerDeser()
    {
      var d = new Data{ Token = new SysAuthToken("sys", "1234") };
      var json = d.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);
      json.See();

      var map = json.JsonToDataObject() as JsonDataMap;
      var got = JsonReader.ToDoc<Data>(map);

      Aver.AreEqual(d.Token, got.Token);
    }

  }
}


