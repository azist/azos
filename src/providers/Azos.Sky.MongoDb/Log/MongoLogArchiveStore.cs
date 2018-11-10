using System;
using System.Collections.Generic;
using System.Text;

using Azos.Conf;
using Azos.Serialization.BSON;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Log;

namespace Azos.Sky.Log.Server
{
  /// <summary>
  /// Implements Log Archive store using MongoDB
  /// </summary>
  public sealed class MongoLogArchiveStore : LogArchiveStore
  {
    public const string CONFIG_MONGO_SECTION = "mongo";
    public const string CONFIG_DEFAULT_CHANNEL_ATTR = "default-channel";

    public const string DEFAULT_CHANNEL = "archive";
    public const int DEFAULT_FETCHBY_SIZE = 32;
    public const int MAX_FETCHBY_SIZE = 4 * 1024;

    private static readonly BSONParentKnownTypes KNOWN_TYPES = new BSONParentKnownTypes(typeof(Message));

    public MongoLogArchiveStore(LogReceiverService director, LogArchiveDimensionsMapper mapper, IConfigSectionNode node) : base(director, mapper, node)
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

    public override void Put(Message message, object transaction)
    {
      if (Disposed) return;

      var channel = message.Channel;

      if (channel.IsNullOrWhiteSpace())
        channel = m_DefaultChannel;

      var doc = m_Serializer.Serialize(message, KNOWN_TYPES);

      var map = Mapper.StoreMap(message.ArchiveDimensions);
      if (map != null)
      {
        foreach (var item in map)
          doc.Set(DataDocConverter.String_CLRtoBSON("__" + item.Key, item.Value));
      }

      m_Database[channel].Insert(doc);
    }

    public override bool TryGetByID(Guid id, out Message message, string channel = null)
    {
      var query = new Query(
        @"{ '$query': { {0}: '$$id' } }".Args(m_Serializer.PKFieldName), true,
        new TemplateArg(new BSONBinaryElement("id", new BSONBinary(BSONBinaryType.UUID, id.ToByteArray()))));

      if (channel.IsNullOrWhiteSpace())
        channel = m_DefaultChannel;

      message = new Message();
      var doc = m_Database[channel].FindOne(query);
      if (doc == null) return false;

      m_Serializer.Deserialize(doc, message);
      return true;
    }

    public override IEnumerable<Message> List(string archiveDimensionsFilter, DateTime startDate, DateTime endDate, MessageType? type = null,
      string host = null, string channel = null, string topic = null,
      Guid? relatedTo = null,
      int skipCount = 0)
    {
      var map = Mapper.FilterMap(archiveDimensionsFilter);

      var query = buildQuery(map, startDate, endDate, type, host, channel, topic, relatedTo);

      if (channel.IsNullOrWhiteSpace())
        channel = m_DefaultChannel;

      var collection = m_Database[channel];
      var result = new List<Message>();

      using (var cursor = collection.Find(query, skipCount, FetchBy))
        foreach (var doc in cursor)
        {
          var message = new Message();
          m_Serializer.Deserialize(doc, message);
          result.Add(message);
        }

      return result;
    }

    private Query buildQuery(
      Dictionary<string, string> archiveDimensionsFilter,
      DateTime startDate, DateTime endDate, MessageType? type = null,
      string host = null, string channel = null, string topic = null,
      Guid? relatedTo = null)
    {
      var args = new List<TemplateArg>();
      var wb = new StringBuilder();

      wb.AppendFormat(@"{0}: {{ '$gte': '$$start_date', '$lt': '$$end_date' }}", Message.BSON_FLD_TIMESTAMP);
      args.Add(new TemplateArg("start_date", startDate));
      args.Add(new TemplateArg("end_date", endDate));

      if (archiveDimensionsFilter != null)
        foreach (var item in archiveDimensionsFilter)
        {
          wb.AppendFormat(", __{0}:'$${0}'", item.Key);
          args.Add(new TemplateArg(item.Key, item.Value));
        }

      if (type.HasValue)
      {
        wb.AppendFormat(", {0}:'$$type'", Message.BSON_FLD_TYPE);
        args.Add(new TemplateArg("type", type.Value));
      }

      if (relatedTo.HasValue)
      {
        wb.AppendFormat(", {0}:'$$related'", Message.BSON_FLD_RELATED_TO);
        args.Add(new TemplateArg("related", relatedTo.Value));
      }

      if (host.IsNotNullOrWhiteSpace())
      {
        wb.AppendFormat(", {0}:'$$host'", Message.BSON_FLD_HOST);
        args.Add(new TemplateArg("host", host));
      }

      if (topic.IsNotNullOrWhiteSpace())
      {
        wb.AppendFormat(", {0}:'$$host'", Message.BSON_FLD_TOPIC);
        args.Add(new TemplateArg("topic", topic));
      }

      var where = "$query: { " + wb.ToString() + "},";
      return new Query(@"{{ {0} $orderby: {{ {1}:1 }} }}".Args(where, Message.BSON_FLD_TIMESTAMP), true, args.ToArray());
    }
  }
}
