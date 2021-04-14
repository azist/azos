/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Data.Heap.Implementation
{
  public abstract class HeapCollection : IHeapCollection
  {
    public IArea Area => throw new NotImplementedException();

    public Type ObjectType => throw new NotImplementedException();

    public HeapAttribute ObjectTypeDefinition => throw new NotImplementedException();

    public Task<SaveResult<ChangeResult>> DeleteAsync(ObjectRef obj, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid))
    {
      throw new NotImplementedException();
    }

    public Task<HeapObject> GetObjectAsync(ObjectRef obj)
    {
      throw new NotImplementedException();
    }

    public Task<SaveResult<ChangeResult>> SetObjectAsync(HeapObject instance, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid))
    {
      throw new NotImplementedException();
    }
  }

  public sealed class HeapCollection<T> : HeapCollection, IHeapCollection<T> where T : HeapObject
  {
    public Task<T> GetAsync(ObjectRef obj)
    {
      throw new NotImplementedException();
    }

    public Task<SaveResult<ChangeResult>> SetAsync(T instance, WriteFlags flags = WriteFlags.None, Guid idempotencyToken = default(Guid))
    {
      throw new NotImplementedException();
    }
  }
}
