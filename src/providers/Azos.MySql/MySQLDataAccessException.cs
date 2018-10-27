
using System;
using System.Runtime.Serialization;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Thrown by MySQL data access classes
  /// </summary>
  [Serializable]
  public class MySqlDataAccessException : DataAccessException
  {
    public MySqlDataAccessException() { }
    public MySqlDataAccessException(string message) : base(message) { }
    public MySqlDataAccessException(string message, Exception inner) : base(message, inner) { }
    public MySqlDataAccessException(string message, KeyViolationKind kvKind, string keyViolation) : base(message, kvKind, keyViolation) { }
    public MySqlDataAccessException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation) : base(message, inner, kvKind, keyViolation) { }
    protected MySqlDataAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}