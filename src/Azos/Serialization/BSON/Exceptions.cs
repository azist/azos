
using System;
using System.Runtime.Serialization;

namespace Azos.Serialization.BSON
{
  /// <summary>
  /// Base exception thrown by the BSON framework
  /// </summary>
  [Serializable]
  public class BSONException : AzosException
  {
    public BSONException() { }
    public BSONException(string message) : base(message) { }
    public BSONException(string message, Exception inner) : base(message, inner) { }
    protected BSONException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}