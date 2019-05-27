/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Serialization.JSON;
using Azos.Conf;
using Azos.Data;
using Azos.Wave.Templatization;
using ErrorPage=Azos.Wave.Templatization.StockContent.Error;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Stops the processing of WorkContext by throwing exception upon match
  /// </summary>
  public class StopFilter : BeforeAfterFilterBase
  {
    #region CONSTS
      public const string VAR_CODE = "code";
      public const string VAR_ERROR = "error";
    #endregion

    #region .ctor
      public StopFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) { }
      public StopFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode) { }
      public StopFilter(WorkHandler handler, string name, int order) : base(handler, name, order) { }
      public StopFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode) { }
    #endregion

    #region Protected
      protected override void DoBeforeWork(WorkContext work, JsonDataMap matched)
      {
        var code = matched[VAR_CODE].AsInt();
        var error = matched[VAR_ERROR].AsString();
        if (code > 0)
          throw new HTTPStatusException(code, error);
        else
          work.Aborted = true;
      }

      protected override void DoAfterWork(WorkContext work, JsonDataMap matched)
      {
        var code = matched[VAR_CODE].AsInt();
        var error = matched[VAR_ERROR].AsString();
        if (code > 0)
          throw new HTTPStatusException(code, error);
        else
          work.Aborted = true;
      }
    #endregion

  }

}
