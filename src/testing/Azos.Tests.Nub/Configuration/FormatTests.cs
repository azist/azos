/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class FormatTests
  {
    private const string LACONIC_SOURCE = @"root=-900{a=1 b=2 sub{z='my\nmessage!'}}";
    private const string XML_SOURCE = @"<root a=""1"" b=""2"">-900<sub z=""my&#10;message!"" /></root>";
    private const string JSON_SOURCE = @"{ ""root"":{ ""-section-value"":""-900"",""a"":""1"",""b"":""2"",""sub"":{ ""z"":""my\nmessage!""} } }";

    private void ensureInvariant(ConfigSectionNode cfg)
    {
      Aver.IsFalse(cfg.Modified);
      Aver.AreEqual("root", cfg.Name);
      Aver.AreEqual(-900, cfg.ValueAsInt());
      Aver.AreEqual(1, cfg.ChildCount);
      Aver.AreEqual(2, cfg.AttrCount);
      Aver.IsTrue(cfg.HasChildren);

      Aver.AreEqual("a", cfg.AttrByIndex(0).Name);
      Aver.AreEqual("1", cfg.AttrByIndex(0).Value);

      Aver.AreEqual("b", cfg.AttrByIndex(1).Name);
      Aver.AreEqual("2", cfg.AttrByIndex(1).Value);

      Aver.IsTrue(cfg[0].Exists);
      Aver.IsFalse(cfg[1900].Exists);
      Aver.AreEqual("sub", cfg[0].Name);
      Aver.AreEqual(null, cfg[0].Value);

      Aver.AreEqual("my\nmessage!", cfg[0].AttrByName("z").Value);
    }


    [Run]
    public void AsLaconicConfig()
    {
      var cfg = LACONIC_SOURCE.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      ensureInvariant(cfg);
    }

    [Run]
    public void AsJsonConfig()
    {
      var cfg = JSON_SOURCE.AsJSONConfig(handling: Data.ConvertErrorHandling.Throw);
      ensureInvariant(cfg);
    }

    [Run]
    public void AsXmlConfig()
    {
      var cfg = XML_SOURCE.AsXMLConfig(handling: Data.ConvertErrorHandling.Throw);
      ensureInvariant(cfg);
    }

    [Run]
    public void LaconicRoundTrip()
    {
      var cfg = LACONIC_SOURCE.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      ensureInvariant(cfg);

      var serializedLaconic = cfg.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact);
      $"SERIALIZED COMPACT LACONIC: \n{serializedLaconic}".See();

      var cfg2 = serializedLaconic.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      ensureInvariant(cfg2);

      serializedLaconic = cfg.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint);
      $"SERIALIZED PRETTY LACONIC: \n{serializedLaconic}".See();

      var cfg3 = serializedLaconic.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      ensureInvariant(cfg3);
    }

    [Run]
    public void XmlRoundTrip()
    {
      var cfg = XML_SOURCE.AsXMLConfig(handling: Data.ConvertErrorHandling.Throw);
      ensureInvariant(cfg);

      var serializedXML = cfg.ToXmlString();
      $"SERIALIZED XML: \n{serializedXML}".See();

      var cfg2 = serializedXML.AsXMLConfig(handling: Data.ConvertErrorHandling.Throw);
      ensureInvariant(cfg2);
    }

    [Run]
    public void JsonRoundTrip()
    {
      var cfg = JSON_SOURCE.AsJSONConfig(handling: Data.ConvertErrorHandling.Throw);
      ensureInvariant(cfg);

      var serializedJSON = cfg.ToJSONString();
      $"SERIALIZED JSON: \n{serializedJSON}".See();

      var cfg2 = serializedJSON.AsJSONConfig(handling: Data.ConvertErrorHandling.Throw);
      ensureInvariant(cfg2);

      serializedJSON = cfg.ToJSONString(Azos.Serialization.JSON.JsonWritingOptions.PrettyPrintASCII);
      $"SERIALIZED PRETTY JSON: \n{serializedJSON}".See();

      var cfg3 = serializedJSON.AsJSONConfig(handling: Data.ConvertErrorHandling.Throw);
      ensureInvariant(cfg3);
    }

    [Run]
    public void JsonRoundTripWithSameName()
    {
      // Laconic:  root{a{n=1}a{}a=3{n=2}}
      // JSON:     {"root":{"a":[{"n":"1"},{},{"-section-value":"3","n":"2"}]}}
      var c1 = @"root
{
  a{ n=1}
  a{ }
  a=3{ n=2}
}
//".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      var json = c1.ToJSONString();
      c1.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact).See();
      json.See();

      var c2 = json.AsJSONConfig();

      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(c1, c2));
    }


    [Run]
    public void ProviderLoadFromString_XML()
    {
      var cfg = Azos.Conf.Configuration.ProviderLoadFromString(XML_SOURCE, "xml");
      ensureInvariant(cfg.Root);
    }

    [Run]
    public void ProviderLoadFromString_Laconic()
    {
      var cfg = Azos.Conf.Configuration.ProviderLoadFromString(LACONIC_SOURCE, "laconf");
      ensureInvariant(cfg.Root);
    }

    [Run]
    public void ProviderLoadFromString_JSON()
    {
      var cfg = Azos.Conf.Configuration.ProviderLoadFromString(JSON_SOURCE, "json");
      ensureInvariant(cfg.Root);
    }


  }
}