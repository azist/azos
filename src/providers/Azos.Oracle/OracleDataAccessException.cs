/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Data.Access.Oracle
{
  /// <summary>
  /// Thrown by Oracle data access classes
  /// </summary>
  [Serializable]
  public class OracleDataAccessException : DataAccessException
  {
    public OracleDataAccessException() { }
    public OracleDataAccessException(string message) : base(message) { }
    public OracleDataAccessException(string message, Exception inner) : base(message, inner) { }
    public OracleDataAccessException(string message, KeyViolationKind kvKind, string keyViolation) : base(message, kvKind, keyViolation) { }
    public OracleDataAccessException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation) : base(message, inner, kvKind, keyViolation) { }
    protected OracleDataAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}