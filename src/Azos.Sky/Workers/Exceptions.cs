using System;
using System.Runtime.Serialization;


namespace Azos.Sky.Workers
{
  /// <summary>
  /// Thrown to indicate workers related problems
  /// </summary>
  [Serializable]
  public class WorkersException : SkyException
  {
    public WorkersException() : base() {}
    public WorkersException(string message) : base(message) {}
    public WorkersException(string message, Exception inner) : base(message, inner) { }
    protected WorkersException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
