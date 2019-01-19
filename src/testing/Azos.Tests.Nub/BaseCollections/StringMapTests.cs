/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Collections;
using Azos.Scripting;

namespace Azos.Tests.Nub.BaseCollections
{

  [Runnable]
  public class StringMapTests
  {
    [Run]
    public void CaseSensitive()
    {
      var m = new StringMap(true);
      m["a"] = "Albert";
      m["A"] = "Albert Capital";

      Aver.AreEqual(2, m.Count);
      Aver.AreEqual("Albert", m["a"]);
      Aver.AreEqual("Albert Capital", m["A"]);
    }

    [Run]
    public void CaseSensitive_dfltCtor()
    {
      var m = new StringMap();
      m["a"] = "Albert";
      m["A"] = "Albert Capital";

      Aver.AreEqual(2, m.Count);
      Aver.AreEqual("Albert", m["a"]);
      Aver.AreEqual("Albert Capital", m["A"]);
    }

    [Run]
    public void CaseInsensitive()
    {
      var m = new StringMap(false);
      m["a"] = "Albert";
      m["A"] = "Albert Capital";

      Aver.AreEqual(1, m.Count);
      Aver.AreEqual("Albert Capital", m["a"]);
      Aver.AreEqual("Albert Capital", m["A"]);
    }

    [Run]
    public void KeyExistence()
    {
      var m = new StringMap();
      m["a"] = "Albert";
      m["b"] = "Benedict";

      Aver.AreEqual(2, m.Count);
      Aver.AreEqual("Albert", m["a"]);
      Aver.AreEqual("Benedict", m["b"]);
      Aver.IsNull(  m["c"] );
      Aver.IsTrue( m.ContainsKey("a"));
      Aver.IsFalse( m.ContainsKey("c"));
    }


  }
}
