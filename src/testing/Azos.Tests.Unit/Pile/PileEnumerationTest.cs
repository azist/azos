/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Reflection;

using Azos.Scripting;

using Azos.Apps;
using Azos.Pile;

namespace Azos.Tests.Unit.Pile
{
  [Runnable]
  public class PileEnumerationTest : IRunHook
  {
      bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
      {
        GC.Collect();
        return false;
      }

      bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
      {
        GC.Collect();
        return false;
      }


      [Run("count=723     segmentSize=67108864")]//count < 1024
      [Run("count=1500    segmentSize=67108864")]//1 segment
      [Run("count=250000  segmentSize=67108864")]
      [Run("count=750000  segmentSize=67108864")]
      public void Buffers(int count, int segmentSize)
      {
        using (var pile = new DefaultPile(NOPApplication.Instance))
        {
          pile.SegmentSize = segmentSize;
          pile.Start();

          var hs = new HashSet<int>();

          for(var i=0; i<count; i++)
          {
            var buf = new byte[4 + (i%512)];
            buf.WriteBEInt32(i);
            pile.Put(buf);
            hs.Add(i);
          }

          Console.WriteLine("Created {0} segments".Args(pile.SegmentCount));

          var j = 0;
          foreach(var entry in pile)
          {
            var buf = pile.Get(entry.Pointer) as byte[];
            Aver.IsNotNull(buf);
            Aver.IsTrue(buf.Length>=4);
            var i = buf.ReadBEInt32();
            Aver.IsTrue(hs.Remove(i));
            Aver.IsTrue( entry.Type == PileEntry.DataType.Buffer );
            j++;
          }
          Aver.AreEqual(j, count);
          Aver.AreEqual(0, hs.Count);
        }
      }
  }
}
