
using System;
using System.Runtime.Serialization;


namespace Azos.Data.Access.MongoDb.Connector
{
  /// <summary>
  /// Thrown by MongoDB data access classes
  /// </summary>
  [Serializable]
  public class MongoDbConnectorException : MongoDbDataAccessException
  {
    public MongoDbConnectorException() { }
    public MongoDbConnectorException(string message) : base(message) { }
    public MongoDbConnectorException(string message, Exception inner) : base(message, inner) { }
    public MongoDbConnectorException(string message, KeyViolationKind kvKind, string keyViolation) : base(message, kvKind, keyViolation) { }
    public MongoDbConnectorException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation) : base(message, inner, kvKind, keyViolation) { }
    protected MongoDbConnectorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by MongoDB data access classes related to protocol
  /// </summary>
  [Serializable]
  public class MongoDbConnectorProtocolException : MongoDbConnectorException
  {
    public MongoDbConnectorProtocolException() { }
    public MongoDbConnectorProtocolException(string message) : base(message) { }
    public MongoDbConnectorProtocolException(string message, Exception inner) : base(message, inner) { }
    protected MongoDbConnectorProtocolException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by MongoDB data access classes related to server errors
  /// </summary>
  [Serializable]
  public class MongoDbConnectorServerException : MongoDbConnectorException
  {
    public MongoDbConnectorServerException() { }
    public MongoDbConnectorServerException(string message) : base(message) { }
    public MongoDbConnectorServerException(string message, Exception inner) : base(message, inner) { }
    protected MongoDbConnectorServerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown in case of query compile error
  /// </summary>
  [Serializable]
  public class MongoDbQueryException : MongoDbConnectorException
  {
    public MongoDbQueryException() { }
    public MongoDbQueryException(string message) : base(message) { }
    public MongoDbQueryException(string message, Exception inner) : base(message, inner) { }
    protected MongoDbQueryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
