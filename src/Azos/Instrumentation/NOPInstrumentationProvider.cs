/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
