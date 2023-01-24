/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Memory statuses
  /// </summary>
  public enum MemoryStatus
  {
    Unset = 0,

    /// <summary>
    /// The memory is locked for the caller - the caller may mutate the memory and then call <see cref="IFiberStoreShard.CheckInAsync(FiberMemory)"/>
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
    private MemoryStatus m_Status;
    private FiberId m_Id;
    private byte[] m_Bin;

    private ArraySegment<byte> m_BinParameters;
    private FiberParameters m_Parameters;

    private ArraySegment<byte> m_BinState;
    private FiberState m_State;


    public MemoryStatus Status => m_Status;
    public FiberId Id { get; set; }

    public Guid ImageTypeId => throw new NotImplementedException();

    public FiberParameters Parameters => null;// materialize(m_PareameterData);
    public FiberState State => null;// materialize(m_StateData);

    public EntityId? ImpersonateAs { get; }

    /// <summary>
    /// Creates s snapshot of data changes which can be commited back into <see cref="IFiberStoreShard"/>
    /// using <see cref="IFiberStoreShard.CheckInAsync(FiberMemoryDelta)"/>
    /// </summary>
    public FiberMemoryDelta MakeDeltaSnapshot()
    {
      return null;
    }

    private T materialize<T>(ref T existing, ArraySegment<byte> bin) where T : class
    {
      if (existing == null)
      {
        existing = null;// materialize(m_Bin, 0, 100);
      }

      return existing;
    }

  }
}
