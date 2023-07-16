/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Conf;
using System.Linq;
using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class IncludeTests3
  {
    [Run]
    public void IncludeFile()
    {
      var cfg = @"cfg
      {
        a=123
        b='I am here'
        _include
        {
          name=CameFromFile
          pre-process-all-includes=true
          file = 'nub-test.laconf'
        }
      }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      cfg.ProcessAllExistingIncludes();

      cfg.ToLaconicString().See();

      Aver.IsTrue(cfg["CameFromFile"].Exists);
      Aver.AreEqual(123, cfg.Of("a").ValueAsInt());
      Aver.AreEqual("I am here", cfg.Of("b").Value);

      Aver.AreEqual(1, cfg["CameFromFile"].Of("a").ValueAsInt());
      Aver.AreEqual(2, cfg["CameFromFile"].Of("b").ValueAsInt());
      Aver.AreEqual("ЭЮЯ?", cfg["CameFromFile"].Of("russian").Value);
      Aver.AreEqual("ვეპხის ტყაოსანი შოთა რუსთაველი", cfg["CameFromFile"].Of("rustaveli").Value);
    }



  }
}