/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azos.Serialization.JSON;

namespace Azos.Data
{

  /// <summary>
  /// Denotes form modes: unspecified | insert | update | delete
  /// </summary>
  public enum FormMode { Unspecified = 0, Insert, Update, Delete}

  /// <summary>
  /// Struct returned from Form.Save(): it is either an error (IsSuccess==false), or TResult
  /// </summary>
  public struct SaveResult<TResult>
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
    /// Returns the result of the form save, e.g. for filters this returns a resulting rowset
    /// </summary>
    public readonly TResult   Result;

    /// <summary>
    /// True if there is no error
    /// </summary>
    public bool IsSuccess => Error==null;

    /// <summary>
    /// Returns SaveResult&lt;object&gt; representation
    /// </summary>
    public SaveResult<object> ToObject() => IsSuccess ? new SaveResult<object>(Result) : new SaveResult<object>(Error);
  }

  /// <summary>
  /// Represents a "model" (in MVC terms) of a data-entry form.
  /// Form models are statically typed - contain fields and can contain "extra amorphous" data
  /// </summary>
  [Serializable]
  public abstract class Form : AmorphousTypedDoc
  {
      public const string JSON_MODE_PROPERTY = "__FormMode";
      public const string JSON_CSRF_PROPERTY = CoreConsts.CSRF_TOKEN_NAME;
      public const string JSON_ROUNDTRIP_PROPERTY = "__Roundtrip";
      public const string JSON_TYPE_PROPERTY = "__FormType";


      protected Form() {}


      /// <summary>
      /// Gets/sets form mode - unspecified|insert|edit. This field may be queried by validate and save, i.e. Validate may perform extra cross checks on Insert - i.e. check whether
      /// some other user is already registered with the specified email in this form etc.
      /// </summary>
      public FormMode FormMode;

      /// <summary>
      /// Gets/sets CSRF token
      /// </summary>
      public string CSRFToken;


      private JsonDataMap m_RoundtripBag;

      /// <summary>
      /// True if RoundtripBag is allocated
      /// </summary>
      public bool HasRoundtripBag{ get{ return m_RoundtripBag!=null;}}

      /// <summary>
      /// Returns lazily-allocated RoundtripBag.
      /// Use HasRoundtripBag to see if it is allocated not to allocate on get
      /// </summary>
      public JsonDataMap RoundtripBag
      {
        get
        {
          if (m_RoundtripBag==null)
            m_RoundtripBag = new JsonDataMap();

          return m_RoundtripBag;
        }
      }

      /// <summary>
      /// False by default for forms, safer for web. For example, no injection of unexpected fields can be done via web form post
      /// </summary>
      public override bool AmorphousDataEnabled { get{return false;}}

      /// <summary>
      /// If non null or empty parses JSON content and sets the RoundtripBag
      /// </summary>
      public void SetRoundtripBagFromJSONString(string content)
      {
         if (content.IsNullOrWhiteSpace()) return;

         m_RoundtripBag = content.JsonToDataObject() as JsonDataMap;
      }

      /// <summary>
      /// Saves form into data store. The form is validated first and validation error is returned which indicates that save did not succeed due to validation error/s.
      /// The core implementation is in DoSave() that can also abort by either returning exception when predictable failure happens on save (i.e. key violation).
      /// Other exceptions are thrown.
      /// Returns extra result obtained during save i.e. a db-assigned auto-inc field.
      /// This is a polymorphic substitute for generic Form&lt;TSaveResult&gt;
      /// </summary>
      public abstract Task<SaveResult<object>> SaveReturningObjectAsync();


      /// <summary>
      /// Override to supply target name used for validation
      /// </summary>
      public abstract string DataStoreTargetName { get; }
  }



  /// <summary>
  /// Represents a "model" (in MVC terms) of a data-entry form.
  /// Form models are statically typed - contain fields and can contain "extra amorphous" data.
  /// </summary>
  [Serializable]
  public abstract class Form<TSaveResult> : Form
  {
    /// <summary>
    /// Saves form into data store. The form is validated first and validation error is returned which indicates that save did not succeed due to validation error/s.
    /// The core implementation is in DoSave() that can also abort by either returning exception when predictable failure happens on save (i.e. key violation).
    /// Other exceptions are thrown.
    /// Returns extra result obtained during save i.e. a db-assigned auto-inc field
    /// </summary>
    public async virtual Task<SaveResult<TSaveResult>> SaveAsync()
    {
      var valError = this.Validate(DataStoreTargetName);

      if (valError != null)
        return new SaveResult<TSaveResult>(valError);

      DoBeforeSave(DataStoreTargetName);

      return await DoSaveAsync();
    }

    public sealed async override Task<SaveResult<object>> SaveReturningObjectAsync()
     => (await SaveAsync()).ToObject();

    /// <summary>
    /// Override to save model into data store. Return "predictable" exception (such as key violation) as a value instead of throwing.
    /// Throw only in "unpredictable" cases (such as DB connection is down, not enough space etc...).
    /// Return extra result obtained during save i.e. a db-assigned auto-inc field
    /// </summary>
    protected abstract Task<SaveResult<TSaveResult>> DoSaveAsync();

  }

}
