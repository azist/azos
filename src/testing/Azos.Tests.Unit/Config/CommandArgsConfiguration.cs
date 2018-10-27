/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;
using CAC = Azos.Conf.CommandArgsConfiguration;

namespace Azos.Tests.Unit.Config
{
    [Runnable(TRUN.BASE)]
    public class CmdArgsConfiguration
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
        public void GeneralCmdArgs()
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

    }
}

