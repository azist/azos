/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Various utilities and extension methods
  /// </summary>
  public static class Extensions
  {
    /// <summary>
    /// Returns a typed space for objects of the specified type
    /// </summary>
    public static ISpace<T> GetSpace<T>(this IHeap heap) where T : HeapObject
    {
       var atr = HeapAttribute.Lookup<HeapSpaceAttribute>(typeof(T));
       var area = heap.NonNull(nameof(heap))[atr.Area];
       return area.GetSpace<T>();
    }

    public static async Task<T> GetAsync<T>(this IHeap heap, ObjectRef obj, INode node = null) where T : HeapObject
      => await heap.GetSpace<T>().GetAsync(obj, node);


    public static async Task<SaveResult<ChangeResult>> SetAsync<T>(this IHeap heap,
                                                                   T instance,
                                                                   WriteFlags flags = WriteFlags.None,
                                                                   Guid idempotencyToken = default(Guid),
                                                                   INode node = null) where T : HeapObject
      => await heap.GetSpace<T>().SetAsync(instance, flags, idempotencyToken, node);


    public static async Task<SaveResult<ChangeResult>> DeleteAsync<T>(this IHeap heap,
                                                                      ObjectRef obj,
                                                                      WriteFlags flags = WriteFlags.None,
                                                                      Guid idempotencyToken = default(Guid),
                                                                      INode node = null) where T : HeapObject
      => await heap.GetSpace<T>().DeleteAsync(obj, flags, idempotencyToken, node);


    public static async Task<SaveResult<object>> ExecResultAsync(this IHeap heap, HeapRequest query, Guid idempotencyToken = default(Guid), INode node = null)
    {
      var atr = HeapAttribute.Lookup<HeapProcAttribute>(query.NonNull(nameof(query)).GetType());
      var area = heap.NonNull(nameof(heap))[atr.Area];
      return await area.ExecuteAsync(query, idempotencyToken, node);
    }

    public static async Task<T> ExecAsync<T>(this IHeap heap, HeapRequest<T> query, Guid idempotencyToken = default(Guid), INode node = null)
      => (await ExecResultAsync(heap, query, idempotencyToken, node)).GetResult().CastTo<T>();
  }
}
