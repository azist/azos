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
  /// Provides functionality for working with heap object spaces - collections of objects of the specified type.
  /// Terminology comes from n-dimensional feature spaces, having `n` represent number of dimensions (e.g. "SSN, LastName"), since
  /// the same object types share the same features (via their declaration type) it makes sense to co-locate those objects in the same
  /// n-dimensional feature space
  /// </summary>
  public interface ISpace : INamed
  {
    /// <summary>
    /// Area of the heap
    /// </summary>
    IArea Area { get; }

    /// <summary>
    /// Returns the type which this instance represents
    /// </summary>
    Type ObjectType { get; }

    /// <summary>
    /// Returns the definition of the object type - its [HeapAttribute]instance
    /// which defines the binding of the CLI type to the logical heap area/collection
    /// </summary>
    HeapSpaceAttribute SpaceDefinition { get; }

    /// <summary>
    /// Allocates an RGDID address for a new root `HeapObject` to use for insert/create.
    /// Root objects act as a "briefcase" handle - they group together child/subordinate objects on the same physical
    /// host to preserve LOCALITY of data reference, effectively forming an object cluster.
    /// Root objects are a kin to domain aggregate roots in DDD or "apexes".
    /// The second parameter is a relative object graph size expressed in kilobytes, which helps the system
    /// to route the new insert into the best possible physical storage such as a data shard.
    /// The roots' subordinate/child objects in the graph would then call <see cref="AllocateNewObjectAddress(uint)"/> passing
    /// the route property of the RGDID allocated for the root here, thus child object cluster around their root objects in logical "briefcases"
    /// on the same physical host.
    /// </summary>
    /// <param name="estimatedObjectGraphSize">
    ///  Relative object graph size in assumed bytes estimated to be necesary for storage of this and clustered subordinate/detail/child objects.
    ///  This is a relative estimated value. For example, given a `Professor` root object, with `Students` and `Classes`,
    ///  we can anticipate 500 bytes relative weight of each record, knowing that an average professor has 50 students, we can assume 50k bytes
    ///  estimated graph size for students and classes.
    /// </param>
    /// <returns>Newly allocated RGDID object address</returns>
    /// <remarks>
    /// If you fail to use the returned value, it is lost which is not a big deal as 2^96 is a pretty large number, however
    /// your use pattern may not purposely lose thousands of values which is a bad design.
    /// </remarks>
    RGDID AllocateNewRootObjectAddress(int estimatedObjectGraphSize);

    /// <summary>
    /// Allocates a new `GDID` using existing route - this is needed for subordinate child objects which cluster
    /// around the graph root object
    /// </summary>
    /// <param name="rootObjectRoute">The existing root object route</param>
    /// <remarks>
    /// Newly allocated GDID with the Route equal to the exisitng one of the root.
    /// If you fail to use the returned value, it is lost which is not a big deal as 2^96 is a pretty large number, however
    /// your use pattern may not purposely lose thousands of values which is a bad design.
    /// </remarks>
    RGDID AllocateNewObjectAddress(uint rootObjectRoute);



//caching???????????????? cancellation  ????

    /// <summary>
    /// Gets object of the corresponding collection type by its direct reference
    /// </summary>
    Task<HeapObject> GetObjectAsync(RGDID id, INode node = null);

    /// <summary>
    /// Metrializes attached objects
    /// </summary>
//    Task MaterializeAttachedAsync(ObjectRef obj, Attached[] attached, INode node = null);

    /// <summary>
    /// Saves object into the corresponding collection type
    /// </summary>
    Task<SaveResult<ChangeResult>> SetObjectAsync(HeapObject instance, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid), INode node = null);
    Task<SaveResult<ChangeResult>> DeleteAsync(RGDID id, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid), INode node = null);
  }

  /// <summary>
  /// Provides functionality for working with heap object spaces of the specified type - collections of objects of the specified type.
  /// Terminology comes from n-dimensional feature spaces, having `n` represent number of dimensions (e.g. "SSN, LastName"), since
  /// the same object types share the same features (via their declaration type) it makes sense to co-locate those objects in the same
  /// n-dimensional feature space
  /// </summary>
  public interface ISpace<T> : ISpace where T : HeapObject
  {
    /// <summary>
    /// Gets object of type T by its direct reference
    /// </summary>
    Task<T> GetAsync(RGDID id, INode node = null);

    /// <summary>
    /// Saves an object instance into the corresponding space type
    /// </summary>
    Task<SaveResult<ChangeResult>> SetAsync(T instance, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid), INode node = null);
  }
}





