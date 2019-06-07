/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Data
{
  /// <summary>
  /// Base exception thrown by the data-related functionality
  /// </summary>
  [Serializable]
  public class DataException : AzosException
  {
    public DataException() { }
    public DataException(string message) : base(message) { }
    public DataException(string message, Exception inner) : base(message, inner) { }
    protected DataException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by data classes when validation does not pass
  /// </summary>
  [Serializable]
  public class ValidationException : DataException, IHttpStatusProvider
  {
    public ValidationException() { }
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception inner) : base(message, inner) { }
    protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public int HttpStatusCode => WebConsts.STATUS_400;
    public string HttpStatusDescription => WebConsts.STATUS_400_DESCRIPTION+"/ Data validation";
  }

  /// <summary>
  /// Thrown by data document validation
  /// </summary>
  [Serializable]
  public class DocValidationException : ValidationException
  {
    public const string SCHEMA_NAME_FLD_NAME = "DOCVE-SCH";

    public const string WHAT = "Schema: '{0}'; ";

    public DocValidationException(string schemaName) : base(WHAT.Args(schemaName)) { SchemaName = schemaName; }
    public DocValidationException(string schemaName, string message) : base(WHAT.Args(schemaName) + message) { SchemaName = schemaName; }
    public DocValidationException(string schemaName, string message, Exception inner) : base(WHAT.Args(schemaName) + message, inner) { SchemaName = schemaName; }
    protected DocValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      SchemaName = info.GetString(SCHEMA_NAME_FLD_NAME);
    }

    public readonly string SchemaName;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new DataException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");

      info.AddValue(SCHEMA_NAME_FLD_NAME, SchemaName);
      base.GetObjectData(info, context);
    }
  }

  /// <summary>
  /// Thrown by data document field-level validation
  /// </summary>
  [Serializable]
  public class FieldValidationException : ValidationException
  {
    public const string SCHEMA_NAME_FLD_NAME    = "DOCVE-SCH";
    public const string FIELD_NAME_FLD_NAME     = "DOCVE-FN";
    public const string CLIENT_MESSAGE_FLD_NAME = "DOCVE-CM";

    public const string WHAT = "Schema field: '{0}'.{1}; ";

    public FieldValidationException(Doc doc, string fieldName, string message)
      : this(doc.NonNull(nameof(doc)).Schema.DisplayName,
             doc.Schema[fieldName].NonNull(name: "field {0} not found in schema".Args(fieldName)).Name, message)
    { }

    public FieldValidationException(string schemaName, string fieldName)
      : base(WHAT.Args(schemaName, fieldName))
    {
      SchemaName = schemaName;
      FieldName = fieldName;
      ClientMessage = "Validation error";
    }

    public FieldValidationException(string schemaName, string fieldName, string message)
      : base(WHAT.Args(schemaName, fieldName) + message)
    {
      SchemaName = schemaName;
      FieldName = fieldName;
      ClientMessage = message;
    }

    public FieldValidationException(string schemaName, string fieldName, string message, Exception inner)
      : base(WHAT.Args(schemaName, fieldName) + message, inner)
    {
      SchemaName = schemaName;
      FieldName = fieldName;
      ClientMessage = message;
    }

    protected FieldValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      SchemaName = info.GetString(SCHEMA_NAME_FLD_NAME);
      FieldName = info.GetString(FIELD_NAME_FLD_NAME);
      ClientMessage = info.GetString(CLIENT_MESSAGE_FLD_NAME);
    }

    public readonly string SchemaName;
    public readonly string FieldName;
    public readonly string ClientMessage;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new DataException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(SCHEMA_NAME_FLD_NAME, SchemaName);
      info.AddValue(FIELD_NAME_FLD_NAME, FieldName);
      info.AddValue(CLIENT_MESSAGE_FLD_NAME, ClientMessage);
      base.GetObjectData(info, context);
    }
  }

}
