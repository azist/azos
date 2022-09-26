/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

namespace Azos.Data.Business
{
  /// <summary>
  /// Marker interface for general forms
  /// </summary>
  public interface IBusinessFormModel : IBusinessModel
  {
    /// <summary>
    /// Polymorphic behavior: Saves the form and returns the save result as an object
    /// </summary>
    Task<SaveResult<object>> SaveReturningObjectAsync();
  }

  /// <summary>
  /// Provides model for a mutable entity, such as a data entry form/API endpoint.
  /// FormModels are usually implemented using various other models (docs/rows et.al.) that they save from/into data stores
  /// </summary>
  public abstract class FormModel<TSaveResult> : Form<TSaveResult>, IBusinessFormModel
  {
    /// <summary>
    /// Override to provide more concrete backend targeting
    /// </summary>
    public override string DataStoreTargetName => TargetedAttribute.ANY_TARGET;
  }

  /// <summary>
  /// Marker interface for general purpose filters
  /// </summary>
  public interface IBusinessFilterModel : IBusinessFormModel
  {
    /// <summary>
    /// Paging offset of the first document/row returned. This property is ignored for filters yielding non-lists
    /// </summary>
    int PagingStartIndex { get; set; }

    /// <summary>
    /// How many docs/rows should be returned. This property is ignored for filters yielding non-lists
    /// </summary>
    int PagingCount { get; set; }
  }

  /// <summary>
  /// Represents a model for data screen filters, such as the ones used in lists/grids and reports.
  /// The filter typically gets posted to the "list" resource endpoint and returns a result of the filtering operation
  /// </summary>
  public abstract class FilterModel<TSaveResult> : FormModel<TSaveResult>, IBusinessFilterModel
  {
    /// <summary>
    /// Paging offset of the first document/row returned. This property is ignored for filters yielding non-lists
    /// </summary>
    [Field(description: "Paging offset of the first document/row returned. This property is ignored for filters yielding non-lists")]
    public int PagingStartIndex { get; set; }

    /// <summary>
    /// How many docs/rows should be returned. This property is ignored for filters yielding non-lists
    /// </summary>
    [Field(description: "How many doc/rows should be returned. This property is ignored for filters yielding non-lists")]
    public int PagingCount { get; set; }
  }

  /// <summary>
  /// Marker interface for persisted forms - forms that can be saved into persistent storage
  /// </summary>
  public interface IBusinessPersistedModel : IBusinessFormModel
  {
  }


  /// <summary>
  /// Represents a model which is typically stored in a datastore/long term storage.
  /// Internally, these models usually delegate the actual data access to Data* access layer (such as docs/rows)
  /// </summary>
  /// <remarks>
  /// Persisted models are source of truth for entities which are stored. They isolate
  /// the business logic from lower-level data access, as data access layer is typically modeled
  /// for data-first/relational schema, whereas business entities are modeled around the business-first
  /// needs (e.g. in DDD)
  /// <code>
  ///   API &lt;--&gt; [Form Model] &lt;--&gt; [PersistedModel] &lt;--&gt; [DataAccessModel/Docs]
  /// </code>
  /// </remarks>
  public abstract class PersistedModel<TSaveResult> : FormModel<TSaveResult>, IBusinessPersistedModel
  {
  }


  /// <summary>
  /// Marker interface for fragment models
  /// </summary>
  /// <remarks>
  /// Although not technically needed because Fragments are not generic, the interface is used for design consistency
  /// </remarks>
  public interface IBusinessFragmentModel : IBusinessModel
  {
  }


  /// <summary>
  /// Provides models which represent a fragment of other models, typically Persisted models.
  /// The fragments usually do not get saved on their own but they may in some cases
  /// </summary>
  /// <remarks>
  /// FragmentModels are used for things like HumanNameBlock, AddressBlock, ContactBlock etc...
  /// </remarks>
  public abstract class FragmentModel : AmorphousTypedDoc, IBusinessFragmentModel
  {
  }


  /// <summary>
  /// Marker interface for transient models
  /// </summary>
  /// <remarks>
  /// Although non technically needed because Transients are not generic, the interface is used for design consistency
  /// </remarks>
  public interface IBusinessTransientModel : IBusinessModel
  {
  }


  /// <summary>
  /// Provides a model for transient data.
  /// Transient data usually represents calculated results and is not stored as-is and is typically needed
  /// for consumption by reports/views and other logic. Typical example: report result returned to the
  /// caller as the result of FilterModel post to the server
  /// </summary>
  /// <remarks>
  /// TransientModel represent results of calculations such as table joins, calculations by code etc.
  /// Their instances are typically returned from a business contract layer
  /// <code>
  ///   IEnumerable&lt;UserInfoTransient&gt; GetUserInfo(UserSearchFilter filter);
  /// </code>
  /// </remarks>
  public abstract class TransientModel : AmorphousTypedDoc, IBusinessTransientModel
  {
  }


  /// <summary>
  /// Marker interface for requests which generate responses.
  /// The concept is similar to filters, difference is in the intent.
  /// The HTTP response codes are always 200, even when the response is null, this is because
  /// request/response is treated more like RPC and does not rely on HTTP response code semantics
  /// </summary>
  public interface IBusinessRequestModel : IBusinessFormModel
  {
  }

  /// <summary>
  /// Represents a model for requests which generate responses.
  /// The concept is similar to filters, difference is in the intent.
  /// The HTTP response codes are always 200, even when the response is null, this is because
  /// request/response is treated more like RPC and does not rely on HTTP response code semantics
  /// </summary>
  public abstract class RequestModel<TResponse> : FormModel<TResponse>, IBusinessRequestModel
  {
  }

}
