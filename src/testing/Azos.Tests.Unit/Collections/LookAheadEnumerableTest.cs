/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;

using Azos.Collections;
using Azos.Scripting;

namespace Azos.Tests.Unit.Collections
{
  [Runnable(TRUN.BASE, 2)]
  public class LookAheadEnumerableTest
  {
    [Run]
    public void EmptyEnumerable()
    {
      var enumerator = string.Empty.AsLookAheadEnumerable().GetLookAheadEnumerator();
      Aver.IsFalse(enumerator.HasNext);
      Aver.Throws<AzosException>(() => {
        var next = enumerator.Next;
      }, StringConsts.OPERATION_NOT_SUPPORTED_ERROR + "{0}.Next(!HasNext)".Args(typeof(LookAheadEnumerator<char>).FullName));
      Aver.Throws<InvalidOperationException>(() => {
        var current = enumerator.Current;
      });
      Aver.IsFalse(enumerator.MoveNext());
    }

    [Run]
    public void SingleEnumerable()
    {
      var enumerator = " ".AsLookAheadEnumerable().GetLookAheadEnumerator();
      Aver.IsTrue(enumerator.HasNext);
      Aver.AreEqual(enumerator.Next, ' ');
      Aver.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Aver.IsTrue(enumerator.MoveNext());
      Aver.IsFalse(enumerator.HasNext);
      Aver.Throws<AzosException>(() =>
      {
        var next = enumerator.Next;
      }, StringConsts.OPERATION_NOT_SUPPORTED_ERROR + "{0}.Next(!HasNext)".Args(typeof(LookAheadEnumerator<char>).FullName));
      Aver.AreEqual(enumerator.Current, ' ');
      Aver.IsFalse(enumerator.MoveNext());
      enumerator.Reset();
      Aver.AreEqual(enumerator.HasNext, true);
      Aver.AreEqual(enumerator.Next, ' ');
      Aver.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Aver.IsTrue(enumerator.MoveNext());
    }

    [Run]
    public void MulripleEnumerable()
    {
      var enumerator = "+-".AsLookAheadEnumerable().GetLookAheadEnumerator();
      Aver.IsTrue(enumerator.HasNext);
      Aver.AreEqual(enumerator.Next, '+');
      Aver.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Aver.IsTrue(enumerator.MoveNext());
      Aver.IsTrue(enumerator.HasNext);
      Aver.AreEqual(enumerator.Next, '-');
      Aver.AreEqual(enumerator.Current, '+');
      Aver.IsTrue(enumerator.MoveNext());
      Aver.IsFalse(enumerator.HasNext);
      Aver.Throws<AzosException>(() =>
      {
        var next = enumerator.Next;
      }, StringConsts.OPERATION_NOT_SUPPORTED_ERROR + "{0}.Next(!HasNext)".Args(typeof(LookAheadEnumerator<char>).FullName));
      Aver.AreEqual(enumerator.Current, '-');
      Aver.IsFalse(enumerator.MoveNext());
      enumerator.Reset();
      Aver.IsTrue(enumerator.HasNext);
      Aver.AreEqual(enumerator.Next, '+');
      Aver.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Aver.IsTrue(enumerator.MoveNext());
    }

    [Run]
    public void EmptyEnumerable_AsEnumerable()
    {
      var enumerator = string.Empty.AsLookAheadEnumerable().GetEnumerator();
      Aver.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Aver.IsFalse(enumerator.MoveNext());
    }

    [Run]
    public void SingleEnumerable_AsEnumerable()
    {
      var enumerator = " ".AsLookAheadEnumerable().GetEnumerator();
      Aver.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Aver.IsTrue(enumerator.MoveNext());
      Aver.AreEqual(enumerator.Current, ' ');
      Aver.IsFalse(enumerator.MoveNext());
      enumerator.Reset();
      Aver.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Aver.IsTrue(enumerator.MoveNext());
    }

    [Run]
    public void MultipleEnumerable_AsEnumerable()
    {
      var enumerator = "+-".AsLookAheadEnumerable().GetEnumerator();
      Aver.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Aver.IsTrue(enumerator.MoveNext());
      Aver.AreEqual(enumerator.Current, '+');
      Aver.IsTrue(enumerator.MoveNext());
      Aver.AreEqual(enumerator.Current, '-');
      Aver.IsFalse(enumerator.MoveNext());
      enumerator.Reset();
      Aver.Throws<InvalidOperationException>(() =>
      {
        var current = enumerator.Current;
      });
      Aver.IsTrue(enumerator.MoveNext());
    }

    [Run]
    public void ForEach()
    {
      var sb = new StringBuilder();
      foreach (var c in string.Empty.AsLookAheadEnumerable()) sb.Append(c);
      Aver.AreEqual(sb.ToString(), string.Empty);
      sb.Clear();
      foreach (var c in "+".AsLookAheadEnumerable()) sb.Append(c);
      Aver.AreEqual(sb.ToString(), "+");
      sb.Clear();
      foreach (var c in "+-".AsLookAheadEnumerable()) sb.Append(c);
      Aver.AreEqual(sb.ToString(), "+-");
    }

    [Run]
    public void DetectSimbolPair()
    {
      var enumerator = @" """" ".AsLookAheadEnumerable().GetLookAheadEnumerator();
      var detect = false;
      while (!detect && enumerator.MoveNext())
      {
        if ('\"' == enumerator.Current && enumerator.HasNext && '\"' == enumerator.Next)
          detect = true;
      }
      Aver.IsTrue(detect);
      enumerator = @"""  """.AsLookAheadEnumerable().GetLookAheadEnumerator();
      detect = false;
      while (!detect && enumerator.MoveNext())
      {
        if ('\"' == enumerator.Current && enumerator.HasNext && '\"' == enumerator.Next)
          detect = true;
      }
      Aver.IsFalse(detect);
    }
  }
}
