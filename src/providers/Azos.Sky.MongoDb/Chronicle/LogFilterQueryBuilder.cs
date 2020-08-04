/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Data.AST;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Serialization.BSON;

namespace Azos.Sky.Chronicle.Server
{
  internal static class LogFilterQueryBuilder
  {
    private static MongoDbXlat m_LogXlat = new MongoDbXlat();

    public static Query BuildLogFilterQuery(LogChronicleFilter filter)
    {
      var query = new Query();

      var andNodes = new List<BSONDocumentElement>();

      void add(BSONElement elm) => andNodes.Add(new BSONDocumentElement(new BSONDocument().Set(elm)));


      if (!filter.Gdid.IsZero)
      {
        add(DataDocConverter.GDID_CLRtoBSON(BsonConvert.FLD_GDID, filter.Gdid) );
      }

      if (filter.Id.HasValue)
      {
        add(DataDocConverter.GUID_CLRtoBSON(BsonConvert.FLD_GUID, filter.Id.Value) );
      }

      if (filter.RelId.HasValue)
      {
        add(DataDocConverter.GUID_CLRtoBSON(BsonConvert.FLD_RELATED_TO, filter.RelId.Value));
      }

      if (filter.Channel.HasValue && !filter.Channel.Value.IsZero)
      {
        add(new BSONInt64Element(BsonConvert.FLD_CHANNEL, (long)filter.Channel.Value.ID));
      }

      if (filter.Application.HasValue && !filter.Application.Value.IsZero)
      {
        add(new BSONInt64Element(BsonConvert.FLD_APP, (long)filter.Application.Value.ID));
      }

      if (filter.TimeRange.HasValue && filter.TimeRange.Value.Start.HasValue)
      {
        add(new BSONDocumentElement(BsonConvert.FLD_TIMESTAMP, new BSONDocument().Set(new BSONDateTimeElement("$gte", filter.TimeRange.Value.Start.Value)) ));
      }

      if (filter.TimeRange.HasValue && filter.TimeRange.Value.End.HasValue)
      {
        add(new BSONDocumentElement(BsonConvert.FLD_TIMESTAMP, new BSONDocument().Set(new BSONDateTimeElement("$lte", filter.TimeRange.Value.End.Value))));
      }


      if (filter.MinType.HasValue)
      {
        add(new BSONDocumentElement(BsonConvert.FLD_TYPE, new BSONDocument().Set(new BSONInt32Element("$gte", (int)filter.MinType.Value))));
      }

      if (filter.MaxType.HasValue)
      {
        add(new BSONDocumentElement(BsonConvert.FLD_TYPE, new BSONDocument().Set(new BSONInt32Element("$lte", (int)filter.MaxType.Value))));
      }

      if (andNodes.Count > 0)
       query.Set(new BSONArrayElement("$and", andNodes.ToArray()));

      //todo: Finish the Advanced filter
      //var ctx = m_LogXlat.TranslateInContext(filter.AdvancedFilter);
      // var query = query;// ctx.Query;

      return query;
    }
  }
}
