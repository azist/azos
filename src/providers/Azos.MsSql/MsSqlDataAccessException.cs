/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Data.Access.MsSql
{
  /// <summary>
  /// Thrown by MsSQL data access classes
  /// </summary>
  [Serializable]
  public class MsSqlDataAccessException : DataAccessException
  {
    public MsSqlDataAccessException() { }
    public MsSqlDataAccessException(string message) : base(message) { }
    public MsSqlDataAccessException(string message, Exception inner) : base(message, inner) { }
    public MsSqlDataAccessException(string message, KeyViolationKind kvKind, string keyViolation) : base(message, kvKind, keyViolation) { }
    public MsSqlDataAccessException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation) : base(message, inner, kvKind, keyViolation) { }
    protected MsSqlDataAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}