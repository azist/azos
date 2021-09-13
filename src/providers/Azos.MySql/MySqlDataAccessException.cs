/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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