/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Log;
using Azos.Serialization.BSON;

namespace Azos.Sky.Chronicle.Feed.Server
{
  public sealed class MongoFactSink : Sink
  {
    public const string DEFAULT_DB = "sky_facts";

    public MongoFactSink(PullAgentDaemon director, IConfigSectionNode cfg) : base(director, cfg)
    {
      m_Db = App.GetMongoDatabaseFromConnectString(m_ConnectString, Name);
    }

    private Database m_Db;

    [Config] private string m_ConnectString;

    public override Task WriteAsync(IEnumerable<Message> data)
    {
      foreach(var msg in data)
      {
        var fact = Azos.Log.ArchiveConventions.LogMessageToFact(msg);

        var collection = m_Db[fact.FactType.Value];//name is from fact type
        var bson = factToBson(fact);
        collection.Insert(bson);
      }
      return Task.CompletedTask;//Mongo driver is sync
    }

    private BSONDocument factToBson(Fact fact)
    {
      var doc = new BSONDocument();

      doc.Set(DataDocConverter.GDID_CLRtoBSON("_id", fact.Gdid));
      doc.Set(DataDocConverter.GUID_CLRtoBSON("id", fact.Id));

      if (fact.RelatedId != Guid.Empty)
      {
        doc.Set(DataDocConverter.GUID_CLRtoBSON("rel", fact.RelatedId));
      }
      else
      {
        doc.Set(new BSONNullElement("rel"));
      }

      doc.Set(new BSONInt64Element("chn", (long)fact.Channel.ID));
      doc.Set(new BSONInt64Element("top", (long)fact.Topic.ID));
      doc.Set(new BSONInt64Element("app", (long)fact.App.ID));
      doc.Set(new BSONInt32Element("rtp", (int)fact.RecordType));
      doc.Set(new BSONInt32Element("src", fact.Source));
      doc.Set(new BSONDateTimeElement("utc", fact.UtcTimestamp));

      if (fact.Host != null)
      {
        doc.Set(new BSONStringElement("hst", fact.Host));
      }
      else
      {
        doc.Set(new BSONNullElement("hst"));
      }

      var dims = BSONExtensions.ToBson(fact.Dimensions);
      if (dims != null)
      {
        doc.Set(new BSONDocumentElement("dim", dims));
      }
      else
      {
        doc.Set(new BSONNullElement("dim"));
      }

      var metrics = BSONExtensions.ToBson(fact.Metrics);
      if (dims != null)
      {
        doc.Set(new BSONDocumentElement("met", metrics));
      }
      else
      {
        doc.Set(new BSONNullElement("met"));
      }

      return doc;
    }
  }
}
