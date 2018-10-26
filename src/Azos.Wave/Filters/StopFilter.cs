
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
      protected override void DoBeforeWork(WorkContext work, JSONDataMap matched)
      {
        var code = matched[VAR_CODE].AsInt();
        var error = matched[VAR_ERROR].AsString();
        if (code > 0)
          throw new HTTPStatusException(code, error);
        else
          work.Aborted = true;
      }

      protected override void DoAfterWork(WorkContext work, JSONDataMap matched)
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
