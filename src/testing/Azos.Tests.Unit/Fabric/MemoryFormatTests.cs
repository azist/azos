/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Sky.Fabric;
using Azos.Sky.Fabric.Server;

namespace Azos.Tests.Unit.Fabric
{
  [Runnable]
  public class MemoryFormatTests
  {
    [Run]
    public void Test01_RawSerDeser_withoutpack()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(0, 1, 1));
      var bin = new byte[]{ 1, 2, 3 };
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), null, bin, 1);

      using var wscope = BixWriterBufferScope.DefaultCapacity;

      mem.WriteOneWay(wscope.Writer, 1);//Serialize by SHARD

      using var rscope = new BixReaderBufferScope(wscope.Buffer);
      var got = new FiberMemory(rscope.Reader);//Read by processor

      Aver.AreEqual(mem.Id, got.Id);
      Aver.IsTrue(mem.Status == got.Status);
      Aver.AreEqual(mem.ImageTypeId, got.ImageTypeId);
      Aver.AreEqual(mem.ImpersonateAs, got.ImpersonateAs);
      Aver.AreArraysEquivalent(mem.Buffer, got.Buffer);
      Aver.AreEqual(mem.StateOffset, got.StateOffset);
      got.See();
    }
  }
}
