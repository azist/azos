/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Returns HTTP 404 - not found.
  /// This should be used in place of returning exceptions where needed as it is faster
  /// </summary>
  public struct Http404NotFound : IActionResult
  {
    public Http404NotFound(string descr = null)
    {
      Description = descr;
    }

    public readonly string Description;

    public Task ExecuteAsync(Controller controller, WorkContext work)
    {
      var txt = WebConsts.STATUS_404_DESCRIPTION;

      if (Description.IsNotNullOrWhiteSpace())
        txt += (": " + Description);

      work.Response.StatusCode = WebConsts.STATUS_404;
      work.Response.StatusDescription = txt;

      if (work.RequestedJson)
       return work.Response.WriteJsonAsync(new {OK = false, http = WebConsts.STATUS_404, descr = txt});
      else
       return work.Response.WriteAsync(txt);
    }
  }


  /// <summary>
  /// Returns HTTP 401 - unauthorized
  /// This should be used in place of returning exceptions where needed as it is faster
  /// </summary>
  public struct Http401Unauthorized : IActionResult
  {
    public Http401Unauthorized(string descr = null)
    {
      Description = descr;
    }

    public readonly string Description;

    public Task ExecuteAsync(Controller controller, WorkContext work)
    {
      var txt = WebConsts.STATUS_401_DESCRIPTION;
      if (Description.IsNotNullOrWhiteSpace())
        txt += (": " + Description);

      work.Response.StatusCode = WebConsts.STATUS_401;
      work.Response.StatusDescription = txt;

      if (work.RequestedJson)
        return work.Response.WriteJsonAsync(new { OK = false, http = WebConsts.STATUS_401, descr = txt });
      else
        return work.Response.WriteAsync(txt);
    }
  }

  /// <summary>
  /// Returns HTTP 403 - forbidden
  /// This should be used in place of returning exceptions where needed as it is faster
  /// </summary>
  public struct Http403Forbidden : IActionResult
  {
    public Http403Forbidden(string descr = null)
    {
      Description = descr;
    }

    public readonly string Description;

    public Task ExecuteAsync(Controller controller, WorkContext work)
    {
      var txt = WebConsts.STATUS_403_DESCRIPTION;
      if (Description.IsNotNullOrWhiteSpace())
        txt += (": " + Description);
      work.Response.StatusCode = WebConsts.STATUS_403;
      work.Response.StatusDescription = txt;

      if (work.RequestedJson)
        return work.Response.WriteJsonAsync(new {OK = false, http = WebConsts.STATUS_403, descr = txt});
      else
        return work.Response.WriteAsync(txt);
    }
  }
}
