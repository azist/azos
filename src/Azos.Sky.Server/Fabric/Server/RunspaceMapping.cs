/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Describes runspaces which are supported by the system.
  /// Runspaces partition fiber execution and storage zones
  /// </summary>
  public sealed class RunspaceMapping : ApplicationComponent<FiberProcessorDaemon>, IAtomNamed
  {
    public const string CONFIG_RUNSPACE_SECTION = "runspace";

    internal RunspaceMapping(FiberProcessorDaemon processor, IConfigSectionNode cfg) : base(processor)
    {
      m_ProcessingFactor = Constraints.PROCESSING_FACTOR_DEFAULT;

      ConfigAttribute.Apply(this, cfg.NonEmpty(nameof(cfg)));
      m_Name.IsValidNonZero("Configured `name`: Atom");


      m_Shards = new AtomRegistry<ShardMapping>();
      foreach(var nshard in cfg.ChildrenNamed(ShardMapping.CONFIG_SHARD_SECTION))
      {
        var shard = FactoryUtils.MakeDirectedComponent<ShardMapping>(this, nshard, typeof(ShardMapping), new[]{nshard});
        m_Shards.Register(shard).IsTrue($"Unique shard name `{shard.Name}`");
      }
    }

    [Config]
    private Atom m_Name;
    private float m_ProcessingFactor;
    private AtomRegistry<ShardMapping> m_Shards;


    public override string ComponentLogTopic => CoreConsts.FABRIC_TOPIC;

    /// <summary>
    /// Unique fiber namespace identifier
    /// </summary>
    public Atom Name => m_Name;


    /// <summary>
    /// Specifies relative weight of this runspace among others for processing
    /// </summary>
    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_QUEUE, CoreConsts.EXT_PARAM_GROUP_FABRIC)]
    public float ProcessingFactor
    {
      get => m_ProcessingFactor;
      private set => m_ProcessingFactor = value.KeepBetween(Constraints.PROCESSING_FACTOR_MIN, Constraints.PROCESSING_FACTOR_MAX);
    }


    /// <summary>
    /// Returns all shard mappings for this runspace
    /// </summary>
    public IAtomRegistry<ShardMapping> Shards => m_Shards;


    /// <summary>
    /// Returns a shard where a new allocation should go next, depending on
    /// <see cref="ShardMapping.AllocationFactor"/> and system state.
    /// Null when no shards can take any allocations
    /// </summary>
    public ShardMapping GetShardForNewAllocation()
    {
      var result = Shards.Select(one => (e: one, f: one.AllocationFactor)) //make a copy
                         .Where(one => one.f > 0)
                         .FirstMin(one => Interlocked.Read(ref one.e.m_AllocationCount) / one.f).e;

      if (result == null) return null;

      Interlocked.Increment(ref result.m_AllocationCount);
      return result;
    }

  }
}
