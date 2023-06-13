/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Log;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Serialization.BSON;
using Azos.Serialization.JSON;

namespace Azos.Sky.Chronicle.Server
{
  internal static class BsonConvert
  {
    public const string FLD_GDID = Query._ID;
    public const string FLD_GUID = "id";
    public const string FLD_RELATED_TO = "rl";

    public const string FLD_CHANNEL   = "c";
    public const string FLD_APP       = "a";
    public const string FLD_TYPE      = "t";
    public const string FLD_SOURCE    = "s";
    public const string FLD_TIMESTAMP = "u";

    public const string FLD_HOST  = "h";
    public const string FLD_FROM  = "f";
    public const string FLD_TOPIC = "z";//topic aka "Zone" of application

    public const string FLD_TEXT       = "x";
    public const string FLD_PARAMETERS = "p";
    public const string FLD_EXCEPTION  = "e";

    public const string FLD_AD = "dim";



    public static BSONDocument ToBson(Message msg)
    {
      var doc = new BSONDocument();

      doc.Set( DataDocConverter.GDID_CLRtoBSON(FLD_GDID, msg.Gdid));
      doc.Set( DataDocConverter.GUID_CLRtoBSON(FLD_GUID, msg.Guid));
      doc.Set( DataDocConverter.GUID_CLRtoBSON(FLD_RELATED_TO, msg.RelatedTo));

      doc.Set(new BSONInt64Element(FLD_CHANNEL, (long)msg.Channel.ID));
      doc.Set(new BSONInt64Element(FLD_APP, (long)msg.App.ID));
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
        doc.Set(new BSONStringElement(FLD_EXCEPTION, msg.ExceptionData.ToJson(JsonWritingOptions.CompactRowsAsMap)));
      else
        doc.Set(new BSONNullElement(FLD_EXCEPTION));

      var ad = ArchiveConventions.DecodeArchiveDimensionsMap(msg);
      if (ad==null)
      {
        doc.Set(new BSONNullElement(FLD_AD));
      }
      else
      {
        var adDoc = Azos.Serialization.BSON.BSONExtensions.ToBson(ad);//Reviewed 05302023 DKh #872
        doc.Set(new BSONDocumentElement(FLD_AD, adDoc));
      }

      return doc;
    }

    public static Message FromBson(BSONDocument bson)
    {
      var msg = new Message();

      if (bson[FLD_GDID] is BSONBinaryElement binGdid)      msg.Gdid = DataDocConverter.GDID_BSONtoCLR(binGdid);
      if (bson[FLD_GUID] is BSONBinaryElement binGuid)      msg.Guid = DataDocConverter.GUID_BSONtoCLR(binGuid);
      if (bson[FLD_RELATED_TO] is BSONBinaryElement binRel) msg.RelatedTo = DataDocConverter.GUID_BSONtoCLR(binRel);

      if (bson[FLD_CHANNEL] is BSONInt64Element chn) msg.Channel = new Atom((ulong)chn.Value);
      if (bson[FLD_APP] is BSONInt64Element app) msg.App = new Atom((ulong)app.Value);
      if (bson[FLD_TYPE] is BSONInt32Element tp) msg.Type = (MessageType)tp.Value;
      if (bson[FLD_SOURCE] is BSONInt32Element src) msg.Source = src.Value;
      if (bson[FLD_TIMESTAMP] is BSONDateTimeElement utc) msg.UTCTimeStamp = utc.Value;

      if (bson[FLD_HOST] is BSONStringElement host) msg.Host = host.Value;
      if (bson[FLD_FROM] is BSONStringElement from) msg.From = from.Value;
      if (bson[FLD_TOPIC] is BSONStringElement topic) msg.Topic = topic.Value;

      if (bson[FLD_TEXT] is BSONStringElement text) msg.Text = text.Value;
      if (bson[FLD_PARAMETERS] is BSONStringElement pars) msg.Parameters = pars.Value;
      if (bson[FLD_EXCEPTION] is BSONStringElement except) msg.ExceptionData = JsonReader.ToDoc<WrappedExceptionData>(except.Value);
      if (bson[FLD_AD] is BSONDocumentElement ad) msg.ArchiveDimensions = ArchiveConventions.EncodeArchiveDimensions(ad.Value);

      return msg;
    }

  }
}
