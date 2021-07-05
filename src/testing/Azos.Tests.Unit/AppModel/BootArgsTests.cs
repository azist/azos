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
      Aver.AreEqual(0, sut.GovernorPort);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(0, sut.ForApplication.Length);
    }

    [Run]
    public void Test02()
    {
      var sut = new BootArgs(new string[]{"xyz.laconf"});

      Aver.IsFalse(sut.IsDaemon);
      Aver.IsFalse(sut.IsGoverned);
      Aver.AreEqual(0, sut.GovernorPort);
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
      Aver.AreEqual(0, sut.GovernorPort);
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
      Aver.AreEqual(0, sut.GovernorPort);
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
      Aver.AreEqual(0, sut.GovernorPort);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("-config", sut.ForApplication[0]);
      Aver.AreEqual("xyz.laconf", sut.ForApplication[1]);
    }

    [Run]
    public void Test06()
    {
      var sut = new BootArgs(new string[] { "daemon", "governor", "5678", "abc.laconf" });

      Aver.IsTrue(sut.IsDaemon);
      Aver.IsTrue(sut.IsGoverned);
      Aver.AreEqual(5678, sut.GovernorPort);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("-config", sut.ForApplication[0]);
      Aver.AreEqual("abc.laconf", sut.ForApplication[1]);
    }

    [Run]
    public void Test07()
    {
      var sut = new BootArgs(new string[] { "governor", "5678", "abc.laconf" });

      Aver.IsFalse(sut.IsDaemon);
      Aver.IsTrue(sut.IsGoverned);
      Aver.AreEqual(5678, sut.GovernorPort);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("-config", sut.ForApplication[0]);
      Aver.AreEqual("abc.laconf", sut.ForApplication[1]);
    }

    [Run]
    public void Test08()
    {
      var sut = new BootArgs(new string[] { "daemon", "governor", "abc2.laconf" });

      Aver.IsTrue(sut.IsDaemon);
      Aver.IsFalse(sut.IsGoverned);
      Aver.AreEqual(0, sut.GovernorPort);
      Aver.IsNotNull(sut.ForApplication);
      Aver.AreEqual(2, sut.ForApplication.Length);
      Aver.AreEqual("governor", sut.ForApplication[0]);
      Aver.AreEqual("abc2.laconf", sut.ForApplication[1]);
    }
  }
}
