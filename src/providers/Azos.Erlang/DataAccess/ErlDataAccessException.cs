
using System;
using System.Runtime.Serialization;

namespace Azos.Data.Access.Erlang
{
  /// <summary>
  /// Thrown by Erlang data access classes
  /// </summary>
  [Serializable]
  public class ErlDataAccessException : DataAccessException
  {

    public ErlDataAccessException() {}

    public ErlDataAccessException(string message) : base(message){}

    public ErlDataAccessException(string message, Exception inner) : base(message, inner){}

    public ErlDataAccessException(string message, KeyViolationKind kvKind, string keyViolation)
      : base(message, kvKind, keyViolation){}

    public ErlDataAccessException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation)
      : base(message, inner, kvKind, keyViolation){}

    protected ErlDataAccessException(SerializationInfo info, StreamingContext context)
      : base(info, context) {}
  }

  /// <summary>
  /// Thrown by Erlang data schema map when server has changed schema. The client needs to restart or
  ///  kill all cached data and refetch schema
  /// </summary>
  [Serializable]
  public class ErlServerSchemaChangedException : ErlDataAccessException
  {

    public ErlServerSchemaChangedException() {}

    protected ErlServerSchemaChangedException(SerializationInfo info, StreamingContext context)
      : base(info, context) {}
  }

}