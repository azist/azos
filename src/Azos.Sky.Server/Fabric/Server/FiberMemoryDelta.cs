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
  /// WARNING: This is a low-level infrastructure class that business app developers should never use!<br/>
  /// Represents a changeset made to <see cref="FiberMemory"/> object.
  /// This changeset is obtained from a call to <see cref="FiberMemory.MakeDeltaSnapshot"/>
  /// and gets commited back into <see cref="IFiberStoreShard"/> using <see cref="IFiberStoreShard.CheckInAsync(FiberMemoryDelta)"/>
  /// </summary>
  public sealed class FiberMemoryDelta
  {
    internal FiberMemoryDelta(){ }

    /// <summary>
    /// system internal use
    /// </summary>
    public struct SlotChange
    {
      public SlotChange(Atom name, FiberState.Slot slot)
      {
        Name = name;
        Mutation = slot.SlotMutation;
        NoPreload = slot.DoNotPreload;
        Data = FiberState.PackSlot(slot);
      }

      public SlotChange(BixReader reader)
      {
        Name = reader.ReadAtom();
        Mutation = (FiberState.SlotMutationType)reader.ReadByte();
        NoPreload = reader.ReadBool();
        Data = reader.ReadBuffer();
      }

      public void WriteOneWay(BixWriter writer)
      {
        writer.Write(Name);
        writer.Write((byte)Mutation);
        writer.Write(NoPreload);
        writer.WriteBuffer(Data);
      }

      public readonly Atom Name;
      public readonly FiberState.SlotMutationType Mutation;
      public readonly bool NoPreload;
      public readonly byte[] Data;
    }


    /// <summary>
    /// Called by shards: reads memory from flat bin content produced by processor.
    /// Processors never read this back as they read <see cref="FiberMemory"/> instead
    /// </summary>
    public FiberMemoryDelta(BixReader reader)
    {
      Version = reader.ReadInt();

      (Version <= Constraints.MEMORY_FORMAT_VERSION).IsTrue("Wire Version <= MEMORY_FORMAT_VERSION");


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

      //Result is stored as string json deserialized into different property
      ResultReceivedJson = reader.ReadString();//deserialize as string

      var changeCount = reader.ReadInt();
      (changeCount <= Constraints.MAX_STATE_SLOT_COUNT).IsTrue("max state slot count");
      ChangesReceived = new SlotChange[changeCount];
      for(var i=0; i< changeCount; i++)
      {
        ChangesReceived[i] = new SlotChange(reader);
      }
    }

    /// <summary>
    /// Processor ===&gt; Shard<br/>
    /// Called by processor: writes binary representation of this class one-way that is:
    /// it can only be read back by <see cref="FiberMemoryDelta(BixReader)"/>.
    /// The changes to memory are NOT communicated back using this method, instead <see cref="FiberMemory"/> is used
    /// </summary>
    public void WriteOneWay(BixWriter writer, int formatVersion)
    {
      (formatVersion <= Constraints.MEMORY_FORMAT_VERSION).IsTrue("Wire Version <= MEMORY_FORMAT_VERSION");
      writer.Write(formatVersion);

      writer.Write(Id.Runspace);
      writer.Write(Id.MemoryShard);
      writer.Write(Id.Gdid);

      if (Crash != null)
      {
        var json = JsonWriter.Write(Crash, JsonWritingOptions.CompactRowsAsMap);
        writer.Write(json);
        return;
      }
      else
      {
        writer.Write((string)null);
      }

      writer.Write(NextStep);
      writer.Write(NextSliceInterval);
      writer.Write(ExitCode);

      string resultJson = null;
      if (Result != null)
      {
        resultJson = JsonWriter.Write(Result, JsonWritingOptions.CompactRowsAsMap);
      }
      writer.Write(resultJson);//json string

      var changeCount = Changes?.Length ?? 0;
      writer.Write(changeCount);
      for(var i=0; i < changeCount; i++)
      {
        var change = new SlotChange(Changes[i].Key, Changes[i].Value);
        change.WriteOneWay(writer);
      }
    }


    public int Version                { get; internal set; }
    public FiberId Id                 { get; internal set; }

    public Atom NextStep              { get; internal set; }
    public TimeSpan NextSliceInterval { get; internal set; }
    public int ExitCode               { get; internal set; }
    public WrappedExceptionData Crash { get; internal set; }

    /// <summary>
    /// Value set by processor to be serialized for shard
    /// </summary>
    public FiberResult Result         { get; internal set; }

    /// <summary>
    /// Value set by processor to be serialized for shard
    /// </summary>
    public KeyValuePair<Atom, FiberState.Slot>[] Changes  { get; internal set; }

    /// <summary>
    /// Since FiberResult is abstract, the memory shard receives and stores data as JSON string, as it has no
    /// way of deserializing the CLR type that processor serialized
    /// </summary>
    public string ResultReceivedJson { get; internal set; }

    /// <summary>
    ///Since FiberState.Slot[] is polymorphic, the memory shard receives and stores state data as byte[], as it has no
    /// way of deserializing the CLR type that processor serialized
    /// </summary>
    public SlotChange[] ChangesReceived { get; internal set; }
  }
}
