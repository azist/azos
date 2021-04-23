/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class SameAttrSectionNamesTests
  {
    [Run]
    public void ReadLaconic()
    {
      var src = @"
app=3
{
  a=1
  a{v=123}
}
";
      var cfg = src.AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      Aver.AreEqual(3, cfg.ValueAsInt());
      Aver.AreEqual(1, cfg.AttrByName("a").ValueAsInt());
      Aver.AreEqual(123, cfg["a"].AttrByName("v").ValueAsInt());
    }

    [Run]
    public void ReadLaconicWriteJson()
    {
      var src = @"
app=3
{
  a=1
  a{v=123}
}
";
      var cfg = src.AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      Aver.AreEqual(3, cfg.ValueAsInt());
      Aver.AreEqual(1, cfg.AttrByName("a").ValueAsInt());
      Aver.AreEqual(123, cfg["a"].AttrByName("v").ValueAsInt());

      var json = cfg.ToJSONString();
      json.See();

      cfg = json.AsJSONConfig(handling: ConvertErrorHandling.Throw);
      Aver.AreEqual(3, cfg.ValueAsInt());
      Aver.AreEqual(1, cfg.AttrByName("a").ValueAsInt());
      Aver.AreEqual(123, cfg["a"].AttrByName("v").ValueAsInt());
    }

    [Run]
    public void ReadLaconicWriteJson_ManyAttrs()
    {
      var src = @"
app=3
{
  a=1
  a=2
  b=3
  a{v=123}
}
";
      var cfg = src.AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      Aver.AreEqual(3, cfg.ValueAsInt());
      Aver.AreEqual(1, cfg.AttrByName("a").ValueAsInt());
      Aver.AreEqual(3, cfg.AttrCount);
      Aver.AreEqual(1, cfg.AttrByIndex(0).ValueAsInt());
      Aver.AreEqual("2", cfg.AttrByIndex(1).Value);
      Aver.AreEqual("3", cfg.AttrByIndex(2).Value);
      Aver.AreEqual("3", cfg.AttrByName("b").Value);
      Aver.AreEqual(123, cfg["a"].AttrByName("v").ValueAsInt());

      var json = cfg.ToJSONString();
      json.See();

      cfg = json.AsJSONConfig(handling: ConvertErrorHandling.Throw);
      Aver.AreEqual(3, cfg.ValueAsInt());
      Aver.AreEqual(1, cfg.AttrByName("a").ValueAsInt());
      Aver.AreEqual(3, cfg.AttrCount);
      Aver.AreEqual(1, cfg.AttrByIndex(0).ValueAsInt());
      Aver.AreEqual("2", cfg.AttrByIndex(1).Value);
      Aver.AreEqual("3", cfg.AttrByIndex(2).Value);
      Aver.AreEqual("3", cfg.AttrByName("b").Value);
      Aver.AreEqual(123, cfg["a"].AttrByName("v").ValueAsInt());
    }

    [Run]
    public void ReadXML()
    {
      var src = @"
<app a=""1"">3
  <a v=""123""/>
</app>
";
      var cfg = src.AsXMLConfig(handling: ConvertErrorHandling.Throw);

      Aver.AreEqual(3, cfg.ValueAsInt());
      Aver.AreEqual(1, cfg.AttrByName("a").ValueAsInt());
      Aver.AreEqual(123, cfg["a"].AttrByName("v").ValueAsInt());
    }

    [Run]
    public void ReadJSON()
    {
      var src = @"
{ ""app"": {""-section-value"": 3, ""a"": [1, {""v"": 123}]}}";
      var cfg = src.AsJSONConfig(handling: ConvertErrorHandling.Throw);

      Aver.AreEqual(3, cfg.ValueAsInt());
      Aver.AreEqual(1, cfg.AttrByName("a").ValueAsInt());
      Aver.AreEqual(123, cfg["a"].AttrByName("v").ValueAsInt());
    }

    [Run]
    public void ReadJSON_WithNullAttr()
    {
      var src = @"
{ ""app"": {""-section-value"": 3, ""a"": [1, null, {""v"": 123}]}}";
      var cfg = src.AsJSONConfig(handling: ConvertErrorHandling.Throw);

      Aver.AreEqual(3, cfg.ValueAsInt());
      Aver.AreEqual(1, cfg.AttrByName("a").ValueAsInt());
      Aver.AreEqual(1, cfg.AttrByIndex(0).ValueAsInt());
      Aver.IsTrue(cfg.AttrByIndex(1).Exists);
      Aver.AreEqual(null, cfg.AttrByIndex(1).Value);
      Aver.AreEqual(123, cfg["a"].AttrByName("v").ValueAsInt());
    }


  }
}