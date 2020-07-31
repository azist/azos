/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Log;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Serialization.BSON;
using Azos.Serialization.JSON;

namespace Azos.Sky.Chronicle.Server
{
  public static class BsonConvert
  {
    public const string FLD_GDID = Query._ID;

    public const string FLD_GUID = "id";
    public const string FLD_RELATED_TO = "rel";

    public const string FLD_CHANNEL = "c";
    public const string FLD_APP = "a";
    public const string FLD_TYPE = "t";
    public const string FLD_SOURCE = "s";
    public const string FLD_TIMESTAMP = "utc";
    public const string FLD_HOST = "h";
    public const string FLD_FROM = "f";
    public const string FLD_TOPIC = "t";
    public const string FLD_TEXT = "txt";
    public const string FLD_PARAMETERS = "par";
    public const string FLD_WED = "wexd";

    public const string FLD_AD = "ad";



    public static BSONDocument ToBson(Message msg)
    {
      var doc = new BSONDocument();

      doc.Set( DataDocConverter.GDID_CLRtoBSON(FLD_GDID, msg.Gdid));
      doc.Set( DataDocConverter.GUID_CLRtoBSON(FLD_GUID, msg.Guid));
      doc.Set( DataDocConverter.GUID_CLRtoBSON(FLD_RELATED_TO, msg.RelatedTo));
      doc.Set(new BSONInt64Element(FLD_CHANNEL, (long)msg.Channel.ID));
      doc.Set(new BSONInt32Element(FLD_TYPE, (int)msg.Type));
      doc.Set(new BSONInt32Element(FLD_SOURCE, msg.Source));
      doc.Set(new BSONDateTimeElement(FLD_TIMESTAMP, msg.UTCTimeStamp));

      if (msg.Host.IsNullOrWhiteSpace())
         doc.Set(new BSONNullElement(FLD_HOST));
      else
         doc.Set(new BSONStringElement(FLD_HOST, msg.Host));

      if (msg.From.IsNullOrWhiteSpace())
        doc.Set(new BSONNullElement(FLD_FROM));
      else
        doc.Set(new BSONStringElement(FLD_FROM, msg.From));

      if (msg.Topic.IsNullOrWhiteSpace())
        doc.Set(new BSONNullElement(FLD_TOPIC));
      else
        doc.Set(new BSONStringElement(FLD_TOPIC, msg.Topic));

      if (msg.Text.IsNullOrWhiteSpace())
        doc.Set(new BSONNullElement(FLD_TEXT));
      else
        doc.Set(new BSONStringElement(FLD_TEXT, msg.Text));

      if (msg.Parameters.IsNullOrWhiteSpace())
        doc.Set(new BSONNullElement(FLD_PARAMETERS));
      else
        doc.Set(new BSONStringElement(FLD_PARAMETERS, msg.Parameters));

      if (msg.ExceptionData != null)
        doc.Set(new BSONStringElement(FLD_WED, msg.ExceptionData.ToJson(JsonWritingOptions.CompactRowsAsMap)));
      else
        doc.Set(new BSONNullElement(FLD_WED));

      var ad = ArchiveConventions.DecodeArchiveDimensionsMap(msg);
      if (ad==null)
      {
        doc.Set(new BSONNullElement(FLD_AD));
      }
      else
      {
        //map archive dimensions into BsonDoc
        var adDoc = new BSONDocument();
        doc.Set(new BSONDocumentElement(FLD_AD, adDoc));
      }

      return doc;
    }

    public static Message FromBson(BSONDocument bson)
    {
      return null;
    }
  }
}
