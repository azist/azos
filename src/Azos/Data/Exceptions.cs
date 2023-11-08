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

    public int HttpStatusCode { get; set; } = WebConsts.STATUS_400;
    public string HttpStatusDescription { get; set;} = WebConsts.STATUS_400_DESCRIPTION + " / Data validation";

    public virtual JsonDataMap ProvideExternalStatus(bool includeDump)
     => this.DefaultBuildErrorStatusProviderMap(includeDump, "data.validation");
  }


  /// <summary>
  /// Thrown by data document validation
  /// </summary>
  [Serializable]
  public class DocValidationException : ValidationException
  {
    public const string SCHEMA_NAME_FLD_NAME = "DOCVE-SCH";
    public const string CLIENT_MESSAGE_FLD_NAME = "DOCVE-CM";
    public const string WHAT = "Schema: '{0}'; ";

    public DocValidationException(string schemaName) : base(WHAT.Args(schemaName))
    {
      SchemaName = schemaName;
      ClientMessage = "Validation error";
    }

    public DocValidationException(string schemaName, string message) : base(WHAT.Args(schemaName) + message)
    {
      SchemaName = schemaName;
      ClientMessage = message;
    }

    public DocValidationException(string schemaName, string message, Exception inner) : base(WHAT.Args(schemaName) + message, inner)
    {
      SchemaName = schemaName;
      ClientMessage = message;
    }

    protected DocValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      SchemaName = info.GetString(SCHEMA_NAME_FLD_NAME);
      ClientMessage = info.GetString(CLIENT_MESSAGE_FLD_NAME);
    }

    public readonly string SchemaName;
    public readonly string ClientMessage;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new DataException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");

      info.AddValue(SCHEMA_NAME_FLD_NAME, SchemaName);
      info.AddValue(CLIENT_MESSAGE_FLD_NAME, ClientMessage);
      base.GetObjectData(info, context);
    }

    public override JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = base.ProvideExternalStatus(includeDump);
      result[CoreConsts.EXT_STATUS_KEY_SCHEMA] = SchemaName;
      result[CoreConsts.EXT_STATUS_KEY_MESSAGE] = ClientMessage;
      return result;
    }
  }


  /// <summary>
  /// Thrown by data document field-level validation
  /// </summary>
  [Serializable]
  public class FieldValidationException : ValidationException
  {
    public const string SCHEMA_NAME_FLD_NAME    = "FVE-SCH";
    public const string FIELD_NAME_FLD_NAME     = "FVE-FN";
    public const string CLIENT_MESSAGE_FLD_NAME = "FVE-CM";
    public const string SCOPE_FLD_NAME          = "FVE-SCOPE";

    public const string WHAT = "Schema field: '{0}'.{1}; ";

    public FieldValidationException(Doc doc, string fieldName, string message, string scope = null)
      : this(doc.NonNull(nameof(doc)).Schema.DisplayName,
             doc.Schema[fieldName].NonNull(name: "field {0} not found in schema".Args(fieldName)).Name, message, scope)
    { }

    public FieldValidationException(string schemaName, string fieldName, string scope)
      : base(WHAT.Args(schemaName, fieldName))
    {
      SchemaName = schemaName;
      FieldName = fieldName;
      ClientMessage = "Validation error";
      Scope = scope;
    }

    public FieldValidationException(string schemaName, string fieldName, string message, string scope)
      : base(WHAT.Args(schemaName, fieldName) + message)
    {
      SchemaName = schemaName;
      FieldName = fieldName;
      ClientMessage = message;
      Scope = scope;
    }

    public FieldValidationException(string schemaName, string fieldName, string message, Exception inner, string scope = null)
      : base(WHAT.Args(schemaName, fieldName) + message, inner)
    {
      SchemaName = schemaName;
      FieldName = fieldName;
      ClientMessage = message;
      Scope = scope;
    }

    protected FieldValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      SchemaName = info.GetString(SCHEMA_NAME_FLD_NAME);
      FieldName = info.GetString(FIELD_NAME_FLD_NAME);
      ClientMessage = info.GetString(CLIENT_MESSAGE_FLD_NAME);
      Scope = info.GetString(SCOPE_FLD_NAME);
    }

    public readonly string SchemaName;
    public readonly string FieldName;
    public readonly string ClientMessage;
    public readonly string Scope;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new DataException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(SCHEMA_NAME_FLD_NAME, SchemaName);
      info.AddValue(FIELD_NAME_FLD_NAME, FieldName);
      info.AddValue(CLIENT_MESSAGE_FLD_NAME, ClientMessage);
      info.AddValue(SCOPE_FLD_NAME, Scope);
      base.GetObjectData(info, context);
    }
    public override JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = base.ProvideExternalStatus(includeDump);
      result[CoreConsts.EXT_STATUS_KEY_SCHEMA] = SchemaName;
      result[CoreConsts.EXT_STATUS_KEY_SCOPE] = Scope;
      result[CoreConsts.EXT_STATUS_KEY_FIELD] = FieldName;
      result[CoreConsts.EXT_STATUS_KEY_MESSAGE] = ClientMessage;
      return result;
    }
  }


  /// <summary>
  /// Aggregates multiple validation errors generated by data classes when validation does not pass
  /// </summary>
  [Serializable]
  public sealed class ValidationBatchException : ValidationException
  {
    public const string BATCH_FLD_NAME = "BATCHVE-BATCH";

    /// <summary>
    /// Links two errors together into ValidationBatchException, creating a new instance if necessary.
    /// Both parameters can be null. A new instance is created only if existing exception is not null
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
    /// Flattens inner exceptions of ValidationBatchException into a single stream
    /// </summary>
    public IEnumerable<Exception> All
     => Batch.Where(e => e != null)
             .SelectMany(e =>
                         e is ValidationBatchException vbe ? vbe.All :  e is AggregateException ae ? ae.InnerExceptions
                                                                                                     : e.ToEnumerable() )
             .Where(e => e != null);

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.NonNull(nameof(info)).AddValue(BATCH_FLD_NAME, Batch);
      base.GetObjectData(info, context);
    }

    public override JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = base.ProvideExternalStatus(includeDump);

      result[CoreConsts.EXT_STATUS_KEY_BATCH] = All.Select( e => {
        var map = e is IExternalStatusProvider esp ? esp.ProvideExternalStatus(includeDump)
                                                   : new JsonDataMap{  };

        if (includeDump)
          map[CoreConsts.EXT_STATUS_KEY_DEV_DUMP] = new WrappedExceptionData(e, false, false);

        return map;
      }).ToArray();

      return result;
    }
  }

}
