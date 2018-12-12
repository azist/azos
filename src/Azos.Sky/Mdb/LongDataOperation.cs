
#warning revise - why is this needed?

//////using System;
//////using System.Threading;

//////namespace Azos.Sky.Mdb
//////{
//////  /// <summary>
//////  /// Gets called by various routines that report long-running data operation progress and allow to cancel further processing.
//////  /// </summary>
//////  public delegate void LongDataOperationCallback<TResult>(LongDataOperation<TResult> operation);

//////  /// <summary>
//////  /// Provides a context for long data operation, such as fetching data from many shards.
//////  /// Callers may cancel operation and track overall progress via a LongDataOperationCallback.
//////  /// This is conceptually similar to CancelationToken
//////  /// </summary>
//////  public sealed class LongDataOperation<TResult>
//////  {
//////    public LongDataOperation()
//////    {
//////      m_StartTimeUTC = App.TimeSource.UTCNow;
//////    }

//////    private DateTime m_StartTimeUTC;
//////    private volatile int m_Total;
//////    private int m_Current;
//////    private volatile bool m_Canceled;

//////    private volatile bool m_HasResult;
//////    private TResult m_Result;


//////    /// <summary>
//////    /// Allows to attach arbitrary object context
//////    /// </summary>
//////    public object Context;


//////    /// <summary>
//////    /// Returns the UTC timestamp of this operation
//////    /// </summary>
//////    public DateTime StartTimeUTC{ get{ return m_StartTimeUTC;} }


//////    /// <summary>
//////    /// The total amount of work
//////    /// </summary>
//////    public int Total{ get{return m_Total;}}

//////    /// <summary>
//////    /// The current progress of work out of Total
//////    /// </summary>
//////    public int Current{ get{ return Thread.VolatileRead(ref m_Current);} }

//////    /// <summary>
//////    /// True if operation was canceled via Cancel()
//////    /// </summary>
//////    public bool Canceled { get{ return m_Canceled;}}

//////    /// <summary>
//////    /// True if operation was completed via a call to SetResult()
//////    /// </summary>
//////    public bool HasResult { get{ return m_HasResult;}}

//////    /// <summary>
//////    /// The final result of operation which is available after completion or cancel
//////    /// </summary>
//////    public TResult Result{ get{ return m_Result;}}

//////    /// <summary>
//////    /// Sets total amount of work
//////    /// </summary>
//////    public void SetTotal(int total){ m_Total = total;}

//////    /// <summary>
//////    /// Sets current progress out of total
//////    /// </summary>
//////    public void SetCurrent(int current){ m_Current = current;}

//////    /// <summary>
//////    /// Sets current progress by +1 via interlocked
//////    /// </summary>
//////    public int AdvanceCurrent()
//////    {
//////      return Interlocked.Increment(ref m_Current);
//////    }

//////    /// <summary>
//////    /// Call to cancel the further processing
//////    /// </summary>
//////    public void Cancel() { m_Canceled = true; }

//////    /// <summary>
//////    /// Call to set result
//////    /// </summary>
//////    public void SetResult(TResult result)
//////    {
//////      m_HasResult = true;
//////      m_Result = result;
//////      Thread.MemoryBarrier();
//////    }
//////  }
//////}
