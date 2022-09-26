/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System.Threading.Tasks;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Represents MVC action result that redirects user to some URL
  /// </summary>
  public struct Redirect : IActionResult
  {
    public Redirect(string url, WebConsts.RedirectCode code = WebConsts.RedirectCode.Found_302)
    {
      URL = url;
      Code = code;
    }
    /// <summary>
    /// Where to redirect user
    /// </summary>
    public readonly string URL;

    /// <summary>
    /// Redirect code
    /// </summary>
    public readonly WebConsts.RedirectCode Code;

    public Task ExecuteAsync(Controller controller, WorkContext work)
    {
      work.Response.Redirect(URL, Code);
      return Task.CompletedTask;
    }
  }
}
