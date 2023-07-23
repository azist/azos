/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Scripting;
using Azos.Apps;
using Azos.Log;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class ExcludeTests
  {
    [Run]
    public void Test_01()
    {
      var cfg = @"
root
{
  env=DEV

  a{ id=1 }
  b
  {
    id=2
    _exclude
    {
      condition{ type='Azos.Scripting.Expressions.Conf.ValuePattern, Azos' expr='$(/$env)' include='DEV' case=false }
    }
  }
  c{ id=3 }
  b{ id=4 }
}".AsLaconicConfig();


      Aver.AreEqual(4, cfg.ChildCount);
      cfg.ProcessExcludes(true);
      Aver.AreEqual(3, cfg.ChildCount);
      Aver.AreEqual(4, cfg["b"].Of("id").ValueAsInt());
    }
  }
}
