using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Business;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Establishes a base protocol for API controllers: filters yielding Info objects on post, saving models returning JSON with OK flags.
  /// The controller works in conjunction with `UnwrapPayloadObject()`, `UnwrapPayloadArray()`, `UnwrapChangeResult()` Http response helpers
  /// on the client side
  /// </summary>
  [ApiControllerDoc(
    Connection = "default/keep alive",
    Title = "ApiProtocolController",
    Description = "Establishes a base protocol for API controllers: filters yielding Info objects on post, saving models returning JSON with OK flags"
  )]
  public abstract class ApiProtocolController : Controller
  {
    /// <summary>
    /// Common NoCache Api doc entry
    /// </summary>
    public const string API_DOC_HDR_NO_CACHE = "NoCache: pragma no cache";

    /// <summary>
    /// Common app/JSON doc entry
    /// </summary>
    public const string API_DOC_HDR_ACCEPT_JSON = "Accept: application/json";


    /// <summary>
    /// Applies the filter to the data store returning JSON result.
    /// Note:
    ///  this method does not return 404 if filter yields no results, this is because 404 would indicate an absence
    /// of the filter resource (filter not found), whereas an empty filter result is returned as HTTP 200 with an empty/null result array/object.
    /// </summary>
    protected async Task<object> ApplyFilterAsync<TFilter>(TFilter filter) where TFilter : class, IBusinessFilterModel
    {
      var filtered = await App.InjectInto(filter.NonNull(nameof(filter)))
                              .SaveReturningObjectAsync().ConfigureAwait(false);
      if (filtered.IsSuccess)
        return new { OK = true, data = filtered.Result };

      throw new BusinessException($"Could not apply filter `{typeof(TFilter).Name}`: {filtered.Error.ToMessageWithType()}", filtered.Error);
    }


    private async Task<object> save(PersistedModel<ChangeResult> model)
    {
      var saved = await App.InjectInto(model)
                           .SaveAsync().ConfigureAwait(false);

      if (saved.IsSuccess)
      {
        var change = saved.Result;
        WorkContext.Response.StatusCode = change.HttpStatusCode;
        WorkContext.Response.StatusDescription = change.HttpStatusDescription;
        return change;
      }

      throw new BusinessException($"Could not save model `{model.GetType().DisplayNameWithExpandedGenericArgs()}`" +
                                  $"in mode `{model.FormMode}`: {saved.Error.ToMessageWithType()}", saved.Error);
    }


    /// <summary>
    /// Persists model obtained from PUT by calling Save in the app scope, returning JSON result
    /// </summary>
    public async Task<object> SaveEditAsync(PersistedModel<ChangeResult> model)
    {
      model.NonNull(nameof(model)).FormMode = FormMode.Update;
      return await save(model).ConfigureAwait(false);
    }


    /// <summary>
    /// Persists model obtained from POST by calling Save in the app scope, returning JSON result
    /// </summary>
    public async Task<object> SaveNewAsync(PersistedModel<ChangeResult> model)
    {
      model.NonNull(nameof(model)).FormMode = FormMode.Insert;
      return await save(model).ConfigureAwait(false);
    }


    /// <summary>
    /// Persists model state obtained from DELETE by calling Save in the app scope, returning JSON result.
    /// This method can be used to logically delete items via call to PersistedModel.Save()
    /// </summary>
    public async Task<object> SaveDeleteAsync(PersistedModel<ChangeResult> model)
    {
      model.NonNull(nameof(model)).FormMode = FormMode.Delete;
      return await save(model).ConfigureAwait(false);
    }


    /// <summary>
    /// Maps ChangeResult returned by logic into HTTP status codes with proper API protocol {OK: true/false, change: ...}
    /// </summary>
    public object GetLogicChangeResult(ChangeResult result)
    {
      WorkContext.Response.StatusCode = result.HttpStatusCode;
      WorkContext.Response.StatusDescription = result.HttpStatusDescription;
      return result;
    }

    /// <summary>
    /// Returns JSON response explicitly categorized as OK or non Ok not depending on the result data object.
    /// `{OK=ok, data: object}` with the specified HTTP status.
    /// You should call this when you want to specifically return either an OK or an error regardless of result object.
    /// For example, an object may represent a non-null "good data" but sometimes you might need to return specific HTTP
    /// status code with accompanying description
    /// </summary>
    public object GetExplicitResult(bool ok, object result, int httpStatus, string httpStatusDescription)
    {
      WorkContext.Response.StatusCode = httpStatus;
      WorkContext.Response.StatusDescription = httpStatusDescription;
      return new { OK = ok, data = result };
    }

    /// <summary>
    /// Analyzes the result of a logic call and returns JSON {OK=true|false, data: object} with HTTP status 404 when nothing was returned
    /// </summary>
    public object GetLogicResult(object result)
    {
      if (result is IHttpStatusProvider hsp)
      {
        var code = hsp.HttpStatusCode;
        //for the purpose of business APIs only Successful responses are treated as OK
        //https://developer.mozilla.org/en-US/docs/Web/HTTP/Status
        var ok = code >= 200 && code <= 299;
        WorkContext.Response.StatusCode = code;
        WorkContext.Response.StatusDescription = hsp.HttpStatusDescription;
        return new { OK = ok, data = result };
      }

      var is404 = result == null;
      if (!is404)
      {
        //20191228 JPK+DKh warning:
        //IEnumerable<struct> is not assignable to IEnumerable<object> see issue #224
        var en = result as IEnumerable;
        is404 = en != null && !en.Cast<object>().Any();
      }

      if (is404)
      {
        WorkContext.Response.StatusCode = WebConsts.STATUS_404;
        WorkContext.Response.StatusDescription = WebConsts.STATUS_404_DESCRIPTION;
      }
      return new { OK = !is404, data = result };
    }
  }
}
