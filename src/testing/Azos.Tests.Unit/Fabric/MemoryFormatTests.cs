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
using Azos.Time;

namespace Azos.Tests.Unit.Fabric
{
  [Runnable]
  public class MemoryFormatTests : IRunnableHook
  {
    public void Prologue(Runner runner, FID id)
    {
      Bixer.RegisterTypeSerializationCores(System.Reflection.Assembly.GetExecutingAssembly());
    }
    public bool Epilogue(Runner runner, FID id, Exception error) => false;

    [Run]
    public void Test01_RawSerDeser_withoutpack_NoImpersonate()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(0, 1, 1));
      var bin = new byte[] { 1, 2, 3 };
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), Guid.NewGuid(), null, bin);

      using var wscope = BixWriterBufferScope.DefaultCapacity;

      mem.WriteOneWay(wscope.Writer);//Serialize by SHARD

      using var rscope = new BixReaderBufferScope(wscope.Buffer);
      var got = new FiberMemory(rscope.Reader);//Read by processor

      Aver.AreEqual(mem.Id, got.Id);
      Aver.IsTrue(mem.Status == got.Status);
      Aver.AreEqual(mem.ImageTypeId, got.ImageTypeId);
      Aver.AreEqual(mem.ImpersonateAs, got.ImpersonateAs);
      Aver.AreArraysEquivalent(mem.Buffer, got.Buffer);
      got.See();
    }

    [Run]
    public void Test02_RawSerDeser_withoutpack_Impersonate()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(0, 1, 1));
      var mikoyan = new EntityId(Atom.Encode("idp"), Atom.Encode("id"), Atom.ZERO, "mikoyan.ashot.kalgy");
      var bin = new byte[] { 1, 2, 3 };
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), Guid.NewGuid(), mikoyan, bin);

      using var wscope = BixWriterBufferScope.DefaultCapacity;

      mem.WriteOneWay(wscope.Writer);//Serialize by SHARD

      using var rscope = new BixReaderBufferScope(wscope.Buffer);
      var got = new FiberMemory(rscope.Reader);//Read by processor

      Aver.AreEqual(mem.Id, got.Id);
      Aver.IsTrue(mem.Status == got.Status);
      Aver.AreEqual(mem.ImageTypeId, got.ImageTypeId);
      Aver.AreEqual(mem.ImpersonateAs, got.ImpersonateAs);
      Aver.AreArraysEquivalent(mem.Buffer, got.Buffer);
      Aver.AreEqual("mikoyan.ashot.kalgy", got.ImpersonateAs.Value.Address);
      got.See();
    }

    public static byte[] GetFiberMemoryBuffer(FiberParameters pars, FiberState state)
    {
      var dbPars = FiberMemory.PackParameters(pars);

      var slots = state._____getInternaldataForUnitTest()
                       .Select(one => new KeyValuePair<Atom, byte[]>(one.Key, one.Value is byte[] buf ? buf : FiberState.PackSlot((FiberState.Slot)one.Value)))
                       .ToArray();

      return FiberMemory.PackBuffer(dbPars, Atom.ZERO, slots);
    }


    [Run]
    public void Test03_RawSerDeser_withSlots()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(0, 1, 1));

      var pars = new TeztParams()
      {
        Int1 = 3456789,
        Bool1 = true,
        String1 = "Apple Cider Vinegar Pooking"
      };

      var state = new TeztState()
      {
        FirstName = "Cheese",
        LastName = "Burgerman",
        AccountNumber = 12345,
        DOB = new DateTime(2000, 12, 07, 14, 23, 09, DateTimeKind.Utc)
      };

      state.SetAttachment("profilepic.jpg", new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

      //shard calls
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), Guid.NewGuid(), null, GetFiberMemoryBuffer(pars, state));

      using var wscope = BixWriterBufferScope.DefaultCapacity;

      mem.WriteOneWay(wscope.Writer);//Serialize by SHARD

      var rawWireData = wscope.Buffer;

      "Raw data to be sent SHARD-->PROCESSOR over the wire is: {0} bytes".SeeArgs(rawWireData.Length);


      using var rscope = new BixReaderBufferScope(rawWireData);
      var got = new FiberMemory(rscope.Reader);//Read by processor

      Aver.AreEqual(mem.Id, got.Id);
      Aver.IsTrue(mem.Status == got.Status);
      Aver.AreEqual(mem.ImageTypeId, got.ImageTypeId);
      Aver.AreEqual(mem.ImpersonateAs, got.ImpersonateAs);
      Aver.AreArraysEquivalent(mem.Buffer, got.Buffer);
      got.See("PROCESSOR RECEIVED MEMORY:");

      var (p, s) = got.UnpackBuffer(typeof(TeztParams), typeof(TeztState));

      var gotPars = p as TeztParams;
      var gotState = s as TeztState;

      Aver.IsNotNull(gotPars);
      Aver.IsNotNull(gotState);


      gotPars.See("PROCESSOR GOT PARAMETERS:");
      gotState.See("PROCESSOR GOT STATE before accessing it:");

      "Last name: {0}".SeeArgs(gotState.LastName);

      gotState.See("PROCESSOR STATE after accessing 1 slot:");

      "Attachment name: {0}".SeeArgs(gotState.AttachmentName);

      gotState.See("PROCESSOR STATE after accessing 2 slot:");

      Aver.AreNotSameRef(pars, gotPars);
      Aver.AreNotSameRef(state, gotState);

      Aver.AreEqual(pars.Int1, gotPars.Int1);
      Aver.AreEqual(pars.Bool1, gotPars.Bool1);
      Aver.AreEqual(pars.String1, gotPars.String1);

      Aver.AreEqual(state.FirstName, gotState.FirstName);
      Aver.AreEqual(state.LastName, gotState.LastName);
      Aver.AreEqual(state.DOB, gotState.DOB);
      Aver.AreEqual(state.AccountNumber, gotState.AccountNumber);

      Aver.AreArraysEquivalent(state.AttachmentContent, gotState.AttachmentContent);
      Aver.AreEqual(state.AttachmentName, gotState.AttachmentName);
    }

    [Run("cnt=100")]
    [Run("cnt=1000")]
    [Run("cnt=250000")]
    public void Test04_RawSerDeser_Benchmark(int cnt)
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(0, 1, 1));

      var pars = new TeztParams()
      {
        Int1 = 3456789,
        Bool1 = true,
        String1 = "Apple Cider Vinegar Pooking"
      };

      var state = new TeztState()
      {
        FirstName = "Cheesergohman",
        LastName = "Burgermaninochertof Khan Piu Vivo",
        AccountNumber = 1234567,
        DOB = new DateTime(2000, 12, 07, 14, 23, 09, DateTimeKind.Utc)
      };

      state.SetAttachment("profilepic.jpg", new byte[23000]);

      //shard calls
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), Guid.NewGuid(), null, GetFiberMemoryBuffer(pars, state));

      using var wscope = BixWriterBufferScope.DefaultCapacity;

      mem.WriteOneWay(wscope.Writer);//warm up

      var timerWrite = Timeter.StartNew();
      for (var i = 0; i < cnt; i++)
      {
        wscope.Reset();
        mem.WriteOneWay(wscope.Writer);
      }
      timerWrite.Stop();

      var rawWireData = wscope.Buffer;

      "Raw data to be sent over the wire is: {0} bytes".SeeArgs(rawWireData.Length);


      using var rscope = new BixReaderBufferScope(rawWireData);
      var got = new FiberMemory(rscope.Reader);//Read by processor
      var timerRead = Timeter.StartNew();
      for (var i = 0; i < cnt; i++)
      {
        rscope.Reset();
        got = new FiberMemory(rscope.Reader);
      }
      timerRead.Stop();

      "Did {0} reads at {1:n0} ops/sec and writes at {2:n0} ops/sec".SeeArgs(cnt, cnt / timerRead.ElapsedSec, cnt / timerWrite.ElapsedSec);

      Aver.AreEqual(mem.Id, got.Id);
      Aver.IsTrue(mem.Status == got.Status);
      Aver.AreEqual(mem.ImageTypeId, got.ImageTypeId);
      Aver.AreEqual(mem.ImpersonateAs, got.ImpersonateAs);
      Aver.AreArraysEquivalent(mem.Buffer, got.Buffer);
    }

  }
}
