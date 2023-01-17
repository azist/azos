/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;


namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Abstraction of fiber persisted store - where the system stores the state of fibers
  /// </summary>
  public sealed class FiberProcessorLogic : ModuleBase//, IFiberManagerLogic
  {
    public FiberProcessorLogic(IApplication application) : base(application)
    {
    }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.FABRIC_TOPIC;

    private Atom m_Id;
    private AtomRegistry<FiberRunspace> m_Runspaces;


    /// <summary>
    /// Processor Id. Must be immutable for lifetime of shard
    /// </summary>
    public Atom Id => m_Id;

    IAtomRegistry<FiberRunspace> Runspaces => m_Runspaces;



    /// <summary>
    /// All shards
    /// </summary>
    IEnumerable<IFiberStoreShard> Shards { get; }


  }
}
