/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Data;
using Azos.Conf;
using Azos.Data.Access.MongoDb.Connector;
using MongoQuery = Azos.Data.Access.MongoDb.Connector.Query;
using Azos.Instrumentation;
using Azos.Serialization.BSON;

using Azos.Sky.MongoDb;

namespace Azos.Sky.Kdb { public sealed partial class DefaultKdbStore {


  public const string CONFIG_SHARD_SECTION = "shard";
  public const string CONFIG_FALLBACK_SECTION = "fallback";
  public const string CONFIG_ORDER_ATTR = "order";
  public const string CONFIG_PRIMARY_CONNECT_STRING_ATTR = "primary-cs";
  public const string CONFIG_SECONDARY_CONNECT_STRING_ATTR = "secondary-cs";

  /// <summary>
  /// Represents partition within the area
  /// </summary>
  public sealed class ShardSet : KdbAppComponent
  {
    internal ShardSet(IApplicationComponent director, IConfigSectionNode config) : base(director)
    {
      //Shards
      var shards = new List<Shard>();
      foreach(var snode in config.Children.Where( cn => cn.IsSameName(CONFIG_SHARD_SECTION)))
      {
        var shard = new Shard(this, snode);
        shards.Add( shard );
      }

      if (shards.Count==0)
        throw new KdbException(StringConsts.KDB_SHARDSET_NO_SHARDS_ERROR + config.RootPath);

      if (shards.Count != shards.Select(sh=>sh.Order).Distinct().Count())
        throw new KdbException(StringConsts.KDB_SHARDSET_DUPLICATE_SHARD_ORDER_ERROR + config.RootPath);

      shards.Sort();
      m_Shards = shards.ToArray();

      var nfb = config[CONFIG_FALLBACK_SECTION];
      if (nfb.Exists)
        m_Fallback = new ShardSet(this, nfb);
    }

    private Shard[] m_Shards;
    private ShardSet m_Fallback;

    public Shard[] Shards             { get{ return m_Shards; }}
    public ShardSet Fallback          { get{ return m_Fallback; }}

    public ShardSet FallbackParent    { get { return ComponentDirector as ShardSet; } }
    public int FallbackLevel          { get { return FallbackParent == null ? 0 : FallbackParent.FallbackLevel + 1; } }

    public DefaultKdbStore Store
    {
      get
      {
        var store = ComponentDirector as DefaultKdbStore;
        return store != null ? store : ((ShardSet)ComponentDirector).Store;
      }
    }

    /// <summary>
    /// Finds appropriate shard for key. See MDB.ShardingUtils
    /// </summary>
    public Shard GetShardForKey(byte[] key)
    {
      ulong subid = new ShardKey(key).Hash;

      return Shards[ subid % (ulong)Shards.Length ];
    }

    public override string ToString()
    {
      return "ShardSet({0}, {1})".Args(m_Shards.Length, m_Fallback == null ? SysConsts.NULL : m_Fallback.ToString());
    }
  }//ShardSet




  /// <summary>
  /// Denotes connection types Primary/Secondary
  /// </summary>
  public enum ShardBackendConnection{Primary=0, Secondary}

  /// <summary>
  /// Represents a SHARD information for the DB particular host
  /// </summary>
  public sealed class Shard : KdbAppComponent, IComparable<Shard>
  {
    internal Shard(ShardSet set, IConfigSectionNode config) : base(set)
    {
      m_Order = config.AttrByName(CONFIG_ORDER_ATTR).ValueAsInt(0);

      PrimaryHostConnectString = ConfigStringBuilder.Build(config, CONFIG_PRIMARY_CONNECT_STRING_ATTR);
      SecondaryHostConnectString = ConfigStringBuilder.Build(config, CONFIG_SECONDARY_CONNECT_STRING_ATTR);

      if (PrimaryHostConnectString.IsNullOrWhiteSpace())
        throw new KdbException(StringConsts.KDB_SHARDSET_CONFIG_SHARD_CSTR_ERROR.Args(CONFIG_PRIMARY_CONNECT_STRING_ATTR, config.RootPath));

      if (SecondaryHostConnectString.IsNullOrWhiteSpace())
        throw new KdbException(StringConsts.KDB_SHARDSET_CONFIG_SHARD_CSTR_ERROR.Args(CONFIG_SECONDARY_CONNECT_STRING_ATTR, config.RootPath));
    }

    private int m_Order;
    private ShardBackendConnection  m_ConnectionType;

    public ShardSet ShardSet         { get{ return ComponentDirector as ShardSet; }}
    public DefaultKdbStore Store     { get{ return ShardSet.Store; }}
    public int Order                 { get{ return m_Order; }}


    public readonly string PrimaryHostConnectString;
    public readonly string SecondaryHostConnectString;


    /// <summary>
    /// Returns Primary then secondary connect strings
    /// </summary>
    public IEnumerable<string> ConnectStrings
    {
      get
      {
        yield return PrimaryHostConnectString;
        yield return SecondaryHostConnectString;
      }
    }


    /// <summary>
    /// Returns either primary or secondary connect string
    /// depending on connection type
    /// </summary>
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public ShardBackendConnection ConnectionType
    {
      get
      {
        return m_ConnectionType;
      }
      set
      {
        if (m_ConnectionType!=value)
        {
          m_ConnectionType = value;
          //todo Instrument
        }
      }
    }

    /// <summary>
    /// Returns either primary or secondary connect string
    /// depending on connection type
    /// </summary>
    public string EffectiveConnectionString
    {
      get
      {
        return m_ConnectionType==ShardBackendConnection.Primary ?
                PrimaryHostConnectString : SecondaryHostConnectString;
      }
    }

    public override string ToString()
    {
      return "Shard({0}, '{1}')".Args(m_Order, EffectiveConnectionString);
    }

    public int CompareTo(Shard other)
    {
      if (other == null) return -1;
      return this.Order.CompareTo(other.Order);
    }

    public const string FIELD_VALUE = "v";
    public const string FIELD_LAST_USE_DATE = "d";
    public const string FIELD_ABSOLUTE_EXPIRATION_DATEUTC = "a";
    public const string FIELD_SLIDING_EXPIRATION_DAYS = "s";


     /* BSON Document Schema:
      * ----------------
      *
      * {_id: key, v: {}|binary, lastUseDate: dateUTC, absoluteExpirationDateUTC: date|null, slidingExpirationDays: int|null}
      * actual field names:
      * {_id: key, v: {}|binary, d: dateUTC, a: date|null, s: int|null}
      */

    internal KdbRecord<TDoc> Get<TDoc>(string table, byte[] key) where TDoc : Doc
    {

      var db = App.GetMongoDatabaseFromConnectString(EffectiveConnectionString);
      var doc = db[table].FindOne(MongoQuery.ID_EQ_BYTE_ARRAY(key));

      if (doc == null) return KdbRecord<TDoc>.Unassigned;
      var elmValue = doc[FIELD_VALUE] as BSONDocumentElement;
      if (elmValue == null) return KdbRecord<TDoc>.Unassigned;

      DateTime lastUseDate;
      DateTime? absoluteExpirationDateUTC;
      int slidingExpirationDays;
      readAttrs(table, doc, out lastUseDate, out absoluteExpirationDateUTC, out slidingExpirationDays);

      var value = elmValue.Value;

      TDoc row;
      if (value == null)
      {
         row = Doc.MakeDoc(new Schema(Guid.NewGuid().ToString()), typeof(TDoc)) as TDoc;
      }
      else
      {
        var schema = Store.m_Converter.InferSchemaFromBSONDocument(value);
        row = Doc.MakeDoc(schema, typeof(TDoc)) as TDoc;
        Store.m_Converter.BSONDocumentToDataDoc(value, row, null);
      }
      return new KdbRecord<TDoc>(row, slidingExpirationDays, lastUseDate, absoluteExpirationDateUTC);
    }



    internal KdbRecord<byte[]> GetRaw(string table, byte[] key)
    {
      var db = App.GetMongoDatabaseFromConnectString(EffectiveConnectionString);
      var doc = db[table].FindOne(MongoQuery.ID_EQ_BYTE_ARRAY(key));

      if (doc == null) return KdbRecord<byte[]>.Unassigned;
      var elmValue = doc[FIELD_VALUE] as BSONBinaryElement;
      if (elmValue == null) return KdbRecord<byte[]>.Unassigned;

      DateTime lastUseDate;
      DateTime? absoluteExpirationDateUTC;
      int slidingExpirationDays;
      readAttrs(table, doc, out lastUseDate, out absoluteExpirationDateUTC, out slidingExpirationDays);

      var value = elmValue.Value;

      return new KdbRecord<byte[]>(value.Data, slidingExpirationDays, lastUseDate, absoluteExpirationDateUTC);
    }


                private void readAttrs(string tbl, BSONDocument doc, out DateTime lastUse, out DateTime? absExp, out int sliding)
                {
                  var elmLastUse = doc[FIELD_LAST_USE_DATE] as BSONDateTimeElement;
                  if (elmLastUse==null)
                  {
                    Store.Log(Azos.Log.MessageType.Error, "GetX().readAttrs", "Table '{0}' DB doc has no '{1}'".Args(tbl, FIELD_LAST_USE_DATE));
                    lastUse = DateTime.MinValue;
                  }

                  lastUse = elmLastUse.Value;

                  var elmAbsExp = doc[FIELD_ABSOLUTE_EXPIRATION_DATEUTC];

                  if (elmAbsExp is BSONDateTimeElement)
                    absExp = ((BSONDateTimeElement)elmAbsExp).Value;
                  else
                    absExp = null;


                  var elmSE = doc[FIELD_SLIDING_EXPIRATION_DAYS];

                  if (elmSE is BSONInt32Element)
                   sliding = ((BSONInt32Element)elmSE).Value;
                  else
                   sliding = -1;
                }


    internal void Put(string table, byte[] key, Doc value, int slidingExpirationDays, DateTime? absoluteExpirationDateUtc)
    {
      var elmValue = Store.m_Converter.DataDocToBSONDocumentElement(value, null, name: FIELD_VALUE);
      putCore(table, key, elmValue, slidingExpirationDays, absoluteExpirationDateUtc);
    }

    internal void PutRaw(string table, byte[] key, byte[] value, int slidingExpirationDays, DateTime? absoluteExpirationDateUtc)
    {
      var elmValue = DataDocConverter.ByteBuffer_CLRtoBSON(FIELD_VALUE, value);
      putCore(table, key, elmValue, slidingExpirationDays, absoluteExpirationDateUtc);
    }

    private void putCore(string table, byte[] key, BSONElement value, int slidingExpirationDays, DateTime? absoluteExpirationDateUtc)
    {
      //todo: Why do we obtain ref to db on very put, need to consider cache for speed
      var db = App.GetMongoDatabaseFromConnectString(EffectiveConnectionString);

      var doc = new BSONDocument()
         .Set(DataDocConverter.ByteBufferID_CLRtoBSON(MongoQuery._ID, key))
         .Set(value)
         .Set(new BSONDateTimeElement(FIELD_LAST_USE_DATE, App.TimeSource.UTCNow))
         .Set(absoluteExpirationDateUtc.HasValue
                ? (BSONElement)new BSONDateTimeElement(FIELD_ABSOLUTE_EXPIRATION_DATEUTC, absoluteExpirationDateUtc.Value)
                : new BSONNullElement(FIELD_ABSOLUTE_EXPIRATION_DATEUTC))
         .Set(slidingExpirationDays > -1
                ? (BSONElement)new BSONInt32Element(FIELD_SLIDING_EXPIRATION_DAYS, slidingExpirationDays)
                : new BSONNullElement(FIELD_SLIDING_EXPIRATION_DAYS));

      db[table].Save(doc);
    }

    internal bool Delete(string table, byte[] key)
    {
      //todo: Why do we obtain ref to db every time, need to consider cache for speed
      var db = App.GetMongoDatabaseFromConnectString(EffectiveConnectionString);

      return db[table].DeleteOne(MongoQuery.ID_EQ_BYTE_ARRAY(key)).TotalDocumentsAffected > 0;
    }

    internal void Touch(string table, byte[] key)
    {
      //todo: Why do we obtain ref to db every time, need to consider cache for speed
      var db = App.GetMongoDatabaseFromConnectString(EffectiveConnectionString);
      var udoc = new BSONDocument()
                   .Set(new BSONDateTimeElement(FIELD_LAST_USE_DATE, App.TimeSource.UTCNow));

      //Need to update document with $set to prevent document clear
      udoc = new BSONDocument().Set(new BSONDocumentElement("$set", udoc));

      db[table].Update(new UpdateEntry(MongoQuery.ID_EQ_BYTE_ARRAY(key), udoc, multi: false, upsert: false));
    }
  }

  }
}
