using System;
using System.Collections.Generic;

using Azos.Glue;
using Azos.Data;

using Azos.Sky.Contracts;

namespace Azos.Sky.Social.Trending
{
  /// <summary>
  /// Represents a facade for trending system that records and keeps track of trending items.
  /// Use TrandingSystemInstrumentationProvider to upload the SocialTrendingGauge instances
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface ITrendingSystem : ISkyService
  {
    /// <summary>
    /// Uploads the batch of SocialTrendingGauge samples to the server
    /// </summary>
    [OneWay]
    void Send(SocialTrendingGauge[] gauges);

    /// <summary>
    /// Returns the specified number of entities in a data range starting from position for the specified entity type
    /// Pass null tEntity to select the top entities across various types
    /// </summary>
    IEnumerable<TrendingEntity> GetTrending(TrendingQuery query);
  }

  /// <summary>
  /// Represents query parameters sent to ITrendingSystem.GetTrending(query)
  /// </summary>
  public struct TrendingQuery
  {
    public const int MAX_FETCH_COUNT = 1024;
    public const int MAX_SAMPLE_COUNT = 1024;

    /// <param name="tEntity">If null, then trending across all types is queried</param>
    /// <param name="startDate">The start timespan of sampling</param>
    /// <param name="endDate">The end timespan of sampling</param>
    /// <param name="sampleCount">How many samples we want to get in a date range</param>
    /// <param name="fetchStart">The starting ranking position of trending</param>
    /// <param name="fetchCount"></param>
    /// <param name="filter">The count of trending records</param>
    public TrendingQuery(string tEntity, DateTime startDate, DateTime endDate, int sampleCount, int fetchStart, int fetchCount, string filter)
    {
      if (!SocialTrendingGauge.TryValidateEntityName(tEntity))
        throw new SocialException(StringConsts.ARGUMENT_ERROR + "TrendingQuery.ctor(tEntity!Valid:'{0}')".Args(tEntity));

      EntityType = tEntity;
      StartDate = startDate;
      EndDate = endDate;
      SampleCount = sampleCount < 0 ? 1 : sampleCount > MAX_SAMPLE_COUNT ? MAX_SAMPLE_COUNT : sampleCount;
      FetchStart = fetchStart < 0 ? 0 : fetchStart;
      FetchCount = fetchCount < 0 ? 1 : fetchCount > MAX_FETCH_COUNT ? MAX_FETCH_COUNT : fetchCount;
      DimensionFilter = filter;
    }

    /// <summary>
    /// If null, then trending across all types is queried
    /// </summary>
    public readonly string EntityType;

    /// <summary>
    /// The start timespan of sampling
    /// </summary>
    public readonly DateTime StartDate;

    /// <summary>
    /// The end timespan of sampling
    /// </summary>
    public readonly DateTime EndDate;

    /// <summary>
    /// How many samples we want to get in a date range
    /// </summary>
    public readonly int SampleCount;

    /// <summary>
    /// The starting ranking position of trending
    /// </summary>
    public readonly int FetchStart;

    /// <summary>
    /// The count of trending records
    /// </summary>
    public readonly int FetchCount;


    /// <summary>
    /// The dimension filter in laconic format
    /// </summary>
    public readonly string DimensionFilter;

  }


  /// <summary>
  /// Contains information about the trending entities
  /// </summary>
  [Serializable]
  public struct TrendingEntity
  {
    public TrendingEntity(DateTime dt, int durationMin, string etp, GDID gShard, GDID gEntity, ulong count)
    {
      TimestampUTC = dt;
      DurationMinutes = durationMin;
      EntityType = etp;
      G_Shard = gShard;
      G_Entity = gEntity;
      Count = count;
    }

    /// <summary>The type of entity, such as "user", "group", "forum"</summary>
    public readonly DateTime TimestampUTC;

    /// <summary>The duration of time span in minutes</summary>
    public readonly int DurationMinutes;

    /// <summary>The type of entity, such as "user", "group", "forum"</summary>
    public readonly string EntityType;

    /// <summary>The entity sharding key</summary>
    public readonly GDID   G_Shard;

    /// <summary>The entity identity</summary>
    public readonly GDID   G_Entity;

    /// <summary>The Count of events for the entity</summary>
    public readonly ulong  Count;
  }


  /// <summary>
  /// Contract for client of ITrendingSystem svc
  /// </summary>
  public interface ITrendingSystemClient : ISkyServiceClient, ITrendingSystem
  {
    CallSlot Async_GetTrending(TrendingQuery query);
  }
}
