/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using Azos.Serialization.Bix;

namespace Azos.Data.Business
{
  /// <summary>
  /// Version log vector: (G_Version, Utc, Origin, Actor, State)'
  /// </summary>
  [Bix("c7ca1df1-4434-42e6-8ef2-94b96bb57d85")]
  [Schema(Description = "Tuple: (G_Version, Utc, Origin, Actor, State)")]
  public class VersionInfo : FragmentModel
  {
    /// <summary>
    /// Defines the state that the data is in this version: Created/Updated/Deleted or Undefined for invalid
    /// </summary>
    public enum DataState
    {
      Undefined = 0,
      Created = 1,
      Updated = 2,
      Deleted = -1
    }

    /// <summary>
    /// Defines canonical values for Create/Update/Delete data state
    /// </summary>
    public const string DATA_STATE_CANONICAL_VALUE_LIST = "C: Created, U: Updated, D: Deleted";

    /// <summary>
    /// Defines canonical value mapping for Create/Update/Delete data state
    /// </summary>
    public static DataState MapCanonicalState(string s)
    {
      switch (s)
      {
        case "C": return DataState.Created;
        case "U": return DataState.Updated;
        case "D": return DataState.Deleted;
        case "c": return DataState.Created;
        case "u": return DataState.Updated;
        case "d": return DataState.Deleted;
        default: return DataState.Undefined;
      }
    }

    /// <summary>
    /// Defines canonical value mapping for Create/Update/Delete data state
    /// </summary>
    public static string MapCanonicalState(DataState s)
    {
      switch (s)
      {
        case DataState.Created: return "C";
        case DataState.Updated: return "U";
        case DataState.Deleted: return "D";
        default: return null;
      }
    }

    /// <summary>
    /// True if the state is either Created or Updated but not undefined or deleted
    /// </summary>
    public static bool IsExistingStateOf(DataState state)
     => state == DataState.Created || state == DataState.Updated;


    [Field(required: true, Description = "An id which uniquely identifies data version of resource representation")]
    public GDID G_Version { get; set; }

    [Field(required: true, Description = "UTC timestamp assigned by the server when data version was created. " +
     "Do not confuse with Start/End date range which designates logical entity applicability")]
    public DateTime Utc { get; set; }

    [Field(required: true, Description = "What origin (data center) has the change originated from")]
    public Atom Origin { get; set; }

    [Field(required: true, Description = "Who made the change - e.g. user principal")]
    public EntityId Actor { get; set; }

    [Field(required: true, Description = "Data state: Inserted/Updated/Deleted")]
    public DataState State { get; set; }

    /// <summary>
    /// True if the state is either Created or Updated but not undefined or deleted
    /// </summary>
    public bool DataExists => IsExistingStateOf(State);
  }
}
