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

namespace Azos.Sky.Workers.Server
{
  /// <summary>
  /// Implements TodoQueueStore using MongoDB
  /// </summary>
  public sealed class MongoProcessStore : ProcessStore
  {
    public string CONFIG_MONGO_SECTION = "mongo";
    public string CONFIG_CONVERTER_SECTION = "converter";

    public const int DEFAULT_FETCHBY_SIZE = 32;
    public const int MAX_FETCHBY_SIZE = 4 * 1024;

    public const int DEFAULT_BATCH_SIZE = 32;
    public const int MAX_BATCH_SIZE = 1024;

    public const string FLD_PROCESS_ZONE            = "z";
    public const string FLD_PROCESS_PROCESSOR_ID    = "p";
    public const string FLD_PROCESS_UNIQUE          = "u";

    public const string FLD_PROCESS_TYPE            = "t";

    public const string FLD_PROCESS_DESCRIPTION     = "ds";
    public const string FLD_PROCESS_TIMESTAMP       = "ts";
    public const string FLD_PROCESS_ABOUT           = "ab";

    public const string FLD_PROCESS_STATUS             = "st";
    public const string FLD_PROCESS_STATUS_DESCRIPTION = "sds";
    public const string FLD_PROCESS_STATUS_TIMESTAMP   = "sts";
    public const string FLD_PROCESS_STATUS_ABOUT       = "sab";

    public const string FLD_PROCESS_SERIALIZER      = "sr";
    public const string FLD_PROCESS_CONTENT         = "c";

    public MongoProcessStore(ProcessControllerService director, IConfigSectionNode node) : base(director, node)
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
    private int m_BatchSize = DEFAULT_BATCH_SIZE;

    [Config(Default = DEFAULT_FETCHBY_SIZE)]
    public int FetchBy
    {
      get { return m_FetchBy; }
      private set
      {
        m_FetchBy = value < 1 ? 1 : value > MAX_FETCHBY_SIZE ? MAX_FETCHBY_SIZE : value;
      }
    }

    [Config(Default = DEFAULT_BATCH_SIZE)]
    public int BatchSize
    {
      get { return m_BatchSize; }
      private set
      {
        m_BatchSize = value < 1 ? 1 : value > MAX_BATCH_SIZE ? MAX_BATCH_SIZE : value;
      }
    }

    public override IEnumerable<ProcessDescriptor> List(int processorID)
    {
      if (Disposed) return Enumerable.Empty<ProcessDescriptor>();

      var collection = m_Database["P" + processorID];

      var query = new Query(@"{'$query': {}}", true);

      var total = BatchSize;

      var result = new List<ProcessDescriptor>();

      var fby = FetchBy;
      if (fby > total) fby = total;

      using (var cursor = collection.Find(query, fetchBy: fby))
        foreach (var doc in cursor)
        {
          var descriptor = toDescriptor(doc);
          result.Add(descriptor);
          var left = total - result.Count;
          if (left == 0) break;
          if (cursor.FetchBy > left) cursor.FetchBy = left;
        }

      return result;
    }

    public override bool TryGetByPID(PID pid, out ProcessFrame frame)
    {
      var query = new Query(@"{ '$query': { _id: '$$id' } }", true, new TemplateArg(new BSONStringElement("id", pid.ID)));

      frame = new ProcessFrame();
      var doc = m_Database["P" + pid.ProcessorID].FindOne(query);
      if (doc == null) return false;

      frame = toFrame(doc);
      return true;
    }

    public override void Put(ProcessFrame frame, object transaction)
    {
      if (Disposed) return;
      var pid = frame.Descriptor.PID;
      var doc = toBSON(frame);
      m_Database["P" + pid.ProcessorID].Insert( doc );
    }

    public override void Update(ProcessFrame frame, bool sysOnly, object transaction)
    {
      if (Disposed) return;
      var pid = frame.Descriptor.PID;
      var doc = toBSONUpdate(frame, sysOnly);
      m_Database["P" + pid.ProcessorID].Update(new UpdateEntry(Query.ID_EQ_String(pid.ID), doc, false, false));
    }

    public override void Delete(ProcessFrame frame, object transaction)
    {
      if (Disposed) return;
      var pid = frame.Descriptor.PID;
      m_Database["P" + pid.ProcessorID].DeleteOne(Query.ID_EQ_String(pid.ID));
    }

    public override object BeginTransaction() { return null; }
    public override void CommitTransaction(object transaction) { }
    public override void RollbackTransaction(object transaction) { }

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

    private BSONDocument toBSON(ProcessFrame frame)
    {
      var result = new BSONDocument();
      var t = frame.GetType();
      var descriptor = frame.Descriptor;
      var pid = descriptor.PID;

      result.Set(new BSONStringElement(Query._ID, pid.ID));
      result.Set(new BSONStringElement(FLD_PROCESS_ZONE, pid.Zone));
      result.Set(new BSONInt32Element(FLD_PROCESS_PROCESSOR_ID, pid.ProcessorID));
      result.Set(new BSONBooleanElement(FLD_PROCESS_UNIQUE, pid.IsUnique));

      result.Set(new BSONStringElement(FLD_PROCESS_TYPE, frame.Type.ToString()));

      result.Set(elmStr(FLD_PROCESS_DESCRIPTION, descriptor.Description));
      result.Set(new BSONDateTimeElement(FLD_PROCESS_TIMESTAMP, descriptor.Timestamp));
      result.Set(elmStr(FLD_PROCESS_ABOUT, descriptor.About));
      result.Set(new BSONInt32Element(FLD_PROCESS_STATUS, (int)descriptor.Status));
      result.Set(elmStr(FLD_PROCESS_STATUS_DESCRIPTION, descriptor.StatusDescription));
      result.Set(new BSONDateTimeElement(FLD_PROCESS_STATUS_TIMESTAMP, descriptor.StatusTimestamp));
      result.Set(elmStr(FLD_PROCESS_STATUS_ABOUT, descriptor.StatusAbout));

      result.Set(new BSONInt32Element(FLD_PROCESS_SERIALIZER, frame.Serializer));
      result.Set(elmBin(FLD_PROCESS_CONTENT, frame.Content));

      return result;
    }

    private BSONDocument toBSONUpdate(ProcessFrame frame, bool sysOnly)
    {
      var setDoc = new BSONDocument();
      var descriptor = frame.Descriptor;

      setDoc.Set(new BSONInt32Element(FLD_PROCESS_STATUS, (int)descriptor.Status));
      setDoc.Set(elmStr(FLD_PROCESS_STATUS_DESCRIPTION, descriptor.StatusDescription));
      setDoc.Set(new BSONDateTimeElement(FLD_PROCESS_STATUS_TIMESTAMP, descriptor.StatusTimestamp));
      setDoc.Set(elmStr(FLD_PROCESS_STATUS_ABOUT, descriptor.StatusAbout));

      if (!sysOnly)
      {
        setDoc.Set(new BSONInt32Element(FLD_PROCESS_SERIALIZER, frame.Serializer));
        setDoc.Set(elmBin(FLD_PROCESS_CONTENT, frame.Content));
      }

      var result = new BSONDocument();

      result.Set(new BSONDocumentElement("$set", setDoc));

      return result;
    }

    private ProcessDescriptor toDescriptor(BSONDocument doc)
    {
      try
      {
        var id = ((BSONStringElement)doc[Query._ID]).Value;
        var zone = ((BSONStringElement)doc[FLD_PROCESS_ZONE]).Value;
        var processID = ((BSONInt32Element)doc[FLD_PROCESS_PROCESSOR_ID]).Value;
        var isUnique = ((BSONBooleanElement)doc[FLD_PROCESS_UNIQUE]).Value;

        var pid = new PID(zone, processID, id, isUnique);

        var description = elmStr(doc[FLD_PROCESS_DESCRIPTION]);
        var timestamp = ((BSONDateTimeElement)doc[FLD_PROCESS_TIMESTAMP]).Value;
        var about = elmStr(doc[FLD_PROCESS_ABOUT]);
        var status = (ProcessStatus)((BSONInt32Element)doc[FLD_PROCESS_STATUS]).Value;
        var statusDescription = elmStr(doc[FLD_PROCESS_STATUS_DESCRIPTION]);
        var statusTimestamp = ((BSONDateTimeElement)doc[FLD_PROCESS_STATUS_TIMESTAMP]).Value;
        var statusAbout = elmStr(doc[FLD_PROCESS_STATUS_ABOUT]);

        return new ProcessDescriptor(pid, description, timestamp, about, status, statusDescription, statusTimestamp, statusAbout);
      }
      catch (Exception error)
      {
        throw new MongoWorkersException(StringConsts.PROCESS_BSON_READ_ERROR.Args(error.ToMessageWithType()), error);
      }
    }

    private ProcessFrame toFrame(BSONDocument doc)
    {
      try
      {
        var descriptor = toDescriptor(doc);

        var result = new ProcessFrame();

        result.Type = Guid.Parse(((BSONStringElement)doc[FLD_PROCESS_TYPE]).Value);
        result.Descriptor = descriptor;

        result.Serializer = ((BSONInt32Element)doc[FLD_PROCESS_SERIALIZER]).Value;
        result.Content = elmBin(doc[FLD_PROCESS_CONTENT]);

        return result;
      }
      catch (Exception error)
      {
        throw new MongoWorkersException(StringConsts.PROCESS_BSON_READ_ERROR.Args(error.ToMessageWithType()), error);
      }
    }
  }
}
