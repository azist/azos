using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Business;

namespace Azos.Wave.Mvc
{
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
                              .SaveReturningObjectAsync();
      if (filtered.IsSuccess)
        return new { OK = true, data = filtered.Result };

      throw new BusinessException($"Could not apply filter `{typeof(TFilter).Name}`: {filtered.Error.ToMessageWithType()}", filtered.Error);
    }


    /// <summary>
    /// Persists model obtained from PUT by calling Save in the app scope, returning JSON result
    /// </summary>
    public async Task<object> SaveEditAsync(PersistedModel<ChangeResult> model)
    {
      model.NonNull(nameof(model)).FormMode = FormMode.Update;
      var saved = await App.InjectInto(model)
                           .SaveAsync();

      if (saved.IsSuccess)
        return saved.Result;

      throw new BusinessException($"Could not save model `{model.GetType().Name}`: {saved.Error.ToMessageWithType()}", saved.Error);
    }


    /// <summary>
    /// Persists model obtained from POST by calling Save in the app scope, returning JSON result
    /// </summary>
    public async Task<object> SaveNewAsync(PersistedModel<ChangeResult> model)
    {
      model.NonNull(nameof(model)).FormMode = FormMode.Insert;
      var saved = await App.InjectInto(model)
                           .SaveAsync();

      if (saved.IsSuccess)
        return saved.Result;

      throw new BusinessException($"Could not save model `{model.GetType().Name}`: {saved.Error.ToMessageWithType()}", saved.Error);
    }

    /// <summary>
    /// Analyzes the result of a logic call and returns either JSON {OK=true|false} with HTTP status 404 when nothing is returned
    /// </summary>
    public object GetLogicResult<T>(T result)
    {
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
