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
    /// Gets object of the corresponding collection type by its direct reference
    /// </summary>
    Task<HeapObject> GetObjectAsync(ObjectRef obj, INode node = null);

    /// <summary>
    /// Saves object into the corresponding collection type
    /// </summary>
    Task<SaveResult<ChangeResult>> SetObjectAsync(HeapObject instance, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid), INode node = null);
    Task<SaveResult<ChangeResult>> DeleteAsync(ObjectRef obj, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid), INode node = null);
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
    Task<T> GetAsync(ObjectRef obj, INode node = null);

    /// <summary>
    /// Saves an object instance into the corresponding space type
    /// </summary>
    Task<SaveResult<ChangeResult>> SetAsync(T instance, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid), INode node = null);
  }
}





