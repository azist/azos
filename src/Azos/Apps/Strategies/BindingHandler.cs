using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;

namespace Azos.Apps.Strategies
{
  /// <summary>
  /// Provides a base for handling binding logic such as matching strategy type and parameters
  /// </summary>
  public class BindingHandler
  {
    public BindingHandler(Type tTarget, IConfigSectionNode config)
    {
      TargetStrategyType = tTarget.IsOfType<IStrategy>()
                                  .IsTrue(t => t.IsInterface);
    }

    private List<Type> m_List = new List<Type>();

    public readonly Type TargetStrategyType;


    /// <summary>
    /// Registers the type, returns true on success, false if type already registered
    /// </summary>
    public bool Register(Type implementor)
    {
      implementor.NonNull(nameof(implementor));

      if (!TargetStrategyType.IsAssignableFrom(implementor))
        throw new StrategyException("The `{0}` is not an implementor of `{1}` strategy contract".Args(implementor.DisplayNameWithExpandedGenericArgs(), TargetStrategyType.DisplayNameWithExpandedGenericArgs()));

      return DoRegister(implementor);
    }

    /// <summary>
    /// Override after registration of implementor types has finished
    /// </summary>
    public virtual void FinalizeRegistration(){ }


    /// <summary>
    /// Performs the binding by matching the implementor type, then allocating or re-using the possibly pre-configured strategy instance
    /// </summary>
    public virtual TStrategy Bind<TStrategy, TContext>(TContext context) where TStrategy : class, IStrategy<TContext>
                                                                 where TContext : IStrategyContext
    {
      var t = FindMatchingImplementor(context);
      if (t==null) return null;

      var result = BindInstance<TStrategy, TContext>(t, context);
      return result;
    }

    /// <summary>
    /// Default impl adds type to list. Override to add type to custom complex structure such as the one used by
    /// complex pattern matching
    /// </summary>
    protected virtual bool DoRegister(Type implementor)
    {
      if (m_List.Contains(implementor)) return false;
      m_List.Add(implementor);
      return true;
    }

    /// <summary>
    /// Finds the implementor of requested TargetStrategyType. The default implementation just takes the first implementor.
    /// Override to do complex matching (e.g. pattern matching) based on context strategy traits
    /// </summary>
    protected virtual Type FindMatchingImplementor(IStrategyContext context)
     => m_List.FirstOrDefault();

    /// <summary>
    /// Returns an instance of T implementing TStrategy. Returns null in case of failure.
    /// Default implementation uses activator. Override to provide custom instance/config matching in the supplied context
    /// </summary>
    protected virtual TStrategy BindInstance<TStrategy, TContext>(Type tImplementor, TContext context) where TStrategy : class, IStrategy<TContext>
                                                                                            where TContext : IStrategyContext
    {
      var result = Serialization.SerializationUtils.MakeNewObjectInstance(tImplementor) as TStrategy;
      var impl = result as IStrategyImplementation<TContext>;

      if (impl==null) return null;

      impl.SetContext(context);
      return result;
    }
  }
}
