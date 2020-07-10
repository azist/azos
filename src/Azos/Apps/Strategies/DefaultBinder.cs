using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Azos.Conf;

namespace Azos.Apps.Strategies
{
  /// <summary>
  /// Provides most basic IStrategyBinder implementation, resolving
  /// strategies from all assemblies by IStrategy
  /// </summary>
  public sealed class DefaultBinderLogic : ModuleBase, IStrategyBinder
  {
    public DefaultBinderLogic(IApplication app) : base(app) {}
    public DefaultBinderLogic(IModule parent) : base(parent) {}

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => CoreConsts.APPLICATION_TOPIC;

#pragma warning disable 0649
    [Config] private string m_Assemblies;
#pragma warning restore 0649

    private Dictionary<Type, Type> m_Cache;

    protected override bool DoApplicationAfterInit()
    {
      m_Assemblies.NonBlank("Assemblies not configured");
      var anames = m_Assemblies.Split(';');

      m_Cache = new Dictionary<Type, Type>();

      var asms = anames.Where(n => n.IsNotNullOrWhiteSpace())
                       .Select(n => Assembly.LoadFrom(n.Trim()));

      foreach (var asm in asms)
      {
        var types = asm.GetTypes()
                       .Where(t => t.IsClass && !t.IsAbstract && typeof(IStrategy).IsAssignableFrom(t));

        foreach (var type in types)
        {
          var intfs = type.GetInterfaces().Where(ti => typeof(IStrategy).IsAssignableFrom(ti));
          foreach (var intf in intfs)
            m_Cache[intf] = type;
        }
      }

      if (m_Cache.Count == 0)
        throw new StrategyException("The {0} is not configured with any strategies. Revise assembly bindings".Args(nameof(DefaultBinderLogic)));

      return base.DoApplicationAfterInit();
    }



    #region IStrategyBinderLogic Implementation

    public TStrategy Bind<TStrategy, TContext>(TContext context) where TStrategy : class, IStrategy<TContext>
                                                                 where TContext : IStrategyContext
    {
      if (!m_Cache.TryGetValue(typeof(TStrategy), out var ttarget))
        throw new StrategyException("Strategy `{0}` could not be resolved".Args(typeof(TStrategy).DisplayNameWithExpandedGenericArgs()));

      var result = Activator.CreateInstance(ttarget) as TStrategy;
      var impl = result as IStrategyImplementation<TContext>;


      result.NonNull("Activation of `{0}` failed".Args(ttarget.DisplayNameWithExpandedGenericArgs()));
      impl.NonNull("Is not `{0}`".Args(typeof(IStrategyImplementation<TContext>).DisplayNameWithExpandedGenericArgs()));

      impl.SetContext(context);
      App.InjectInto(result);

      return result;
    }

    #endregion
  }
}
