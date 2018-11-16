/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using Azos.Log;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Represents a provider that writes aggregated datums to log
  /// </summary>
  public class LogInstrumentationProvider : InstrumentationProvider
  {
    #region .ctor
    public LogInstrumentationProvider(InstrumentationDaemon director) : base(director) { }
    #endregion

    protected internal override void Write(Datum aggregatedDatum, object batchContext, object typeContext) { App.Log.Write(toMsg(aggregatedDatum)); }

    private Message toMsg(Datum datum)
    {
      var msg = new Message
      {
        Type = MessageType.PerformanceInstrumentation,
        Topic = CoreConsts.INSTRUMENTATION_TOPIC,
        From = datum.GetType().FullName,
        Text = datum.ToString()
      };

      return msg;
    }
  }
}