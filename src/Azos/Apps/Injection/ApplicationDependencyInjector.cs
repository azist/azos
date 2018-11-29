using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Apps.Injection
{
  /// <summary>
  /// Implements base app dependency injection services
  /// </summary>
  public class ApplicationDependencyInjector : ApplicationComponent, IApplicationDependencyInjector
  {
    public ApplicationDependencyInjector(IApplication app) : base (app)
    {
    }

    /// <summary>
    /// Performs Injection into target fields decorated with [Inject] attribute
    /// </summary>
    public void InjectInto(object target)
    {
      target.NonNull(nameof(target));
      DoInjectInto(target);
    }

    private object m_InfoLock = new object();
    private volatile Dictionary<Type, Injector> m_Infos = new Dictionary<Type, Injector>();

    public override string ComponentLogTopic => CoreConsts.APPLICATION_TOPIC;

    /// <summary>
    /// Injects app context into target checking for IApplicationInjection first,
    /// then uses Injector(type) to inject into specific type
    /// </summary>
    protected virtual void DoInjectInto(object target)
    {
      if (target is IApplicationInjection ai)
      {
        var done = ai.InjectApplication(this.App);
        if (done) return;
      }

      var tp = target.GetType();
      var injector = GetInjector(tp);
      injector.Inject(target, this.App);
    }

    /// <summary>
    /// Creates injector for the specified type if the type was not injected yet, otherwise
    /// returns an existing injector
    /// </summary>
    protected Injector GetInjector(Type type)
    {
      Injector result;
      if (m_Infos.TryGetValue(type, out result)) return result;

      lock(m_InfoLock)
      {
        if (m_Infos.TryGetValue(type, out result)) return result;
        result = MakeInjector(type);
        var dict = new Dictionary<Type, Injector>(m_Infos);
        dict.Add(type, result);
        m_Infos = dict;//atomic
      }

      return result;
    }

    /// <summary>
    /// Factory method that makes instance of Injector. Override to create custom injectors
    /// </summary>
    protected virtual Injector MakeInjector(Type type) => new Injector(type);
  }
}
