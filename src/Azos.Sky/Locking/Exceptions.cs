using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Locking
{
  /// <summary>
  /// Thrown to indicate problems related to distributed locking/mutexes
  /// </summary>
  [Serializable]
  public class LockingException : SkyException
  {
    public LockingException() : base() { }
    public LockingException(string message) : base(message) { }
    public LockingException(string message, Exception inner) : base(message, inner) { }
    protected LockingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
