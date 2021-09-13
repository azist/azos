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
  public class EventedListTests
  {
    [Run]
    public void List_SwitchContext_1()
    {
      var lst = new EventedList<string, string>("CONTEXT", false);
      Aver.AreEqual("CONTEXT", lst.Context);
      Aver.IsFalse(lst.ContextReadOnly);
      lst.Context = "yesyes";
      Aver.AreEqual("yesyes", lst.Context);
    }

    [Run]
    [Aver.Throws(typeof(AzosException), Message = "Invalid operation", MsgMatch = Aver.ThrowsAttribute.MatchType.Contains)]
    public void List_SwitchContext_2()
    {
      var lst = new EventedList<string, string>("CONTEXT", true);
      Aver.AreEqual("CONTEXT", lst.Context);
      Aver.IsTrue(lst.ContextReadOnly);
      lst.Context = "yesyes";
    }

    [Run]
    public void List_Readonly()
    {
      var lst = new EventedList<string, string>("CONTEXT", false);

      var ro = false;

      lst.GetReadOnlyEvent = (l) => ro;

      lst.Add("a");
      lst.Add("b");
      lst.Add("c");

      Aver.AreEqual(3, lst.Count);
      ro = true;

      Aver.Throws<AzosException>(() => lst.Add("d"));
    }

    [Run]
    public void List_Add()
    {
      var lst = new EventedList<string, string>("CONTEXT", false);

      var first = true;
      lst.GetReadOnlyEvent = (_) => false;

      lst.ChangeEvent = (l, ct, p, idx, v) =>
                        {
                          Aver.AreObjectsEqual(EventedList<string, string>.ChangeType.Add, ct);
                          Aver.AreObjectsEqual(first ? EventPhase.Before : EventPhase.After, p);
                          Aver.AreEqual(-1, idx);
                          Aver.AreEqual("a", v);
                          first = false;
                        };

      lst.Add("a");
    }

    [Run]
    public void List_Remove()
    {
      var lst = new EventedList<string, string>("CONTEXT", false);

      var first = true;
      lst.GetReadOnlyEvent = (_) => false;

      lst.Add("a");
      lst.Add("b");
      lst.Add("c");

      Aver.AreEqual(3, lst.Count);

      lst.ChangeEvent = (l, ct, p, idx, v) =>
                        {
                          Aver.AreObjectsEqual(EventedList<string, string>.ChangeType.Remove, ct);
                          Aver.AreObjectsEqual(first ? EventPhase.Before : EventPhase.After, p);
                          Aver.AreEqual(-1, idx);
                          Aver.AreEqual("b", v);
                          first = false;
                        };

      lst.Remove("b");
      Aver.AreEqual(2, lst.Count);
    }

    [Run]
    public void List_Set()
    {
      var lst = new EventedList<string, string>("CONTEXT", false);

      var first = true;
      lst.GetReadOnlyEvent = (_) => false;

      lst.Add("a");
      lst.Add("b");
      lst.Add("c");

      Aver.AreEqual(3, lst.Count);

      lst.ChangeEvent = (l, ct, p, idx, v) =>
                        {
                          Aver.AreObjectsEqual(EventedList<string, string>.ChangeType.Set, ct);
                          Aver.AreObjectsEqual(first ? EventPhase.Before : EventPhase.After, p);
                          Aver.AreEqual(1, idx);
                          Aver.AreEqual("z", v);
                          first = false;
                        };

      lst[1] = "z";
      Aver.AreEqual("z", lst[1]);
    }


    [Run]
    public void List_Clear()
    {
      var lst = new EventedList<string, string>("CONTEXT", false);

      var first = true;
      lst.GetReadOnlyEvent = (_) => false;

      lst.Add("a");
      lst.Add("b");
      lst.Add("c");

      Aver.AreEqual(3, lst.Count);

      lst.ChangeEvent = (l, ct, p, idx, v) =>
                        {
                          Aver.AreObjectsEqual(EventedList<string, string>.ChangeType.Clear, ct);
                          Aver.AreObjectsEqual(first ? EventPhase.Before : EventPhase.After, p);
                          Aver.AreEqual(-1, idx);
                          Aver.AreEqual(null, v);
                          first = false;
                        };

      lst.Clear();
      Aver.AreEqual(0, lst.Count);
    }

  }
}
