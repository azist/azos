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
using Azos.Conf;
using Azos.Log;

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Describes runspaces which are supported by the system.
  /// Runspaces partition fiber execution and storage zones
  /// </summary>
  public sealed class ShardMapping : ApplicationComponent<RunspaceMapping>, IAtomNamed, IFiberStoreShard
  {
    public const string CONFIG_SHARD_SECTION = "shard";


    internal ShardMapping(RunspaceMapping runspace, IConfigSectionNode cfg) : base(runspace)
    {
      m_ProcessingFactor = Constraints.PROCESSING_FACTOR_DEFAULT;

      ConfigAttribute.Apply(this, cfg.NonEmpty(nameof(cfg)));
      m_Name.IsValidNonZero("Configured `name`: Atom");

      //todo Service connection
    }


    [Config]
    private Atom m_Name;
    private float m_ProcessingFactor;
    private float m_AllocationFactor;

    public override string ComponentLogTopic => CoreConsts.FABRIC_TOPIC;

    /// <summary>
    /// Unique shard identifier
    /// </summary>
    public Atom Name => m_Name;

    /// <summary>
    /// Runspace which this shard is under
    /// </summary>
    public RunspaceMapping Runspace => ComponentDirector;

    /// <summary>
    /// Specifies relative weight of this shard among others for processing
    /// </summary>
    public float ProcessingFactor
    {
      get => m_ProcessingFactor;
      private set => m_ProcessingFactor = value.KeepBetween(Constraints.PROCESSING_FACTOR_MIN, Constraints.PROCESSING_FACTOR_MAX);
    }

    /// <summary>
    /// Specifies relative weight of this shard among others for new data allocation
    /// </summary>
    public float AllocationFactor
    {
      get => m_AllocationFactor;
      private set => m_AllocationFactor = value.KeepBetween(Constraints.ALLOCATION_FACTOR_MIN, Constraints.ALLOCATION_FACTOR_MAX);
    }

    #region IFiberStoreShard

    public Task<FiberInfo> StartFiberAsync(FiberStartArgs args)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<FiberInfo>> GetFiberListAsync(FiberFilter args)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> GetFiberInfoAsync(FiberId idFiber)
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

    public Task<FiberInfo> SetPriorityAsync(FiberId idFiber, float priority, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> PauseAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> SuspendAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> ResumeAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> AbortAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberMemory> CheckOutNextPendingAsync(Atom runspace, Atom procId)
    {
      throw new NotImplementedException();
    }

    public Task<FiberMemory> TryCheckOutSpecificAsync(FiberId idFiber, Atom procId)
    {
      throw new NotImplementedException();
    }

    public Task<bool> CheckInAsync(FiberMemoryDelta fiber)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
