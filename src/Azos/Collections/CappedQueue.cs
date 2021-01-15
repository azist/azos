using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Azos.Conf;

namespace Azos.Collections
{
    /// <summary>
    /// Designates modes of operation when queue exceeds the set limit
    /// </summary>
    public enum QueueLimitHandling
    {
      /// <summary>
      /// Dequeue and discard the old data at the head to make enough space for the new data which is being inserted at the tail
      /// </summary>
      DiscardOld = 0,

      /// <summary>
      /// Discard the new data which is being inserted at the tail, keep the already queued head as-is
      /// </summary>
      DiscardNew,

      /// <summary>
      /// Throw an error when the new data does not fit into the queue
      /// </summary>
      Throw
    }

  /// <summary>
  /// Represents a thread-safe concurrent queue of T with limits imposed on the total count and
  /// an approximated total size of all items expressed in relative units. Once the limit/s is/are reached a queue can either
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
  public sealed class CappedQueue<T> : IEnumerable<T>
  {

    /// <summary>
    /// Creates an instance of a thread-safe concurrent queue of T with limits imposed on the total count and
    /// an approximated total size of all items expressed in relative units. Once the limit/s is/are reached a queue can either
    /// discard old or new messages or throw exception.
    /// </summary>
    /// <param name="getItemSize">Required functor which computes the size of queue item. Keep in mind that for reference types it can be null</param>
    /// <param name="discardedItem">Optional functor which is called for every item discarded at the queue head (Handling=DiscardOld)</param>
    public CappedQueue(Func<T, long> getItemSize, Action<T> discardedItem = null)
    {
      m_GetItemSize = getItemSize.NonNull(nameof(getItemSize));
      m_DiscardedItem = discardedItem;
    }


    private Func<T, long> m_GetItemSize;
    private Action<T> m_DiscardedItem;
    private ConcurrentQueue<T> m_Data = new ConcurrentQueue<T>();
    private int m_EnqueuedCount;
    private long m_EnqueuedSize;


    public IEnumerator<T> GetEnumerator()   => m_Data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_Data.GetEnumerator();

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
    [Config] public QueueLimitHandling Handling { get; set; }

    /// <summary>
    /// When set to a number &gt;0 imposes a limit on the count of items in this queue
    /// </summary>
    [Config] public int CountLimit{  get; set; }

    /// <summary>
    /// When set to a value &gt;0 imposes a limit on the total size of all items in this queue.
    /// The size is measured in relative units (see GetSize(T) override)
    /// </summary>
    [Config] public long SizeLimit{  get; set; }


    /// <summary>
    /// Gets total count directly from the queue
    /// </summary>
    public int Count => m_Data.Count;

    /// <summary>
    ///  Tries to enqueue an item in the queue, depending on the set limits and the Handling property,
    ///  the operation may either succeed, partially succeed causing removal of the older data or fail either with an exception
    ///  or just return false
    /// </summary>
    /// <param name="item">Item to enqueue</param>
    /// <returns>True when item was enqueued; throws if set to QueueLimitHandling.Throw</returns>
    public bool TryEnqueue(T item)
    {
      var maxCount = CountLimit;
      var maxSize = SizeLimit;

      if (
           (maxCount > 0 && Thread.VolatileRead(ref m_EnqueuedCount) >= maxCount) ||
           (maxSize  > 0 && Thread.VolatileRead(ref m_EnqueuedSize)  >= maxSize)
         )
      {
        if (Handling==QueueLimitHandling.DiscardOld)
          trimOldExcess();
        else if (Handling==QueueLimitHandling.Throw)
          throw new AzosException(StringConsts.COLLECTION_CAPPED_QUEUE_LIMIT_ERROR.Args(nameof(CappedQueue<T>), Handling));
        else return false;//DiscardNew
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

    //this is needed to prevent thread stall
    //if one thread gets to trimOldExcess and another one keeps adding, the first thread may never exit the delete loop
    //it will exit the loop after MAX iterations
    private static readonly int MAX_ITERATIONS = Math.Max(10, System.Environment.ProcessorCount);

    private void trimOldExcess()
    {
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
        if (!m_Data.TryDequeue(out var discarded)) break;
        var sz = m_GetItemSize(discarded);
        Interlocked.Decrement(ref m_EnqueuedCount);
        Interlocked.Add(ref m_EnqueuedSize, -sz);

        m_DiscardedItem?.Invoke(discarded);
      }
    }

  }
}
