/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.IO;

using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Decorates types that represent a result of an idempotent operation with a `IdempotencyToken: string`.
  /// This interface addresses the response part and does not specify how the token value gets supplied
  /// by the calling client (e.g. call header, request parameter, etc.)
  /// </summary>
  public interface IIdempotentResult
  {
    /// <summary>
    /// Provide idempotency key value which server can use to prevent secondary execution of the
    /// request marked with the same token
    /// </summary>
    string IdempotencyToken {  get; }
  }


  /// <summary>
  /// Describes data change operation result: {ChangeType, AffectedCount, Message, Data}
  /// </summary>
  public struct ChangeResult : IJsonWritable, IJsonReadable, IHttpStatusProvider, IIdempotentResult
  {
    /// <summary>
    /// Change types: Undefined/Inserted/Updated/Upserted/Deleted/Other.
    /// Undefined is treated as non-success (OK: false)
    /// </summary>
    public enum ChangeType
    {
      /// <summary> Change type is undefined - the structure does not represent a valid change, OK: false </summary>
      Undefined = 0,

      /// <summary> A new item was inserted/added, e.g. a row into database </summary>
      Inserted,

      /// <summary> An existing item was updated/patched </summary>
      Updated,

      /// <summary> Either a new item was inserted or existing item was updated </summary>
      Upserted,

      /// <summary> An item was deleted </summary>
      Deleted,

      /// <summary>
      /// A complex change happened, a system may have inserted/updated/deleted items in different parts of the system.
      /// This value is different from others in that it does not specify how data has changed, it just indicates that
      /// request was processed, OK: true
      /// </summary>
      Processed
    }

    /// <summary>
    /// Describes data change operation result: Inserted/Deleted/etc.., rows affected, extra data etc.
    /// </summary>
    /// <param name="change">Change type: Inserted/Updated...</param>
    /// <param name="affectedCount">Affected entity count</param>
    /// <param name="msg">Optional message from the serving party</param>
    /// <param name="data">Returns optional extra data which is returned from the data change operation</param>
    /// <param name="statusCode">Status code mappable to http response, zero by default</param>
    /// <param name="idempotencyToken">Optional idempotency token</param>
    public ChangeResult(ChangeType change, long affectedCount, string msg, object data, int statusCode = 0, string idempotencyToken = null)
    {
      Change = change;
      StatusCode = statusCode;
      AffectedCount = affectedCount;
      Message = msg;
      Data = data;
      IdempotencyToken = idempotencyToken;
    }

    /// <summary>
    /// Describes data change operation non-successful result such as 404 not found/Undefined change
    /// </summary>
    /// <param name="msg">Optional message from the serving party</param>
    /// <param name="statusCode">Status code mappable to http response, zero by default which maps to HTTP 404</param>
    /// <param name="data">Returns optional extra data which is returned from the data change operation</param>
    /// <param name="idempotencyToken">Optional idempotency token</param>
    public ChangeResult(string msg, int statusCode = 0, object data = null, string idempotencyToken = null)
    {
      Change = ChangeType.Undefined;
      StatusCode = statusCode;
      AffectedCount = 0;
      Message = msg;
      Data = data;
      IdempotencyToken = idempotencyToken;
    }

    /// <summary>
    /// Describes data change operation result: Inserted/Deleted/etc.., rows affected, extra data etc.
    /// Creates instance from JsonDataMap dictionary.
    /// </summary>
    /// <param name="map">Non-null map with keys: {change, affected, message, data}</param>
    public ChangeResult(JsonDataMap map)
    {
      map.NonNull(nameof(map));
      Change        = map["change"].AsEnum(ChangeType.Undefined);
      StatusCode    = map["status"].AsInt();
      AffectedCount = map["affected"].AsLong();
      Message       = map["message"].AsString();
      Data          = map["data"];
      IdempotencyToken = map["idempotency_token"].AsString();
    }

    /// <summary> True if change is not `Undefined` </summary>
    public bool IsOk => Change != ChangeType.Undefined;

    /// <summary>
    /// Is StatusCode is set then returns it, otherwise returns 200 for non-undefined changes, or 404 for undefined change types
    /// </summary>
    public int HttpStatusCode =>  StatusCode != 0 ? StatusCode : IsOk ? 200 : 404;

    /// <summary> Http status description </summary>
    public string HttpStatusDescription => Message;

    /// <summary> Specifies the change type Insert/Update/Delete etc.. </summary>
    public readonly ChangeType Change;

    /// <summary> Operation Status code, which is returned for HTTP callers </summary>
    public readonly int StatusCode;

    /// <summary> How many entities/rows/docs was/were affected by the change </summary>
    public readonly long AffectedCount;

    /// <summary> Provides an optional message from the serving party </summary>
    public readonly string Message;

    /// <summary>
    /// Attaches optional extra data which is returned from the data change operation,
    /// for example a posted sale may return an OrderId object generated by the target store
    /// </summary>
    public readonly object Data;

    /// <summary>
    /// Optional idempotency token issued by the server/processing entity. Guid.Empty is used when no token was issued.
    /// You can use the returned token to retry the seemingly failed server operation which may already have succeeded
    /// on the server in which case the token will ensure that the secondary re-executions are not going to affect server state
    /// beyond the very first change
    /// </summary>
    public readonly string IdempotencyToken;

    string IIdempotentResult.IdempotencyToken => this.IdempotencyToken;

    /// <summary>
    /// Writes this ChangeResult as a typical JSON object like: {OK: true, change: Inserted ... }
    /// </summary>
    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      JsonWriter.WriteMap(wri, nestingLevel, options,
                    new DictionaryEntry("OK", IsOk),
                    new DictionaryEntry("change", Change),
                    new DictionaryEntry("status", StatusCode),
                    new DictionaryEntry("affected", AffectedCount),
                    new DictionaryEntry("message", Message),
                    new DictionaryEntry("data", Data),
                    new DictionaryEntry("idempotency_token", IdempotencyToken)
                   );
    }

    /// <summary>
    /// Reads ChangeResult back from JSON
    /// </summary>
    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
        return (true, new ChangeResult(map));

      return (false, null);
    }

    //20230907 DKh, JPK, JGW #894
    /// <summary>
    /// Represents the <see cref="Data"/> field as <see cref="IJsonDataObject"/> - either a null, a map, or an array
    /// as-if the Data came over wire.
    /// Sometimes you need to read the data, but it contains an anonymous object in which case you can convert
    /// such an object into a JsonDataMap/array.
    /// If the Data is already populated with IJsonDataOBject then this method returns it as-is
    /// </summary>
    public IJsonDataObject GetDataAsJsonObject()
    {
      if (Data == null) return null;
      if (Data is IJsonDataObject jdo) return jdo;
      if (Data is Doc doc)
      {
        return doc.ToJsonDataMap(JsonWritingOptions.CompactRowsAsMapWithTypeHints);
      }
      var json = Data.ToJson(JsonWritingOptions.CompactRowsAsMapWithTypeHints);
      jdo = json.JsonToDataObject(JsonReadingOptions.DefaultWithTypeHints);
      return jdo;
    }
  }


  /// <summary>
  /// Struct returned from Form.Save(): it is either an error (IsSuccess==false), or TResult
  /// </summary>
  public struct SaveResult<TResult> : IIdempotentResult
  {
    /// <summary>
    /// Creates error result
    /// </summary>
    public SaveResult(Exception error)
    {
      Error = error;
      Result = default(TResult);
    }

    /// <summary>
    /// Creates successful result
    /// </summary>
    public SaveResult(TResult result)
    {
      Error = null;
      Result = result;
    }

    /// <summary>
    /// Null on success or Error which prevented successful Save
    /// </summary>
    public readonly Exception Error;

    /// <summary>
    /// Returns the result of the form save, e.g. for filters this returns a resulting rowset.
    /// accessing this field does not throw exception if one is set.
    /// Use <see cref="GetResult"/> to return a valid result or throw
    /// </summary>
    public readonly TResult Result;

    /// <summary>
    /// True if there is no error - a success
    /// </summary>
    public bool IsSuccess => Error == null;

    /// <summary>
    /// True if there is error - not success
    /// </summary>
    public bool IsError => !IsSuccess;

    /// <summary>
    /// Returns a SaveResult&lt;object&gt; representation
    /// </summary>
    public SaveResult<object> ToObject() => IsSuccess ? new SaveResult<object>(Result) : new SaveResult<object>(Error);

    /// <summary>
    /// If result is successful then returns it, otherwise throws an error
    /// </summary>
    public TResult GetResult() => IsSuccess ? Result : throw Error;

    /// <summary>
    /// If the embodied result is <see cref="IIdempotentResult"/> then returns its <see cref="IIdempotentResult.IdempotencyToken"/>, null otherwise
    /// </summary>
    public string IdempotencyToken => Result is IIdempotentResult ir ? ir.IdempotencyToken : null;
  }
}
