using System;
using System.Text;

using Azos.Data;
using Azos.Instrumentation;
using Azos.Serialization.BSON;

namespace Azos.Sky.Social.Trending
{
  /// <summary>
  /// Advances entity trending by the specified count of events
  /// </summary>
  [Serializable]
  [BSONSerializable("BAC4E8C4-9ED2-49C4-A95D-17B6A402FA7E")]
  public sealed class SocialTrendingGauge : LongGauge, ISocialLogic
  {
    public const string BSON_FLD_ENTITY    = "ent";
    public const string BSON_FLD_G_SHARD   = "g_s";
    public const string BSON_FLD_G_ENTITY  = "g_e";
    public const string BSON_FLD_DIMS      = "dims";

    public const int MAX_ENTITY_NAME_LENGTH = 8;
    public const int MAX_DIMENSION_LENGTH = 500;

    public static bool TryValidateEntityName(string entityName)
    {
      if (entityName.IsNullOrWhiteSpace()) return false;
      if (entityName.Length > MAX_ENTITY_NAME_LENGTH) return false;
      for (var i = 0; i < entityName.Length; i++)
      {
        var c = entityName[i];
        if ((c < 'A' || c > 'Z') &&
            (c < 'a' || c > 'z') &&
            (c < '0' || c > '9' || i == 0) &&
            (c != '_'))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Records the trending information.
    /// </summary>
    /// <param name="tEntity">Entity type - what is trending</param>
    /// <param name="gShard">Sharding area key</param>
    /// <param name="gEntity">GDID of the trending entity</param>
    /// <param name="count">Trending count</param>
    /// <param name="dimensions">Dimensions vector in plain or laconic format</param>
    public static void Emit(string tEntity, GDID gShard, GDID gEntity, long count, string dimensions)
    {
      if (!TryValidateEntityName(tEntity))
        throw new SocialException(StringConsts.ARGUMENT_ERROR + "Emit(tEntity!Valid:'{0}')".Args(tEntity));

      if (dimensions != null && dimensions.Length > MAX_DIMENSION_LENGTH)
        throw new SocialException(StringConsts.ARGUMENT_ERROR + "Emit(dims to long)");

      var inst = App.Instrumentation;
      if (!inst.Enabled) return;

      var datum = new SocialTrendingGauge(count)
      {
        m_Entity     = tEntity,
        m_G_Shard    = gShard,
        m_G_Entity   = gEntity,
        m_Dimensions = dimensions
      };

      inst.Record(datum);
    }

    private SocialTrendingGauge(long count) : base(null, count)
    {
    }


    private string m_Entity;
    private GDID   m_G_Shard;
    private GDID   m_G_Entity;

    private string m_Dimensions;

    public override string Source
    {
      get
      {
        var sb = new StringBuilder(128);
        sb.Append(m_Entity);   sb.Append('|');
        sb.Append(m_G_Shard);  sb.Append('|');
        sb.Append(m_G_Entity); sb.Append('|');
        sb.Append(m_Dimensions);

        return sb.ToString();
      }
    }

    /// <summary> Returns entity type </summary>
    public string Entity { get { return m_Entity;} }

    /// <summary> Returns entity sharding key </summary>
    public GDID   G_Shard { get { return m_G_Shard;} }

    /// <summary> Returns entity GDID </summary>
    public GDID   G_Entity { get { return m_G_Entity;} }

    /// <summary>
    /// Dimensions used for classification.  e.g. 'USA', 'Accounting' etc.
    /// Use laconic to assign multiple.
    /// ATTENTION! Making dimensions too detailed significantly increases the number of samples
    /// that the system needs to process in real time.
    /// ATTENTION! In a particular business system, if dimension is a vector, it must be an ordered tuple of attr/values of the fixed size
    /// (e.g "category,class" and "class,category" are different dimensions)
    /// </summary>
    public string Dimensions { get { return m_Dimensions;} }

    public override string Description { get { return "Advances '{0}' social trending by the specified count of events".Args(Entity);}}
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_EVENT; }}

    protected override Datum MakeAggregateInstance()
    {
       var aggregated = new SocialTrendingGauge(this.Value)
       {
          m_Entity    = this.m_Entity,
          m_G_Shard   = this.m_G_Shard,
          m_G_Entity  = this.m_G_Entity,
          m_Dimensions  = this.m_Dimensions
       };
       return aggregated;
    }

    public override void SerializeToBSON(BSONSerializer serializer, BSONDocument doc, IBSONSerializable parent, ref object context)
    {
      base.SerializeToBSON(serializer, doc, parent, ref context);

      doc.Add(BSON_FLD_ENTITY,    m_Entity);
      doc.Add(BSON_FLD_G_SHARD,   m_G_Shard.ToString());
      doc.Add(BSON_FLD_G_ENTITY,  m_G_Entity.ToString());
      doc.Add(BSON_FLD_DIMS, m_Dimensions);
    }

    public override void DeserializeFromBSON(BSONSerializer serializer, BSONDocument doc, ref object context)
    {
      base.DeserializeFromBSON(serializer, doc, ref context);

      m_Entity     = doc.TryGetObjectValueOf(BSON_FLD_ENTITY)   .AsString();
      m_G_Shard    = doc.TryGetObjectValueOf(BSON_FLD_G_SHARD)  .AsGDID  ();
      m_G_Entity   = doc.TryGetObjectValueOf(BSON_FLD_G_ENTITY) .AsGDID  ();
      m_Dimensions = doc.TryGetObjectValueOf(BSON_FLD_DIMS)     .AsString();
    }
  }
}
