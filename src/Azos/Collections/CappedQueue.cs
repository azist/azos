using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Azos.Conf;

namespace Azos.Collections
{
  /// <summary>
  /// Represents a thread-safe concurrent queue of T with limits imposed on total count and
  /// approximated total size of all items in relative units. Once the limit/s is/are reached a queue can either
  /// discard old or new messages or throw exception.
  /// This class can be used as a base building block for flow control.
  /// The class itself is Disposable which is a form of cancellation token
  /// </summary>
  /// <typeparam name="T">Type of Item in a queue</typeparam>
  /// <remarks>
  /// The class is thread-safe, however imposed limits are approximated between threads and are never exact.
  /// This is expected by design as the limits are necessary to guarantee system stability in general.
  /// For example, you may place a limit of 1,000 total items, but a queue might have 1000+(cpu count) items (e.g. 1004) at any given time
  /// before it starts to shrink. This is an expected behavior as the queue guarantees only an eventual relative limit enforcement in general.
  /// </remarks>
  public sealed class CappedQueue<T>
  {
    public enum LimitHandling { LoseOld = 0, LoseOverflow, Throw }

    public CappedQueue(Func<T, long> getItemSize, Action<T> lostItem = null)
    {
      m_GetItemSize = getItemSize.NonNull(nameof(getItemSize));
      m_LostItem = lostItem;
    }

    private Func<T, long> m_GetItemSize;
    private Action<T> m_LostItem;
    private ConcurrentQueue<T> m_Data = new ConcurrentQueue<T>();
    private int m_EnqueuedCount;
    private long m_EnqueuedSize;

    /// <summary>
    /// Returns the a total count of enqueued items
    /// </summary>
    public int EnqueuedCount => m_EnqueuedCount;

    /// <summary>
    /// Returns a total size of all enqueued messages in the relative units (e.g. character counts or byte sizes)
    /// </summary>
    public long EnqueuedSize => m_EnqueuedSize;


    /// <summary>
    /// Specifies how to handle the case when limit is reached: Lose old data, lose new (over the limit) data, or throw
    /// </summary>
    [Config] public LimitHandling Handling { get; set; }

    /// <summary>
    /// When set to a number &gt;0 imposes a limit on the count of items in this queue
    /// </summary>
    [Config] public int CountLimit{  get; set; }

    /// <summary>
    /// Whens set to a value &gt;0 imposes a limit on the total size of all items in this queue.
    /// The size is measured in relative units (see GetSize(T) override)
    /// </summary>
    [Config] public long SizeLimit{  get; set; }

    /// <summary>
    ///  Tries to enqueue an item in the queue, depending on the set limits and Handling property,
    ///  the operation may either succeed, partially succeed causing removal of older data or fail either with exception
    ///  or returning false
    /// </summary>
    /// <param name="item">Item to enqueue</param>
    /// <returns>True when item was enqueued</returns>
    public bool TryEnqueue(T item)
    {
      var maxCount = CountLimit;
      var maxSize = SizeLimit;

      if (
           (maxCount > 0 && Thread.VolatileRead(ref m_EnqueuedCount) >= maxCount) ||
           (maxSize  > 0 && Thread.VolatileRead(ref m_EnqueuedSize)  >= maxSize)
         )
      {
        if (Handling==LimitHandling.LoseOld)
          trimOldExcess();
        else if (Handling==LimitHandling.Throw)
          throw new AzosException(StringConsts.COLLECTION_CAPPED_QUEUE_LIMIT_ERROR.Args(nameof(CappedQueue<T>), Handling));
        else return false;//LoseOverflow
      }

      var sz = m_GetItemSize(item);
      Interlocked.Increment(ref m_EnqueuedCount);
      Interlocked.Add(ref m_EnqueuedSize, sz);

      m_Data.Enqueue(item);
      return true;
    }


    public bool TryDequeue(out T item)
    {
      var result = m_Data.TryDequeue(out item);
      if (result)
      {
        var sz = m_GetItemSize(item);
        Interlocked.Decrement(ref m_EnqueuedCount);
        Interlocked.Add(ref m_EnqueuedSize, -sz);
      }
      return result;
    }

    public bool TryPeek(out T item) => m_Data.TryPeek(out item);


    private void trimOldExcess()
    {
      const int MAX_ITERATIONS = 16;
      var maxCount = CountLimit;
      var maxSize = SizeLimit;

      for (var i = 0; i < MAX_ITERATIONS &&
              (
                (maxCount > 0 && Thread.VolatileRead(ref m_EnqueuedCount) >= maxCount) ||
                (maxSize > 0 && Thread.VolatileRead(ref m_EnqueuedSize) >= maxSize)
              );
              i++
          )
      {
        if (!m_Data.TryDequeue(out var lost)) break;
        var sz = m_GetItemSize(lost);
        Interlocked.Decrement(ref m_EnqueuedCount);
        Interlocked.Add(ref m_EnqueuedSize, -sz);

        m_LostItem?.Invoke(lost);
      }
    }

  }
}
