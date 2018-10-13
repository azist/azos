/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Data.Access
{

  /// <summary>
  /// Specifies the sub-type of key violation
  /// </summary>
  public enum KeyViolationKind { Unspecified = 0, Primary, Secondary }

  /// <summary>
  /// Thrown by data access classes
  /// </summary>
  [Serializable]
  public class DataAccessException : DataException
  {
    public const string KEY_VIOLATION_KIND_FLD_NAME = "DAE-KVK";
    public const string KEY_VIOLATION_FLD_NAME = "DAE-KV";

    public DataAccessException() { }
    public DataAccessException(string message) : base(message) { }
    public DataAccessException(string message, Exception inner) : base(message, inner) { }
    public DataAccessException(string message, KeyViolationKind kvKind, string keyViolation)
      : base(message)
    {
      KeyViolationKind = kvKind;
      KeyViolation = keyViolation;
    }

    public DataAccessException(string message, Exception inner, KeyViolationKind kvKind, string keyViolation)
      : base(message, inner)
    {
      KeyViolationKind = kvKind;
      KeyViolation = keyViolation;
    }

    protected DataAccessException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      KeyViolationKind = (KeyViolationKind)info.GetInt32(KEY_VIOLATION_KIND_FLD_NAME);
      KeyViolation = info.GetString(KEY_VIOLATION_FLD_NAME);
    }

    /// <summary>
    /// Spcifies the sub-type of key violation
    /// </summary>
    public readonly KeyViolationKind KeyViolationKind;

    /// <summary>
    /// Provides the name of entity/index/field that was violated and resulted in this exception
    /// </summary>
    public readonly string KeyViolation;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(KEY_VIOLATION_KIND_FLD_NAME, KeyViolationKind);
      info.AddValue(KEY_VIOLATION_FLD_NAME, KeyViolation);
      base.GetObjectData(info, context);
    }
  }
}