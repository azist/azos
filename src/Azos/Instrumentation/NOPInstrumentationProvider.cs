
namespace Azos.Instrumentation
{
  /// <summary>
  /// Represents a provider that does nothing
  /// </summary>
  public class NOPInstrumentationProvider : InstrumentationProvider
  {
    #region .ctor
    public NOPInstrumentationProvider(InstrumentationService director) : base(director) {}
    #endregion

    protected internal override void Write(Datum aggregatedDatum, object batchContext, object typeContext) {}
  }
}
