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
    public void LaconicCtorFileName_UTF8()
    {
      var sut = new LaconicConfiguration("nub-test.laconf");
      Aver.AreEqual("utf8", sut.Root.ValOf("encoding"));
      check(sut);
    }

    [Run]
    public void LaconicCtorFileName_UTF16BE()
    {
      var sut = new LaconicConfiguration("nub-test-utf16-be.laconf");
      Aver.AreEqual("utf16be", sut.Root.ValOf("encoding"));
      check(sut);
    }

    [Run]
    public void LaconicCtorFileName_UTF16LE()
    {
      var sut = new LaconicConfiguration("nub-test-utf16-le.laconf");
      Aver.AreEqual("utf16le", sut.Root.ValOf("encoding"));
      check(sut);
    }


    [Run]
    public void ProviderLoadFromFile_Laconic_UTF8()
    {
      var sut = Conf.Configuration.ProviderLoadFromFile("nub-test.laconf");
      Aver.AreEqual("utf8", sut.Root.ValOf("encoding"));
      check(sut);
    }

    private void check(Conf.Configuration sut)
    {
      sut.See();
      Aver.AreEqual("𝄞𝄢doremi", sut.Root.ValOf("music"));
      Aver.AreEqual("我能吞下玻璃而不傷身體", sut.Root.ValOf("chinese"));
      Aver.AreEqual("ვეპხის ტყაოსანი შოთა რუსთაველი", sut.Root.ValOf("rustaveli"));
      Aver.AreEqual("ЭЮЯ?", sut.Root.ValOf("russian"));
      Aver.AreEqual("simpletext", sut.Root.ValOf("english"));

    }

  }
}
