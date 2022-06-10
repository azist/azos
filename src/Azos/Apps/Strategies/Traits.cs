/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Azos.Conf;
using Azos.Platform;

namespace Azos.Apps.Strategies
{
  /// <summary>
  /// Marker interface for strategy traits/qualities. This interfaces are used by IStrategyContext derivatives
  /// to designate specific traits used for strategy pattern matching performed by IStrategyBinder/BinderHandler
  /// based on a supplied IStrategyContext instance. A IPatternStrategyTrait is the most frequently used derivative
  /// </summary>
  public interface IStrategyTrait
  {

  }

  /// <summary>
  /// Provides pattern matching trait for strategies which depend on multiple parameters.
  /// This interface is implemented by specific IStrategyContext implementors, allowing for custom
  /// pattern matching logic for the supplied "pattern" vector which typically comes from
  /// StrategyPatternAttribute decorations on the strategy-implementing class
  /// </summary>
  public interface IPatternStrategyTrait : IStrategyTrait
  {
    /// <summary>
    /// Returns a relative match score of this implementor matching the specified
    /// requirements pattern vector. The higher the score, the better the match.
    /// In most cases you should return zero for a non-match.
    /// </summary>
    /// <remarks>
    /// The pattern are supplied as config vector which typically comes from  StrategyPatternAttribute
    /// which declare attributes for pattern matching. This way, you can develop various strategy implementations
    /// for the same strategy contract, differentiating implementor types using StrategyPatternAttribute.
    /// When request comes for a specific strategy type in a context, the system tries to apply StrategyPatternAttribute
    /// on every type, taking the one with the highest, score above zero
    /// </remarks>
    /// <example>
    /// For example, a geo strategy context may match the closest
    /// point in space to the required point. It will calculate the proximity score
    /// based on lat/lng pattern supplied as config. Then we can calculate the distance
    /// form that lat/lng "pattern" setting to the actual lat/lng which this decorated context
    /// represents, thus performing distance-based pattern matching
    /// </example>
    double GetPatternMatchScore(IConfigSectionNode pattern);
  }

  /// <summary>
  /// Performs pattern matching logic on a candidate type.
  /// The base class implementation uses the IPatternStrategyTrait of IStrategyContext to
  /// match the best strategy type based on the highest match score.
  /// </summary>
  /// <example>
  /// Suppose there are two implementations of the same strategy contract IMyStrategy.
  /// Both implementations declare [StrategyPattern("a=1")] attribute with different values:
  /// [StrategyPattern("a=2")]. When we call the binder with IMyStrategyContext : IPatternStrategyTrait,
  /// having that context react to setting "a", we will get the higher match score for the type
  /// which has a "better" value for "a=2" vs "a=1" thus allowing for dynamic context-driven
  /// dependency injection at runtime
  /// </example>
  [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public class StrategyPatternAttribute : Attribute
  {
    public StrategyPatternAttribute(string laconicPatternContent)
     =>  Pattern = laconicPatternContent.NonBlank(nameof(laconicPatternContent))
                                        .AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw)
                                        .NonEmpty(nameof(laconicPatternContent));

    /// <summary>
    /// Returns configuration vector representing a pattern for making
    /// matches
    /// </summary>
    public readonly IConfigSectionNode Pattern;


    private static FiniteSetLookup<Type, IEnumerable<StrategyPatternAttribute>> s_Lookup = new FiniteSetLookup<Type, IEnumerable<StrategyPatternAttribute>>
    (
      t => t.GetCustomAttributes<StrategyPatternAttribute>(false)
    );

    /// <summary>
    /// Override to do custom score matching, the default implementation uses IPatternStrategyTrait
    /// </summary>
    public virtual double GetMatchScoreScore(IStrategyContext context)
     => context is IPatternStrategyTrait patternContext ? patternContext.GetPatternMatchScore(Pattern) : 0;

    /// <summary>
    /// Tries to match the first type which matches the context or null if nothing matches
    /// </summary>
    /// <returns>(any(bool): true when any strategy type was decorated with StrategyPatternAttribute and context is IPatternStrategyTrait)</returns>
    public static (bool any, Type type, double score) MatchBest(IStrategyContext context, IEnumerable<Type> types)
    {
      if (context== null || types == null) return (false, null, 0);

      bool any = false;
      Type bestMatch = null;
      double bestScore = 0d;//ZERO is the LOWEST bound = NON MATCH

      foreach(var t in types)
      {
        var allAttrs = s_Lookup[t];
        foreach(var atr in allAttrs)
        {
          any = true;

          var score = atr.GetMatchScoreScore(context);
          if (score > bestScore) //get the maximum
          {
            bestScore = score;
            bestMatch = t; //is the type which bares the attribute
          }
        }//attributes
      }//types

      return (any, bestMatch, bestScore);
    }
  }

}
