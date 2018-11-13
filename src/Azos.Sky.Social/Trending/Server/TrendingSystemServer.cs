using System.Collections.Generic;

namespace Azos.Sky.Social.Trending.Server
{
  /// <summary>
  /// Implements Glue server per ITrendingSystem contract
  /// </summary>
  public sealed class TrendingSystemServer : ITrendingSystem
  {
    public void Send(SocialTrendingGauge[] gauges)
    {
      TrendingSystemService.Instance.Send(gauges);
    }

    public IEnumerable<TrendingEntity> GetTrending(TrendingQuery query)
    {
      return TrendingSystemService.Instance.GetTrending(query);
    }
  }
}