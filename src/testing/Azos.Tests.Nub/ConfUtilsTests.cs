/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class ConfUtilsTests
  {
    [Run]
    public void ToMapOfAttrs_1()
    {
      var cfg = "a=1 b=2 c=3".AsLaconicConfig();
      var got = cfg.ToMapOfAttrs(true, "a", "b");
      Aver.IsNotNull(got);
      Aver.AreEqual(2, got.Count);
      Aver.AreEqual("1", got["a"].AsString());
      Aver.AreEqual("2", got["b"].AsString());
    }

    [Run]
    public void ToMapOfAttrs_2()
    {
      var cfg = "a=1 b=2 c=3".AsLaconicConfig();
      var got = cfg.ToMapOfAttrs(true, "a", "q");
      Aver.IsNotNull(got);
      Aver.AreEqual(2, got.Count);
      Aver.AreEqual("1", got["a"].AsString());
      Aver.IsTrue(got.ContainsKey("q"));
      Aver.IsNull(got["q"]);
    }

    [Run]
    public void ToMapOfAttrs_3()
    {
      var cfg = "a=1 b=2 c=3".AsLaconicConfig();
      var got = cfg.ToMapOfAttrs(false, "a", "q");
      Aver.IsNotNull(got);
      Aver.AreEqual(1, got.Count);
      Aver.AreEqual("1", got["a"].AsString());
    }

    [Run]
    public void ToMapOfAttrs_4()
    {
      var cfg = "a=1 b=2 c=3".AsLaconicConfig();
      var got = cfg.ToMapOfAttrs(false, "a", "c->ququ");
      Aver.IsNotNull(got);
      Aver.AreEqual(2, got.Count);
      Aver.AreEqual("1", got["a"].AsString());
      Aver.AreEqual("3", got["ququ"].AsString());
    }

    [Run]
    public void ToDynOfAttrs_1()
    {
      var cfg = "a=1 b=2 c=3".AsLaconicConfig();
      dynamic got = cfg.ToDynOfAttrs("a", "b", "c->newc", "de");
      Aver.IsNotNull(got);
      Aver.AreEqual("1", got.a);
      Aver.AreEqual("2", got.b);
      Aver.AreEqual("3", got.newc);
      Aver.AreEqual(null, got.de);
    }


  }
}