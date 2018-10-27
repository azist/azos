
using System;
using System.Runtime.Serialization;

namespace Azos.Data.Access.MongoDb
{
  /// <summary>
  /// Thrown by MongoDB data access classes
  /// </summary>
  [Serializable]
  public class MongoDbDataAccessException : DataAccessException
  {
    public MongoDbDataAccessException() { }
    public MongoDbDataAccessException(string message) : base(message) { }
    public MongoDbDataAccessException(string message, Exception inner) : base(message, inner) { }
    public MongoDbDataAccessException(string message, KeyViolationKind kvKind, string keyViolation) : base(message, kvKind, keyViolation) { }
    public MongoDbDataAccessException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation) : base(message, inner, kvKind, keyViolation) { }
    protected MongoDbDataAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}