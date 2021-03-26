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
//     public object ShardingId;
//     public GDID Id;

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
//    IRegistry<IArea> Areas{ get; }
//  }


//  public interface IArea
//  {
//    IAreaDef Definition {  get; }

//    /// <summary>
//    /// Heap Instance
//    /// </summary>
//    IInstance Instance { get; }

//    IHeapCollection<T> GetCollection<T>() where T : HeapObject;
//  }

//  public interface IHeapCollection<T> where T : HeapObject
//  {
//    IArea Area {  get; }

//    Task<T> GetAsync(ObjectPtr ptr);
//    Task<SaveResult<T>> SetAsync(T instance);
//    Task<SaveResult<ObjectVersion>> FreeAsync(ObjectPtr ptr);
//  }
//}
