/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Log;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Logs information extracted from WorkContext
  /// </summary>
  public class LoggingFilter : BeforeAfterFilterBase
  {
    #region CONSTS
      public const string VAR_TYPE = "type";
      public const string VAR_TEXT = "text";
      public const string VAR_FROM = "from";
    #endregion

    #region .ctor
      public LoggingFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order){ }
      public LoggingFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode){ }
      public LoggingFilter(WorkHandler handler, string name, int order) : base(handler, name, order){ }
      public LoggingFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode){ }
    #endregion

    #region Protected

      protected override void DoBeforeWork(WorkContext work, JsonDataMap matched)
      {
        work.Log(
           matched[VAR_TYPE].AsEnum<MessageType>(MessageType.Info),
           matched[VAR_TEXT].AsString(work.About),
           matched[VAR_FROM].AsString("{0}.Before".Args(GetType().FullName)),
           pars: matched.ToJson(JsonWritingOptions.CompactASCII)
           );
      }

      protected override void DoAfterWork(WorkContext work, JsonDataMap matched)
      {
        work.Log(
           matched[VAR_TYPE].AsEnum<MessageType>(MessageType.Info),
           matched[VAR_TEXT].AsString(work.About),
           matched[VAR_FROM].AsString("{0}.After".Args(GetType().FullName)),
           pars: matched.ToJson(JsonWritingOptions.CompactASCII)
           );
      }

    #endregion
  }

}
