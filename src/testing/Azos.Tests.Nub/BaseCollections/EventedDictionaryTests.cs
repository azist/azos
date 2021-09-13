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
  public class EventedDictionaryTests
  {
    [Run]
    public void Dict_Readonly()
    {
      var dict = new EventedDictionary<int, string, string>("CONTEXT", false);

      var ro = false;

      dict.GetReadOnlyEvent = (l) => ro;

      dict.Add(1, "a");
      dict.Add(2, "b");
      dict.Add(3, "c");

      Aver.AreEqual(3, dict.Count);
      ro = true;

      Aver.Throws<AzosException>(() => dict.Add(4, "d"));
    }

    [Run]
    public void Dict_Add()
    {
      var dict = new EventedDictionary<int, string, string>("CONTEXT", false);

      var first = true;
      dict.GetReadOnlyEvent = (_) => false;

      dict.ChangeEvent = (d, ct, p, k, v) =>
                        {
                          Aver.AreObjectsEqual(EventedDictionary<int, string, string>.ChangeType.Add, ct);
                          Aver.IsTrue((first ? EventPhase.Before : EventPhase.After) == p);
                          Aver.AreEqual(1, k);
                          Aver.AreEqual("a", v);
                          first = false;
                        };

      dict.Add(1, "a");
    }

    [Run]
    public void Dict_Remove()
    {
      var dict = new EventedDictionary<int, string, string>("CONTEXT", false);

      var first = true;
      dict.GetReadOnlyEvent = (_) => false;

      dict.Add(1, "a");
      dict.Add(2, "b");
      dict.Add(3, "c");

      Aver.AreEqual(3, dict.Count);

      dict.ChangeEvent = (d, ct, p, k, v) =>
                        {
                          Aver.AreObjectsEqual(EventedDictionary<int, string, string>.ChangeType.Remove, ct);
                          Aver.IsTrue((first ? EventPhase.Before : EventPhase.After) == p);
                          Aver.AreEqual(2, k);
                          first = false;
                        };

      dict.Remove(2);
      Aver.AreEqual(2, dict.Count);
    }

    [Run]
    public void Dict_Set()
    {
      var dict = new EventedDictionary<int, string, string>("CONTEXT", false);

      var first = true;
      dict.GetReadOnlyEvent = (_) => false;

      dict.Add(1, "a");
      dict.Add(2, "b");
      dict.Add(3, "c");

      Aver.AreEqual(3, dict.Count);

      dict.ChangeEvent = (d, ct, p, k, v) =>
                        {
                          Aver.AreObjectsEqual(EventedDictionary<int, string, string>.ChangeType.Set, ct);
                          Aver.IsTrue((first ? EventPhase.Before : EventPhase.After) == p);
                          Aver.AreEqual(1, k);
                          Aver.AreEqual("z", v);
                          first = false;
                        };

      dict[1] = "z";
      Aver.AreEqual("z", dict[1]);
    }

    [Run]
    public void Dict_Clear()
    {
      var dict = new EventedDictionary<int, string, string>("CONTEXT", false);

      var first = true;
      dict.GetReadOnlyEvent = (_) => false;

      dict.Add(1, "a");
      dict.Add(2, "b");
      dict.Add(3, "c");

      Aver.AreEqual(3, dict.Count);

      dict.ChangeEvent = (d, ct, p, k, v) =>
                        {
                          Aver.AreObjectsEqual(EventedDictionary<int, string, string>.ChangeType.Clear, ct);
                          Aver.IsTrue((first ? EventPhase.Before : EventPhase.After) == p);
                          Aver.AreEqual(0, k);
                          Aver.AreEqual(null, v);
                          first = false;
                        };

      dict.Clear();
      Aver.AreEqual(0, dict.Count);
    }

  }
}
