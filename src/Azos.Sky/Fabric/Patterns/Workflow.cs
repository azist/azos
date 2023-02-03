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
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using Azos.Serialization.Slim;

namespace Azos.Sky.Fabric.Patterns
{
  /// <summary>
  /// Sagas are multi-part processing chains with possibly compensating transactions.
  /// They represent a mixture of "Saga" design pattern with "supervisor" actor model pattern
  /// </summary>
  public abstract class Workflow<TParameters, TState> : Fiber<TParameters, TState> where TParameters : FiberParameters
                                                                                   where TState : WorkflowState
  {
    //public abstract StartPhase{ get;}

    //public FiberStep Loop()
    //{
    //
    //}

    //public FiberStep CurrentPhase();
    //public FiberStep NextPhase(Atom phase);
  }


  /// <summary>
  /// EXAMPLE ONLY
  /// </summary>
  public abstract class WorkflowState : FiberState
  {
    //Only one phase can be active
    protected abstract class Current : Slot
    {
      [Field(Required = true, Description = "Current Phase")]
      public Atom CurrentPhase { get; set; }
    }


    //Only one phase can be active
    protected abstract class Phase : Slot
    {
      [Field(Required = true, Description = "When Phase started")]
      public DateTime StartUtc { get;  set; }

      [Field(Required = true, Description = "Collection of work items")]
      public List<Item> Items {  get; set; }
    }

    /// <summary>
    /// Items can happen simultaneously within phase
    /// </summary>
    [BixJsonHandler(ThrowOnUnresolvedType = true)]
    protected abstract class Item : AmorphousTypedDoc
    {
      [Field(Required = true, Description = "A part which this chapter is in")]
      public Atom Phase { get; set; }

      protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
      {
        if (def?.Order == 0)
        {
          BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);
        }

        base.AddJsonSerializerField(def, options, jsonMap, name, value);
      }
    }

    private static readonly Atom SLOT_DATA = Atom.Encode("d");


    public Atom CurrentPhase => Get<Current>(SLOT_DATA)?.CurrentPhase ?? Atom.ZERO;


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
