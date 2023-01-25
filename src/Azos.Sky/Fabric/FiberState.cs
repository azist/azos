﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Sky.Fabric
{
  /// <summary>
  /// Represents an abstraction of fiber state which is segmented into named slots.
  /// Each slot represents a structured serializable piece of data which can be fetched/changed separately;
  /// this is needed for efficiency for fibers which require large states.
  /// Upon fiber execution, the runtime fetches pending fiber records from the store along with their current state from
  /// persisted storage, then the `ExecSlice` method is called which possibly mutates the state of a fiber represented by this instance.
  /// Upon return from `ExecSlice()`, the runtime inspects the state slots for mutations, saving changes (if any) into persisted storage.
  /// The system does this slot-by-slot significantly improving overall performance.
  /// Particular fiber state implementations derive their own classes which reflect the required business logic
  /// </summary>
  public class FiberState
  {
    /// <summary>
    /// Describes how slot data has changed - this is needed for change tracking
    /// when system saves state changes into persisted storage.
    /// </summary>
    public enum SlotMutationType
    {
      Unchanged = 0,
      Modified = 1,
      Deleted = 2
    }

    /// <summary>
    /// Represents an abstraction of a unit of change in fiber's state.
    /// Particular fiber implementations derive their own PRIVATE classes which reflect the necessary business logic.
    /// Warning: declare slot-derived classes as private within your particular state facade.
    /// Do not expose those classes directly to users as this may lead to inadvertent mutation of inner state
    /// which is not tracked, instead use functional-styled data accessors which mark the whole containing slot as changed
    /// so the system can save the changes in state into persisted storage
    /// </summary>
    public abstract class Slot : AmorphousTypedDoc
    {
      public override bool AmorphousDataEnabled => true;

      private SlotMutationType m_SlotMutation;
      private bool m_SlotWasLoaded;

      internal void MarkSlotAsModified() => m_SlotMutation = SlotMutationType.Modified;
      internal void MarkSlotAsDeleted() => m_SlotMutation = SlotMutationType.Deleted;

      public SlotMutationType SlotMutation => m_SlotMutation;

      /// <summary>
      /// True if the runtime loaded the data for this slot already.
      /// Slots may request to not pre-load the data via `DoNotPreload = true`
      /// </summary>
      public bool SlotWasLoaded => m_SlotWasLoaded;

      /// <summary>
      /// Override to return true to instruct runtime not to always pre-load the data for this slot type.
      /// This is needed for data slots which are NOT always needed for job execution, and can be lazy-loaded
      /// only WHEN needed according to specific job state control logic (e.g. customer image content, call history list etc.)
      /// </summary>
      public virtual bool DoNotPreload => false;
    }


    private Atom m_CurrentStep;
    private readonly Dictionary<Atom, Slot> m_Data = new Dictionary<Atom, Slot>();


    /// <summary>
    /// Current step of fiber execution finite state machine. Steps are needed for cooperative multitasking;
    /// The states are returned from fiber execution slices and transition the status until the finite finished
    /// terminal state is reached.
    /// </summary>
    public Atom CurrentStep => m_CurrentStep;

    /// <summary>
    /// True when any of slots have changed
    /// </summary>
    public bool SlotsHaveChanges => m_Data.Any(one => one.Value.SlotMutation != SlotMutationType.Unchanged);

    /// <summary>
    /// Enumerates all of the named slots in this state bag.
    /// You should NOT use this to directly mutate the data on the slots
    /// as this does not mark slots as modified and the changes will be lost.
    /// Use the specific field accessors provided by your derived class instead
    /// </summary>
    public virtual IEnumerable<KeyValuePair<Atom, Slot>> Data => m_Data;

    /// <summary> Returns slot by id, or null if such slot does not exist </summary>
    protected Slot Get(Atom id)
    {
      id.HasRequiredValue(nameof(id)).AsValid(nameof(id));
      if (m_Data.TryGetValue(id, out var existing)) return existing;
      return null;
    }

    /// <summary> Returns slot by id, or null if such slot does not exist </summary>
    protected T Get<T>(Atom id) where T : Slot => Get(id) as T;

    /// <summary> Sets slot data </summary>
    protected FiberState Set(Atom id, Slot data)
    {
      id.HasRequiredValue(nameof(id)).AsValid(nameof(id));
      data.NonNull(nameof(data));
      data.MarkSlotAsModified();
      m_Data[id] = data;
      return this;
    }


    /// <summary> Sets slot data </summary>
    protected FiberState Set<T>(Atom id, Action<T> fset) where T : Slot, new()
    {
      fset.NonNull(nameof(fset));
      var seg = Get<T>(id) ?? new T();
      fset(seg);
      Set(id, seg);
      return this;
    }

    protected bool Delete(Atom id)
    {
      var slot = Get(id);
      if (slot != null)
      {
        slot.MarkSlotAsDeleted();
        return true;
      }
      return false;
    }
  }


  /// <summary>
  /// EXAMPLE ONLY
  /// </summary>
  public sealed class BakerState : FiberState
  {
    private sealed class counts : Slot
    {
      [Field] public int DonutCount { get; set; }

      [Field] public int CakeCount { get; set; }
    }

    private sealed class cakeimg : Slot
    {
      [Field] public byte[] Image { get; set; }

      public override bool DoNotPreload => true;
    }

    private static readonly Atom SLOT_DATA = Atom.Encode("d");


    public int DonutCount
    {
      get => Get<counts>(SLOT_DATA)?.DonutCount ?? 0;
      set => Set<counts>(SLOT_DATA, data => data.DonutCount = value);
    }

    public int CakeCount
    {
      get => Get<counts>(SLOT_DATA)?.CakeCount ?? 0;
      set => Set<counts>(SLOT_DATA, data => data.CakeCount = value);
    }

    //  public byte[] GetImage(runtime) => Get<cakeimg>(SLOT_IMG)?.EnsureLoaded(runtime) ?? null;

  }

}
