
using System;
using System.Runtime.Serialization;

namespace Azos.Serialization.Arow
{
  /// <summary>
  /// Base exception thrown by the Arow serialization format
  /// </summary>
  [Serializable]
  public class ArowException : AzosSerializationException
  {
    public ArowException() { }
    public ArowException(string message) : base(message) { }
    public ArowException(string message, Exception inner) : base(message, inner) { }
    protected ArowException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}