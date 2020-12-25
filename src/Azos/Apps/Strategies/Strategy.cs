/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;

namespace Azos.Apps.Strategies
{
  /// <summary>
  /// Represents a strategy which embodies context-specific logic along with its configuration parameters.
  /// Unlike regular "logic" which represents context-agnostic behavior, strategies implement
  /// behaviors specific to given business context. Strategy instances are obtained by calling
  /// `IStrategyBinder.Bind(ctx)` and should NOT be cached for reuse.
  /// The binder matches the most appropriate strategy configuration for the requested context, fetching
  /// strategy config parameters from an external store if necessary and aggressively caching them.
  /// </summary>
  public abstract class Strategy<TContext> : TypedDoc, IStrategyImplementation<TContext> where TContext : IStrategyContext
  {
    [NonSerialized] private TContext m_Context;

    IStrategyContext IStrategy.Context => Context;
    void IStrategyImplementation<TContext>.SetContext(TContext context) => m_Context = context;

    /// <summary>
    /// Provides context in which the strategy methods are called.
    /// The context is not a [Field] because it is a transitive volatile information
    /// set per every request
    /// </summary>
    public TContext Context { get => m_Context; set => m_Context = value; }

  }
}
