/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading;

namespace Azos.Platform
{
  /// <summary>
  /// Provides a thread-safe lock-free lookup by TKey of a constrained set of TValue items, such as system types.
  /// The set is constrained by design in two ways: it can only add items if they are not present in the set already,
  /// and the total item count must be limited by system design (e.g. system Type instances, Attribute instances etc.).
  /// This class is used for efficient thread-safe caching of items by TKey, speculating on the fact that total set
  /// of items is limited and with time the system will add all of items, consequently no mutations will be necessary
  /// and all requests will be served by lock-free thread-safe read-only Get() operation on an immutable copy.
  /// This class is inefficient for adding items which do not exist in the set, as it creates yet another copy of internal dictionary.
  /// The AddItem functor is called under the lock and must not have any side effects (must contain only read code which gets TValue by TKey from source).
  /// </summary>
  /// <typeparam name="TKey">Type of key to lookup-by</typeparam>
  /// <typeparam name="TValue">Type of value to map to key</typeparam>
  /// <remarks>This is an advanced class which should rarely be used by business app developers</remarks>
  public sealed class ConstrainedSetLookup<TKey, TValue>
  {
    private object m_Lock = new object();
    private volatile Dictionary<TKey, TValue> m_Data;

    /// <summary>
    /// Creates an instance of ConstrainedSetLookup. The instance is thread safe and it may be beneficial
    /// to allocate it as a static field
    /// </summary>
    /// <param name="addItem">
    /// References the functor which gets called on addition of items when their keys do not exist in the set yet.
    /// The functor is invoked under the lock and must not have any side-effects and be relatively efficient (must not have any long (50ms+) blocking code)
    /// </param>
    /// <param name="comparer">
    /// Optional Key equality comparer
    /// </param>
    public ConstrainedSetLookup(Func<TKey, TValue> addItem, IEqualityComparer<TKey> comparer = null)
    {
      AddItem = addItem.NonNull(nameof(addItem));
      m_Data = comparer == null ? new Dictionary<TKey, TValue>() : m_Data = new Dictionary<TKey, TValue>(comparer);
      Thread.MemoryBarrier();
    }

    /// <summary>
    /// References the functor which gets called on addition of items when their keys do not exist in the set yet.
    /// The functor is invoked under the lock and must not have any side-effects and be relatively efficient (must not have any long (50ms+) blocking code)
    /// </summary>
    public readonly Func<TKey, TValue> AddItem;

    /// <summary>
    /// Tries to find a TValue by key in the set. If TKey is not present, then invokes a AddItem function
    /// </summary>
    public TValue this[TKey key] => Get(key);

    /// <summary>
    /// Returns snapshot count at this moment
    /// </summary>
    public int Count => m_Data.Count;

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

        var cache = new Dictionary<TKey, TValue>(m_Data, m_Data.Comparer);
        cache[key] = result;
        Thread.MemoryBarrier();
        m_Data = cache;//atomic
      }

      return result;
    }

    /// <summary>
    /// Removes one keyed item returning true if key was found and removed
    /// </summary>
    public bool Remove(TKey key)
    {
      lock(m_Lock)
      {
        if (!m_Data.ContainsKey(key)) return false;
        var cache = new Dictionary<TKey, TValue>(m_Data, m_Data.Comparer);
        cache.Remove(key);
        Thread.MemoryBarrier();
        m_Data = cache;//atomic
      }
      return true;
    }

    /// <summary>
    /// Deletes all data at once
    /// </summary>
    public void Purge()
    {
      var data = new Dictionary<TKey, TValue>(m_Data.Comparer);
      Thread.MemoryBarrier();
      m_Data = data;
    }

  }
}
