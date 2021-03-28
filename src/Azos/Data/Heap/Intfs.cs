///*<FILE_LICENSE>
// * Azos (A to Z Application Operating System) Framework
// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
// * See the LICENSE file in the project root for more information.
//</FILE_LICENSE>*/

//using Azos.Collections;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace Azos.Data.Heap
//{

//  public struct ObjectPtr : IEquatable<ObjectPtr>
//  {
//    public object ShardingId;
//    public GDID Id;

//    public bool Equals(ObjectPtr other)
//    {
//      throw new NotImplementedException();
//    }
//  }

//  public interface IHeap : IModule
//  {
//    /// <summary>
//    /// Returns names of database schemas supported by the store.
//    /// Every database instances implements a particular schema
//    /// </summary>
//    IRegistry<ISchema> Schemas { get; }

//  }


//  public interface ISchema
//  {
//    IRegistry<IAreaDef> AreaDefs { get; }

//    IRegistry<IInstance> Instances { get; }
//  }

//  public interface IAreaDef
//  {
//    ISchema Schema { get; }
//  }


//  public interface IInstance
//  {
//    ISchema Schema { get; }
//    IRegistry<IArea> Areas { get; }
//  }


//  public interface IArea
//  {
//    IAreaDef Definition { get; }

//    /// <summary>
//    /// Heap Instance
//    /// </summary>
//    IInstance Instance { get; }

//    IHeapCollection<T> GetCollection<T>() where T : HeapObject;
//  }

//  public interface IHeapCollection<T> where T : HeapObject
//  {
//    IArea Area { get; }

//    Task<T> GetAsync(ObjectPtr ptr);
//    Task<SaveResult<T>> SetAsync(T instance);
//    Task<SaveResult<ObjectVersion>> FreeAsync(ObjectPtr ptr);
//  }
//}



//HeapObject:   TTL

//  ObjectVersion is not needed because area engine may not know how to handle specific replication
//  needed for every object type, therefore those replication-controlling fields a-la CRDT are really needed
//  in the data buffer itself.

//  State-based CRDTs are called convergent replicated data types, or CvRDTs. In contrast to CmRDTs, CvRDTs send their full local state
//  to other replicas, where the states are merged by a function which must be [COMMUTATIVE, ASSOCIATIVE, AND IDEMPOTENT].
//The merge function provides a join for any pair of replica states, so the set of all states forms a semilattice.The update function must monotonically increase the internal state, according to the same partial order rules as the semilattice.

//http://jtfmumm.com/blog/2015/11/17/crdt-primer-1-defanging-order-theory/
//https://lars.hupel.info/topics/crdt/07-deletion/

