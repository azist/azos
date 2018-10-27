
using System;
using System.Collections.Generic;
using System.Threading;

namespace Azos.Erlang.Internal
{
  public class ErlBlockingQueue<T> : DisposableObject
  {
  #region CONSTS
    private const int SLEEP_GRANULARITY_MSEC = 5000;
  #endregion

  #region .ctor

    protected override void Destructor()
    {
      base.Destructor();
      m_Active = false;
      m_Wakeup.Set();
      Clear();
    }

  #endregion

  #region Fields

    private EventWaitHandle m_Wakeup = new AutoResetEvent(false);
    private Queue<T> m_Queue = new Queue<T>();
    private bool m_Active = true;

  #endregion

  #region Props

    public bool Active { get { return m_Active; } set { m_Active = value; } }
    public bool Empty { get { return m_Queue.Count == 0; } }
    public int Count { get { return m_Queue.Count; } }

    /// <summary>
    /// Queue message arrival notification handle
    /// </summary>
    public EventWaitHandle Handle { get { return m_Wakeup; } }

  #endregion

  #region Public

    public void Enqueue(T data)
    {
      lock (m_Queue)
      {
        m_Queue.Enqueue(data);
        if (m_Queue.Count == 1)
          m_Wakeup.Set();
      }
    }

    public bool TryDequeue(ref T value)
    {
      lock (m_Queue)
      {
        if (!m_Active || m_Queue.Count == 0)
          return false;
        value = m_Queue.Dequeue();
        return true;
      }
    }

    public T Dequeue()
    {
      return Dequeue(-1);
    }

    public T Dequeue(int timeoutMsec)
    {
      var timeout       = timeoutMsec < 0 ? int.MaxValue : timeoutMsec;
      var sleepInterval = Math.Min(timeout, SLEEP_GRANULARITY_MSEC);
      var wakeupTime    = DateTime.UtcNow.AddMilliseconds(timeout);
      var timedout      = false;
      var wasActive     = Azos.App.Active;

      while ((!wasActive || Azos.App.Active) && !Disposed)
      {
        do
        {
          if (!m_Active || m_Queue.Count > 0)  break;
          if (m_Wakeup.WaitOne(sleepInterval)) break;
          timedout = timeoutMsec >= 0 && DateTime.UtcNow > wakeupTime;
        }
        while (!timedout);

        if (!m_Active || timedout) return default(T);

        lock (m_Queue)
        {
          if (m_Queue.Count > 0)
            return m_Queue.Dequeue();
        }
      }
      return default(T);
    }

    public void Clear()
    {
      lock (m_Queue)
          m_Queue.Clear();
    }

  #endregion
  }
}
