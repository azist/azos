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
    /// The memory is locked for the caller - the caller may mutate the memory and then call
    /// <see cref="IFiberStoreShard.CheckInAsync(FiberMemoryDelta)"/>
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
    /// .ctor used by shards as it packs the params and the state objects
    /// </summary>
    public FiberMemory(
              int version,
              MemoryStatus status,
              FiberId id,
              Guid instanceGuid,
              Guid imageTypeId,
              EntityId? impersonateAs,
              byte[] fiberParameters,
              Atom currentStep,
              KeyValuePair<Atom, byte[]>[] slots) :
     this(version, status, id, instanceGuid, imageTypeId, impersonateAs, PackBuffer(fiberParameters, currentStep, slots)) { }

    /// <summary>
    /// .ctor used by unit tests - obtains packed payload
    /// </summary>
    public FiberMemory(
              int          version,
              MemoryStatus status,
              FiberId      id,
              Guid         instanceGuid,
              Guid         imageTypeId,
              EntityId?    impersonateAs,
              byte[]       buffer)
    {
      m_Version        =   version.IsTrue(v => v <= Constraints.MEMORY_FORMAT_VERSION);
      m_Status         =   status;
      m_Id             =   id;
      m_InstanceGuid   =   instanceGuid;
      m_ImageTypeId    =   imageTypeId;
      m_ImpersonateAs  =   impersonateAs;
      m_Buffer         =   buffer.NonNull(nameof(buffer));
    }


    /// <summary>
    /// .ctor called by processors: reads memory from flat bin content produced by shard.
    /// Shards never read this back as they read <see cref="FiberMemoryDelta"/> instead
    /// </summary>
    public FiberMemory(BixReader reader)
    {
      m_Version     = reader.ReadInt();

      (m_Version <= Constraints.MEMORY_FORMAT_VERSION).IsTrue("Wire Version <= MEMORY_FORMAT_VERSION");


      m_Status      = (MemoryStatus)reader.ReadByte();

      m_Id          = new FiberId(reader.ReadAtom(),
                                  reader.ReadAtom(),
                                  reader.ReadGDID());

      m_InstanceGuid = reader.ReadGuid();
      m_ImageTypeId = reader.ReadGuid();
      m_ImpersonateAs = reader.ReadNullableEntityId();
      m_Buffer = reader.ReadBuffer();
    }

    /// <summary>
    /// Processor &lt;=== Shard<br/>
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

      writer.Write(m_InstanceGuid);
      writer.Write(m_ImageTypeId);
      writer.Write(m_ImpersonateAs);
      writer.WriteBuffer(m_Buffer);
    }

    private int m_Version;
    private MemoryStatus m_Status;
    private FiberId m_Id;
    private Guid m_InstanceGuid;
    private Guid m_ImageTypeId;
    private EntityId? m_ImpersonateAs;
    private byte[] m_Buffer;
    private Exception m_CrashException;

    public int Version => m_Version;
    public MemoryStatus Status => m_Status;
    public FiberId Id => m_Id;
    public Guid InstanceGuid => m_InstanceGuid;
    public Guid ImageTypeId => m_ImageTypeId;
    public EntityId? ImpersonateAs => m_ImpersonateAs;
    public byte[] Buffer => m_Buffer;

    /// <summary>
    /// Error which crashed this memory, or null if not crashed
    /// </summary>
    public Exception CrashException => m_CrashException;

    /// <summary>
    /// True when this memory has any changes such as changes in state or a crash
    /// </summary>
    public bool HasDelta(FiberState state) => m_CrashException != null || state.NonNull(nameof(state)).SlotsHaveChanges;

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
    /// otherwise Delta can not be obtained.
    /// Performs validation and throws if slots are in invalid state
    /// </summary>
    public FiberMemoryDelta MakeDeltaSnapshot(FiberStep? nextStep, FiberState currentState)
    {
      (m_Status == MemoryStatus.LockedForCaller).IsTrue("Delta obtained for LockedForCaller memory");

      var result = new FiberMemoryDelta
      {
        Version = this.Version,//respond using the same version
        Id = m_Id
      };

      var crash = m_CrashException;
      if (crash == null && !nextStep.HasValue) crash = new FabricException("Unspecified next step");

      if (crash != null)
      {
        result.Crash = new WrappedExceptionData(crash, true, true);
      }
      else
      {
        var nxt = nextStep.Value;
        result.NextStep = nxt.NextStep;
        result.NextSliceInterval = nxt.NextSliceInterval;
        result.ExitCode = nxt.ExitCode;
        result.Result = nxt.Result;
        result.Tags = currentState.Tags;

        var changes = currentState.SlotChanges.ToArray();
        foreach(var change in changes)
        {
          var error = change.Value.Validate();
          if (error != null)
          {
            throw new FabricStateValidationException(
              "Validation error for state slot {0}.{1}(`{2}`) validation failed: {3}".Args(
                  currentState.GetType().DisplayNameWithExpandedGenericArgs(),
                  change.Value.GetType().DisplayNameWithExpandedGenericArgs(),
                  change.Key,
                  error.ToMessageWithType()),
              error);
          }
        }

        result.Changes = changes;
      }

      return  result;
    }

    /// <summary>
    /// Called only when fibers are created by processor.
    /// Complementary pair with <see cref="UnpackParameters(Type, byte[])"/>
    /// </summary>
    public static byte[] PackParameters(FiberParameters pars)
    {
      using var wscope = BixWriterBufferScope.DefaultCapacity;

      wscope.Writer.WriteFixedBE32bits(0);//csum 4 bytes
      wscope.Writer.Write(Constraints.MEMORY_FORMAT_VERSION);
      //====================================================

      //todo future use bix with newer version
      wscope.Writer.Write(JsonWriter.Write(pars, JsonWritingOptions.CompactRowsAsMap));

      //====================================================
      var result = wscope.Buffer;
      var csum = IO.ErrorHandling.Adler32.ForBytes(result, sizeof(uint));
      IOUtils.WriteBEUInt32(result, csum);
      return result;
    }

    /// <summary>
    /// Called by processor complement of <see cref="PackParameters(FiberParameters)"/>
    /// </summary>
    public static FiberParameters UnpackParameters(Type tParameters, byte[] buffer)
    {
      if (buffer == null) return null;
      (buffer.Length > sizeof(uint)).IsTrue("Memory parameters buffer > 4 bytes");

      var csum = IO.ErrorHandling.Adler32.ForBytes(buffer, sizeof(uint));

      tParameters.IsOfType<FiberParameters>(nameof(tParameters));
      using var rscope = new BixReaderBufferScope(buffer);
      var gotCsum = rscope.Reader.ReadFixedBE32bits();

      (csum == gotCsum).IsTrue("Valid parameter memory signature");

      var version = rscope.Reader.ReadInt();
      (version <= Constraints.MEMORY_FORMAT_VERSION).IsTrue("Wire Version <= MEMORY_FORMAT_VERSION");

      //todo future use bix with newer version
      var json = rscope.Reader.ReadString();
      if (json == null) return null;

      var result = (FiberParameters)JsonReader.ToDoc(tParameters, json, fromUI: false, options: JsonReader.DocReadOptions.BindByCode);

      return result;
    }

    /// <summary>
    /// Used by shard - creates buffer representation from data fields in mem shard storage
    /// </summary>
    public static byte[] PackBuffer(byte[] fiberParameters,
                                    Atom currentStep,
                                    KeyValuePair<Atom, byte[]>[] slots)
    {
      slots.NonNull(nameof(slots));
      using var wscope = BixWriterBufferScope.DefaultCapacity;
      wscope.Writer.WriteBuffer(fiberParameters);
      FiberState.WriteShardData(wscope.Writer, currentStep, slots);
      return wscope.Buffer;
    }


    /// <summary>
    /// Materializes raw buffer into <see cref="FiberParameters"/> and <see cref="FiberState"/> of the specified types.
    /// You can use <see cref="Version"/> to perform backward-compatible upgrades of serialization methods
    /// </summary>
    public (FiberParameters pars, FiberState state) UnpackBuffer(Type tParameters, Type tState)
    {
      FiberParameters pars = null;

      using var rscope = new BixReaderBufferScope(m_Buffer);

      var parsBuffer = rscope.Reader.ReadBuffer();
      if (parsBuffer != null)
      {
        pars = UnpackParameters(tParameters, parsBuffer);
      }

      var state = (FiberState)Serialization.SerializationUtils.MakeNewObjectInstance(tState);
      state.__fromStream(rscope.Reader, m_Version);

      return (pars, state);
    }

  }
}
