/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Scripting;
using System;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class IncludeTests2
  {
    public static readonly IConfigSectionNode CONF = @"

root
{
  a{}
  b{}
}
      ".AsLaconicConfig();

    [Run]
    public void Include()
    {
      var c1 = @"root{ a{} b{} }".AsLaconicConfig().Configuration;
      var c2 = @"root2{ z=900 zuza{ looza{x=100}} }".AsLaconicConfig().Configuration;


      c1.Include(c1.Root["b"], c2.Root);

      Console.WriteLine(c1.Root.ToLaconicString());

      Aver.AreEqual(900, c1.Root.AttrByName("z").ValueAsInt());//got merged from root2
      Aver.IsTrue(c1.Root["a"].Exists);
      Aver.IsFalse(c1.Root["b"].Exists);
      Aver.IsTrue(c1.Root["zuza"].Exists);
      Aver.AreEqual(100, c1.Root.Navigate("zuza/looza/$x").ValueAsInt());
    }

  }
}