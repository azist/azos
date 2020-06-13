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
  /// Provides Monitor thread synchronization functionality over lock objects addressable by key(name).
  /// This class is thread-safe. The internal implementation is based on a fixed-size array of Dictionary objects
  /// to minimize inter-locking. Do not allocate/deallocate this class often, instead allocate
  /// once per service that needs to synchronize by keys and call methods on the instance.
  /// The locks provided by this class are re-entrant by the same thread
  /// </summary>
  public sealed class KeyedMonitor<TKey>
  {
    //lock ticket accessible by TKey; ref counting control lifecycle
    private class _slot
    {
      public _slot() { RefCount = 1; }
      public volatile int RefCount;
    }


    public KeyedMonitor(IEqualityComparer<TKey> comparer = null)
    {
      m_Buckets = new Dictionary<TKey, _slot>[0xff + 1];
      for(var i=0; i< m_Buckets.Length; i++)
        m_Buckets[i] =  comparer !=null ? new Dictionary<TKey, _slot>(comparer) : new Dictionary<TKey, _slot>();
    }

    private Dictionary<TKey, _slot>[] m_Buckets;

    /// <summary>
    /// Executes an action under a lock() taken on TKey value
    /// </summary>
    public void Synchronized(TKey key, Action action)
    {
      Enter(key);
      try
      {
        action();
      }
      finally
      {
        Exit(key);
      }
    }

    /// <summary>
    /// Executes a function under a lock() taken on TKey value
    /// </summary>
    public TResult Synchronized<TResult>(TKey key, Func<TResult> action)
    {
      Enter(key);
      try
      {
        return action();
      }
      finally
      {
        Exit(key);
      }
    }

    /// <summary>
    /// Performs Monitor.Enter() on TKey value.
    /// Unlike TryEnter() this method does block
    /// </summary>
    public void Enter(TKey key)
    {
      var bucket = getBucket(key);
      _slot _lock;
      lock(bucket)
      {
        if (!bucket.TryGetValue(key, out _lock))
        {
          _lock = new _slot();
          Monitor.Enter(_lock);
          bucket.Add(key, _lock);
          return;
        }
        else
          _lock.RefCount++;
      }

      Monitor.Enter(_lock);
    }

    /// <summary>
    /// Tries to perform Monitor.TryEnter() on TKey value.
    /// Returns true when lock was taken. Unlike Enter() this method does not block
    /// </summary>
    public bool TryEnter(TKey key)
    {
      var bucket = getBucket(key);
      _slot _lock;
      lock (bucket)
      {
        if (!bucket.TryGetValue(key, out _lock))
        {
          _lock = new _slot();
          Monitor.Enter(_lock);
          bucket.Add(key, _lock);
          return true;
        }
        var taken = Monitor.TryEnter(_lock);
        if (taken)
        {
          _lock.RefCount++;
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Performs Monitor.Exit() on TKey value. Returns false in cases when lock was not taken which indicates an error
    /// in the calling control flow
    /// </summary>
    public bool Exit(TKey key)
    {
      var bucket = getBucket(key);
      _slot _lock;
      lock (bucket)
      {
        if (!bucket.TryGetValue(key, out _lock)) return false;
        Monitor.Exit(_lock);
        _lock.RefCount--;
        if (_lock.RefCount==0)
          bucket.Remove(key);
      }
      return true;
    }

    private Dictionary<TKey, _slot> getBucket(TKey key)
    {
      var hc = key.GetHashCode();
      return m_Buckets[hc & 0xff];
    }
  }
}

