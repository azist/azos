/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Scripting;


using Azos.Apps;
using Azos.Log;
using Azos.Conf;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class ProviderLoadTests
  {
    [Run]
    public void LaconicCtorFileName()
    {
      var sut = new LaconicConfiguration("nub-test.laconf");
      sut.See();
    }


    [Run]
    public void ProviderLoadFromFile_Laconic()
    {
      var sut = Conf.Configuration.ProviderLoadFromFile("nub-test.laconf");
      sut.See();
    }
  }
}
