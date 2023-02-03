/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Sky.Fabric
{
  /// <summary>
  /// Represents an abstraction of fiber state which is segmented into named slots.
  /// Each slot is a DTO - a structured serializable piece of data which can be fetched/changed piece-by-piece individually on an as-needed basis;
  /// this is important for efficiency for fibers which require large DTO states.
  /// <br/>
  ///  ATTENTION: This class should NOT have any business logic beyond slot data management, and validation without relying on any 3rd party dependencies
  /// <br/>
  /// Upon fiber execution, the runtime fetches pending fiber records from the store along with their current state from
  /// persisted storage, then the `ExecSlice` method is called which possibly mutates the state of a fiber represented by this instance.
  /// Upon return from `ExecSlice()`, the runtime inspects the state slots for mutations, saving changes (if any) into persisted storage.
  /// The system does this slot-by-slot significantly improving overall performance in most cases when not the whole state changes all the time.
  /// Particular fiber state implementations derive their own classes which reflect the required business logic.
  /// <br/>
  /// WARNING: The objects of this class are designed as structured DTO and are not intended to be used for business rules , and are not thread safe
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
    /// Particular fiber implementations derive their own PRIVATE classes which reflect the necessary business field structure.
    /// <br/>
    /// WARNING: declare slot-derived classes as private within your particular state facade.
    /// <br/>
    /// Do not expose these classes directly to users as this may lead to inadvertent mutation of inner state
    /// which is not tracked, instead use functional-styled data accessors <see cref="FiberState.Get{T}(Atom)"/> and <see cref="FiberState.Set{T}(Atom, Action{T})"/>
    /// which mark the whole containing slot as changed so the system can save the changes in state into persisted storage.
    /// <br/>
    /// WARNING: The objects of this class are designed as structured DTO and are not intended to be used for business rules, and are not thread safe
    /// </summary>
    [BixJsonHandler(ThrowOnUnresolvedType = true)]
    public abstract class Slot : AmorphousTypedDoc
    {
      public override bool AmorphousDataEnabled => true;

      private SlotMutationType m_SlotMutation;
      private bool m_SlotWasLoaded;

      internal void MarkSlotAsUnchanged() => m_SlotMutation = SlotMutationType.Unchanged;
      internal void MarkSlotAsModified() => m_SlotMutation = SlotMutationType.Modified;
      internal void MarkSlotAsDeleted() => m_SlotMutation = SlotMutationType.Deleted;

      [Field(Description = "Defines a type of data change of this slot")]
      public SlotMutationType SlotMutation
      {
        get => m_SlotMutation;
        private set => m_SlotMutation = value;
      }


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

      /// <summary>
      /// Adds type code using BIX, so the system will add Guids from <see cref="Azos.Serialization.Bix.BixAttribute"/>
      /// which are used for both binary and json polymorphism
      /// </summary>
      protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
      {
        if (def?.Order == 0)
        {
          BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);
        }

        base.AddJsonSerializerField(def, options, jsonMap, name, value);
      }
    }

    internal void __fromStream(BixReader reader, int version)
    {
      m_MemoryVersion = version;
      m_CurrentStep = reader.ReadAtom();

      var slotCount = reader.ReadInt();
      (slotCount <= Constraints.MAX_STATE_SLOT_COUNT).IsTrue("Bound check: slot count <= MAX_SLOT_COUNT");

      m_Data.Clear();
      for(var i=0; i < slotCount; i++)
      {
        var slotId = reader.ReadAtom();
        var slotData = reader.ReadBuffer();
        m_Data[slotId] = slotData;
      }
    }

    internal void __toStream(BixWriter writer, int memoryVersion)
    {
      writer.Write(m_CurrentStep);
      writer.Write(m_Data.Count);
      using var wbuf = BixWriterBufferScope.DefaultCapacity;
      foreach(var kvp in m_Data)
      {
        writer.Write(kvp.Key);

        var json = JsonWriter.Write(kvp.Value, JsonWritingOptions.CompactRowsAsMap);
        wbuf.Reset();
        wbuf.Writer.Write(json);
        writer.WriteBuffer(wbuf.Buffer);
      }
    }

    private int m_MemoryVersion;
    private Atom m_CurrentStep;
    private readonly Dictionary<Atom, object> m_Data = new Dictionary<Atom, object>();//Variant data type - stores either byte[] or Slot

    // Unpacks Slot from variant data type which either stores byte[] or already unpacked slot
    // null if key does not exist
    private Slot getSlot(Dictionary<Atom, object> data, Atom key)
    {
      if (!data.TryGetValue(key, out var got)) return null;

      if (got is Slot slot) return slot;
      if (got is byte[] buf)
      {
        //todo: deserialize using BIX
        //if m_MemroyVersion < 100 then use_json else use_bix
        using var scope = new BixReaderBufferScope(buf);
        var json = scope.Reader.ReadString();
        var slotFromJson = JsonReader.ToDoc<Slot>(json, fromUI: false, JsonReader.DocReadOptions.BindByCode);
        data[key] = slotFromJson;
        return slotFromJson;
      }

      throw new FabricException("Implementation exception: ! slot and !byte[]");
    }


    /// <summary>
    /// Current step of fiber execution finite state machine. Steps are needed for cooperative multitasking;
    /// The states are returned from fiber execution slices and transition the status until the finite finished
    /// terminal state is reached.
    /// </summary>
    public Atom CurrentStep => m_CurrentStep;

    /// <summary>
    /// True when any of slots have changed
    /// </summary>
    public bool SlotsHaveChanges => SlotChanges.Any();

    /// <summary>
    /// Slots that have changes
    /// </summary>
    public IEnumerable<KeyValuePair<Atom, Slot>> SlotChanges => MaterializedData.Where(one => one.Value.SlotMutation != SlotMutationType.Unchanged);

    /// <summary>
    /// Enumerates all of the named slots in this state bag which have materialized -have been accessed.
    /// The slots that have not be gotten or written are NOt returned.
    /// You should NOT use this to directly mutate the data on the slots
    /// as this does not mark slots as modified and the changes will be lost.
    /// Use the specific field accessors provided by your derived class instead
    /// </summary>
    public virtual IEnumerable<KeyValuePair<Atom, Slot>> MaterializedData
    {
      get
      {
        foreach(var kvp in m_Data)
        {
          if (kvp.Value is Slot slot) yield return new KeyValuePair<Atom, Slot>(kvp.Key, slot);
        }
      }
    }

    /// <summary>
    /// Override this property to set indexable tags for this state.
    /// The system uses state tags to find fibers by tagged values - this mechanism is used
    /// in addition to fiber tags which are immutable
    /// <br/>
    /// Return:
    /// <list>
    ///   <item>null - keep existing tags as-is</item>
    ///   <item>empty array - delete all existing tags</item>
    ///   <item>array - replace all tags with the specified ones</item>
    /// </list>
    /// The default implementation returns null - keep tags as-is.<br/>
    /// State tags introduce performance overhead proportional to number of tags, so for all practical reasons
    /// you should ONLY use tags when necessary and keep their count at minimum (2-3)
    /// </summary>
    public virtual Data.Adlib.Tag[] Tags => null;


    internal void ResetAllSlotModificationFlags()
    {
      SlotChanges.ForEach(one => one.Value.MarkSlotAsUnchanged());
    }


    /// <summary> Returns slot by id, or null if such slot does not exist </summary>
    protected Slot Get(Atom id)
    {
      id.IsValidNonZero(nameof(id));
      return getSlot(m_Data, id);
    }

    /// <summary> Returns slot by id, or null if such slot does not exist </summary>
    protected T Get<T>(Atom id) where T : Slot => Get(id) as T;

    /// <summary> Sets slot data </summary>
    protected FiberState Set(Atom id, Slot data)
    {
      id.IsValidNonZero(nameof(id));
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
