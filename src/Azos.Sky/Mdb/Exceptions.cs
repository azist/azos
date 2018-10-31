using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Mdb
{
  /// <summary>
  /// Thrown to indicate Mdb-related problems
  /// </summary>
  [Serializable]
  public class MdbException : SkyException
  {
    public MdbException() : base() { }
    public MdbException(string message) : base(message) { }
    public MdbException(string message, Exception inner) : base(message, inner) { }
    protected MdbException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
