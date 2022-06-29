/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;

using Azos.Apps;
using Azos.Pile;
using Azos.IO;
using Azos.Scripting;

namespace Azos.Tests.Nub.Pile
{
  [Runnable]
  public class BasePileTests : IRunHook
  {
    bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
    {
      GC.Collect();
      return false;
    }

    bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
    {
      GC.Collect();
      return false;
    }

    [Run]
    public void PutString()
    {
      using(var pile = new DefaultPile(NOPApplication.Instance))
      {
        pile.Start();

        var ptr = pile.Put("abcd123");
        var got = pile.Get(ptr) as string;
        Aver.IsNotNull(got);
        Aver.AreEqual("abcd123", got);
      }
    }

    [Run]
    public void PutByteArray()
    {
      using (var pile = new DefaultPile(NOPApplication.Instance))
      {
        pile.Start();

        var ptr = pile.Put(new byte[]{1,6,9});
        var got = pile.Get(ptr) as byte[];
        Aver.IsNotNull(got);
        Aver.AreEqual(3, got.Length);
        Aver.AreEqual(1, got[0]);
        Aver.AreEqual(6, got[1]);
        Aver.AreEqual(9, got[2]);
      }
    }

    [Run]
    public void PutSubarray()
    {
      using (var pile = new DefaultPile(NOPApplication.Instance))
      {
        pile.Start();

        var src = new byte[] { 1, 6, 9, 250 };
        var sub = new Subarray<byte>();
        sub.Set(src, 2);

        var ptr1 = pile.Put(sub);
        sub.Set(src, 4);
        var ptr2 = pile.Put(sub);
        var got1 = pile.Get(ptr1) as byte[];
        var got2 = pile.Get(ptr2) as byte[];

        Aver.IsNotNull(got1);
        Aver.AreEqual(2, got1.Length);
        Aver.AreEqual(1, got1[0]);
        Aver.AreEqual(6, got1[1]);

        Aver.IsNotNull(got2);
        Aver.AreEqual(4, got2.Length);
        Aver.AreEqual(1, got2[0]);
        Aver.AreEqual(6, got2[1]);
        Aver.AreEqual(9, got2[2]);
        Aver.AreEqual(250, got2[3]);
      }
    }

    [Run]
    public void PutByteArray_GetDirectUnsafe()
    {
      using (var pile = new DefaultPile(NOPApplication.Instance))
      {
        pile.Start();

        var ptr = pile.Put(new byte[] { 1, 6, 9 });
        var got = pile.GetDirectMemoryBufferUnsafe(ptr, out var sflag);
        Aver.IsNotNull(got.Array);

        for(var i=0; i<8; i++)
         "{0} {1:x2} {2}".SeeArgs(got.Offset, got.Array[got.Offset+i], (char)got.Array[got.Offset + i]);

        Aver.AreEqual(3, got.Count );

        Aver.AreEqual(1, got.Array[got.Offset + 0]);
        Aver.AreEqual(6, got.Array[got.Offset + 1]);
        Aver.AreEqual(9, got.Array[got.Offset + 2]);
      }
    }

  }
}
