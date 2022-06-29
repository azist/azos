/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Denotes form modes: unspecified | insert | update | delete
  /// </summary>
  public enum FormMode { Unspecified = 0, Insert, Update, Delete}

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

      protected Form(){ }

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
      public bool HasRoundtripBag => m_RoundtripBag != null;

      /// <summary>
      /// Returns lazily-allocated RoundtripBag.
      /// Use HasRoundtripBag to see if it is allocated not to allocate on get
      /// </summary>
      public JsonDataMap RoundtripBag
      {
        get
        {
          if (m_RoundtripBag == null) m_RoundtripBag = new JsonDataMap();

          return m_RoundtripBag;
        }
      }

      /// <summary>
      /// False by default for forms, safer for web. For example, no injection of unexpected fields can be done via web form post
      /// </summary>
      public override bool AmorphousDataEnabled => false;

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
      DoAmorphousDataBeforeSave(DataStoreTargetName);

      var validationState = DoBeforeValidateOnSave();

      validationState = this.Validate(validationState);

      validationState = await DoAfterValidateOnSaveAsync(validationState).ConfigureAwait(false);

      if (validationState.HasErrors)
        return new SaveResult<TSaveResult>(validationState.Error);

      await DoBeforeSaveAsync().ConfigureAwait(false);

      var result = await DoSaveAsync().ConfigureAwait(false);
      return result;
    }

    public sealed async override Task<SaveResult<object>> SaveReturningObjectAsync()
     => (await SaveAsync()).ToObject();

    /// <summary>
    /// Override to perform pre-validate step on Save(). This can be used to default required field values among other things.
    /// Returns a ValidState initialized to DataStoreTargetName and ValidErrorMode as desired in a specific case.
    /// Default implementation sets `ValidErrorMode.FastBatch` meaning: the system will stop validation on a first validation error
    /// and return it. Override to use other modes such as `ValidErrorMode.Batch`
    /// Note: this is called after amorphous data save, so the validation can assume that the state of AmorphousData is initialized
    /// </summary>
    protected virtual ValidState DoBeforeValidateOnSave() => new ValidState(DataStoreTargetName, ValidErrorMode.FastBatch);

    /// <summary>
    /// Override to perform post-validate step on Save(). For example, you can mask/disregard validation error
    /// by returning a clean ValidState instance
    /// </summary>
    /// <param name="state">Validation state instance which you can disregard by returning a new ValidState without an error</param>
    protected virtual Task<ValidState> DoAfterValidateOnSaveAsync(ValidState state) => Task.FromResult(state);

    /// <summary>
    /// Override to perform post-successful-validate pre-save step on Save().
    /// This override is typically used to generate a unique ID for inserts (as determined by FormMode)
    /// in the absence of validation errors so the ID does NOT get generated and wasted when there is/are validation errors.
    /// This method is NOT called if validation finds errors in prior steps of Save() flow
    /// </summary>
    protected virtual Task DoBeforeSaveAsync() => Task.CompletedTask;

    /// <summary>
    /// Override to save model into data store. Return "predictable" exception (such as key violation) as a value instead of throwing.
    /// Throw only in "unpredictable" cases (such as DB connection is down, not enough space etc...).
    /// Return extra result obtained during save i.e. a db-assigned auto-inc field
    /// </summary>
    protected abstract Task<SaveResult<TSaveResult>> DoSaveAsync();

  }
}
