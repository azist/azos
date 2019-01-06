/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Serialization.BSON;
using Azos.Data.Access.MongoDb.Connector;

using Azos.Sky.MongoDb;

namespace Azos.Sky.Workers.Server.Queue
{
  /// <summary>
  /// Implements TodoQueueStore using MongoDB
  /// </summary>
  public sealed class MongoTodoQueueStore : TodoQueueStore
  {
    public string CONFIG_MONGO_SECTION = "mongo";
    public string CONFIG_CONVERTER_SECTION = "converter";

    public const int DEFAULT_FETCHBY_SIZE = 32;
    public const int MAX_FETCHBY_SIZE = 4 * 1024;

    public const int FULL_BATCH_SIZE = 1024;

    public const string FLD_TODO_TYPE            = "t";
    public const string FLD_TODO_CREATETIMESTAMP = "cts";
    public const string FLD_TODO_SHARDINGKEY     = "sk";
    public const string FLD_TODO_PARALLELKEY     = "plk";
    public const string FLD_TODO_PRIORITY        = "pri";
    public const string FLD_TODO_STARTDATE       = "sd";
    public const string FLD_TODO_CORRELATIONKEY  = "cky";
    public const string FLD_TODO_STATE           = "st";
    public const string FLD_TODO_TRIES           = "tr";
    public const string FLD_TODO_SERIALIZER      = "sr";
    public const string FLD_TODO_CONTENT         = "c";



    public MongoTodoQueueStore(TodoQueueService director, IConfigSectionNode node) : base(director, node)
    {
      var cstring = ConfigStringBuilder.Build(node, CONFIG_MONGO_SECTION);
      m_Database = App.GetMongoDatabaseFromConnectString( cstring );
      m_Converter = FactoryUtils.MakeAndConfigure<DataDocConverter>( node[CONFIG_CONVERTER_SECTION], typeof(DataDocConverter));
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Database);
      base.Destructor();
    }


    private Database m_Database;
    private DataDocConverter m_Converter;
    private int m_FetchBy = DEFAULT_FETCHBY_SIZE;


    [Config(Default = DEFAULT_FETCHBY_SIZE)]
    public int FetchBy
    {
        get {return m_FetchBy;}
        private set
        {
          m_FetchBy = value<1 ? 1 : value > MAX_FETCHBY_SIZE ? MAX_FETCHBY_SIZE : value;
        }
    }



    public override object BeginTransaction(TodoQueue queue) { return null; }
    public override void CommitTransaction(TodoQueue queue, object transaction) { }
    public override void RollbackTransaction(TodoQueue queue, object transaction) { }


    public override int GetInboundCapacity(TodoQueue queue)
    {
      return TodoQueueService.FULL_BATCH_SIZE;
    }

    public override TodoFrame FetchLatestCorrelated(TodoQueue queue, string correlationKey, DateTime utcStartingFrom)
    {
      if (Disposed || queue==null || correlationKey==null) return new TodoFrame();

      var collection = m_Database[queue.Name];


      var query = new Query(
      @"{
         '$query': { cky: '$$cky', sd: {'$gt': '$$utcFrom'} },
         '$orderby': { sd: -1}
        }",
      true, new TemplateArg(new BSONStringElement("cky", correlationKey)),
            new TemplateArg(new BSONDateTimeElement("utcFrom", utcStartingFrom)) );

      var doc = collection.FindOne(query);
      if (doc==null) return new TodoFrame();

      var frame = toFrame(queue, doc);
      return frame;
    }



    public override IEnumerable<TodoFrame> Fetch(TodoQueue queue, DateTime utcNow)
    {
      if (Disposed || queue==null) return Enumerable.Empty<TodoFrame>();

      var collection = m_Database[queue.Name];

#warning This needs revision
      //todo Napisat query - kak sdelat server-side SORT? mopjet sdelat ID ne GDID a vremya?
      //{ $query: {}, $orderby: { dt: 1 } }
      // akto sdelaet v mongo index? mojet ne xranit null v __sd a xranit MIN date vmesto null?

      //Mojet zdes nado sdelat Db.RunCommand i peredat sort tuda? Nujen index!
      //https://docs.mongodb.com/manual/reference/command/find/#dbcmd.find
      //kak eto vliyaet na mongo sort size buffer (32 mb? tam ili chtoto typa togo)?
      var query = new Query(
      @"{
         '$query': { sd: {'$lt': '$$now'} },
         '$orderby': { sd: 1}
        }",
      true, new TemplateArg(new BSONDateTimeElement("now", utcNow)));

      var total = queue.BatchSize;

      var result = new List<TodoFrame>();

      var fby = FetchBy;
      if (fby > total) fby = total;

      using(var cursor = collection.Find(query, fetchBy: fby))
        foreach(var doc in cursor)
        {
          var todo = toFrame(queue, doc);
          result.Add( todo );
          var left = total - result.Count;
          if (left==0) break;
          if (cursor.FetchBy > left) cursor.FetchBy = left;
        }

      return result;
    }


    public override void Complete(TodoQueue queue, TodoFrame todo, Exception error = null, object transaction = null)
    {
      if (Disposed || queue==null || !todo.Assigned) return;

      //this provider ignores exceptions passed in error var
      m_Database[queue.Name].DeleteOne( Query.ID_EQ_GDID(todo.ID) );
    }


    public override void Put(TodoQueue queue, TodoFrame todo, object transaction)
    {
      if (Disposed || queue==null || !todo.Assigned) return;
      var tName = queue.Name;
      var doc = toBSON(queue, todo);
      m_Database[tName].Insert( doc );
    }

    public override void Update(TodoQueue queue, Todo todo, bool sysOnly, object transaction)
    {
      if (Disposed || queue==null || todo==null) return;

      var frame = new TodoFrame(todo, sysOnly ? (int?)null : TodoFrame.SERIALIZER_BSON);
      var doc = toBSONUpdate(queue, frame, sysOnly);
      m_Database[queue.Name].Update(new UpdateEntry(Query.ID_EQ_GDID(todo.SysID), doc, false, false));
    }



    private BSONElement elmStr(string name, string value)
    {
      if (value==null) return new BSONNullElement(name);
      return new BSONStringElement(name, value);
    }

    private string elmStr(BSONElement elm)
    {
      if (elm==null || elm is BSONNullElement) return null;
      return ((BSONStringElement)elm).Value;
    }

    private BSONElement elmBin(string name, byte[] value)
    {
      if (value==null) return new BSONNullElement(name);
      return new BSONBinaryElement(name, new BSONBinary(BSONBinaryType.GenericBinary, value));
    }

    private byte[] elmBin(BSONElement elm)
    {
      if (elm==null || elm is BSONNullElement) return null;
      return ((BSONBinaryElement)elm).Value.Data;
    }

    private BSONDocument toBSON(TodoQueue queue, TodoFrame todo)
    {
      var result = new BSONDocument();
      var t = todo.GetType();

      result.Set( DataDocConverter.GDID_CLRtoBSON( Query._ID, todo.ID) );

      result.Set( new BSONStringElement(FLD_TODO_TYPE, todo.Type.ToString()) );
      result.Set( new BSONDateTimeElement(FLD_TODO_CREATETIMESTAMP, todo.CreateTimestampUTC) );

      result.Set( elmStr(FLD_TODO_SHARDINGKEY, todo.ShardingKey) );
      result.Set( elmStr(FLD_TODO_PARALLELKEY, todo.ParallelKey) );
      result.Set( new BSONInt32Element(FLD_TODO_PRIORITY, todo.Priority) );
      result.Set( new BSONDateTimeElement(FLD_TODO_STARTDATE, todo.StartDate) );
      result.Set( elmStr(FLD_TODO_CORRELATIONKEY, todo.CorrelationKey) );
      result.Set( new BSONInt32Element(FLD_TODO_STATE, todo.State) );
      result.Set( new BSONInt32Element(FLD_TODO_TRIES, todo.Tries) );

      result.Set( new BSONInt32Element(FLD_TODO_SERIALIZER, todo.Serializer) );
      result.Set( elmBin(FLD_TODO_CONTENT, todo.Content) );

      return result;
    }

    private BSONDocument toBSONUpdate(TodoQueue queue, TodoFrame todo, bool sysOnly)
    {
      var setDoc = new BSONDocument();

      setDoc.Set(elmStr(FLD_TODO_SHARDINGKEY, todo.ShardingKey));
      setDoc.Set(elmStr(FLD_TODO_PARALLELKEY, todo.ParallelKey));
      setDoc.Set(new BSONInt32Element(FLD_TODO_PRIORITY, todo.Priority));
      setDoc.Set(new BSONDateTimeElement(FLD_TODO_STARTDATE, todo.StartDate));
      setDoc.Set(elmStr(FLD_TODO_CORRELATIONKEY, todo.CorrelationKey));
      setDoc.Set(new BSONInt32Element(FLD_TODO_STATE, todo.State));
      setDoc.Set(new BSONInt32Element(FLD_TODO_TRIES, todo.Tries));

      if (!sysOnly)
      {
        setDoc.Set(new BSONInt32Element(FLD_TODO_SERIALIZER, todo.Serializer));
        setDoc.Set(elmBin(FLD_TODO_CONTENT, todo.Content));
      }

      var result = new BSONDocument();

      result.Set(new BSONDocumentElement("$set", setDoc));

      return result;
    }


    private TodoFrame toFrame(TodoQueue queue, BSONDocument doc)
    {
      try
      {
        var result = new TodoFrame();

        result.ID = DataDocConverter.GDID_BSONtoCLR( doc[Query._ID] as BSONBinaryElement );
        result.Type = Guid.Parse( ((BSONStringElement)doc[FLD_TODO_TYPE]).Value );
        result.CreateTimestampUTC = ((BSONDateTimeElement)doc[FLD_TODO_CREATETIMESTAMP]).Value;

        result.ShardingKey = elmStr( doc[FLD_TODO_SHARDINGKEY] );
        result.ParallelKey = elmStr( doc[FLD_TODO_PARALLELKEY] );

        result.Priority = ((BSONInt32Element)doc[FLD_TODO_PRIORITY]).Value;
        result.StartDate = ((BSONDateTimeElement)doc[FLD_TODO_STARTDATE]).Value;

        result.CorrelationKey = elmStr( doc[FLD_TODO_CORRELATIONKEY] );

        result.State = ((BSONInt32Element)doc[FLD_TODO_STATE]).Value;
        result.Tries = ((BSONInt32Element)doc[FLD_TODO_TRIES]).Value;

        result.Serializer = ((BSONInt32Element)doc[FLD_TODO_SERIALIZER]).Value;
        result.Content = elmBin( doc[FLD_TODO_CONTENT] );

        return result;
      }
      catch(Exception error)
      {
        throw new MongoWorkersException(StringConsts.TODO_QUEUE_BSON_READ_ERROR.Args(queue, error.ToMessageWithType()), error);
      }
    }

  }
}
