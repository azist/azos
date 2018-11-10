
using Azos.Conf;
using Azos.Serialization.BSON;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Instrumentation;

namespace Azos.Sky.Instrumentation.Server
{
  /// <summary>
  /// Implements Telemetry Archive using MongoDb
  /// </summary>
  public sealed class MongoTelemetryArchiveStore : TelemetryArchiveStore
  {
    public const string CONFIG_MONGO_SECTION = "mongo";
    public const string CONFIG_DEFAULT_CHANNEL_ATTR = "default-channel";

    public const string DEFAULT_CHANNEL = "telemetry";
    public const int DEFAULT_FETCHBY_SIZE = 32;
    public const int MAX_FETCHBY_SIZE = 4 * 1024;

    public MongoTelemetryArchiveStore(TelemetryReceiverService director, IConfigSectionNode node) : base(director, node)
    {
      var cstring = ConfigStringBuilder.Build(node, CONFIG_MONGO_SECTION);
      m_Database = MongoClient.DatabaseFromConnectString( cstring );
      m_DefaultChannel = node.AttrByName(CONFIG_DEFAULT_CHANNEL_ATTR).ValueAsString(DEFAULT_CHANNEL);
      m_Serializer = new BSONSerializer(node);
      m_Serializer.PKFieldName = Query._ID;
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Database);
      base.Destructor();
    }

    private BSONSerializer m_Serializer;
    private Database m_Database;
    private string m_DefaultChannel;
    private int m_FetchBy = DEFAULT_FETCHBY_SIZE;

    [Config(Default = DEFAULT_FETCHBY_SIZE)]
    public int FetchBy
    {
      get { return m_FetchBy; }
      private set
      {
        m_FetchBy = value < 1 ? 1 : value > MAX_FETCHBY_SIZE ? MAX_FETCHBY_SIZE : value;
      }
    }

    public override object BeginTransaction() { return null; }
    public override void CommitTransaction(object transaction) { }
    public override void RollbackTransaction(object transaction) { }

    public override void Put(Datum[] data, object transaction)
    {
      if (Disposed) return;

      var channel = m_DefaultChannel;
      foreach (var datum in data)
      {
        var doc = m_Serializer.Serialize(datum);
        m_Database[channel].Insert(doc);
      }
    }
  }
}
