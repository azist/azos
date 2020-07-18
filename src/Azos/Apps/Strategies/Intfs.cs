/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;

namespace Azos.Apps.Strategies
{
  /// <summary>
  /// Marker interfaces for strategy contexts - data vectors under which strategy instance executes
  /// </summary>
  /// <remarks>
  /// The implementation does not have to be a data Doc/ because data may come from transient models in memory
  /// </remarks>
  public interface IStrategyContext
  {
  }

  /// <summary>
  /// Outlines contract for strategy factories which bind specific strategy contract instances to specific business cases (contexts).
  /// Finds a strategy implementation which satisfies the specified contract type, and is configured in a
  /// way specific for the concrete use-case as supplied via Context property.
  /// Strategies couple context-specific logic along with their configuration parameters. See: https://en.wikipedia.org/wiki/Strategy_pattern
  /// </summary>
  public interface IStrategyBinder : IModule
  {
    /// <summary>
    /// Gets a strategy instance per the specified contract and binds it to the specified call context.
    /// This method is synchronous because by design it is expected to be CPU-bound and
    /// use cache for performance internally instead of relying on external data store access
    /// </summary>
    TStrategy Bind<TStrategy, TContext>(TContext context) where TStrategy : class, IStrategy<TContext>
                                                          where TContext : IStrategyContext;
  }

  /// <summary>
  /// Represents a strategy which embodies context-specific logic along with its configuration parameters.
  /// Unlike regular "logic" which represents context-agnostic behavior, strategies implement
  /// behaviors specific to given business context. Strategy instances are obtained by calling
  /// `IStrategyBinderLogic.Bind(ctx)` and should NOT be cached for reuse.
  /// The binder matches the most appropriate strategy configuration for the requested context, fetching
  /// strategy config parameters from an external store if necessary and aggressively caching them.
  /// </summary>
  /// <remarks>
  /// A strategy is a data document for uniformity and performance (e.g. uses Bix/Pile cache).
  /// Strategies' Context property is not cached and gets set by Binder every time the strategy is bound
  /// </remarks>
  public interface IStrategy : IDataDoc
  {
    /// <summary>
    /// Strategy context provides business-specific case data
    /// </summary>
    IStrategyContext Context { get; }
  }

  /// <summary>
  /// Represents a strategy which embodies context-specific logic along with its configuration params.
  /// Unlike regular "logic" which represents context-agnostic behavior, strategies implement
  /// behaviors specific to given business context. Strategy instances are obtained by calling
  /// `IStrategyBinderLogic.Bind(ctx)` and should NOT be cached for reuse.
  /// The binder matches the most appropriate strategy configuration for the requested context, fetching
  /// strategy config parameters from an external store if necessary and aggressively caching them.
  /// </summary>
  public interface IStrategy<TContext> : IStrategy where TContext : IStrategyContext
  {
    /// <summary>
    /// Strategy context provides business-specific case data
    /// </summary>
    new TContext Context { get; }
  }

  public interface IStrategyImplementation<TContext> : IStrategy<TContext> where TContext : IStrategyContext
  {
    /// <summary>
    /// Framework method that sets context, not used in typical business logic
    /// </summary>
    void SetContext(TContext context);
  }

}
