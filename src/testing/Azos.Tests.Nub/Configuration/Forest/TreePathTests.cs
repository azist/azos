/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Conf.Forest;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration.Forest
{
  [Runnable]
  public class TreePathTests
  {
    [Run, Aver.Throws(typeof(CallGuardException))]
    public void Null()
    {
      var sut = new TreePath(null);
    }

    [Run, Aver.Throws(typeof(CallGuardException))]
    public void Empty()
    {
      var sut = new TreePath("");
    }

    [Run, Aver.Throws(typeof(CallGuardException))]
    public void Blank()
    {
      var sut = new TreePath("                      ");
    }

    [Run, Aver.Throws(typeof(ConfigException), "segment length")]
    public void TooLong()
    {
      var sut = new TreePath("a/00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000/b");
    }

    [Run, Aver.Throws(typeof(ConfigException), "segment count")]
    public void TooMany()
    {
      var path = new StringBuilder();
      for(var i=0; i < Constraints.PATH_SEGMENT_MAX_COUNT + 1; i++)
      {
        path.Append("/a");
      }

      var sut = new TreePath(path.ToString());
    }


    [Run]
    public void OnlySlashes()
    {
      var sut = new TreePath("//////////");
      //sut.See();
      Aver.IsTrue(sut.IsRoot);
      Aver.AreEqual(0, sut.Count);
    }

    [Run]
    public void OnlySlashesWithSpaces()
    {
      var sut = new TreePath("/ /// //// //");
      Aver.IsTrue(sut.IsRoot);
      Aver.AreEqual(0, sut.Count);
    }

    [Run]
    public void Normal_000()
    {
      var sut = new TreePath("/a");
      Aver.AreEqual(1, sut.Count);
      Aver.AreEqual("a", sut[0]);
    }

    [Run]
    public void Normal_010()
    {
      var sut = new TreePath("/ a");
      Aver.AreEqual(1, sut.Count);
      Aver.AreEqual("a", sut[0]);
    }

    [Run]
    public void Normal_020()
    {
      var sut = new TreePath("/ a              ");
      Aver.AreEqual(1, sut.Count);
      Aver.AreEqual("a", sut[0]);
    }

    [Run]
    public void Normal_030()
    {
      var sut = new TreePath("/ a  / ");
      Aver.AreEqual(1, sut.Count);
      Aver.AreEqual("a", sut[0]);
    }

    [Run]
    public void Normal_040()
    {
      var sut = new TreePath("/ A  / ");
      Aver.AreEqual(1, sut.Count);
      Aver.AreEqual("a", sut[0]);
    }

    [Run]
    public void Normal_050()
    {
      var sut = new TreePath("A/b/C");
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual("a", sut[0]);
      Aver.AreEqual("b", sut[1]);
      Aver.AreEqual("c", sut[2]);
    }

    [Run]
    public void Normal_060()
    {
      var sut = new TreePath(" A/b/C");
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual("a", sut[0]);
      Aver.AreEqual("b", sut[1]);
      Aver.AreEqual("c", sut[2]);
    }

    [Run]
    public void Normal_070()
    {
      var sut = new TreePath(" A/b/C   ");
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual("a", sut[0]);
      Aver.AreEqual("b", sut[1]);
      Aver.AreEqual("c", sut[2]);
    }

    [Run]
    public void Normal_080()
    {
      var sut = new TreePath(" A/b/C  / ");
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual("a", sut[0]);
      Aver.AreEqual("b", sut[1]);
      Aver.AreEqual("c", sut[2]);
    }

    [Run]
    public void Normal_090()
    {
      var sut = new TreePath(" A/b/C /  / ");
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual("a", sut[0]);
      Aver.AreEqual("b", sut[1]);
      Aver.AreEqual("c", sut[2]);
    }

    [Run]
    public void Normal_100()
    {
      var sut = new TreePath(" A/b vs d/C /  / ");
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual("a", sut[0]);
      Aver.AreEqual("b vs d", sut[1]);
      Aver.AreEqual("c", sut[2]);
    }

    [Run]
    public void Normal_110()
    {
      var sut = new TreePath(" A/             b vs d    /C");
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual("a", sut[0]);
      Aver.AreEqual("b vs d", sut[1]);
      Aver.AreEqual("c", sut[2]);
    }

    [Run]
    public void Normal_120()
    {
      var sut = new TreePath(" A/             b vs d    /C or q               ");
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual("a", sut[0]);
      Aver.AreEqual("b vs d", sut[1]);
      Aver.AreEqual("c or q", sut[2]);
    }

    [Run]
    public void Normal_130_Unicode()
    {
      var sut = new TreePath("English/АНГЛИЙСКИЙ/englisch/英语/Αγγλικά");
      Aver.AreEqual(5, sut.Count);
      Aver.AreEqual("english", sut[0]);
      Aver.AreEqual("английский", sut[1]);
      Aver.AreEqual("englisch", sut[2]);
      Aver.AreEqual("英语", sut[3]);
      Aver.AreEqual("αγγλικά", sut[4]);
    }

    [Run]
    public void Escapes_000()
    {
      var sut = new TreePath("a/b%2fd/c");
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual("a", sut[0]);
      Aver.AreEqual("b/d", sut[1]);
      Aver.AreEqual("c", sut[2]);
    }

    [Run]
    public void Escapes_010()
    {
      var sut = new TreePath("100%25");
      Aver.AreEqual(1, sut.Count);
      Aver.AreEqual("100%", sut[0]);
    }

    [Run]
    public void Escapes_015()
    {
      var sut = new TreePath("100%25 ");
      Aver.AreEqual(1, sut.Count);
      Aver.AreEqual("100%", sut[0]);
    }

    [Run]
    public void Escapes_016()
    {
      var sut = new TreePath("100 %25 ");
      Aver.AreEqual(1, sut.Count);
      Aver.AreEqual("100 %", sut[0]);
    }

    [Run]
    public void Escapes_020()
    {
      var sut = new TreePath(@"\a/\b%2f\d/c");
      Aver.AreEqual(3, sut.Count);
      Aver.AreEqual(@"\a", sut[0]);
      Aver.AreEqual(@"\b/\d", sut[1]);
      Aver.AreEqual("c", sut[2]);
    }

    [Run, Aver.Throws(typeof(ConfigException), "Invalid escape")]
    public void Escapes_030()
    {
      var sut = new TreePath(@"a/%zz");
    }

    [Run, Aver.Throws(typeof(ConfigException), "Invalid escape")]
    public void Escapes_040()
    {
      var sut = new TreePath(@"a/%2");
    }

    [Run, Aver.Throws(typeof(ConfigException), "Invalid escape")]
    public void Escapes_050()
    {
      var sut = new TreePath(@"a/%");
    }

    [Run, Aver.Throws(typeof(ConfigException), "Invalid escape")]
    public void Escapes_060()
    {
      var sut = new TreePath(@"%a/b");
    }

    [Run, Aver.Throws(typeof(ConfigException), "Invalid escape")]
    public void Escapes_070()
    {
      var sut = new TreePath(@"a%/%b");
    }
  }
}
