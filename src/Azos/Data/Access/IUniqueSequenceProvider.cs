/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

namespace Azos.Data.Access
{

  /// <summary>
  /// Denotes entities that provide ULONG STABLE hash code for use in a distributed (large scale) system.
  /// This is needed primarily for cluster/large datasets to properly compute 64bit sharding addresses and to differentiate
  /// from GetHashCode() that returns 32 bits unstable hash for local object location in hashtables.
  /// DO not confuse with object.GetHashCode() which is un-suitable for long-term persistence
  /// </summary>
  public interface IDistributedStableHashProvider
  {
    /// <summary>
    /// Provides 64 bit STABLE hash suitable for distributed system application.
    /// This hash may NOT depend on platform as it is used for storage.
    /// Warning! DO NOT CALL object.GetHashCode() as it may not be suitable for storage
    /// </summary>
    ulong GetDistributedStableHash();
  }

  /// <summary>
  /// Provides basic information about a named sequence.
  /// Warning!!! This class represents informational-only data which CAN NOT be used for real identification
  /// </summary>
  public interface ISequenceInfo : Collections.INamed
  {
    /// <summary>
    /// Current Era in which the IDs are generated. This CAN NOT be used to obtain real ID, just the info
    /// </summary>
    uint Era{get;}

    /// <summary>
    /// Approximate current ID. This CAN NOT be used to obtain real ID, just the info
    /// </summary>
    ulong ApproximateCurrentValue{get;}

    /// <summary>
    /// The size of block (if any) that was pre-allocated (instead of generating IDs every time)
    /// </summary>
    int TotalPreallocation{get;}

    /// <summary>
    /// Remaining IDs from the preallocated count
    /// </summary>
    int RemainingPreallocation{get;}

    /// <summary>
    /// The name of issuing authority
    /// </summary>
    string IssuerName{get;}

    /// <summary>
    /// Time stamp of issue
    /// </summary>
    DateTime IssueUTCDate{get;}
  }


  /// <summary>
  /// Represents a starting ID along with the number of consecutive generated IDs of the sequence
  /// </summary>
  public struct ConsecutiveUniqueSequenceIds
  {
    public ConsecutiveUniqueSequenceIds(ulong startInclusive, int count)
    {
      this.StartInclusive = startInclusive;
      this.Count = count;
    }

    public readonly ulong StartInclusive;
    public readonly int Count;
  }

  /// <summary>
  /// Represents an entity that provides unique identifiers via named sequences
  /// </summary>
  public interface IUniqueSequenceProvider : Collections.INamed
  {

    /// <summary>
    /// Returns the list of all scope names in the instance
    /// </summary>
    IEnumerable<string> SequenceScopeNames { get; }


    /// <summary>
    /// Returns sequnce information enumerable for all sequences in the named scope
    /// </summary>
    IEnumerable<ISequenceInfo> GetSequenceInfos(string scopeName);

    /// <summary>
    /// Generates one ID for the supplied sequence name.
    /// Note: do not confuse with block pre-allocation, which is an internal optimization.
    /// Even if 100 IDs are pre-allocated the method returns one unique ID
    /// </summary>
    /// <param name="scopeName">The name of scope where sequences are kept</param>
    /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
    /// <param name="blockSize">If >0 specifies how many sequence values to pre-allocate, otherwise provider would use its default setting</param>
    /// <param name="vicinity">The location on ID counter scale, the issuing authority may disregard this parameter</param>
    /// <param name="noLWM">
    ///  When true, does not start async fetch of the next ID block while the current block reaches low-water-mark.
    ///  This may not be desired in some short-lived processes.
    ///  The provider may disregard this flag
    /// </param>
    /// <returns>The new ULONG sequence value</returns>
    ulong GenerateOneSequenceId(string scopeName,
                                string sequenceName,
                                int blockSize = 0,
                                ulong? vicinity = ulong.MaxValue,
                                bool noLWM = false);


    /// <summary>
    /// Tries to generate many consecutive IDs. If the reserved block gets exhausted, then the returned ID count may be less than requested.
    /// </summary>
    /// <param name="scopeName">The name of scope where sequences are kept</param>
    /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
    /// <param name="idCount">How many Consecutive IDs should the system try to reserve</param>
    /// <param name="vicinity">The location on ID counter scale, the issuing authority may disregard this parameter</param>
    /// <param name="noLWM">
    ///  When true, does not start async fetch of the next ID block while the current block reaches low-water-mark.
    ///  This may not be desired in some short-lived processes.
    ///  The provider may disregard this flag
    /// </param>
    /// <returns>The first uniqueID along with the number of Consecutive IDs that the system could allocate which can be less than requested number of IDs</returns>
    ConsecutiveUniqueSequenceIds TryGenerateManyConsecutiveSequenceIds(string scopeName,
                                                                        string sequenceName,
                                                                        int idCount,
                                                                        ulong? vicinity = ulong.MaxValue,
                                                                        bool noLWM = false);
  }

  /// <summary>
  /// Represents an entity that provides unique Global Distributed IDs (GDIDs) via named sequences.
  /// Note: GDID.Zero is never returned as it indicates the absence of a value
  /// </summary>
  public interface IGdidProvider : IUniqueSequenceProvider
  {
    /// <summary>
    /// Generates Globally-Unique distributed ID (GDID) for the supplied sequence name.
    /// Note: do not confuse with block pre-allocation, which is an internal optimization.
    /// Even if 100 IDs are pre-allocated the method returns one unique GDID
    /// </summary>
    /// <param name="scopeName">The name of scope where sequences are kept</param>
    /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
    /// <param name="blockSize">If >0 specifies how many sequence values to pre-allocate, otherwise provider would use its default setting</param>
    /// <param name="vicinity">The location on ID counter scale, the authority may disregard this parameter</param>
    /// <param name="noLWM">
    ///  When true, does not start async fetch of the next ID block while the current block reaches low-water-mark.
    ///  This may not be desired in some short-lived processes.
    ///  The provider may disregard this flag
    /// </param>
    /// <returns>The GDID instance</returns>
    GDID GenerateOneGdid(string scopeName,
                                      string sequenceName,
                                      int blockSize = 0,
                                      ulong? vicinity = GDID.COUNTER_MAX,
                                      bool noLWM = false);


    /// <summary>
    /// Tries to generate many consecutive Globally-Unique distributed ID (GDID) from the same authority for the supplied sequence name.
    /// If the reserved block gets exhausted, then the returned ID array length may be less than requested
    /// </summary>
    /// <param name="scopeName">The name of scope where sequences are kept</param>
    /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
    /// <param name="gdidCount">How many Consecutive GDIDs from the same authority should the system try to reserve</param>
    /// <param name="vicinity">The location on ID counter scale, the authority may disregard this parameter</param>
    /// <param name="noLWM">
    ///  When true, does not start async fetch of the next ID block while the current block reaches low-water-mark.
    ///  This may not be desired in some short-lived processes.
    ///  The provider may disregard this flag
    /// </param>
    /// <returns>The GDID[] instance which may have less elements than requested by gdidCount</returns>
    GDID[] TryGenerateManyConsecutiveGdids(string scopeName,
                                                        string sequenceName,
                                                        int gdidCount,
                                                        ulong? vicinity = GDID.COUNTER_MAX,
                                                        bool noLWM = false);

    /// <summary>
    /// Gets/sets Authority Glue Node for testing. It can only be set once in the testing app container init before the first call to
    ///  Generate is made. When this setting is set then any cluster authority nodes which would have been normally used will be
    ///  completely bypassed during block allocation
    /// </summary>
    string TestingAuthorityNode { get; set;}
  }
}
