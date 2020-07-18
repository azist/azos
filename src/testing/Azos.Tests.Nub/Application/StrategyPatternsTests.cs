/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/



using Azos.Apps;
using Azos.Apps.Strategies;
using Azos.Conf;
using Azos.Geometry;
using Azos.Scripting;

namespace Azos.Tests.Nub.Application
{
  /// <summary>
  /// This is a unit test and an example of using business logic-specific pattern matching of strategies
  /// </summary>
  [Runnable]
  public class StrategyPatternsTests
  {
    static readonly ConfigSectionNode BASE_CONF = @"
  app{
    modules
    {
      module
      {
        type='Azos.Apps.Strategies.DefaultBinder, Azos'
        assemblies='Azos.Tests.Nub.dll'
      }
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

    /// <summary>
    /// In this example we will match the strategy implementing type which is geographically closer
    /// to the point of interest - our context. This is done using standard [StrategyPattern] attribute
    /// and custom pattern score calculation in IPatternStrategyTrait implementation (see below)
    /// </summary>
    [Run]
    public void TestGeoStrategy_Proximity()
    {
      using( var app = new AzosApplication(null, BASE_CONF))
      {
        //Get the binder which will make and bind strategy instances for the requested contract and context data
        var binder = app.ModuleRoot.Get<IStrategyBinder>();

        //The first case runs in the context of Cleveland, OHIO customers
        var ctx = new GeoProximityContext{  Location = new LatLng("41.500136,-81.7005492", "Cleveland")};
        var got = binder.Bind<IGeoStrat, IGeoStratContext>(ctx);

        //We get NY strategy implementation, because NY is closer to Cleveland than others in California
        Aver.IsTrue(got is NewYorkStrat);
        Aver.AreEqual("New York customers in Cleveland", got.RunStrategyWork());

        //Now try Washington DC
        ctx.Location = new LatLng("38.9079407,-77.0355777", "Washington");
        got = binder.Bind<IGeoStrat, IGeoStratContext>(ctx);
        //Still get NY, because it is closer
        Aver.IsTrue(got is NewYorkStrat);
        Aver.AreEqual("New York customers in Washington", got.RunStrategyWork());

        //Now Sacramento which is closer to SF than LA
        ctx.Location = new LatLng("38.5755851,-121.4925168", "Sacramento");
        got = binder.Bind<IGeoStrat, IGeoStratContext>(ctx);
        //And we do get San Francisco
        Aver.IsTrue(got is SanFranciscoStrat);
        Aver.AreEqual("San Francisco California customers in Sacramento", got.RunStrategyWork());

        //Now Sand Diego
        ctx.Location = new LatLng("32.729915,-117.1577203", "San Diego");
        got = binder.Bind<IGeoStrat, IGeoStratContext>(ctx);
        //Which is closer to LA
        Aver.IsTrue(got is LosAngelesStrat);
        Aver.AreEqual("LA California customers in San Diego", got.RunStrategyWork());
      }
    }

    /// <summary>
    /// This example uses sister cities to match the best strategy for the requested location name
    /// </summary>
    [Run]
    public void TestGeoStrategy_SisterCities()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        //Get the binder which will make and bind strategy instances for the requested contract and context data
        var binder = app.ModuleRoot.Get<IStrategyBinder>();

        //NOTICE:  We allocate a different strategy context type (compare with the above test)
        var ctx = new GeoSisterContext { Location = new LatLng("41.500136,-81.7005492", "Cleveland") };
        var got = binder.Bind<IGeoStrat, IGeoStratContext>(ctx);

        //We get LA strategy implementation, because LA is a sister city of Cleveland per GeoSisterContext
        Aver.IsTrue(got is LosAngelesStrat);
        Aver.AreEqual("LA California customers in Cleveland", got.RunStrategyWork());

        //Now try Washington DC
        ctx.Location = new LatLng("38.9079407,-77.0355777", "Washington");
        got = binder.Bind<IGeoStrat, IGeoStratContext>(ctx);
        //SF is sister of Washington
        Aver.IsTrue(got is SanFranciscoStrat);
        Aver.AreEqual("San Francisco California customers in Washington", got.RunStrategyWork());

        //Now Sacramento is going to throw because you cant bind it using this matching (no sister exists for it)
        ctx.Location = new LatLng("38.5755851,-121.4925168", "Sacramento");
        //can't bind it
        Aver.Throws<AzosException>(()=> binder.Bind<IGeoStrat, IGeoStratContext>(ctx));
      }
    }

    /// <summary>
    /// Our context has a location Lat/LNG coordinates which represent the point of interest per call.
    /// We also want to use [StrategyPattern] which uses IPatternStrategyTrait
    /// </summary>
    interface IGeoStratContext : IStrategyContext, IPatternStrategyTrait
    {
      LatLng Location{ get; set;}
    }

    /// <summary>
    /// We could implement many different context with various pattern matching logic
    /// </summary>
    public class GeoProximityContext : IGeoStratContext
    {
      public LatLng Location{ get; set;}

      /// <summary>
      /// Here we implement pattern matching score logic. The higher = the better the match.
      /// The 'pattern' parameter delivers the config vector from [StrategyPattern(....)] candidates
      /// </summary>
      public double GetPatternMatchScore(IConfigSectionNode pattern)
      {
        //for flexibility: attribute is optional
        var center = pattern.ValOf("center");
        if (center.IsNullOrWhiteSpace()) return 0;

        //calc the distance between to LAT/LNG in KM
        var distance = Location.HaversineEarthDistanceKm(new LatLng(center));

        //the smaller the distance - the higher the score
        var ratio = LatLng.EARTH_CIRCUMFERENCE_KM / (distance == 0 ? 0.001 : distance);

        return ratio;//is the score for this case
      }
    }

    /// <summary>
    /// This one matches by twin/sister city name
    /// </summary>
    public class GeoSisterContext : IGeoStratContext
    {
      public LatLng Location { get; set; }

      /// <summary>
      /// By sister city names
      /// </summary>
      public double GetPatternMatchScore(IConfigSectionNode pattern)
       => Location.Name.EqualsIgnoreCase(pattern.ValOf("sister")) ? 10000 : 0;
    }


    /// <summary>
    /// The primary contract for the strategy itself. The best strategy implementation of this contract
    /// gets "bound" to a context by binder
    /// </summary>
    interface IGeoStrat : IStrategy<IGeoStratContext>
    {
      string RunStrategyWork();
    }

    /// <summary>
    /// Our pattern matching may use any logic, including the geo-proximity one.
    /// This class would serve customers near NY
    /// </summary>
    [StrategyPattern("center='40.7202971,-73.9936111'")]
    class NewYorkStrat : Strategy<IGeoStratContext>, IGeoStrat
    {
      public string RunStrategyWork()
      {
        return "New York customers in {0}".Args(Context.Location.Name);
      }
    }

    /// <summary>
    /// This class would serve customers near LA
    /// </summary>
    [StrategyPattern("center='34.0534724,-118.2439744' sister='Cleveland'")] //notice: we put both args on the same attribute
    class LosAngelesStrat : Strategy<IGeoStratContext>, IGeoStrat
    {
      public string RunStrategyWork()
      {
        return "LA California customers in {0}".Args(Context.Location.Name);
      }
    }

    /// <summary>
    /// This class would serve customers near Frisco
    /// </summary>
    [StrategyPattern("center='37.7872996,-122.4060625'")]
    [StrategyPattern("sister='Washington'")]//we put sister city name on another attribute
    class SanFranciscoStrat : Strategy<IGeoStratContext>, IGeoStrat
    {
      public string RunStrategyWork()
      {
        return "San Francisco California customers in {0}".Args(Context.Location.Name);
      }
    }


  }
}