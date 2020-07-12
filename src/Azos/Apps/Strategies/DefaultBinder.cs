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
  public class DefaultBinder : ModuleBase, IStrategyBinder
  {
    public DefaultBinder(IApplication app) : base(app) {}
    public DefaultBinder(IModule parent) : base(parent) {}

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => CoreConsts.APPLICATION_TOPIC;

#pragma warning disable 0649
    [Config] private string m_Assemblies;
#pragma warning restore 0649

    private Dictionary<Type, BindingHandler> m_Cache;

    private IConfigSectionNode m_BindingHandlerConfig;

    protected virtual BindingHandler MakeBindingHandler(Type tTarget)
     => FactoryUtils.Make<BindingHandler>(m_BindingHandlerConfig, typeof(BindingHandler), new object[]{ tTarget, m_BindingHandlerConfig});

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node!=null)
      {
        m_BindingHandlerConfig = node["binding-handler","binding","handler"];
        if (m_BindingHandlerConfig.Exists)
        {
          var cfg = new MemoryConfiguration();
          cfg.CreateFromNode(m_BindingHandlerConfig);
          m_BindingHandlerConfig = cfg.Root;
          return;
        }
      }

      m_BindingHandlerConfig = Configuration.NewEmptyRoot();
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Assemblies.NonBlank("Assemblies not configured");
      var anames = m_Assemblies.Split(';');

      m_Cache = new Dictionary<Type, BindingHandler>();

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
          {
            if (m_Cache.TryGetValue(intf, out var handler))
            {
              handler.Register(type);
            }
            else
            {
              handler = MakeBindingHandler(intf);
              handler.Register(type);
              m_Cache[intf] = handler;
            }
          }
        }
      }

      if (m_Cache.Count == 0)
        throw new StrategyException("The {0} is not configured with any strategies. Revise assembly bindings".Args(nameof(DefaultBinder)));

      return base.DoApplicationAfterInit();
    }



    #region IStrategyBinderLogic Implementation

    public TStrategy Bind<TStrategy, TContext>(TContext context) where TStrategy : class, IStrategy<TContext>
                                                                 where TContext : IStrategyContext
    {
      if (!m_Cache.TryGetValue(typeof(TStrategy), out var handler))
        throw new StrategyException("Strategy `{0}` could not be resolved".Args(typeof(TStrategy).DisplayNameWithExpandedGenericArgs()));

      var result = handler.Bind<TStrategy, TContext>(context);

      result.NonNull("Activation of `{0}` failed".Args(typeof(TStrategy).DisplayNameWithExpandedGenericArgs()));

      App.InjectInto(result);

      return result;
    }

    #endregion
  }
}
