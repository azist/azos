/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Text;
using Azos.Conf;

namespace Azos.Apps.Injection
{
  /// <summary>
  /// Implements base app dependency injection services
  /// </summary>
  public class ApplicationDependencyInjector : ApplicationComponent, IApplicationDependencyInjectorImplementation
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
    private volatile Dictionary<Type, TypeInjector> m_Infos = new Dictionary<Type, TypeInjector>();

    public override string ComponentLogTopic => CoreConsts.APPLICATION_TOPIC;

    /// <summary>
    /// Enumerates app injectable roots (root application chassis objects).
    /// This method is usually used by [Inject]-derived attributes for defaults
    /// </summary>
    public virtual IEnumerable<object> GetApplicationRoots()
    {
      yield return App;
      yield return App.Log;
      yield return App.DataStore;
      yield return App.Instrumentation;
      yield return App.SecurityManager;
      yield return App.Random;
      yield return App.TimeSource;
      yield return App.Glue;
      yield return App.ObjectStore;
      yield return App.EventTimer;
    }

    /// <summary>
    /// Injects app context into target checking for IApplicationInjection first,
    /// then uses Injector(type) to inject into specific type
    /// </summary>
    protected virtual void DoInjectInto(object target)
    {
      if (target is IApplicationInjection ai)
      {
        var done = ai.InjectApplication(this);
        if (done) return;
      }

      var tp = target.GetType();
      var injector = GetInjector(tp);
      injector.Inject(target, this);
    }

    /// <summary>
    /// Creates injector for the specified type if the type was not injected yet, otherwise
    /// returns an existing injector
    /// </summary>
    protected TypeInjector GetInjector(Type type)
    {
      TypeInjector result;
      if (m_Infos.TryGetValue(type, out result)) return result;

      lock(m_InfoLock)
      {
        if (m_Infos.TryGetValue(type, out result)) return result;
        result = MakeInjector(type);
        var dict = new Dictionary<Type, TypeInjector>(m_Infos);
        dict.Add(type, result);
        m_Infos = dict;//atomic
      }

      return result;
    }

    /// <summary>
    /// Factory method that makes instance of Injector. Override to create custom injectors
    /// </summary>
    protected virtual TypeInjector MakeInjector(Type type) => new TypeInjector(type);


    void IConfigurable.Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
      DoConfigure(node);
    }

    /// <summary>
    /// Override to perform custom configuration
    /// </summary>
    protected virtual void DoConfigure(IConfigSectionNode node)
    {
    }

  }
}
