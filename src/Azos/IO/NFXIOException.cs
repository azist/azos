
using System;
using System.Runtime.Serialization;

namespace Azos.IO
{
  /// <summary>
  /// Base exception thrown by the IO-related classes
  /// </summary>
  [Serializable]
  public class AzosIOException : AzosException
  {
    public AzosIOException() { }
    public AzosIOException(string message) : base(message) { }
    public AzosIOException(string message, Exception inner) : base(message, inner) { }
    protected AzosIOException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}