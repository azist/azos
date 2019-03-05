/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;
using System.Threading;

namespace Azos
{
#pragma warning disable CA1063

  /// <summary>
  /// Advanced use, provides extra information about object being disposed
  /// </summary>
  public interface IDisposableLifecycle : IDisposable
  {
    /// <summary>
    /// For advanced use. Returns true when object is being disposed by a finalizer called from CLR GC.
    /// You may want to check for this flag in Destructor() to bypass some deallocation as the
    /// condition is equivalent to typical Dispose(false) pattern, however it indicates a possible memory leak
    /// as all entities implementing IDisposable must be deterministically deallocated using a call to .Dispose() and
    /// system finalizers should NEVER run
    /// </summary>
    /// <remarks>
    /// Application developers should disregard this flag and assume that objects are ALWAYS disposed using
    /// .Dispose() invocation from code. Finalizers should never run, and you can track possible memory leaks using
    /// <see cref="Apps.ExecutionContext.__MemoryLeakTracker"/> event
    /// </remarks>
    bool DisposedByFinalizer {  get; }
  }


  /// <summary>
  /// General-purpose base class for objects that need to be disposed
  /// </summary>
  [Serializable]
  public abstract class DisposableObject : IDisposableLifecycle
  {
    private const int STATE_ALIVE = 0;
    private const int STATE_DISPOSED_USER = 1;
    private const int STATE_DISPOSED_FINALIZER = 2;

    #region STATIC
    /// <summary>
    /// Checks to see if the IDisposable reference is not null and sets it to null in a thread-safe way then calls Dispose().
    /// Returns false if it is already null or not the original reference
    /// </summary>
    public static bool DisposeAndNull<T>(ref T obj) where T : class, IDisposable
    {
      var original = obj;
      var was = Interlocked.CompareExchange(ref obj, null, original);
      if (was==null || !object.ReferenceEquals(was, original)) return false;

      was.Dispose();
      return true;
    }

    /// <summary>
    /// Checks to see if the reference is not null and sets it to null in a thread-safe way then calls Dispose()
    /// if the reference is IDisposable.
    /// Returns false if it is already null or not the original reference or the original reference is not IDisposable
    /// </summary>
    public static bool DisposeIfDisposableAndNull<T>(ref T obj) where T : class
    {
      var original = obj;
      var was = Interlocked.CompareExchange(ref obj, null, original);
      if (was == null || !object.ReferenceEquals(was, original)) return false;

      if (was is IDisposable disposable)
      {
        disposable.Dispose();
        return true;
      }

      return false;
    }
    #endregion

    #region .ctor / .dctor
    ~DisposableObject()
    {
      if (STATE_ALIVE == Interlocked.CompareExchange(ref m_DisposeState, STATE_DISPOSED_FINALIZER, STATE_ALIVE))
      {
        //Disposable objects should be disposed by user code and this finalizer call should never happen
        //if it does happen, it indicates a possible memory leak
        Apps.ExecutionContext.__TrackMemoryLeak(this.GetType());
        Destructor();
      }
    }

    #endregion

    #region Private Fields
    private int m_DisposeState;
    #endregion

    #region Properties

    /// <summary>
    /// Explicitly implements <see cref="IDisposableLifecycle.DisposedByFinalizer"/>
    /// </summary>
    bool IDisposableLifecycle.DisposedByFinalizer => Thread.VolatileRead(ref m_DisposeState) == STATE_DISPOSED_FINALIZER;

    /// <summary>
    /// Indicates whether this object Dispose() or finalizer has been called and dispose started (but
    /// may not have finished yet). Thread safe
    /// </summary>
    public bool Disposed => Thread.VolatileRead(ref m_DisposeState) != STATE_ALIVE;
    #endregion


    #region Public/Protected

    /// <summary>
    /// Override this method to do actual destructor work.
    /// Destructor should not throw exceptions - must handle internally/use logging
    /// </summary>
    /// <remarks>
    /// This method is called as the result of both the deterministic .Dispose() call by user code
    /// and non-deterministic system finalizer invocations. The typical MS .Dispose(bool) pattern is not used on purpose because
    /// all object implementing Disposable() must be deallocated ONLY via a call to Dispose() and
    /// invocation of system finalizer is considered to be a memory leak in Azos.
    /// You could check the <seealso cref="IDisposableLifecycle.DisposedByFinalizer"/> property,
    /// however this is considered to be a special case such as reporting of memory leaks using instrumentation/gauges.
    /// </remarks>
    protected virtual void Destructor()
    {
    }

    /// <summary>
    /// Checks to see whether object dispose started or has already been disposed and throws an exception if Dispose() was called
    /// </summary>
    public void EnsureObjectNotDisposed()
    {
      if (Disposed)
        throw new DisposedObjectException(StringConsts.OBJECT_DISPOSED_ERROR.Args(this.GetType().FullName));
    }

    #endregion

    #region IDisposable Members
    /// <summary>
    /// Deterministically disposes this object in a thread-safe way.
    /// DO NOT TRY TO OVERRIDE this method, override Destructor() instead
    /// </summary>
    public void Dispose()
    {
      if (STATE_ALIVE == Interlocked.CompareExchange(ref m_DisposeState, STATE_DISPOSED_USER, STATE_ALIVE))
      {
        try
        {
          Destructor();
        }
        finally
        {
          GC.SuppressFinalize(this);
        }
      }
    }
    #endregion
  }



  /// <summary>
  /// This exception is thrown from DisposableObject.EnsureObjectNotDisposed() method
  /// </summary>
  [Serializable]
  public class DisposedObjectException : AzosException
  {
    public DisposedObjectException() { }
    public DisposedObjectException(string message) : base(message) { }
    public DisposedObjectException(string message, Exception inner) : base(message, inner) { }
    protected DisposedObjectException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
#pragma warning restore CA1063
}
