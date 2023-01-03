/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Sky.Jobs
{
  /// <summary>
  /// Represents an abstraction of job state which is segmented by named slots.
  /// Each slot represents a structured serializable piece of data which can be fetched/changed separately;
  /// this is needed for efficiency for jobs which require large states.
  /// Upon job execution, the runtime fetches pending jobs from the store along with their current state from
  /// persisted storage, then the `ExecSlice` method is called which possibly mutates the state of a job represented by this instance.
  /// Upon return from `ExecSlice()`, the runtime inspects the state slots for mutations, saving changes (if any) into persisted storage.
  /// The system does this slot-by-slot significantly improving overall performance.
  /// Particular job state implementations derive their own classes which reflect the required business logic
  /// </summary>
  public class State
  {
    /// <summary>
    /// Describes how slot data has changed
    /// </summary>
    public enum SlotMutationType
    {
      Unchanged = 0,
      Modified = 1,
      Deleted = 2
    }

    /// <summary>
    /// Represents an abstraction of a unit of change in job's state.
    /// Particular job implementations derive their own classes which reflect the necessary business logic
    /// </summary>
    public abstract class Slot : AmorphousTypedDoc
    {
      public const int SLOT_NAME_MIN_LEN = 1;
      public const int SLOT_NAME_MAX_LEN = 64;
      public static string CheckName(string name) => name.NonBlankMinMax(SLOT_NAME_MIN_LEN, SLOT_NAME_MAX_LEN, nameof(name));

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

    private readonly Dictionary<string, Slot> m_Data = new Dictionary<string, Slot>();


    /// <summary>
    /// Enumerates all of the named slots in this state bag
    /// </summary>
    public virtual IEnumerable<KeyValuePair<string, Slot>> Data => m_Data;

    /// <summary> Returns slot by name, or null if such slot does not exist </summary>
    protected Slot Get(string name)
    {
      if (m_Data.TryGetValue(Slot.CheckName(name), out var existing)) return existing;
      return null;
    }

    /// <summary> Returns slot by name, or null if such slot does not exist </summary>
    protected T Get<T>(string name) where T : Slot => Get(name) as T;

    /// <summary> Sets slot data </summary>
    protected State Set(string name, Slot data)
    {
      Slot.CheckName(name);
      data.NonNull(nameof(data));
      data.MarkSlotAsModified();
      m_Data[name] = data;
      return this;
    }

    protected bool Delete(string name)
    {
      var slot = Get(name);
      if (slot != null)
      {
        slot.MarkSlotAsDeleted();
        return true;
      }
      return false;
    }

    /// <summary> Sets slot data </summary>
    protected State Set<T>(string name, Action<T> fset) where T : Slot, new()
    {
      fset.NonNull(nameof(fset));
      var seg = Get<T>(name) ?? new T();
      fset(seg);
      Set(name, seg);
      return this;
    }
  }


  /// <summary>
  /// EXAMPLE ONLY
  /// </summary>
  public sealed class BakerState : State
  {
    internal sealed class counts : Slot
    {
      [Field] public int DonutCount { get; set; }

      [Field] public int CakeCount { get; set; }
    }

    internal sealed class cakeimg : Slot
    {
      [Field] public byte[] Image { get; set; }

      public override bool DoNotPreload => true;
    }

    public int DonutCount
    {
      get => Get<counts>("d")?.DonutCount ?? 0;
      set => Set<counts>("d", data => data.DonutCount = value);
    }

    public int CakeCount
    {
      get => Get<counts>("d")?.CakeCount ?? 0;
      set => Set<counts>("d", data => data.CakeCount = value);
    }

  //  public byte[] GetImage(runtime) => Get<cakeimg>("i")?.EnsureLoaded(runtime) ?? null;

  }

}
