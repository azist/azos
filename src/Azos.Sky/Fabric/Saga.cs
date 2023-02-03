/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Data;
using Azos.Security;
using Azos.Serialization.Slim;

namespace Azos.Sky.Fabric
{
  /// <summary>
  /// Sagas are multi-part processing chains with possibly compensating transactions.
  /// They represent a mixture of "Saga" design pattern with "supervisor" actor model pattern
  /// </summary>
  public abstract class Flow<TParameters, TState> : Fiber<TParameters, TState> where TParameters : FiberParameters
                                                                               where TState : FlowState
  {
    //public abstract StartPhase{ get;}

    //public FiberStep CurrentPhase();
    //public FiberStep NextPhase(Atom phase);
  }


  /// <summary>
  /// EXAMPLE ONLY
  /// </summary>
  public abstract class FlowState : FiberState
  {
    //Only one part can be active
    protected abstract class Phase : Slot
    {
      //[Field(Required = true, Description = "Current Part")]
      //public Atom Phase { get; set; }

      public override bool DoNotPreload => true;
    }

    /// <summary>
    /// Items can happen simultaneously within phase
    /// </summary>
    protected abstract class Item : Slot
    {
      [Field(Required = true, Description = "A part which this chapter is in")]
      public Atom Phase { get; set; }

      public override bool DoNotPreload => true;
    }

    private static readonly Atom SLOT_DATA = Atom.Encode("d");


    //public int DonutCount
    //{
    //  get => Get<counts>(SLOT_DATA)?.DonutCount ?? 0;
    //  set => Set<counts>(SLOT_DATA, data => data.DonutCount = value);
    //}

    //public int CakeCount
    //{
    //  get => Get<counts>(SLOT_DATA)?.CakeCount ?? 0;
    //  set => Set<counts>(SLOT_DATA, data => data.CakeCount = value);
    //}

    //  public byte[] GetImage(runtime) => Get<cakeimg>(SLOT_IMG)?.EnsureLoaded(runtime) ?? null;

  }


}
