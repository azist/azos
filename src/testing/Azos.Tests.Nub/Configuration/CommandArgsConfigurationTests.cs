/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;
using CAC = Azos.Conf.CommandArgsConfiguration;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class CmdArgsConfigurationTests
  {
    private string[] args = {
        @"tool.exe",
        @"c:\input.file",
        @"d:\output.file",
        @"-compress",
        @"level=100",
        @"method=zip",
        @"-shadow",
        @"fast",
        @"swap=1024",
        @"-large"
      };


    [Run]
    public void General()
    {
      var conf = new CAC(args);

      Aver.AreEqual(@"tool.exe", conf.Root.AttrByIndex(0).ValueAsString());
      Aver.AreEqual(@"c:\input.file", conf.Root.AttrByIndex(1).ValueAsString());
      Aver.AreEqual(@"d:\output.file", conf.Root.AttrByIndex(2).ValueAsString());

      Aver.AreEqual(true, conf.Root["compress"].Exists);
      Aver.AreEqual(100, conf.Root["compress"].AttrByName("level").ValueAsInt());
      Aver.AreEqual("zip", conf.Root["compress"].AttrByName("method").ValueAsString());

      Aver.AreEqual(true, conf.Root["shadow"].Exists);
      Aver.AreEqual("fast", conf.Root["shadow"].AttrByIndex(0).Value);
      Aver.AreEqual(1024, conf.Root["shadow"].AttrByName("swap").ValueAsInt());

      Aver.AreEqual(true, conf.Root["large"].Exists);
    }

    [Run]
    public void Case2()
    {
      var args = new[] { "exe", "1", "2", "3", "-h", "ok=yes" };
      var cfg = new CAC(args).Root;

      Aver.AreEqual(4, cfg.AttrCount);
      Aver.AreEqual("exe", cfg.AttrByIndex(0).Value);
      Aver.AreEqual("1", cfg.AttrByIndex(1).Value);
      Aver.AreEqual("2", cfg.AttrByIndex(2).Value);
      Aver.AreEqual("3", cfg.AttrByIndex(3).Value);

      Aver.AreEqual(1, cfg.ChildCount);
      Aver.AreEqual(1, cfg["h"].AttrCount);
      Aver.AreEqual("yes", cfg["h"].AttrByIndex(0).Value);
      Aver.AreEqual("yes", cfg["h"].AttrByName("ok").Value);
    }

    [Run]
    public void Case3()
    {
      var args = new[] { "exe", "-h", "ok=yes", "-d", "1", "2", "3" };
      var cfg = new CAC(args).Root;
      Aver.AreEqual(2, cfg.ChildCount);

      var d = cfg["d"];
      Aver.AreEqual(3, d.AttrCount);
      Aver.AreEqual("1", d.AttrByIndex(0).Value);
      Aver.AreEqual("2", d.AttrByIndex(1).Value);
      Aver.AreEqual("3", d.AttrByIndex(2).Value);

      var h = cfg["h"];
      Aver.AreEqual(1, h.AttrCount);
      Aver.AreEqual("yes", h.AttrByIndex(0).Value);
      Aver.AreEqual("yes", h.AttrByName("ok").Value);
    }
  }
}

