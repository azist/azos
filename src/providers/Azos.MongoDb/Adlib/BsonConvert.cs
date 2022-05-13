/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Data.Access.MongoDb.Connector;
using Azos.Serialization.BSON;
using Azos.Serialization.JSON;

namespace Azos.Data.Adlib.Server
{
  public static class BsonConvert
  {
    public const string ADLIB_COLLECTION_PREFIX = "adl_";

    public const string FLD_GDID = Query._ID;
    public const string FLD_SHARDTOPIC = "shrd";
    public const string FLD_CREATEUTC = "cutc";
    public const string FLD_ORIGIN = "org";
    public const string FLD_HEADERS = "hdr";
    public const string FLD_CONTENT_TYPE = "ctp";
    public const string FLD_CONTENT = "cont";

    public const string FLD_TAGS = "tags";

    public const string FLD_TAG_PROP = "p";
    public const string FLD_TAG_VAL  = "v";


    public static Atom MongoToCanonicalCollectionName(string mongoName)
    {
      mongoName.NonBlankMin(ADLIB_COLLECTION_PREFIX.Length + 1);
      var canonical = mongoName.Substring(ADLIB_COLLECTION_PREFIX.Length);
      return Atom.Encode(canonical);
    }

    private static Platform.FiniteSetLookup<Atom, string> REF_POOL = new Platform.FiniteSetLookup<Atom, string>(a => ADLIB_COLLECTION_PREFIX + a.Value);
    public static string CanonicalCollectionNameToMongo(Atom collection) => REF_POOL.Get(collection);


    public static BSONDocument CreateIndex(Atom cname) //https://www.mongodb.com/docs/manual/reference/command/createIndexes/
     => new BSONDocument(@"{
          createIndexes: '##CNAME##',
          indexes: [{key: {tags.v: 1, tags.p: 1}, name: 'idx_##CNAME##_tags', unique: false}]
        }".Replace("##CNAME##", CanonicalCollectionNameToMongo(cname)), false);


    public static BSONDocument ToBson(Item item)
    {
      var doc = new BSONDocument();

      doc.Set(DataDocConverter.GDID_CLRtoBSON(FLD_GDID, item.Gdid));

      if (item.ShardTopic != null)
        doc.Set(new BSONStringElement(FLD_SHARDTOPIC, item.ShardTopic));
      else
        doc.Set(new BSONNullElement(FLD_SHARDTOPIC));

      doc.Set(new BSONInt64Element(FLD_CREATEUTC, (long)item.CreateUtc));
      doc.Set(new BSONInt64Element(FLD_ORIGIN, (long)item.Origin.ID));

      doc.Set(new BSONStringElement(FLD_HEADERS, item.Headers));
      doc.Set(new BSONInt64Element(FLD_CONTENT_TYPE, (long)item.ContentType.ID));

      if (item.Content != null)
        doc.Set(new BSONBinaryElement(FLD_CONTENT, new BSONBinary(BSONBinaryType.GenericBinary, item.Content )));
      else
        doc.Set(new BSONNullElement(FLD_CONTENT));

      if (item.Tags==null)
      {
        doc.Set(new BSONNullElement(FLD_TAGS));
      }
      else
      {
        var btags = item.Tags.Select(t => new BSONDocumentElement(
             new BSONDocument().Set(  new BSONInt64Element(FLD_TAG_PROP, (long)t.Prop.ID)  )
                               .Set(  t.IsText ? new BSONStringElement(FLD_TAG_VAL, t.SValue) as BSONElement
                                               : new BSONInt64Element(FLD_TAG_VAL, t.NValue))
        )).ToArray();
        doc.Set(new BSONArrayElement(FLD_TAGS, btags));
      }

      return doc;
    }

    public static Item ItemFromBson(Atom space, Atom collection, BSONDocument bson, Action<string> log)
    {
      var item = new Item
      {
        Space = space,
        Collection = collection
      };

      item.Gdid = bson[FLD_GDID] is BSONBinaryElement binGdid ? DataDocConverter.GDID_BSONtoCLR(binGdid) : GDID.ZERO;
      item.ShardTopic = bson[FLD_SHARDTOPIC] is BSONStringElement stopic ? stopic.Value : null;
      item.CreateUtc = bson[FLD_CREATEUTC] is BSONInt64Element cutc ? (ulong)cutc.Value : 0ul;
      item.Origin = bson[FLD_ORIGIN] is BSONInt64Element orig ? new Atom((ulong)orig.Value) : Atom.ZERO;
      item.Headers = bson[FLD_HEADERS] is BSONStringElement hdrs ? hdrs.Value : null;
      item.ContentType = bson[FLD_CONTENT_TYPE] is BSONInt64Element ctp ? new Atom((ulong)ctp.Value) : Atom.ZERO;
      item.Content = bson[FLD_CONTENT] is BSONBinaryElement bin ? bin.Value.Data : null;

      var tags = bson[FLD_TAGS] as BSONArrayElement;
      if (tags != null)
      {
        item.Tags = new List<Tag>();
        foreach(var tagDocElm in tags.Value.OfType<BSONDocumentElement>())
        {
          Tag tag;
          BSONDocument tagDoc = tagDocElm.Value;
          var prop = tagDoc[FLD_TAG_PROP] is BSONInt64Element pelm ? new Atom((ulong)pelm.Value) : Atom.ZERO;
          if (prop.IsZero || !prop.IsValid)
          {
            log("Corrupted tag data for GDID='{0}': prop id".Args(item.Gdid));
            continue;
          }

          var val = tagDoc[FLD_TAG_VAL];

          if (val is BSONStringElement selm)
          {
            tag = new Tag(prop, selm.Value);
          }
          else if (val is BSONInt64Element lelm)
          {
            tag = new Tag(prop, lelm.Value);
          }
          else
          {
            log("Corrupted tag '{1}' data for GDID='{0}': !sval | !nval".Args(item.Gdid, prop));
            continue;
          }

          item.Tags.Add(tag);
        }
      }

      return item;
    }

    public static (Query qry, BSONDocument selector) GetFilterQuery(ItemFilter filter)
    {
      BSONDocument selector = null;//all
      if (!filter.FetchContent || !filter.FetchTags)
      {
        selector = new BSONDocument();
        selector.Set(new BSONInt32Element(FLD_GDID, 1));
        selector.Set(new BSONInt32Element(FLD_CREATEUTC, 1));
        selector.Set(new BSONInt32Element(FLD_ORIGIN, 1));
        selector.Set(new BSONInt32Element(FLD_HEADERS, 1));
        selector.Set(new BSONInt32Element(FLD_CONTENT_TYPE, 1));
        if (filter.FetchContent) selector.Set(new BSONInt32Element(FLD_CONTENT, 1));
        if (filter.FetchTags)    selector.Set(new BSONInt32Element(FLD_TAGS, 1));
      }

      var qry =  buildQueryDoc(filter);

      return (qry, selector);
    }

    private static TagXlat s_TagXlat = new TagXlat();

    private static Query buildQueryDoc(ItemFilter filter)
    {
      if (!filter.Gdid.IsZero) Query.ID_EQ_GDID(filter.Gdid);
      if (filter.TagFilter==null) return new Query();

      var ctx = s_TagXlat.TranslateInContext(filter.TagFilter);
      return ctx.Query;
    }

  }
}
