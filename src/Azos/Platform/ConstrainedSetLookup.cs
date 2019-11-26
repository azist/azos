using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Azos.Platform
{
  /// <summary>
  /// Provides a thread-safe lock-free lookup by TKey of a constrained set of TValue items, such as system types.
  /// The set is constrained by design in two ways: it can only add items if they are not present in the set already,
  /// and the total item count must be limited by system design (e.g. system Type instances, Attribute instances etc.).
  /// This class is used for efficient thread-safe caching of items by TKey, speculating on the fact that total set
  /// of items is limited and with time the system will add all of items, consequently no mutations will be necessary
  /// and all requests will be served by lock-free thread-safe read-only Get() operation on and immutable copy.
  /// This class is inefficient for adding items which do not exist in the set, as it creates yet another copy of internal dictionary.
  /// The AddItem functor is called under the lock and must not have any side effects (must contain only read code which gets TValue by TKey from source).
  /// </summary>
  /// <typeparam name="TKey">Type of key to lookup-by</typeparam>
  /// <typeparam name="TValue">Type of value to map to key</typeparam>
  /// <remarks>This is an advanced class which should rarely be used by business app developers</remarks>
  public sealed class ConstrainedSetLookup<TKey, TValue>
  {
    private object m_Lock = new object();
    private volatile Dictionary<TKey, TValue> m_Data = new Dictionary<TKey, TValue>();

    public ConstrainedSetLookup(Func<TKey, TValue> addItem) => AddItem = addItem.NonNull(nameof(addItem));

    /// <summary>
    /// References the functor which gets called on addition of items when their keys do not exist in the set yet.
    /// The functor is invoked under the lock and must not have any side-effects and be relatively efficient (must not have any long (50ms+) blocking code)
    /// </summary>
    public readonly Func<TKey, TValue> AddItem;

    /// <summary>
    /// Tries to find a TValue by key in the set. If TKey is not present, then invokes a AddItem function
    /// </summary>
    public TValue Get(TKey key)
    {
      if (m_Data.TryGetValue(key, out var result)) return result; //lock free lookup

      lock(m_Lock)
      {
        if (m_Data.TryGetValue(key, out result)) return result;//2nd check is under the lock

        result = AddItem(key);

        var cache = new Dictionary<TKey, TValue>(m_Data);
        cache[key] = result;
        Thread.MemoryBarrier();
        m_Data = cache;//atomic
      }

      return result;
    }

    /// <summary>
    /// Tries to find a TValue by key in the set. If TKey is not present, then invokes a AddItem function
    /// </summary>
    public TValue this[TKey key] => Get(key);
  }
}
