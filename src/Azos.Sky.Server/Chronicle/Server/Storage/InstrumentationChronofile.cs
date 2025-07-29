// Ignore Spelling: Chronofile

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.IO.Archiving;
using Azos.Serialization.JSON;

namespace Azos.Sky.Chronicle.Server.Storage
{
  /// <summary>
  /// Implements `IInstrumentationChronicleStore` database using an index archive technology
  /// </summary>
  public sealed partial class InstrumentationChronofile : Daemon, IInstrumentationChronicleStore
  {
    public InstrumentationChronofile(IApplication application) : base(application)
    {
      //new DefaultVolume()
    }

    public InstrumentationChronofile(IApplicationComponent director) : base(director)
    {
    }

    public override string ComponentLogTopic => throw new NotImplementedException();


    private string m_DataPath;//root
    private List<IVolume> m_Volumes;
    private PilePageCache m_PageCache;


    public string DataPath{ get; set; }

    #region IInstrumentationChronicleStore Implementation
    public Task<IEnumerable<JsonDataMap>> GetAsync(InstrumentationChronicleFilter filter)
    {
      filter.NonNull(nameof(filter));
      if (!Running) return Task.FromResult(Enumerable.Empty<JsonDataMap>());

      return Task.FromResult<IEnumerable<JsonDataMap>>(new JsonDataMap[0]);
    }

    public Task WriteAsync(InstrumentationBatch data)
    {
      CheckDaemonActive();
      data.NonNull(nameof(data));

      //append to volume as of the date in data
      return null;
    }
    #endregion
  }
}
