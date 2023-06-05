/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Serialization.JSON;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Represents MVC action result that returns Bixon object with options
  /// </summary>
  public struct BixonResult : IActionResult
  {
    public BixonResult(object data, JsonWritingOptions options)
    {
      Data = data;
      Options = options;
    }

    public BixonResult(Exception error, JsonWritingOptions options)
    {
      var http = WebConsts.STATUS_500;
      var descr = WebConsts.STATUS_500_DESCRIPTION;
      if (error != null)
      {
        descr = error.Message;
        var httpError = error as Web.HTTPStatusException;
        if (httpError != null)
        {
          http = httpError.StatusCode;
          descr = httpError.StatusDescription;
        }
      }
      Data = new { OK = false, http, descr };
      Options = options;
    }

    public readonly object Data;
    public readonly JsonWritingOptions Options;

    public Task ExecuteAsync(Controller controller, WorkContext work)
      => work.Response.WriteBixonAsync(Data, Options);
  }
}
