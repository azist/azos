/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Serialization.JSON;
using Azos.Serialization.Bix;

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Represents a changeset made to <see cref="FiberMemory"/> object.
  /// This changeset is obtained from a call to <see cref="FiberMemory.MakeDeltaSnapshot"/>
  /// and gets commited back into <see cref="IFiberStoreShard"/> using <see cref="IFiberStoreShard.CheckInAsync(FiberMemoryDelta)"/>
  /// </summary>
  public sealed class FiberMemoryDelta
  {
    internal FiberMemoryDelta(){ }

    /// <summary>
    /// Called by shards: reads memory from flat bin content produced by processor.
    /// Processors never read this back as they read <see cref="FiberMemory"/> instead
    /// </summary>
    public FiberMemoryDelta(BixReader reader)
    {
      Version = reader.ReadInt();
      Id = new FiberId(reader.ReadAtom(),
                       reader.ReadAtom(),
                       reader.ReadGDID());

      var crashJson = reader.ReadString();
      if (crashJson != null)//read crash
      {
        Crash = JsonReader.ToDoc<WrappedExceptionData>(crashJson, fromUI: false, JsonReader.DocReadOptions.BindByCode);
        return;
      }
      //else read next step

      NextStep = reader.ReadAtom();
      NextSliceInterval = reader.ReadTimeSpan();
      ExitCode = reader.ReadInt();

      var resultJson = reader.ReadString();
      if (resultJson != null)
      {
        Result = JsonReader.ToDoc<FiberResult>(resultJson, fromUI: false, JsonReader.DocReadOptions.BindByCode);
      }

      var changeCount = reader.ReadInt();
      (changeCount <= Constraints.MAX_STATE_SLOT_COUNT).IsTrue("max state slot count");
      Changes = new KeyValuePair<Atom, FiberState.Slot>[changeCount];
      for(var i=0; i< changeCount; i++)
      {
        var idSlot = reader.ReadAtom();
        var slotJson = reader.ReadString();
        var slot = JsonReader.ToDoc<FiberState.Slot>(slotJson, fromUI: false, JsonReader.DocReadOptions.BindByCode);
        Changes[i] = new KeyValuePair<Atom, FiberState.Slot>(idSlot, slot);
      }
    }

    /// <summary>
    /// Called by memory shard: writes binary representation of this class one-way that is:
    /// it can only be read back by <see cref="FiberMemory(BixReader)"/>.
    /// The changes to memory are NOT communicated back using this method, instead <see cref="FiberMemoryDelta"/> is used
    /// </summary>
    public void WriteOneWay(BixWriter writer)
    {
      writer.Write(Version);

      writer.Write(Id.Runspace);
      writer.Write(Id.MemoryShard);
      writer.Write(Id.Gdid);

      if (Crash != null)
      {
        var json = JsonWriter.Write(Crash, JsonWritingOptions.CompactRowsAsMap);
        writer.Write(json);
        return;
      }

      writer.Write(NextStep);
      writer.Write(NextSliceInterval);
      writer.Write(ExitCode);

      string resultJson = null;
      if (Result != null)
      {
        resultJson = JsonWriter.Write(Result, JsonWritingOptions.CompactRowsAsMap);
      }
      writer.Write(resultJson);

      var changeCount = Changes?.Length ?? 0;
      writer.Write(changeCount);
      for(var i=0; i < changeCount; i++)
      {
        var change = Changes[i];
        writer.Write(change.Key);
        JsonWriter.Write(change.Value, JsonWritingOptions.CompactRowsAsMap);
      }
    }


    public int Version                { get; internal set; }
    public FiberId Id                 { get; internal set; }

    public Atom NextStep              { get; internal set; }
    public TimeSpan NextSliceInterval { get; internal set; }
    public int ExitCode               { get; internal set; }
    public FiberResult Result         { get; internal set; }
    public KeyValuePair<Atom, FiberState.Slot>[] Changes  { get; internal set; }

    public WrappedExceptionData Crash { get; internal set; }
  }
}
