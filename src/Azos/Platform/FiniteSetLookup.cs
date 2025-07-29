﻿/*<FILE_LICENSE>
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
  /// Provides a thread-safe lock-free lookup by TKey of a finite(constrained by design to 512K) set of TValue items, such as system types.
  /// The set is constrained by design in two ways: it can only add items if they are not present in the set already,
  /// and the total item count must be finite/limited by system design (e.g. system Type instances, Attribute instances etc.), 512K is the hard maximum count limit.
  /// This class is used for efficient thread-safe caching of items by TKey, speculating on the fact that total set
  /// of items is limited and with time the system will add all of items, consequently no mutations will be necessary
  /// and all requests will be served by lock-free thread-safe read-only Get() operation on an immutable copy.
  /// This class is inefficient for adding items which do not exist in the set, as it creates yet another copy of the internal dictionary.
  /// The AddItem functor is called under the lock and must not have any side effects (must contain only read code which gets TValue by TKey from source).
  /// </summary>
  /// <typeparam name="TKey">Type of key to lookup-by</typeparam>
  /// <typeparam name="TValue">Type of value to map to key</typeparam>
  /// <remarks>
  /// This is an advanced class which should rarely be used by business app developers.
  /// This is a high performance data structure which is optimized specifically for read-heavy storage of a LIMITED set of system (e.g. Types)
  /// objects with a maximum object count hard limit set to 512K.
  /// DO NOT store business data which changes or has high value variability.
  /// </remarks>
  public sealed class FiniteSetLookup<TKey, TValue>
  {
    /// <summary>
    /// Absolute maximum count of entries which this instance can contain.
    /// If you ever get an error message below, you have misused this class as it may never contain such a great number of entries
    /// </summary>
    public const int MAX_ENTRIES = 512_000;
    private static readonly string MAX_ENTRIES_CONDITION = $"{typeof(FiniteSetLookup<TKey, TValue>).DisplayNameWithExpandedGenericArgs()}.Count < MAX_ENTRIES({MAX_ENTRIES}) by design";


    private object m_Lock = new object();
    private volatile Dictionary<TKey, TValue> m_Data;

    /// <summary>
    /// Creates an instance of FiniteSetLookup. The instance is thread safe and it may be beneficial
    /// to allocate it as a static field
    /// </summary>
    /// <param name="addItem">
    /// References the functor which gets called on addition of items when their keys do not exist in the set yet.
    /// The functor is invoked under the lock and must not have any side-effects and be relatively efficient (must not have any long (50ms+) blocking code)
    /// </param>
    /// <param name="comparer">
    /// Optional Key equality comparer
    /// </param>
    public FiniteSetLookup(Func<TKey, TValue> addItem, IEqualityComparer<TKey> comparer = null)
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
    public TValue Get(TKey key) => GetWithFlag(key).value;

    /// <summary>
    /// Tries to find a TValue by key in the set. If TKey is not present, then invokes a AddItem function
    /// </summary>
    public (TValue value, bool preExisted) GetWithFlag(TKey key)
    {
      if (m_Data.TryGetValue(key, out var result)) return (result, true); //lock free lookup

      lock(m_Lock)
      {
        if (m_Data.TryGetValue(key, out result)) return (result, true);//2nd check is under the lock

        (m_Data.Count < MAX_ENTRIES).IsTrue(MAX_ENTRIES_CONDITION);

        result = AddItem(key);

        var cache = new Dictionary<TKey, TValue>(m_Data, m_Data.Comparer);
        cache[key] = result;
        Thread.MemoryBarrier();
        m_Data = cache;//atomic
      }

      return (result, false);
    }

    /// <summary>
    /// Removes one keyed item returning true if key was found and removed.
    /// This should be used in advanced cases only
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
    /// Deletes all data at once.
    /// This should be used in advanced cases only
    /// </summary>
    public void Purge()
    {
      var data = new Dictionary<TKey, TValue>(m_Data.Comparer);
      Thread.MemoryBarrier();
      m_Data = data;
    }

  }
}
