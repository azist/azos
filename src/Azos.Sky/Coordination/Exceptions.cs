using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Coordination
{
  /// <summary>
  /// Thrown to indicate coordination-related problems
  /// </summary>
  [Serializable]
  public class CoordinationException : SkyException
  {
    public CoordinationException() : base() { }
    public CoordinationException(string message) : base(message) { }
    public CoordinationException(string message, Exception inner) : base(message, inner) { }
    protected CoordinationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
