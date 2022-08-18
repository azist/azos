/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Instrumentation;
using Azos.Sky.Chronicle;

namespace Azos.Sky.Instrumentation
{
  /// <summary>
  /// Sends data to IInstrumentationChronicle
  /// </summary>
  public sealed class ChronicleInstrumentationProvider : InstrumentationProvider
  {
    public const int BATCH_SIZE = 96;

    public ChronicleInstrumentationProvider(InstrumentationDaemon director) : base(director){ }

    //The dependency may NOT be available at time of construction of this object
    //because this boots before other framework services, hence - service location
    IInstrumentationChronicleLogic m_Chronicle;
    IInstrumentationChronicleLogic Chronicle
    {
      get
      {
        if (m_Chronicle==null)
          m_Chronicle = App.ModuleRoot.Get<IInstrumentationChronicleLogic>();

        return m_Chronicle;
      }
    }

    protected internal override object BeforeBatch() => new List<Datum>();

    protected internal override void Write(Datum aggregatedDatum, object batchContext, object typeContext)
    {
      var datumList = (batchContext as List<Datum>).NonNull(nameof(batchContext));
      datumList.Add(aggregatedDatum);
    }

    protected internal override void AfterBatch(object batchContext)
    {
      var datumList = (batchContext as List<Datum>).NonNull(nameof(batchContext));
      datumList.BatchBy(BATCH_SIZE).ForEach(batch => send(batch.ToArray()));
    }

    private void send(Datum[] data)
    {
      var batch = new InstrumentationBatch
      {
        Data = data.Select(d => d.ToJsonDataMap() ).ToArray()
      };

      Chronicle.WriteAsync(batch)
               .GetAwaiter()
               .GetResult();
    }
  }
}
