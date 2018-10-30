using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Dynamic
{
  /// <summary>
  /// Thrown to indicate workers related problems
  /// </summary>
  [Serializable]
  public class DynamicException : SkyException
  {
    public DynamicException() : base() {}
    public DynamicException(string message) : base(message) { }
    public DynamicException(string message, Exception inner) : base(message, inner) { }
    protected DynamicException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
