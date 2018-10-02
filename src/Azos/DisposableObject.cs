using System;
using System.Runtime.Serialization;
using System.Threading;

namespace Azos
{
  /// <summary>
  /// General-purpose base class for objects that need to be disposed
  /// </summary>
  [Serializable]
  public abstract class DisposableObject : IDisposable
  {

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

    #endregion


    #region .ctor / .dctor

      ~DisposableObject()
      {
        if (0 == Interlocked.CompareExchange(ref m_DisposeStarted, 1, 0))
        {
          try
          {
            Destructor();
          }
          finally
          {
            m_Disposed = true;
          }
        }
      }

    #endregion

    #region Private Fields
      private int m_DisposeStarted;
      private volatile bool m_Disposed;
    #endregion

    #region Properties

      /// <summary>
      /// Indicates whether this object Dispose() has been called and dispose started but not finished yet
      /// </summary>
      public bool DisposeStarted
      {
        get { return Thread.VolatileRead(ref m_DisposeStarted) != 0; }
      }

      /// <summary>
      /// Indicates whether this object was already disposed - the Dispose() has finished
      /// </summary>
      public bool Disposed
      {
        get { return m_Disposed; }
      }
    #endregion


    #region Public/Protected

    /// <summary>
    /// Override this method to do actual destructor work
    /// </summary>
    protected virtual void Destructor()
    {
    }

    /// <summary>
    /// Checks to see whether object dispose started or has already been disposed and throws an exception if Dispose() was called
    /// </summary>
    public void EnsureObjectNotDisposed()
    {
      if (DisposeStarted || m_Disposed)
        throw new DisposedObjectException(StringConsts.OBJECT_DISPOSED_ERROR+" {0}".Args(this.GetType().FullName));
    }

    #endregion

    #region IDisposable Members

        /// <summary>
        /// Deterministically disposes object. DO NOT OVERRIDE this method, override Destructor() instead
        /// </summary>
        public void Dispose()
        {
            if (0 == Interlocked.CompareExchange(ref m_DisposeStarted, 1, 0))
            {
                try
                {
                    Destructor();
                }
                finally
                {
                    m_Disposed = true;
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
}
