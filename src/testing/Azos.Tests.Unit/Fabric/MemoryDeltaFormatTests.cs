/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using Azos.Sky.Fabric;
using Azos.Sky.Fabric.Server;
using Azos.Time;

namespace Azos.Tests.Unit.Fabric
{
  [Runnable]
  public class MemoryDeltaFormatTests : IRunnableHook
  {
    public void Prologue(Runner runner, FID id)
    {
       Bixer.RegisterTypeSerializationCores(System.Reflection.Assembly.GetExecutingAssembly());
    }
    public bool Epilogue(Runner runner, FID id, Exception error) => false;

    [Run]
    public void Test01_Roundtrip_NextStep_NoResult()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(7, 12, 1234567890));
      var pars = new TeztParams
      {
          Bool1 = true,
          Int1 = 123456,
          String1 = "BoltJolt"
      };

      var state = new TeztState();
      state.FirstName = "Alex";
      state.LastName = "Booster";
      state.DOB = new DateTime(1980, 1, 2, 14, 15, 00, DateTimeKind.Utc);
      state.AccountNumber = 987654321;
      state.SetAttachment("donkey_fun.jpeg", new byte[]{1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5});
      state.ResetAllSlotModificationFlags();
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), null, pars, state);//this packs buffer

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
      var (gotParsBase, gotStateBase) = got.UnpackBuffer(typeof(TeztParams), typeof(TeztState));
      var (gotPars, gotState) = (gotParsBase.CastTo<TeztParams>(), gotStateBase.CastTo<TeztState>());
      Aver.AreEqual(pars.Int1, gotPars.Int1);
      Aver.AreEqual(pars.String1, gotPars.String1);
      Aver.AreEqual(state.AccountNumber, gotState.AccountNumber);
      Aver.AreEqual(state.AttachmentName, gotState.AttachmentName);
      Aver.AreArraysEquivalent(state.AttachmentContent, gotState.AttachmentContent);

      //==== We are in processor pretending to use state now

      Aver.IsFalse(got.HasDelta(gotState));
      gotState.AccountNumber = 223322;//mutate state <====================
      Aver.IsTrue(got.HasDelta(gotState));

      var delta = got.MakeDeltaSnapshot(FiberStep.ContinueImmediately(Atom.Encode("step2")), gotState);

      Aver.AreEqual("step2", delta.NextStep.Value);
      Aver.AreEqual(new TimeSpan(0), delta.NextSliceInterval);
      Aver.AreEqual(1, delta.Changes.Length);
      Aver.IsNull(delta.Result);
      Aver.AreEqual(0, delta.ExitCode);

      wscope.Reset();
      delta.WriteOneWay(wscope.Writer, 1);//written by processor

      using var rscope2 = new BixReaderBufferScope(wscope.Buffer);
      var gotDelta = new FiberMemoryDelta(rscope2.Reader);//read back by shard

      gotDelta.See("Got delta: ");

      Aver.AreEqual(mem.Id, gotDelta.Id);
      Aver.AreEqual(got.Id, gotDelta.Id);


      Aver.AreEqual("step2", gotDelta.NextStep.Value);
      Aver.AreEqual(new TimeSpan(0), gotDelta.NextSliceInterval);
      Aver.IsNull(gotDelta.Changes);
      Aver.IsNotNull(gotDelta.ChangesReceived);
      Aver.AreEqual(1, gotDelta.ChangesReceived.Length);
      Aver.IsNull(gotDelta.Result);
      Aver.AreEqual(0, gotDelta.ExitCode);

      Aver.AreEqual("d", gotDelta.ChangesReceived[0].Name.Value);
      Aver.IsNotNull(gotDelta.ChangesReceived[0].Data);

      //manually deserialize content, in future this will need to be updated to reflect BIX etc...
      var map = JsonReader.DeserializeDataObject(new MemoryStream(gotDelta.ChangesReceived[0].Data)) as JsonDataMap;
      map.See("map");
      var deserManually = JsonReader.ToDoc<TeztState.DemographicsSlot>(map);

      Aver.AreEqual(223322ul, deserManually.AccountNumber);
      Aver.AreEqual("Booster", deserManually.LastName);
      Aver.IsTrue(FiberState.SlotMutationType.Modified == deserManually.SlotMutation);
    }


    [Run]
    public void Test02_Roundtrip_FinalStep_WithResult()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(7, 12, 1234567890));
      var pars = new TeztParams
      {
        Bool1 = true,
        Int1 = 123456,
        String1 = "BoltJolt"
      };

      var state = new TeztState();
      state.FirstName = "Alex";
      state.LastName = "Rooster";
      state.DOB = new DateTime(1980, 1, 2, 14, 15, 00, DateTimeKind.Utc);
      state.AccountNumber = 987654321;
      state.SetAttachment("hockey_fun.jpeg", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5 });
      state.ResetAllSlotModificationFlags();
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), null, pars, state);//this packs buffer

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
      var (gotParsBase, gotStateBase) = got.UnpackBuffer(typeof(TeztParams), typeof(TeztState));
      var (gotPars, gotState) = (gotParsBase.CastTo<TeztParams>(), gotStateBase.CastTo<TeztState>());
      Aver.AreEqual(pars.Int1, gotPars.Int1);
      Aver.AreEqual(pars.String1, gotPars.String1);
      Aver.AreEqual(state.AccountNumber, gotState.AccountNumber);
      Aver.AreEqual(state.AttachmentName, gotState.AttachmentName);
      Aver.AreArraysEquivalent(state.AttachmentContent, gotState.AttachmentContent);

      //==== We are in processor pretending to use state now

      Aver.IsFalse(got.HasDelta(gotState));
      gotState.AccountNumber = 55166;//mutate state <====================
      gotState.LastName = "Shuster";
      Aver.IsTrue(got.HasDelta(gotState));
      var result = new TeztResult
      {
        Int1 = 900,
        String1 = "Dallas"
      };

      var delta = got.MakeDeltaSnapshot(FiberStep.FinishWithResult(123, result), gotState);

      Aver.IsTrue(delta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), delta.NextSliceInterval);
      Aver.AreEqual(1, delta.Changes.Length);
      Aver.IsNotNull(delta.Result);
      Aver.AreEqual(123, delta.ExitCode);

      wscope.Reset();
      delta.WriteOneWay(wscope.Writer, 1);//written by processor

      var wire = wscope.Buffer;
      "{0} bytes of delta wired back to shard".SeeArgs(wire.Length);

      using var rscope2 = new BixReaderBufferScope(wire);
      var gotDelta = new FiberMemoryDelta(rscope2.Reader);//read back by shard

      gotDelta.See("Got delta: ");

      Aver.AreEqual(mem.Id, gotDelta.Id);
      Aver.AreEqual(got.Id, gotDelta.Id);


      Aver.IsTrue(gotDelta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), gotDelta.NextSliceInterval);
      Aver.IsNull(gotDelta.Changes);
      Aver.IsNotNull(gotDelta.ChangesReceived);
      Aver.AreEqual(1, gotDelta.ChangesReceived.Length);
      Aver.IsNull(gotDelta.Result);
      Aver.IsNotNull(gotDelta.ResultReceivedJson);
      Aver.AreEqual(123, gotDelta.ExitCode);

      var resultmap = JsonReader.DeserializeDataObject(gotDelta.ResultReceivedJson) as JsonDataMap;

      Aver.AreEqual(result.Int1, resultmap["Int1"].AsInt());
      Aver.AreEqual(result.String1, resultmap["String1"].AsString());

      Aver.AreEqual("d", gotDelta.ChangesReceived[0].Name.Value);
      Aver.IsNotNull(gotDelta.ChangesReceived[0].Data);

      //manually deserialize content, in future this will need to be updated to reflect BIX etc...
      var map = JsonReader.DeserializeDataObject(new MemoryStream(gotDelta.ChangesReceived[0].Data)) as JsonDataMap;
      map.See("map");
      var deserManually = JsonReader.ToDoc<TeztState.DemographicsSlot>(map);

      Aver.AreEqual(55166ul, deserManually.AccountNumber);
      Aver.AreEqual("Shuster", deserManually.LastName);
      Aver.IsTrue(FiberState.SlotMutationType.Modified == deserManually.SlotMutation);
    }


    [Run]
    public void Test03_Roundtrip_FinalStep_NoResult()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(7, 12, 1234567890));
      var pars = new TeztParams
      {
        Bool1 = true,
        Int1 = 123456,
        String1 = "BoltJolt"
      };

      var state = new TeztState();
      state.FirstName = "Alex";
      state.LastName = "Rooster";
      state.DOB = new DateTime(1980, 1, 2, 14, 15, 00, DateTimeKind.Utc);
      state.AccountNumber = 987654321;
      state.SetAttachment("hockey_fun.jpeg", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5 });
      state.ResetAllSlotModificationFlags();
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), null, pars, state);//this packs buffer

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
      var (gotParsBase, gotStateBase) = got.UnpackBuffer(typeof(TeztParams), typeof(TeztState));
      var (gotPars, gotState) = (gotParsBase.CastTo<TeztParams>(), gotStateBase.CastTo<TeztState>());
      Aver.AreEqual(pars.Int1, gotPars.Int1);
      Aver.AreEqual(pars.String1, gotPars.String1);
      Aver.AreEqual(state.AccountNumber, gotState.AccountNumber);
      Aver.AreEqual(state.AttachmentName, gotState.AttachmentName);
      Aver.AreArraysEquivalent(state.AttachmentContent, gotState.AttachmentContent);

      //==== We are in processor pretending to use state now

      Aver.IsFalse(got.HasDelta(gotState));
      gotState.AccountNumber = 55166;//mutate state <====================
      gotState.LastName = "Monster";
      gotState.SetAttachment("windows.bmp", new byte[5]);
      Aver.IsTrue(gotState.SlotsHaveChanges);
      Aver.IsTrue(got.HasDelta(gotState));


      var delta = got.MakeDeltaSnapshot(FiberStep.Finish(321), gotState);

      Aver.IsTrue(delta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), delta.NextSliceInterval);
      Aver.AreEqual(2, delta.Changes.Length);
      Aver.IsNull(delta.Result);
      Aver.AreEqual(321, delta.ExitCode);

      wscope.Reset();
      delta.WriteOneWay(wscope.Writer, 1);//written by processor

      var wire = wscope.Buffer;
      "{0} bytes of delta wired back to shard".SeeArgs(wire.Length);

      using var rscope2 = new BixReaderBufferScope(wire);
      var gotDelta = new FiberMemoryDelta(rscope2.Reader);//read back by shard

      gotDelta.See("Got delta: ");

      Aver.AreEqual(mem.Id, gotDelta.Id);
      Aver.AreEqual(got.Id, gotDelta.Id);


      Aver.IsTrue(gotDelta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), gotDelta.NextSliceInterval);
      Aver.AreEqual(2, gotDelta.ChangesReceived.Length);
      Aver.IsNull(gotDelta.Result);
      Aver.IsNull(gotDelta.ResultReceivedJson);
      Aver.AreEqual(321, gotDelta.ExitCode);

      Aver.AreEqual("d", gotDelta.ChangesReceived[0].Name.Value);
      Aver.IsNotNull(gotDelta.ChangesReceived[0].Data);

      Aver.AreEqual("a", gotDelta.ChangesReceived[1].Name.Value);
      Aver.IsNotNull(gotDelta.ChangesReceived[1].Data);

      //manually deserialize content, in future this will need to be updated to reflect BIX etc...
      var map = JsonReader.DeserializeDataObject(new MemoryStream(gotDelta.ChangesReceived[0].Data)) as JsonDataMap;
      map.See("map 1");
      var deserManually = JsonReader.ToDoc<TeztState.DemographicsSlot>(map);

      Aver.AreEqual(55166ul, deserManually.AccountNumber);
      Aver.AreEqual("Monster", deserManually.LastName);
      Aver.IsTrue(FiberState.SlotMutationType.Modified == deserManually.SlotMutation);

      map = JsonReader.DeserializeDataObject(new MemoryStream(gotDelta.ChangesReceived[1].Data)) as JsonDataMap;
      map.See("map 2");
      var deserManually2 = JsonReader.ToDoc<TeztState.AttachmentSlot>(map);

      Aver.AreEqual("windows.bmp", deserManually2.AttachmentName);
      Aver.AreEqual(5, deserManually2.AttachContent.Length);
      Aver.IsTrue(FiberState.SlotMutationType.Modified == deserManually2.SlotMutation);

    }

    [Run]
    public void Test04_Roundtrip_Crash()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(7, 12, 1234567890));
      var pars = new TeztParams
      {
        Bool1 = true,
        Int1 = 123456,
        String1 = "BoltJolt"
      };

      var state = new TeztState();
      state.FirstName = "Alex";
      state.LastName = "Rooster";
      state.DOB = new DateTime(1980, 1, 2, 14, 15, 00, DateTimeKind.Utc);
      state.AccountNumber = 987654321;
      state.SetAttachment("hockey_fun.jpeg", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5 });
      state.ResetAllSlotModificationFlags();
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), null, pars, state);//this packs buffer

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
      var (gotParsBase, gotStateBase) = got.UnpackBuffer(typeof(TeztParams), typeof(TeztState));
      var (gotPars, gotState) = (gotParsBase.CastTo<TeztParams>(), gotStateBase.CastTo<TeztState>());
      Aver.AreEqual(pars.Int1, gotPars.Int1);
      Aver.AreEqual(pars.String1, gotPars.String1);
      Aver.AreEqual(state.AccountNumber, gotState.AccountNumber);
      Aver.AreEqual(state.AttachmentName, gotState.AttachmentName);
      Aver.AreArraysEquivalent(state.AttachmentContent, gotState.AttachmentContent);

      //==== We are in processor pretending to use state now

      Aver.IsFalse(got.HasDelta(gotState));
      got.Crash(new FabricException("Problem X"));//crash memory

      var delta = got.MakeDeltaSnapshot(FiberStep.Finish(321), gotState);

      Aver.IsTrue(delta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), delta.NextSliceInterval);
      Aver.IsNull(delta.Changes);
      Aver.IsNull(delta.Result);
      Aver.IsNotNull(delta.Crash);
      Aver.AreEqual(0, delta.ExitCode);

      wscope.Reset();
      delta.WriteOneWay(wscope.Writer, 1);//written by processor

      var wire = wscope.Buffer;
      "{0} bytes of delta wired back to shard".SeeArgs(wire.Length);

      using var rscope2 = new BixReaderBufferScope(wire);
      var gotDelta = new FiberMemoryDelta(rscope2.Reader);//read back by shard

      gotDelta.See("Got delta: ");

      Aver.AreEqual(mem.Id, gotDelta.Id);
      Aver.AreEqual(got.Id, gotDelta.Id);


      Aver.IsTrue(gotDelta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), gotDelta.NextSliceInterval);
      Aver.IsNull(gotDelta.Changes);
      Aver.IsNull(gotDelta.Result);
      Aver.AreEqual(0, gotDelta.ExitCode);
      Aver.IsNotNull(gotDelta.Crash);

      Aver.AreEqual("Problem X", gotDelta.Crash.Message);
    }
  }
}
