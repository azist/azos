
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Azos.Serialization
{
  /// <summary>
  /// Base exception thrown by the serialization-related classes
  /// </summary>
  [Serializable]
  public class AzosSerializationException : AzosException
  {
    public AzosSerializationException() { }
    public AzosSerializationException(string message) : base(message) { }
    public AzosSerializationException(string message, Exception inner) : base(message, inner) { }
    protected AzosSerializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}