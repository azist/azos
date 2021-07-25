/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Data;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Serialization.BSON;

namespace Azos.Sky.EventHub.Server
{
  /// <summary>
  /// Assists in creation of BSON representation of Events and vice versa
  /// </summary>
  internal static class BsonConvert
  {
    public const string FLD_GDID = Query._ID;

    public const string FLD_CREATEUTC     = "u";
    public const string FLD_ORIGIN        = "o";
    public const string FLD_CHECKPOINTUTC = "chk";
    public const string FLD_HEADERS       = "h";
    public const string FLD_CONTENTTYPE   = "ctp";
    public const string FLD_CONTENT       = "c";

    public static BSONDocument ToBson(Event evt)
    {
      var doc = new BSONDocument();
      doc.Set(DataDocConverter.GDID_CLRtoBSON(FLD_GDID, evt.Gdid));

      //long is enough to hold positive date (2^63) for 292 million years
      doc.Set(new BSONInt64Element(FLD_CREATEUTC, (long)evt.CreateUtc));
      doc.Set(new BSONInt64Element(FLD_ORIGIN, (long)evt.Origin.ID));
      doc.Set(new BSONInt64Element(FLD_CHECKPOINTUTC, (long)evt.CheckpointUtc));//same as above

      if (evt.Headers.IsNullOrWhiteSpace())
        doc.Set(new BSONNullElement(FLD_HEADERS));
      else
        doc.Set(new BSONStringElement(FLD_HEADERS, evt.Headers));

      doc.Set(new BSONInt64Element(FLD_CONTENTTYPE, (long)evt.ContentType.ID));
      doc.Set(DataDocConverter.ByteBuffer_CLRtoBSON(FLD_CONTENT, evt.Content));

      return doc;
    }

    public static Event FromBson(BSONDocument bson)
    {
      var gdid = bson[FLD_GDID] is BSONBinaryElement binGdid ? DataDocConverter.GDID_BSONtoCLR(binGdid) : GDID.ZERO;

      var createUtc     = bson[FLD_CREATEUTC] is BSONInt64Element cutc       ? (ulong)cutc.Value : 0ul;
      var origin        = bson[FLD_ORIGIN] is BSONInt64Element orig          ? new Atom((ulong)orig.Value) : Atom.ZERO;
      var checkpointUtc = bson[FLD_CHECKPOINTUTC] is BSONInt64Element chkutc ? (ulong)chkutc.Value : 0ul;

      var headers     = bson[FLD_HEADERS] is BSONStringElement hdrs   ? hdrs.Value : null;

      var contentType = bson[FLD_CONTENTTYPE] is BSONInt64Element ctp ? new Atom((ulong)ctp.Value) : Atom.ZERO;
      var content     = bson[FLD_CONTENT] is BSONBinaryElement bin    ? bin.Value.Data : null;

      return Event.__AsDeserialized(gdid, createUtc, origin, checkpointUtc, headers, contentType, content);
    }

    private const string QRY = "{'$query': {'chk': {'$gt': '$$chk'}}, '$orderby': {'chk': 1}}";
    private static readonly BSONDocument FULL_SELECTOR = null;//SELECT *
    private static readonly BSONDocument ONLYID_SELECTOR = new BSONDocument("{_id: 1, u: 1, o: 1, chk: 1}", true);//SELECT _id, u, o, chk

    public static BSONDocument GetFetchSelector(bool onlyid)
     => onlyid ? ONLYID_SELECTOR : FULL_SELECTOR;

    public static Query GetFetchQuery(ulong checkpoint)
     => new Query(QRY, cacheTemplate: true, args: new TemplateArg(FLD_CHECKPOINTUTC, BSONElementType.Int64, (long) checkpoint));

  }
}
