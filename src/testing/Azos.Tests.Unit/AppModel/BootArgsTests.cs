/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps.Hosting;
using Azos.Scripting;

namespace Azos.Tests.Unit.AppModel
{
  [Runnable]
  public class BootArgsTests
  {
    [Run]
    public void Test01()
    {
      var sut = new BootArgs(new string[0]);

      Aver.IsFalse(sut.IsDaemon);
      Aver.IsFalse(sut.IsGoverned);
      Aver.AreEqual(0, sut.GovPort);
      Aver.AreEqual(null, sut.GovHost);
      Aver.AreEqual(null, sut.GovApp);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(0, sut.ForApplication.Length);
    }

    [Run]
    public void Test02()
    {
      var sut = new BootArgs(new string[]{"xyz.laconf"});

      Aver.IsFalse(sut.IsDaemon);
      Aver.IsFalse(sut.IsGoverned);
      Aver.AreEqual(0, sut.GovPort);
      Aver.AreEqual(null, sut.GovHost);
      Aver.AreEqual(null, sut.GovApp);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("-config", sut.ForApplication[0]);
      Aver.AreEqual("xyz.laconf", sut.ForApplication[1]);
    }

    [Run]
    public void Test03()
    {
      var sut = new BootArgs(new string[] { "some-argument" });

      Aver.IsFalse(sut.IsDaemon);
      Aver.IsFalse(sut.IsGoverned);
      Aver.AreEqual(0, sut.GovPort);
      Aver.AreEqual(null, sut.GovHost);
      Aver.AreEqual(null, sut.GovApp);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(1, sut.ForApplication.Length);
      Aver.AreEqual("some-argument", sut.ForApplication[0]);
    }

    [Run]
    public void Test04()
    {
      var sut = new BootArgs(new string[] { "a.laconf", "b.laconf"  });

      Aver.IsFalse(sut.IsDaemon);
      Aver.IsFalse(sut.IsGoverned);
      Aver.AreEqual(0, sut.GovPort);
      Aver.AreEqual(null, sut.GovHost);
      Aver.AreEqual(null, sut.GovApp);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("a.laconf", sut.ForApplication[0]);
      Aver.AreEqual("b.laconf", sut.ForApplication[1]);
    }

    [Run]
    public void Test05()
    {
      var sut = new BootArgs(new string[] { "daemon", "xyz.laconf" });

      Aver.IsTrue(sut.IsDaemon);
      Aver.IsFalse(sut.IsGoverned);
      Aver.AreEqual(0, sut.GovPort);
      Aver.AreEqual(null, sut.GovHost);
      Aver.AreEqual(null, sut.GovApp);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("-config", sut.ForApplication[0]);
      Aver.AreEqual("xyz.laconf", sut.ForApplication[1]);
    }

    [Run]
    public void Test06()
    {
      var sut = new BootArgs(new string[] { "daemon", "gov://5678:app/1", "abc.laconf" });

      Aver.IsTrue(sut.IsDaemon);
      Aver.IsTrue(sut.IsGoverned);
      Aver.AreEqual(5678, sut.GovPort);
      Aver.AreEqual(null, sut.GovHost);
      Aver.AreEqual("app/1", sut.GovApp);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("-config", sut.ForApplication[0]);
      Aver.AreEqual("abc.laconf", sut.ForApplication[1]);
    }

    [Run]
    public void Test07()
    {
      var sut = new BootArgs(new string[] { "gov://5678:app/123", "abc.laconf" });

      Aver.IsFalse(sut.IsDaemon);
      Aver.IsTrue(sut.IsGoverned);
      Aver.AreEqual(5678, sut.GovPort);
      Aver.AreEqual(null, sut.GovHost);
      Aver.AreEqual("app/123", sut.GovApp);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("-config", sut.ForApplication[0]);
      Aver.AreEqual("abc.laconf", sut.ForApplication[1]);
    }

    [Run]
    public void Test08()
    {
      var sut = new BootArgs(new string[] { "gov://mix02.local#1678:app/123", "abc.laconf" });

      Aver.IsFalse(sut.IsDaemon);
      Aver.IsTrue(sut.IsGoverned);
      Aver.AreEqual(1678, sut.GovPort);
      Aver.AreEqual("mix02.local", sut.GovHost);
      Aver.AreEqual("app/123", sut.GovApp);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("-config", sut.ForApplication[0]);
      Aver.AreEqual("abc.laconf", sut.ForApplication[1]);
    }

    [Run]
    public void Test09()
    {
      var sut = new BootArgs(new string[] { "daemon", "gov://", "abc2.laconf" });

      Aver.IsTrue(sut.IsDaemon);
      Aver.IsFalse(sut.IsGoverned);
      Aver.AreEqual(0, sut.GovPort);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("gov://", sut.ForApplication[0]);
      Aver.AreEqual("abc2.laconf", sut.ForApplication[1]);
    }

    [Run]
    public void Test10()
    {
      var sut = new BootArgs(new string[] { "daemon", "gov://123:", "abc2.laconf" });

      Aver.IsTrue(sut.IsDaemon);
      Aver.IsFalse(sut.IsGoverned);
      Aver.AreEqual(0, sut.GovPort);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("gov://123:", sut.ForApplication[0]);
      Aver.AreEqual("abc2.laconf", sut.ForApplication[1]);
    }

    [Run]
    public void Test11()
    {
      var sut = new BootArgs(new string[] { "daemon", "gov://#123:app", "abc2.laconf" });

      Aver.IsTrue(sut.IsDaemon);
      Aver.IsFalse(sut.IsGoverned);
      Aver.AreEqual(0, sut.GovPort);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("gov://#123:app", sut.ForApplication[0]);
      Aver.AreEqual("abc2.laconf", sut.ForApplication[1]);
    }
  }
}
