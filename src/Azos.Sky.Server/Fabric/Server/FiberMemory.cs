/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Memory statuses
  /// </summary>
  public enum MemoryStatus : byte
  {
    Unset = 0,

    /// <summary>
    /// The memory is locked for the caller - the caller may mutate the memory and then call <see cref="IFiberStoreShard.CheckInAsync(FiberMemoryDelta)"/>
    /// </summary>
    LockedForCaller = 1,

    /// <summary>
    /// The memory is locked by some other processor and memory representation may be incomplete.
    /// You may NOT change this memory at this time
    /// </summary>
    LockedForSomeoneElse
  }

  /// <summary>
  /// Represents a piece of TRANSITIVE virtual persisted memory which stores fiber execution finite state machine:
  /// its start parameters, transitive state and result object.
  /// FiberMemory gets operated-on by processors
  /// </summary>
  public sealed class FiberMemory
  {
    /// <summary>
    /// Called by processors: reads memory from flat bin content produced by shard.
    /// Shards never read this back as they read <see cref="FiberMemoryDelta"/> instead
    /// </summary>
    public FiberMemory(BixReader reader)
    {
      m_Version     = reader.ReadInt();
      m_Status      = (MemoryStatus)reader.ReadByte();

      m_Id          = new FiberId(reader.ReadAtom(),
                                  reader.ReadAtom(),
                                  reader.ReadGDID());

      m_ImageTypeId = reader.ReadGuid();
      m_ImpersonateAs = reader.ReadNullableEntityId();
      m_Buffer = reader.ReadBuffer();
      m_StateOffset = reader.ReadInt();
      (m_StateOffset >= 0 && m_StateOffset < m_Buffer.Length).IsTrue("stateOffset bounds");
    }

    /// <summary>
    /// Called by memory shard: writes binary representation of this class one-way that is:
    /// it can only be read back by <see cref="FiberMemory(BixReader)"/>.
    /// The changes to memory are NOT communicated back using this method, instead <see cref="FiberMemoryDelta"/> is used
    /// </summary>
    public void WriteOneWay(BixWriter writer)
    {
      writer.Write(m_Version);
      writer.Write((byte)m_Status);

      writer.Write(m_Id.Runspace);
      writer.Write(m_Id.MemoryShard);
      writer.Write(m_Id.Gdid);

      writer.Write(m_ImageTypeId);
      writer.Write(m_ImpersonateAs);
      writer.WriteBuffer(m_Buffer);
      writer.Write(m_StateOffset);
    }

    private int m_Version;
    private MemoryStatus m_Status;
    private FiberId m_Id;
    private Guid m_ImageTypeId;
    private EntityId? m_ImpersonateAs;
    private byte[] m_Buffer;
    private int m_StateOffset;
    private Exception m_CrashException;

    public int Version => m_Version;
    public MemoryStatus Status => m_Status;
    public FiberId Id => m_Id;
    public Guid ImageTypeId => m_ImageTypeId;
    public EntityId? ImpersonateAs => m_ImpersonateAs;
    public byte[] Buffer => m_Buffer;
    public int StateOffset => m_StateOffset;

    /// <summary>
    /// Error which crashed this memory, or null if not crashed
    /// </summary>
    public Exception CrashException => m_CrashException;


    /// <summary>
    /// True when this memory has any changes such as changes in state or a crash
    /// </summary>
    public bool HasDelta(FiberState state) => m_CrashException != null|| state.SlotsHaveChanges;

    /// <summary>
    /// Marks memory as failed with the specified exception.
    /// This memory can not be used anymore but to save the crash delta.
    /// Crashes are permanent - once fiber crashes - it fails permanently (unless recovered)
    /// </summary>
    public void Crash(Exception error)
    {
      m_CrashException = error.NonNull(nameof(error));
    }


    /// <summary>
    /// Creates s snapshot of data changes which can be commited back into <see cref="IFiberStoreShard"/>
    /// using <see cref="IFiberStoreShard.CheckInAsync(FiberMemoryDelta)"/>.
    /// This only succeeds if the <see cref="Status"/> is <see cref="MemoryStatus.LockedForCaller"/>
    /// otherwise Delta can not be obtained
    /// </summary>
    public FiberMemoryDelta MakeDeltaSnapshot(FiberStep? nextStep)
    {
      (m_Status == MemoryStatus.LockedForCaller).IsTrue("Delta obtained for LockedForCaller memory");
      return null;
    }

    public (FiberParameters pars, FiberState state) Unpack(Type tParameters, Type tState)
    {
      using var ms = new MemoryStream(m_Buffer);

      var pars = (FiberParameters)Serialization.SerializationUtils.MakeNewObjectInstance(tParameters);
      //todo: Replace with Bix in future
      var map = JsonReader.DeserializeDataObject(ms) as JsonDataMap;
      JsonReader.ToDoc(pars, map, fromUI: false, JsonReader.DocReadOptions.BindByCode);

      //the position here should be set by JsonReader above
      (ms.Position == m_StateOffset).IsTrue("Proper buffer position offset");

      var state = (FiberState)Serialization.SerializationUtils.MakeNewObjectInstance(tState);
      state.__fromStream(ms, m_Version);
      return (pars, state);
    }

  }
}
