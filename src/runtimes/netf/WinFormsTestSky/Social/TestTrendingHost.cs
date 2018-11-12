using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Serialization.JSON;

using Azos.Sky.Social.Trending.Server;

namespace WinFormsTestSky.Social
{
  public class TestTrendingHost : TrendingSystemHost
  {
    private const string USER_ENTITY    = "user";
    private const string PRODUCT_ENTITY = "product";

    private readonly HashSet<string> ENTITIES = new HashSet<string> { USER_ENTITY, PRODUCT_ENTITY };
    private readonly Dictionary<string, string[]> DIMENSIONS = new Dictionary<string, string[]>
    {
      { USER_ENTITY,    new[] { "age", "sex", "country" } },
      { PRODUCT_ENTITY, new[] { "size", "color" } }
    };

    protected TestTrendingHost(TrendingSystemService director, IConfigSectionNode config) : base(director, config)
    {
      ConfigAttribute.Apply(this, config);
    }


    public override IEnumerable<string> AllEntities { get { return ENTITIES.ToList(); } }


    public override string[] GetDimensionNamesForEntity(string tEntity)
    {
      return DIMENSIONS[tEntity];
    }

    public override bool HasEntity(string tEntity)
    {
      return ENTITIES.Contains(tEntity);
    }

    public override IEnumerable<KeyValuePair<string, string>> MapGaugeDimensions(string tEntity, string dimensions)
    {
      var allDims = DIMENSIONS[tEntity];
      var dimConfig = JSONReader.DeserializeDataObject(dimensions) as JSONDataMap;

      foreach (var kvp in dimConfig)
      {
        if (!allDims.Contains(kvp.Key)) continue; // ERROR
        yield return new KeyValuePair<string, string>(kvp.Key, (string)kvp.Value);
      }
    }

    public override string MapGaugeDimensions(string tEntity, IEnumerable<KeyValuePair<string, string>> dimensions)
    {
      var map = new JSONDataMap();
      var allDims = DIMENSIONS[tEntity];

      foreach (var dim in dimensions)
      {
        if (!allDims.Contains(dim.Key)) continue; // ERROR
        map[dim.Key] = dim.Value;
      }

      return map.ToJSON();
    }
  }
}
