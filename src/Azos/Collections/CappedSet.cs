/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;

namespace Azos.Collections
{
  /// <summary>
  /// Implements a set of T with the optional size limit and optional item lifespan limit.
  /// This class is thread-safe and must be disposed in a deterministic way
  /// </summary>
  public sealed class CappedSet<T> : ApplicationComponent, IEnumerable<KeyValuePair<T, DateTime>>
  {
    private class bucket : Dictionary<T, DateTime>
    {
      public bucket(IEqualityComparer<T> comparer) : base(comparer){}
      public bucket(IEnumerable<KeyValuePair<T, DateTime>> collection, IEqualityComparer<T> comparer) : base(collection, comparer) { }
      public DateTime m_LastLock;
      public int m_ApproximateCount;
    }


    #region .ctor

    public CappedSet(IApplication app, IEqualityComparer<T> comparer = null, ICappedSetPersistenceHandler<T> persistence = null) : base(app) => ctor(comparer, persistence);
    public CappedSet(IApplicationComponent director, IEqualityComparer<T> comparer = null, ICappedSetPersistenceHandler<T> persistence = null) : base(director) => ctor(comparer, persistence);

    private void ctor(IEqualityComparer<T> comparer, ICappedSetPersistenceHandler<T> persistence)
    {
      m_Comparer = comparer ?? EqualityComparer<T>.Default;
      m_Data = new bucket[BUCKET_COUNT];
      m_Persistence = persistence;

      for (var i = 0; i < m_Data.Length; i++)
      {
        bucket one = null;
        if (m_Persistence != null)
        {
          var existing = m_Persistence.GetInitBucketData(i);
          one = new bucket(existing, m_Comparer);
        }
        else
        {
          one = new bucket(m_Comparer);
        }

        m_Data[i] = one;
      }//for

      Task.Delay(VISIT_GRANULARITY_MS).ContinueWith(_ => visit());

      Thread.MemoryBarrier();
    }

    #endregion

    #region Fields

    private const int BUCKET_COUNT = 251;
    private const int VISIT_GRANULARITY_MS = 5000;
    private const int MUST_LOCK_BUCKET_SEC = 179;
    private ICappedSetPersistenceHandler<T> m_Persistence;
    private IEqualityComparer<T> m_Comparer;
    private int m_SizeLimit;
    private int m_TimeLimitSec;
    private bucket[] m_Data;

    #endregion

    #region Properties

    public override string ComponentLogTopic => CoreConsts.COLLECTIONS_TOPIC;

    /// <summary>
    /// If set > 0, imposes a limit on the maximum number of entries.
    /// When Count exceeds the limit the set asynchronously caps the total number of entries by
    /// removing the older elements. The limit does not guarantee the instantaneous or exact consistency with the Count property
    /// </summary>
    public int SizeLimit
    {
      get { return m_SizeLimit; }
      set { m_SizeLimit = value > 0 ? value : 0; }
    }

    /// <summary>
    /// If set >0 then evicts the entries older than the specified value.
    /// Does not guarantee the exact consistency of eviction time
    /// </summary>
    public int TimeLimitSec
    {
      get => m_TimeLimitSec;
      set => m_TimeLimitSec = value > 0 ? value : 0;
    }

    /// <summary>
    /// Returns the number of members in the set
    /// </summary>
    public int Count => m_Data.Sum((b) => { lock (b) return b.Count; });

    #endregion

    #region Public

    /// <summary>
    /// Atomic operation which tries to include a member in the set. Returns true if member was included.
    /// </summary>
    public bool Put(T item)
    {
      var (i, data) = getBucket(item);
      lock (data)
      {
        if (data.ContainsKey(item)) return false;
        data.Add(item, App.TimeSource.UTCNow);
        persistOneBucket(true, i, data);
        return true;
      }
    }

    /// <summary>
    /// Atomic operation which tries to get a member data returning true if it was found
    /// </summary>
    public bool Get(T item, out DateTime createDate)
    {
      var (i, data) = getBucket(item);
      lock (data)
        return data.TryGetValue(item, out createDate);
    }

    /// <summary>
    /// Atomic operation which tries to get a member data returning true if it was found
    /// </summary>
    public bool Contains(T item)
    {
      var (_, data) = getBucket(item);
      lock (data)
        return data.ContainsKey(item);
    }

    /// <summary>
    /// Atomic operation which tries to remove and existing member returning true
    /// </summary>
    public bool Remove(T item)
    {
      var (i, data) = getBucket(item);
      lock (data)
      {
        if (!data.Remove(item)) return false;
        persistOneBucket(true, i, data);
      }
      return true;
    }

    /// <summary>
    /// Removes all items
    /// </summary>
    public void Clear()
    {
      for(var i=0; i< m_Data.Length; i++)
      {
        var one = m_Data[i];
        lock(one)
        {
          one.Clear();
          persistOneBucket(true, i, one);
        }
      }
    }

    public IEnumerator<KeyValuePair<T, DateTime>> GetEnumerator() => all.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      => all.GetEnumerator();

    #endregion

    #region .pvt

    private IEnumerable<KeyValuePair<T, DateTime>> all
    {
      get
      {
        for(var i=0; i<m_Data.Length; i++)
        {
          var data = m_Data[i];
          KeyValuePair<T, DateTime>[] copy;
          lock(data)
           copy = data.ToArray();

          foreach(var e in copy)
            yield return e;
        }
      }
    }

    private (int i, bucket d) getBucket(T item)
    {
      var hc = m_Comparer.GetHashCode(item);
      var i = (hc & CoreConsts.ABS_HASH_MASK) % m_Data.Length;
      return (i, m_Data[i]);
    }

    private void persistOneBucket(bool sync, int idxBucket, IEnumerable<KeyValuePair<T, DateTime>> data)
    {
      if (m_Persistence != null) return;

      if (sync != m_Persistence.IsAsync)
      {
        m_Persistence.PersistBucketData(idxBucket, data);
      }
    }

    private void visit()
    {
      if (Disposed) return;
      try
      {
        visitCore();
      }
      catch(Exception error)
      {
        WriteLog(Log.MessageType.Error, nameof(visitCore), "Leaked: "+ error.ToMessageWithType(), error);
      }
      finally
      {
        if (!Disposed)
          Task.Delay(IntUtils.ChangeByRndPct(VISIT_GRANULARITY_MS, 0.25f))
              .ContinueWith( _ => visit() );
      }
    }

    private void visitCore()
    {
      var now = App.TimeSource.UTCNow;
      var toDelete = new HashSet<T>(m_Comparer);


      var ttlSec =  TimeLimitSec;
      var maxCount = SizeLimit;

      var overage = 0;
      if (maxCount>0)
        overage = m_Data.Sum( d => d.m_ApproximateCount ) - maxCount;

      var overagePerBucket = overage / m_Data.Length; //equal distribution is assumed per getBucket()

      for(var i=0; i<m_Data.Length; i++)
      {
        var dict = m_Data[i];
        var mustLock = (now - dict.m_LastLock).TotalSeconds > MUST_LOCK_BUCKET_SEC;

        if (!mustLock)
        {
          if (!Monitor.TryEnter(dict)) continue;
        }
        else Monitor.Enter(dict);
        try
        {
          dict.m_LastLock = now;//lock taken now

          toDelete.Clear();

          if (ttlSec>0) //impose time limit
            dict.Where(kvp => ((now - kvp.Value).TotalSeconds > ttlSec))
                .ForEach( kvp => toDelete.Add(kvp.Key));

          //impose count limit
          if (overagePerBucket > toDelete.Count)
          {
            var timeSorted = dict.OrderBy(kvp => kvp.Value);//ascending = older first
            foreach(var item in timeSorted)
             if (overagePerBucket>toDelete.Count)
              toDelete.Add( item.Key );
             else
              break;
          }

          toDelete.ForEach( k => dict.Remove(k) );
          dict.m_ApproximateCount = dict.Count;

          persistOneBucket(false, i, dict);
        }
        finally
        {
          Monitor.Exit(dict);
        }
      }
    }//visitCore

    #endregion
  }
}
