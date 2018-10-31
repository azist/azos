using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Instrumentation
{
  /// <summary>
  /// Thrown to indicate log archive related problems
  /// </summary>
  [Serializable]
  public class TelemetryArchiveException : SkyException
  {
    public TelemetryArchiveException() : base() {}
    public TelemetryArchiveException(string message) : base(message) {}
    public TelemetryArchiveException(string message, Exception inner) : base(message, inner) { }
    protected TelemetryArchiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
