/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Azos.Serialization.JSON;

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
  public class ValidationException : DataException, IHttpStatusProvider, IExternalStatusProvider
  {
    public ValidationException() { }
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception inner) : base(message, inner) { }
    protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public int HttpStatusCode => WebConsts.STATUS_400;
    public string HttpStatusDescription => WebConsts.STATUS_400_DESCRIPTION + " / Data validation";

    public virtual JsonDataMap ProvideExternalStatus(bool includeDump)
     => new JsonDataMap{ {"ns", "data.validation"}, {"type", GetType().Name}, {"error-code", this.Code} };
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

    public override JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = base.ProvideExternalStatus(includeDump);
      result["schema"] = SchemaName;
      return result;
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

    public override JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = base.ProvideExternalStatus(includeDump);
      result["schema"] = SchemaName;
      result["field"] = FieldName;
      result["msg"] = ClientMessage;
      return result;
    }
  }

  /// <summary>
  /// Aggregates multiple validation errors generated by data classes when validation does not pass
  /// </summary>
  [Serializable]
  public sealed class ValidationBatchException : ValidationException, IHttpStatusProvider
  {
    public const string BATCH_FLD_NAME = "BACTHVE-BATCH";

    /// <summary>
    /// Links two errors together into ValidationBatchException, creating a new instance if necessary.
    /// Both parameters can be null. A new instance is create only if existing exception is not null
    /// and is not of ValidationBatchException type, otherwise new exception is added to an existing batch instance.
    /// The method may return null if both exceptions are null.
    /// </summary>
    public static Exception Concatenate(Exception existingException, Exception newException)
    {
      if (existingException==null) return newException;
      if (newException==null) return existingException;

      var result = existingException as ValidationBatchException;

      if (result == null)
        result = new ValidationBatchException(existingException);

      result.Batch.Add(newException);

      return result;
    }

    public ValidationBatchException(Exception first) : base("Error Batch")
    {
      Batch = new List<Exception>();
      if (first!=null) Batch.Add(first);
    }

    private ValidationBatchException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      Batch = (List<Exception>)info.GetValue(BATCH_FLD_NAME, typeof(List<Exception>));
    }

    /// <summary>
    /// A list of errors in this batch
    /// </summary>
    public readonly List<Exception> Batch;

    /// <summary>
    /// Flattens inner ValidationBatchException into a single stream
    /// </summary>
    public IEnumerable<Exception> All
     => Batch.Where(e => e != null)
             .SelectMany(e =>
                         e is ValidationBatchException vbe ? vbe.Batch :  e is AggregateException ae ? ae.InnerExceptions
                                                                                                     : e.ToEnumerable() );

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.NonNull(nameof(info)).AddValue(BATCH_FLD_NAME, Batch);
      base.GetObjectData(info, context);
    }

    public override JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = base.ProvideExternalStatus(includeDump);
      result["batch"] = All.Select( e => {
        var map = e is IExternalStatusProvider esp ? esp.ProvideExternalStatus(includeDump) : new JsonDataMap{ };

        if (includeDump)
         map["dev-dump"] = new WrappedExceptionData(e, true);

        return map;
      });

      return result;
    }
  }

}
