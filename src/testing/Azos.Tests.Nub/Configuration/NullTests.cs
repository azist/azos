/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Scripting;

using Azos.Conf;
using Azos.Data;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class NullTests
  {
    [Run]
    public void Laconic_01()
    {
      var conf = "root='null'{}".AsLaconicConfig();

      Aver.AreEqual("root", conf.Name);
      Aver.IsTrue(conf.Exists);
      Aver.IsNotNull(conf.Value);
      Aver.AreEqual("null", conf.Value);
    }

    [Run]
    public void Json_01()
    {
      var conf = "{root: {\"-section-value\": \"null\"}}".AsJSONConfig();

      Aver.AreEqual("root", conf.Name);
      Aver.IsTrue(conf.Exists);
      Aver.IsNotNull(conf.Value);
      Aver.AreEqual("null", conf.Value);
    }

    [Run]
    public void Laconic_02()
    {
      var conf = "root=null{}".AsLaconicConfig();

      Aver.AreEqual("root", conf.Name);
      Aver.IsTrue(conf.Exists);
      Aver.IsNull(conf.Value);
    }

    [Run]
    public void Json_02()
    {
      var conf = "{root:{\"-section-value\": null}}".AsJSONConfig();

      Aver.AreEqual("root", conf.Name);
      Aver.IsTrue(conf.Exists);
      Aver.IsNull(conf.Value);
    }

    [Run]
    public void Laconic_03()
    {
      var conf = "root   =   null  {  }".AsLaconicConfig();

      Aver.AreEqual("root", conf.Name);
      Aver.IsTrue(conf.Exists);
      Aver.IsNull(conf.Value);
    }

    [Run]
    public void Laconic_04()
    {
      var conf = "root{a=\"null\" }".AsLaconicConfig();

      Aver.AreEqual("a", conf.AttrByIndex(0).Name);
      Aver.IsTrue(conf.AttrByIndex(0).Exists);
      Aver.IsNotNull(conf.AttrByIndex(0).Value);
      Aver.AreEqual("null", conf.AttrByIndex(0).Value);
    }

    [Run]
    public void Json_04()
    {
      var conf = "{root: { a: \"null\" }}".AsJSONConfig();

      Aver.AreEqual("a", conf.AttrByIndex(0).Name);
      Aver.IsTrue(conf.AttrByIndex(0).Exists);
      Aver.IsNotNull(conf.AttrByIndex(0).Value);
      Aver.AreEqual("null", conf.AttrByIndex(0).Value);
    }

    [Run]
    public void Laconic_05()
    {
      var conf = "root{a=null }".AsLaconicConfig();

      Aver.AreEqual("a", conf.AttrByIndex(0).Name);
      Aver.IsTrue(conf.AttrByIndex(0).Exists);
      Aver.IsNull(conf.AttrByIndex(0).Value);
    }

    [Run]
    public void Json_05()
    {
      var conf = "{root: { a: null }}".AsJSONConfig();

      Aver.AreEqual("a", conf.AttrByIndex(0).Name);
      Aver.IsTrue(conf.AttrByIndex(0).Exists);
      Aver.IsNull(conf.AttrByIndex(0).Value);
    }

    [Run]
    public void Laconic_06()
    {
      var conf = "root{a=null{} }".AsLaconicConfig();

      Aver.AreEqual("a", conf[0].Name);
      Aver.IsTrue(conf[0].Exists);
      Aver.IsNull(conf[0].Value);
    }

    [Run]
    public void Json_06()
    {
      var conf = "{root: {a: {\"-section-value\": null }}}".AsJSONConfig();

      Aver.AreEqual("a", conf[0].Name);
      Aver.IsTrue(conf[0].Exists);
      Aver.IsNull(conf[0].Value);
    }

    [Run]
    public void Laconic_07()
    {
      var conf = "root{a='null'{} }".AsLaconicConfig();

      Aver.AreEqual("a", conf[0].Name);
      Aver.IsTrue(conf[0].Exists);
      Aver.IsNotNull(conf[0].Value);
      Aver.AreEqual("null", conf[0].Value);
    }

    [Run]
    public void Laconic_08()
    {
      var conf = "root{ a{ } }".AsLaconicConfig();

      Aver.AreEqual("a", conf[0].Name);
      Aver.IsTrue(conf[0].Exists);
      Aver.IsNull(conf[0].Value);
    }

    [Run]
    public void Laconic_09()
    {
      var conf = "root{ a{ b=null } }".AsLaconicConfig();

      Aver.Throws<ConfigException>(() => conf.Navigate("!/a/$doesnotexist"));
      Aver.IsNull(conf.Navigate("!/a/$b").Value);
    }

    [Run]
    public void Laconic_10()
    {
      var conf = "root{ a=null b=2 c='32' d='snake' }".AsLaconicConfig();


      Aver.AreEqual(null, conf.Of("a").ValueAsNullableInt());
      Aver.AreEqual(2, conf.Of("b").ValueAsNullableInt());
      Aver.AreEqual(32, conf.Of("c").ValueAsNullableInt());
      Aver.AreEqual(-9, conf.Of("d").ValueAsNullableInt(-9));
    }

    [Run]
    public void Json_10()
    {
      var conf = "{root: { a: null, b: 2, c: \"32\", d: \"snake\" }}".AsJSONConfig();

      conf.See();

      Aver.AreEqual(null, conf.Of("a").ValueAsNullableInt());
      Aver.AreEqual(2, conf.Of("b").ValueAsNullableInt());
      Aver.AreEqual(32, conf.Of("c").ValueAsNullableInt());
      Aver.AreEqual(-9, conf.Of("d").ValueAsNullableInt(-9));
    }

    [Run]
    public void Laconic_11()
    {
      var c1 = "root=null{ a=null b=2 c='32' d='snake' }".AsLaconicConfig();
      var save = c1.ToLaconicString();
      var c2 = save.AsLaconicConfig();

      c1.See();
      save.See();
      c2.See();

      Aver.IsNull(c1.Value);
      Aver.IsNull(c2.Value);
      Aver.IsNull(c1.Of("a").Value);
      Aver.IsNull(c2.Of("a").Value);
      Aver.IsNotNull(c1.Of("b").Value);
      Aver.IsNotNull(c2.Of("b").Value);

      Aver.AreEqual(null, c2.Value);
      Aver.AreEqual(null, c2.Of("a").Value);
      Aver.AreEqual(2, c2.Of("b").ValueAsInt());
      Aver.AreEqual(32, c2.Of("c").ValueAsInt());
      Aver.AreEqual(0, c2.Of("d").ValueAsInt());
      Aver.AreEqual("snake", c2.Of("d").ValueAsString());
    }

    [Run]
    public void Json_11()
    {
      var c1 = "{root: { a: null, b: 2, c: \"32\", d: \"snake\" }}".AsJSONConfig();
      var save = c1.ToJSONString();
      var c2 = save.AsJSONConfig();

      c1.See();
      save.See();
      c2.See();

      Aver.IsNull(c1.Value);
      Aver.IsNull(c2.Value);
      Aver.IsNull(c1.Of("a").Value);
      Aver.IsNull(c2.Of("a").Value);
      Aver.IsNotNull(c1.Of("b").Value);
      Aver.IsNotNull(c2.Of("b").Value);

      Aver.AreEqual(null, c2.Value);
      Aver.AreEqual(null, c2.Of("a").Value);
      Aver.AreEqual(2, c2.Of("b").ValueAsInt());
      Aver.AreEqual(32, c2.Of("c").ValueAsInt());
      Aver.AreEqual(0, c2.Of("d").ValueAsInt());
      Aver.AreEqual("snake", c2.Of("d").ValueAsString());
    }

    [Run]
    public void Json_12()
    {
      var c1 = "{root: { \"-section-value\": null, a: null, b: 2, c: \"32\", d: \"snake\" }}".AsJSONConfig();
      var save = c1.ToJSONString();
      var c2 = save.AsJSONConfig();

      c1.See();
      save.See();
      c2.See();

      Aver.IsNull(c1.Value);
      Aver.IsNull(c2.Value);
      Aver.IsNull(c1.Of("a").Value);
      Aver.IsNull(c2.Of("a").Value);
      Aver.IsNotNull(c1.Of("b").Value);
      Aver.IsNotNull(c2.Of("b").Value);

      Aver.AreEqual(null, c2.Value);
      Aver.AreEqual(null, c2.Of("a").Value);
      Aver.AreEqual(2, c2.Of("b").ValueAsInt());
      Aver.AreEqual(32, c2.Of("c").ValueAsInt());
      Aver.AreEqual(0, c2.Of("d").ValueAsInt());
      Aver.AreEqual("snake", c2.Of("d").ValueAsString());
    }

    [Run]
    public void Json_13()
    {
      var c1 = "{root: { \"-section-value\": -123, a: null, b: 2, c: \"32\", d: \"snake\" }}".AsJSONConfig();
      var save = c1.ToJSONString();
      var c2 = save.AsJSONConfig();

      c1.See();
      save.See();
      c2.See();

      Aver.IsNotNull(c1.Value);
      Aver.IsNotNull(c2.Value);
      Aver.IsNull(c1.Of("a").Value);
      Aver.IsNull(c2.Of("a").Value);
      Aver.IsNotNull(c1.Of("b").Value);
      Aver.IsNotNull(c2.Of("b").Value);

      Aver.AreEqual(-123, c2.ValueAsInt());
      Aver.AreEqual(null, c2.Of("a").Value);
      Aver.AreEqual(2, c2.Of("b").ValueAsInt());
      Aver.AreEqual(32, c2.Of("c").ValueAsInt());
      Aver.AreEqual(0, c2.Of("d").ValueAsInt());
      Aver.AreEqual("snake", c2.Of("d").ValueAsString());
    }

  }

}
