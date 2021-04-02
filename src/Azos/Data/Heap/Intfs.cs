/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Collections;

namespace Azos.Data.Heap
{

  public struct ObjectPtr : IEquatable<ObjectPtr>
  {
    public object ShardingId;
    public GDID Id;

    public bool Equals(ObjectPtr other)
    {
      throw new NotImplementedException();
    }
  }

  /// <summary>
  /// Represents a globally-distributed replicated heap of eventually consistent instances of CvRDTs
  /// (Convergent Replicated Data Types)
  /// </summary>
  public interface IHeap : IModule
  {
    /// <summary>
    /// Registry of areas of the heap
    /// </summary>
    IRegistry<IArea> Areas { get; }
  }


  /// <summary>
  /// Represents data heap server node
  /// </summary>
  public interface IHeapNode
  {
    /// <summary>
    /// Globally-unique cluster node identifier. The system currently supports up to 64 total nodes with IDs 1 .. 64. All other ids are invalid.
    /// You can use this value efficiently (fits in CPU register) in vector clocks
    /// and other structures. Every HeapObject instance gets Sys_VerNode stamp by the server
    /// </summary>
    byte NodeId     { get; }

    /// <summary>
    /// Cluster host name, such as sky regional catalog path e.g. /world/us/east/cle/db/z1/lmed002.h
    /// </summary>
    string HostName { get; }

    /// <summary>
    /// The high precision Universal Time Coordinate(d)
    /// </summary>
    DateTime UtcNow { get; }
  }


  public interface IArea : INamed
  {
    IHeap Heap { get; }

    /// <summary>
    /// Returns an enumerable of all types HeapObjects-derived types supported by this heap area instance
    /// </summary>
    IEnumerable<Type> ObjectTypes{ get; }

    /// <summary>
    /// Returns an enumerable of all types AreaQuery-derived types supported by this heap area instance
    /// </summary>
    IEnumerable<Type> QueryTypes { get; }

    /// <summary>
    /// Returns a heap object collection for the specified type
    /// </summary>
    IHeapCollection GetCollection(Type tObject);

    /// <summary>
    /// Returns a heap object collection of the specified type
    /// </summary>
    IHeapCollection<T> GetCollection<T>() where T : HeapObject;

   // Task<object> ExecuteQueryAsync(AreaQuery query);
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

    Task<HeapObject> GetObjectAsync(ObjectPtr ptr);
    Task<SaveResult<HeapObject>> SetAsync(HeapObject instance);
    Task<SaveResult<ObjectVersion>> DeleteAsync(ObjectPtr ptr);
  }

  /// <summary>
  /// Provides functionality for working with collections of HeapObject-derivatives of the specified type
  /// </summary>
  public interface IHeapCollection<T> : IHeapCollection where T : HeapObject
  {
    Task<T> GetAsync(ObjectPtr ptr);
    Task<SaveResult<T>> SetAsync(T instance);
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

