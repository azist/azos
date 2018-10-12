using Azos.Log;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Represents a provider that writes aggregated datums to log
  /// </summary>
  public class LogInstrumentationProvider : InstrumentationProvider
  {
    #region .ctor
    public LogInstrumentationProvider(InstrumentationService director) : base(director) { }
    #endregion

    protected internal override void Write(Datum aggregatedDatum, object batchContext, object typeContext) { App.Log.Write(toMsg(aggregatedDatum)); }

    private Message toMsg(Datum datum)
    {
      var msg = new Message
      {
        Type = MessageType.PerformanceInstrumentation,
        Topic = CoreConsts.INSTRUMENTATIONSVC_TOPIC,
        From = datum.GetType().FullName,
        Text = datum.ToString()
      };

      return msg;
    }
  }
}