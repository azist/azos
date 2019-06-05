using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.IO
{
  /// <summary>
  /// Delimits a sub-array (a chunk) of the specified size at the beginning of the target array.
  /// Unlike ArraySegment, this is a mutable class (to avoid boxing of multiple instances) where
  /// instance can be-reused to delimit typically a byte[]. The sub-array purposely always starts at index zero of
  /// the source array. This class is used for optimization of low-level memory access, such as the one used in Pile
  /// not to re-allocate buffers. It allows for memory re-use.
  /// </summary>
  public sealed class Subarray<T>
  {
    /// <summary>
    /// The source array which holds the data for this sub-array
    /// </summary>
    public T[] Array { get; private set; }

    /// <summary>
    /// The size of this sub-array in the source Array. The size is ALWAYS relative to the very first element of the source array
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Sets the state of this instance. This method is used to re-use the single sub-array instance with multiple source arrays,
    /// as it saves on extra GC allocations in places where payload is taken as `object` and ArraySegment would have created boxing instance
    /// </summary>
    public void Set(T[] array, int length)
    {
      Array = array.NonNull(nameof(array));

      if (length < 0) throw new AzosIOException("{0}.Set(length={1} <0)".Args(GetType().DisplayNameWithExpandedGenericArgs(), length));
      if (length > array.Length)
        throw new AzosIOException("{0}.Set(length={1} > array[{2}])".Args(GetType().DisplayNameWithExpandedGenericArgs(), length, array.Length));

      Length = length;
    }
  }
}
