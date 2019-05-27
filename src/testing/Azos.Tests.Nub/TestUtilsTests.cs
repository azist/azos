/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class TestUtilsTests
  {
    [Run]
    public void MapValueContains_Nulls()
    {
      JsonDataMap map = null;
      Aver.IsFalse(map.MapValueContains(null, null));
      Aver.IsFalse(map.MapValueContains("a", null));
      Aver.IsFalse(map.MapValueContains("a", ""));
      Aver.IsFalse(map.MapValueContains("a", "a"));
    }

    [Run]
    public void MapValueContains_Cases()
    {
      JsonDataMap map = new JsonDataMap{ {"a", "Alex Macedonian"}, {"b", "Bobby Fisher"} , {"c", null}};
      Aver.IsTrue(map.MapValueContains("a", "Alex"));
      Aver.IsTrue(map.MapValueContains("a", "ALEX"));
      Aver.IsFalse(map.MapValueContains("a", "ALEX", senseCase: true));
      Aver.IsFalse(map.MapValueContains("b", "ALEX"));

      Aver.IsTrue(map.MapValueContains("b", "Fisher"));
      Aver.IsTrue(map.MapValueContains("b", "Bobby Fisher"));
      Aver.IsFalse(map.MapValueContains("b", "BOBBY FISHER", senseCase: true));
      Aver.IsTrue(map.MapValueContains("b", "BOBBY FISHER"));
      Aver.IsFalse(map.MapValueContains("b", "AFisher"));

      Aver.IsTrue(map.MapValueContains("c", null));
    }

    [Run]
    public void MapValueMatches_Nulls()
    {
      JsonDataMap map = null;
      Aver.IsFalse(map.MapValueMatches(null, null));
      Aver.IsFalse(map.MapValueMatches("a", null));
      Aver.IsFalse(map.MapValueMatches("a", ""));
      Aver.IsFalse(map.MapValueMatches("a", "a"));
    }

    [Run]
    public void MapValueMatches_Cases()
    {
      JsonDataMap map = new JsonDataMap { { "a", "Alex Macedonian" }, { "b", "Bobby Fisher" }, { "c", null } };
      Aver.IsTrue(map.MapValueMatches("a", "Alex*"));
      Aver.IsTrue(map.MapValueMatches("a", "ALEX*"));
      Aver.IsFalse(map.MapValueMatches("a", "ALEX*", senseCase: true));
      Aver.IsFalse(map.MapValueMatches("b", "ALEX*"));

      Aver.IsTrue(map.MapValueMatches("a", "Al*Mac*n?an"));
      Aver.IsFalse(map.MapValueMatches("a", "Al*Mac*nY?an"));

      Aver.IsTrue(map.MapValueMatches("b", "*"));
      Aver.IsTrue(map.MapValueMatches("b", "????? ??????"));
      Aver.IsFalse(map.MapValueMatches("b", "????  ??????"));

      Aver.IsTrue(map.MapValueMatches("b", "*Fi?her"));
      Aver.IsTrue(map.MapValueMatches("b", "Bobb*Fisher"));
      Aver.IsFalse(map.MapValueMatches("b", "BOB*", senseCase: true));
      Aver.IsTrue(map.MapValueMatches("b", "BOB*"));
      Aver.IsFalse(map.MapValueMatches("b", "*AFisher"));

      Aver.IsTrue(map.MapValueMatches("c", null));
    }



  }
}