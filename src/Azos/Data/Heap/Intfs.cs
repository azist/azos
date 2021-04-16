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

namespace Azos.Data.Heap
{
  /// <summary>
  /// Represents a globally-distributed replicated heap of eventually consistent instances of CvRDTs
  /// (Convergent Replicated Data Types)
  /// </summary>
  public interface IHeap : IApplicationComponent
  {
    /// <summary>
    /// Registry of areas of the heap. Each area is backed by a storage engine
    /// and has its own partitioning(sharding) rules
    /// </summary>
    IRegistry<IArea> Areas { get; }
  }

  /// <summary>
  /// Denotes IHeap implementation logic module
  /// </summary>
  public interface IHeapLogic : IHeap , IModule
  {
  }

  /// <summary>
  /// Represents data heap server node
  /// </summary>
  public interface IServerNode
  {
    /// <summary>
    /// Globally-unique cluster node identifier. Every HeapObject instance gets Sys_VerNode stamp by the server
    /// </summary>
    Atom NodeId     { get; }

    /// <summary>
    /// Cluster host name, such as sky regional catalog path e.g. `/world/us/east/cle/db/z1/lmed002.h`
    /// </summary>
    string HostName { get; }

    /// <summary>
    /// The high precision Universal Time Coordinate(d)
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Returns the data partitioning handler which performs shard routing of heap data
    /// </summary>
    Router Sharding { get; }

    //todo: Storage backend - each node may have its own storage engine, e.g. we can have a node which is used only as backup/archive
    //so it does no take writes or queries, but only replicates data to a file-based log locally
    //this should probably go to more detailed server interface
  }

  /// <summary>
  /// Defines a logical division/area/namespace of the data heap.
  /// Each area has its own set of nodes which service it.
  /// All nodes must be EVENTUALLY deployed same assemblies.
  /// The replication is paused until all nodes run assemblies with the same BUILD-INFO,
  /// otherwise the data may not eventually converge
  /// </summary>
  public interface IArea : INamed
  {
    /// <summary>
    /// Directing heap
    /// </summary>
    IHeap Heap { get; }

    /// <summary>
    /// Returns an enumerable of all types HeapObjects-derived types supported by this heap area instance
    /// </summary>
    IEnumerable<Type> ObjectTypes{ get; }

    /// <summary>
    /// Returns an enumerable of all types AreaQuery-derived types supported by this heap area instance
    /// </summary>
    IEnumerable<Type> QueryTypes { get; }

    //Ordered by proximity? Primary/secondary etc...
    //IEnumerable<INode> Nodes{ get; }
    //And then Get Collection and Exec query go into INode?


    /// <summary>
    /// Returns a heap collection for the specified object type
    /// </summary>
    IHeapCollection GetCollection(Type tObject);

    /// <summary>
    /// Returns a heap collection for the specified object type
    /// </summary>
    IHeapCollection<T> GetCollection<T>() where T : HeapObject;

    /// <summary>
    /// Executes a query in this area
    /// </summary>
    /// <param name="query">Query object to execute</param>
    /// <returns>Returns result of polymorphic type (e.g. an enumerable of data documents) </returns>
    Task<object> ExecuteQueryAsync(AreaQuery query);
  }

  [Flags]
  public enum WriteFlags
  {
    None = 0,

    /// <summary> Write one more copy into the mirrors/backup locations. This increases safety in case of immediate node loss at the expense of time </summary>
    Backup = 1,

    /// <summary> Do not wait for operation to complete, e.g. post mutation to queue and return ASAP </summary>
    Async = 1 << 30,

    /// <summary> Flush write to disk if possible </summary>
    Flush = 1 << 31
  }

  /// <summary>
  /// Provides functionality for working with collections of HeapObject-derivatives of the specified type
  /// </summary>
  public interface IHeapCollection
  {
    IArea Area { get; }

    /// <summary>
    /// Returns the type which this instance represents
    /// </summary>
    Type ObjectType { get; }

    /// <summary>
    /// Returns the definition of the object type - its [HeapAttribute]instance
    /// which defines the binding of the CLI type to the logical heap area/collection
    /// </summary>
    HeapAttribute ObjectTypeDefinition { get; }

    /// <summary>
    /// Gets object of the corresponding collection type by its direct reference
    /// </summary>
    Task<HeapObject> GetObjectAsync(ObjectRef obj);

    /// <summary>
    /// Saves object into the corresponding collection type
    /// </summary>
    Task<SaveResult<ChangeResult>> SetObjectAsync(HeapObject instance, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid));
    Task<SaveResult<ChangeResult>> DeleteAsync(ObjectRef obj, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid));
  }

  /// <summary>
  /// Provides functionality for working with collections of HeapObject-derivatives of the specified type
  /// </summary>
  public interface IHeapCollection<T> : IHeapCollection where T : HeapObject
  {
    /// <summary>
    /// Gets object of type T by its direct reference
    /// </summary>
    Task<T> GetAsync(ObjectRef obj);

    /// <summary>
    /// Saves object into the corresponding collection type
    /// </summary>
    Task<SaveResult<ChangeResult>> SetAsync(T instance, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid));
  }
}



////HeapObject:   TTL

////  ObjectVersion is not needed because area engine may not know how to handle specific replication
////  needed for every object type, therefore those replication-controlling fields a-la CRDT are really needed
////  in the data buffer itself.

////  State-based CRDTs are called convergent replicated data types, or CvRDTs. In contrast to CmRDTs, CvRDTs send their full local state
////  to other replicas, where the states are merged by a function which must be[COMMUTATIVE, ASSOCIATIVE, AND IDEMPOTENT].
////The merge function provides a join for any pair of replica states, so the set of all states forms a semilattice.The update function must monotonically increase the internal state, according to the same partial order rules as the semilattice.

////http://jtfmumm.com/blog/2015/11/17/crdt-primer-1-defanging-order-theory/
////https://lars.hupel.info/topics/crdt/07-deletion/

