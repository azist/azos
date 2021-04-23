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
  public class SectionValuesTests
  {

    private void invariant(IConfigSectionNode cfg)
    {
      Aver.AreEqual("root", cfg.Value);
      Aver.AreEqual("1", cfg[0].Value);
      Aver.AreEqual("10", cfg[0].AttrByIndex(0).Value);
      Aver.AreEqual("20", cfg[1].AttrByIndex(0).Value);
    }


    [Run]
    public void Laconic()
    {
      var src = @"
app=root
{
  a=1{v=10}
  a=2{v=20}
}
";
      var cfg = src.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
      invariant(cfg);
    }

    [Run]
    public void JSON()
    {
      var src = @"
{app: 
 {'-section-value': 'root',
  a: [
     {'-section-value': 1, v: 10},
     {'-section-value': 2, v: 20}
    ]
 }
}
";
      var cfg = src.AsJSONConfig(handling: ConvertErrorHandling.Throw);
      invariant(cfg);
    }

    [Run]
    public void XML()
    {
      var src = @"
<app>
  <a v=""10"">1</a>
  <a v=""20"">2</a>root</app>
";
      var cfg = src.AsXMLConfig(handling: ConvertErrorHandling.Throw);
      invariant(cfg);
    }

    [Run]
    public void XMLRoundtrip()
    {
      var src = @"
<app>
  <a v=""10"">1</a>
  <a v=""20"">2</a>root</app>
";
      var cfg = src.AsXMLConfig(handling: ConvertErrorHandling.Throw);
      invariant(cfg);
      var xml = cfg.ToXmlString();
      cfg = xml.AsXMLConfig(handling: ConvertErrorHandling.Throw);
      invariant(cfg);
    }

  }
}