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
      var bin = new byte[]{ 1, 2, 3 };
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), null, bin);

      using var wscope = BixWriterBufferScope.DefaultCapacity;

      mem.WriteOneWay(wscope.Writer, 1);//Serialize by SHARD

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
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), mikoyan, bin);

      using var wscope = BixWriterBufferScope.DefaultCapacity;

      mem.WriteOneWay(wscope.Writer, 1);//Serialize by SHARD

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

      var state= new TeztState()
      {
         FirstName = "Cheese",
         LastName = "Burgerman",
         AccountNumber = 12345,
         DOB = new DateTime(2000, 12, 07, 14, 23, 09, DateTimeKind.Utc)
      };

      state.SetAttachment("profilepic.jpg", new byte[]{0,1,2,3,4,5,6,7,8,9});

      //shard calls
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), null, pars, state);




      using var wscope = BixWriterBufferScope.DefaultCapacity;

      mem.WriteOneWay(wscope.Writer, 1);//Serialize by SHARD

      var rawWireData = wscope.Buffer;

      "Raw data to be sent over the wire is: {0} bytes".SeeArgs(rawWireData.Length);


      using var rscope = new BixReaderBufferScope(rawWireData);
      var got = new FiberMemory(rscope.Reader);//Read by processor

      Aver.AreEqual(mem.Id, got.Id);
      Aver.IsTrue(mem.Status == got.Status);
      Aver.AreEqual(mem.ImageTypeId, got.ImageTypeId);
      Aver.AreEqual(mem.ImpersonateAs, got.ImpersonateAs);
      Aver.AreArraysEquivalent(mem.Buffer, got.Buffer);
      got.See("MEMORY:");

      var (gotPars, gotState) = got.UnpackBuffer(typeof(TeztParams), typeof(TeztState));

      //todo compare what I have in: gotPars, gotState
      gotPars.See("PARAMETERS:");
      gotState.See("STATE before accesing it:");

      "Last name: {0}".SeeArgs(((TeztState)gotState).LastName);

      gotState.See("STATE after accessing 1 slot:");

      "Attachment name: {0}".SeeArgs(((TeztState)gotState).AttachmentName);

      gotState.See("STATE after accessing 2 slot:");
    }

  }
}
