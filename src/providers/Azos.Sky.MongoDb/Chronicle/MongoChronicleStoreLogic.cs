/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Log;
using Azos.Serialization.JSON;

using Azos.Data.Access.MongoDb;
using Azos.Conf;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Sky.Identification;
using Azos.Apps.Injection;

namespace Azos.Sky.Chronicle.Server
{
  /// <summary>
  /// Implements ILogChronicleStoreLogic and IInstrumentationChronicleStoreLogic using MongoDb
  /// </summary>
  public sealed class MongoChronicleStoreLogic : Daemon, ILogChronicleStoreLogicImplementation, IInstrumentationChronicleStoreLogicImplementation
  {
    public const string CONFIG_BUNDLED_MONGO_SECTION = "bundled-mongo";

    public const string DEFAULT_DB = "sky_chron";

    public const string COLLECTION_LOG = "sky_log";
    public const string COLLECTION_INSTR = "sky_ins";

    public const string SEQUENCE_LOG = "chronicle_sky_log";
    public const string SEQUENCE_INSTR = "chronicle_sky_ins";

    public const int MAX_FETCH_DOC_COUNT = 8 * 1024;
    public const int MAX_INSERT_DOC_COUNT = 8 * 1024;
    public const int FETCH_BY_LOG = 128;


    public MongoChronicleStoreLogic(IApplication application) : base(application) { }
    public MongoChronicleStoreLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Bundled);
      base.Destructor();
    }

#pragma warning disable 0649
    [Inject] IGdidProviderModule m_Gdid;

    private string m_GdidScopeName;
    private Database m_LogDb;
    private Database m_InstrDb;


    [Config(Default = DEFAULT_DB)]
    private string m_DbNameLog = DEFAULT_DB;

    [Config(Default = DEFAULT_DB)]
    private string m_DbNameInstr = DEFAULT_DB;

    [Config]
    private string m_CsLogDatabase;

    [Config]
    private string m_CsInstrDatabase;
#pragma warning restore 0649

    public override string ComponentLogTopic => CoreConsts.INSTRUMENTATION_TOPIC;

    private BundledMongoDb m_Bundled;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Bundled);
      if (node==null) return;

      var nbundled = node[CONFIG_BUNDLED_MONGO_SECTION];
      if (nbundled.Exists)
      {
        m_Bundled = FactoryUtils.MakeAndConfigureDirectedComponent<BundledMongoDb>(this, nbundled);
      }
    }

    protected override void DoStart()
    {
      m_GdidScopeName = App.EnvironmentName.NonBlank("App.EnvironmentName configured of `app/$environment-name`");

      WriteLog(MessageType.Info, nameof(DoStart), "Chronicle store is configured with GDID scope `app/$environment-name`: " + m_GdidScopeName);

      base.DoStart();

      if (m_Bundled != null)
      {
        m_Bundled.Start();
        m_LogDb = m_Bundled.GetDatabase(m_DbNameLog);
        m_InstrDb = m_Bundled.GetDatabase(m_DbNameInstr);
      }
      else
      {
        m_LogDb = App.GetMongoDatabaseFromConnectString(m_CsLogDatabase);
        m_InstrDb = App.GetMongoDatabaseFromConnectString(m_CsInstrDatabase);
      }
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      if (m_Bundled != null) m_Bundled.SignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      if (m_Bundled != null) m_Bundled.WaitForCompleteStop();
    }

    public Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter) => Task.FromResult(get(filter));
    private IEnumerable<Message> get(LogChronicleFilter filter)
    {
      filter.NonNull(nameof(filter));
      if (!Running) yield break;

      var cLog = m_LogDb[COLLECTION_LOG];

      var query = LogFilterQueryBuilder.BuildLogFilterQuery(filter);
      var totalCount = Math.Min(filter.PagingCount <= 0 ? FETCH_BY_LOG : filter.PagingCount, MAX_FETCH_DOC_COUNT);
      using (var cursor = cLog.Find(query, filter.PagingStartIndex, FETCH_BY_LOG))
      {
        int i = 0;
        foreach (var bdoc in cursor)
        {

          var msg = BsonConvert.FromBson(bdoc);
          yield return msg;

          if (++i > totalCount || !Running) break;
        }
      }
    }

    public Task WriteAsync(LogBatch data)
    {
      var toSend = data.NonNull(nameof(data)).Data.NonNull(nameof(data));
      if (!Running) return Task.CompletedTask;

      var cLog = m_LogDb[COLLECTION_LOG];

      int i = 0;
      using(var errors = new ErrorLogBatcher(App.Log){Type = MessageType.Critical, From = this.ComponentLogFromPrefix+nameof(WriteAsync), Topic = ComponentLogTopic})
      foreach (var batch in toSend.BatchBy(0xf))
      {
        var bsons = batch.Select(msg => {
          if (msg.Gdid.IsZero)
          {
            msg.Gdid = m_Gdid.Provider.GenerateOneGdid(scopeName: m_GdidScopeName, sequenceName: COLLECTION_LOG);
          }
          return BsonConvert.ToBson(msg);
        });

        try
        {
          var result = cLog.Insert(bsons.ToArray());
          if (result.WriteErrors!=null)
           result.WriteErrors.ForEach(we => new MongoDbConnectorServerException(we.Message));
        }
        catch(Exception genError)
        {
          errors.Add(genError);
        }

        if (!Running) break;
        if (++i > MAX_INSERT_DOC_COUNT)
        {
          WriteLog(MessageType.Critical, nameof(WriteAsync), "LogBatch exceeds max allowed count of {0}. The rest discarded".Args(MAX_INSERT_DOC_COUNT));
          break;
        }
      }

      return Task.CompletedTask;
    }

    public Task<IEnumerable<JsonDataMap>> GetAsync(InstrumentationChronicleFilter filter)
    {
      throw new NotImplementedException();
    }

    public Task WriteAsync(InstrumentationBatch data)
    {
      throw new NotImplementedException();
    }
  }
}
