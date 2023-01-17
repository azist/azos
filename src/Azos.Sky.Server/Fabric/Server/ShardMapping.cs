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
using Azos.Apps;
using Azos.Collections;
using Azos.Log;

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Describes runspaces which are supported by the system.
  /// Runspaces partition fiber execution and storage zones
  /// </summary>
  public sealed class ShardMapping : IAtomNamed, IFiberStoreShard
  {
    /// <summary>
    /// Unique fiber namespace identifier
    /// </summary>
    public Atom Name => throw new NotImplementedException();


    /// <summary>
    /// Specifies relative weight of this shard among others for processing
    /// </summary>
    public float ProcessingFactor{ get; }

    /// <summary>
    /// Specifies relative weight of this shard among others for new data allocation
    /// </summary>
    public float AllocationFactor { get; }



    public IApplication App => throw new NotImplementedException();

    public ulong ComponentSID => throw new NotImplementedException();

    public IApplicationComponent ComponentDirector => throw new NotImplementedException();

    public DateTime ComponentStartTime => throw new NotImplementedException();

    public string ComponentCommonName => throw new NotImplementedException();

    public MessageType? ComponentLogLevel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public MessageType ComponentEffectiveLogLevel => throw new NotImplementedException();

    public string ComponentLogTopic => throw new NotImplementedException();

    public string ComponentLogFromPrefix => throw new NotImplementedException();

    public int ExpectedShutdownDurationMs => throw new NotImplementedException();

    public Task<FiberInfo> AbortAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<bool> CheckInAsync(FiberMemoryDelta fiber)
    {
      throw new NotImplementedException();
    }

    public Task<FiberMemory> CheckOutNextPendingAsync(Atom runspace, Atom procId)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> GetFiberInfoAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<FiberInfo>> GetFiberListAsync(FiberFilter args)
    {
      throw new NotImplementedException();
    }

    public Task<FiberParameters> GetFiberParametersAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<FiberResult> GetFiberResultAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<FiberState> GetFiberStateAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<bool> LoadFiberStateSlotAsync(FiberId idFiber, FiberState.Slot slot)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> PauseAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> ResumeAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> SetPriorityAsync(FiberId idFiber, float priority, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> StartFiberAsync(FiberStartArgs args)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> SuspendAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberMemory> TryCheckOutSpecificAsync(FiberId idFiber, Atom procId)
    {
      throw new NotImplementedException();
    }
  }
}
