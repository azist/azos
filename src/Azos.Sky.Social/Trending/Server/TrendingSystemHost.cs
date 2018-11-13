using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Sky.Social.Trending.Server
{
  /// <summary>
  /// Implemented in the particular business system to map SocialTrendingGauge.Dimensions vector into the KVP that can be stored in the database
  /// </summary>
  public abstract class TrendingSystemHost : ApplicationComponent
  {
    protected TrendingSystemHost(TrendingSystemService director, IConfigSectionNode config) : base(director)
    {
      ConfigAttribute.Apply(this, config);
    }

    /// <summary>
    /// Return all entity names supported by the system
    /// </summary>
    public abstract IEnumerable<string> AllEntities { get; }

    /// <summary>
    /// Returns true if entity is supported by the particular system
    /// </summary>
    public abstract bool HasEntity(string tEntity);

    /// <summary>
    /// Returns ordered set of dimensions for the entity
    /// </summary>
    public abstract string[] GetDimensionNamesForEntity(string tEntity);

    /// <summary>
    /// Maps gauge packed string dimensions into dictionary (i.e. using JSON or Laconic)
    /// </summary>
    public abstract IEnumerable<KeyValuePair<string, string>> MapGaugeDimensions(string tEntity, string dimensions);

    /// <summary>
    /// Maps dictionary dimensions into packed string (i.e. using JSON or Laconic)
    /// </summary>
    public abstract string MapGaugeDimensions(string tEntity, IEnumerable<KeyValuePair<string, string>> dimensions);
  }
}
