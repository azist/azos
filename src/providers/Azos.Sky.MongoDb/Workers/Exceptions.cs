using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Workers
{
  /// <summary>
  /// Thrown to indicate mongo-related workers problems
  /// </summary>
  [Serializable]
  public class MongoWorkersException : WorkersException
  {
    public MongoWorkersException() : base() { }
    public MongoWorkersException(string message) : base(message) { }
    public MongoWorkersException(string message, Exception inner) : base(message, inner) { }
    protected MongoWorkersException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
