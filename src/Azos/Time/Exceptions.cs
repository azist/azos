
using System;
using System.Runtime.Serialization;

namespace Azos.Time
{
  /// <summary>
  /// Base exception thrown by the Time framework
  /// </summary>
  [Serializable]
  public class TimeException : AzosException
  {
    public TimeException() { }
    public TimeException(string message) : base(message) { }
    public TimeException(string message, Exception inner) : base(message, inner) { }
    protected TimeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
