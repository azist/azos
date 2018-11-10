using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Kdb
{
  /// <summary>
  /// Thrown to indicate KDB-related problems
  /// </summary>
  [Serializable]
  public class KdbException : SkyException
  {
    public KdbException() : base() { }
    public KdbException(string message) : base(message) { }
    public KdbException(string message, Exception inner) : base(message, inner) { }
    protected KdbException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
