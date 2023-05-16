/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Idgen;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Sky.Identification;

namespace Azos.Sky.Chronicle.Server
{
  /// <summary>
  /// Provides server implementation for ILogChronicle and  IInstrumentationChronicle
  /// </summary>
  public sealed class ChronicleServerLogic : ModuleBase, ILogChronicleLogic, IInstrumentationChronicleLogic
  {
    public const string SEQ_SKY_LOG = "sky_log";
    public const string SEQ_SKY_INSTR = "sky_ins";

    public const string CONFIG_STORE_SECTION = "store";
    public const string CONFIG_LOG_STORE_SECTION = "log-store";
    public const string CONFIG_INSTRUMENTATION_STORE_SECTION = "instrumentation-store";
    public const string CONFIG_LOG_ARCHIVE_SECTION = "log-archive";

    public ChronicleServerLogic(IApplication application) : base(application) { }
    public ChronicleServerLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_LogArchiveGraph);
      DisposeAndNull(ref m_Log);
      DisposeAndNull(ref m_Instrumentation);
      base.Destructor();
    }

    [Inject] IGdidProviderModule m_Gdid;

    private ILogImplementation m_LogArchiveGraph;
    private ILogChronicleStoreImplementation m_Log;
    private IInstrumentationChronicleStoreImplementation m_Instrumentation;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.INSTRUMENTATION_TOPIC;



    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_LogArchiveGraph);
      DisposeAndNull(ref m_Log);
      DisposeAndNull(ref m_Instrumentation);

      if (node == null) return;

      var nArchive = node[CONFIG_LOG_ARCHIVE_SECTION];
      if (nArchive.Exists)
      {
        m_LogArchiveGraph = FactoryUtils.MakeAndConfigureDirectedComponent<ILogImplementation>(this, nArchive, typeof(LogDaemon));
      }

      var nStore = node[CONFIG_STORE_SECTION];
      if (nStore.Exists)
      {
        m_Log = FactoryUtils.MakeAndConfigureDirectedComponent<ILogChronicleStoreImplementation>(this, nStore);
        m_Instrumentation = m_Log.CastTo<IInstrumentationChronicleStoreImplementation>("cfg section `{0}`".Args(CONFIG_STORE_SECTION));
      }
      else
      {
        m_Log = FactoryUtils.MakeAndConfigureDirectedComponent<ILogChronicleStoreImplementation>(this,
                                  node[CONFIG_LOG_STORE_SECTION].NonEmpty(CONFIG_LOG_STORE_SECTION));

        m_Instrumentation = FactoryUtils.MakeAndConfigureDirectedComponent<IInstrumentationChronicleStoreImplementation>(this,
                                  node[CONFIG_INSTRUMENTATION_STORE_SECTION].NonEmpty(CONFIG_INSTRUMENTATION_STORE_SECTION));
      }
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Log.NonNull($"Configured {nameof(m_Log)}").Start();
      m_Instrumentation.NonNull($"Configured {nameof(m_Instrumentation)}").Start();

      if (m_LogArchiveGraph != null)
      {
        if (m_LogArchiveGraph is Daemon d)  d.StartByApplication();
      }

      App.InjectInto(m_Log);
      m_Log.Start();

      if (m_Log != m_Instrumentation)
      {
        App.InjectInto(m_Instrumentation);
        m_Instrumentation.Start();
      }

      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      if (m_LogArchiveGraph is Daemon d)  d.WaitForCompleteStop();

      m_Log.WaitForCompleteStop();
      m_Instrumentation.WaitForCompleteStop();
      return base.DoApplicationBeforeCleanup();
    }

    public async Task WriteAsync(LogBatch data)
    {
      data.NonNull(nameof(data))
          .Data
          .NonNull(nameof(data.Data));

      //do not realloc if not needed
      if (data.Data.Any(m => m == null))
        data.Data = data.Data.Where(m => m != null).ToArray();

      data.Data.IsTrue(d => d.Any(), "No data supplied");

      //0 Prepare messages for insertion
      GDID[] gdids = null;
      Exception gdidFailure = null;
      for(int i=0, j=0; i < data.Data.Length; i++)
      {
        var msg = data.Data[i];

        if (gdidFailure == null)
        {
          if (gdids == null || j == gdids.Length)
          {
            try
            {
              j = 0;
              gdids = m_Gdid.Provider.TryGenerateManyConsecutiveGdids(scopeName: SysConsts.GDID_NS_CHRONICLES,
                                                                      sequenceName: SEQ_SKY_LOG,
                                                                      gdidCount: data.Data.Length - i);
            }
            catch(Exception error)
            {
              gdidFailure = error;
            }
          }
        }

        if (gdidFailure == null)
        {
          //gdid regenerated
          msg.Gdid = gdids[j++];
        }

        msg.InitDefaultFields(App);
      }

      //1 Write to archive graph ASAP
      Exception archiveFailure = null;
      try
      {
        var arch = m_LogArchiveGraph;
        if (arch != null) data.Data.ForEach( m => arch.Write(m) );
      }
      catch (Exception error)
      {
        archiveFailure = error;
      }

      //2 Write to store
      Exception storeFailure = null;
      try
      {
        await m_Log.NonNull().WriteAsync(data).ConfigureAwait(false);
      }
      catch (Exception error)
      {
        storeFailure = error;
      }


      //catastrophic notification may trigger something like SolarWind/Everbridge et.al. alert
      if (gdidFailure != null)
      {
        WriteLog(MessageType.CatastrophicError, nameof(WriteAsync), "Gdid generation failed: " + gdidFailure.ToMessageWithType(), gdidFailure, Ambient.CurrentCallFlow?.ID);
      }

      if (archiveFailure != null)
      {
        WriteLog(MessageType.CatastrophicError, nameof(WriteAsync), "Archive failed: " + archiveFailure.ToMessageWithType(), archiveFailure, Ambient.CurrentCallFlow?.ID);
      }

      if (storeFailure != null)
      {
        WriteLog(MessageType.CatastrophicError, nameof(WriteAsync), "Store failed: " + storeFailure.ToMessageWithType(), storeFailure, Ambient.CurrentCallFlow?.ID);
      }
    }

    public async Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter)
      => await m_Log.NonNull().GetAsync(filter.NonNull(nameof(filter))).ConfigureAwait(false);

    //20230515 DKh #861
    public async Task<IEnumerable<Fact>> GetFactsAsync(LogChronicleFactFilter filter)
    {
      var messages = await m_Log.NonNull().GetAsync(filter.NonNull(nameof(filter)).LogFilter.NonNull(nameof(filter.LogFilter))).ConfigureAwait(false);

      //Server-side convert to facts
      var facts = messages.Select(one => ArchiveConventions.LogMessageToFact(one))
                          .ToArray();//materialize, as conversion is costly

      return facts;
    }

    public async Task WriteAsync(InstrumentationBatch data)
      => await m_Instrumentation.NonNull().WriteAsync(data.NonNull(nameof(data))).ConfigureAwait(false);

    public async Task<IEnumerable<JsonDataMap>> GetAsync(InstrumentationChronicleFilter filter)
      => await m_Instrumentation.NonNull().GetAsync(filter.NonNull(nameof(filter))).ConfigureAwait(false);

  }
}
