/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;

namespace Azos.Apps
{
  /// <summary>
  /// Provides base for IModuleImplementation. The descendants of this class are always injected
  /// via configuration injection process and never by code because modules are not practically
  /// expected to be created dynamically at runtime - they always get created by application chassis at boot.
  /// </summary>
  /// <remarks>
  /// Modules shall not have public constructors because they get created via config DI.
  /// You can use the following pattern in testing fixtures:
  /// <code>
  ///  FactoryUtils.MakeAndConfigureDirectedComponent&lt;IMyModuleImplementation&gt;(app, "module{ a=1 b=2}".AsLaconicConfig());
  /// </code>
  /// </remarks>
  public abstract class ModuleBase : ApplicationComponent, IModuleImplementation, IApplicationInjection
  {
    /// <summary>
    /// Creates a root module without a parent
    /// </summary>
    protected ModuleBase(IApplication application) : base(application){ }

    /// <summary>
    /// Creates a module under a parent module, such as HubModule
    /// </summary>
    protected ModuleBase(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      cleanupChildren(true);
      base.Destructor();
    }

    private void cleanupChildren(bool all)
    {
      var toClean = m_Children.OrderedValues
                              .Reverse()
                              .Where(c => c.ParentModule==this && (all || !c.IsHardcodedModule))
                              .ToList();//need copy
      toClean.ForEach( c =>
                       {
                         c.Dispose();
                         m_Children.Unregister(c);
                       });
    }

    [Config] private string m_Name;
    [Config] private int m_Order;

    protected Collections.OrderedRegistry<ModuleBase> m_Children = new Collections.OrderedRegistry<ModuleBase>();

    public override string ComponentCommonName => this.Name;

    public IModule ParentModule => ComponentDirector as IModule;

    public Collections.IOrderedRegistry<IModule> ChildModules => m_Children;

    public virtual string Name  => m_Name.IsNotNullOrWhiteSpace() ? m_Name : GetType().FullName;

    public int Order => m_Order;

    public virtual bool InstrumentationEnabled { get; set; }

    public abstract bool IsHardcodedModule{ get; }

    public virtual TModule Get<TModule>(Func<TModule, bool> filter = null) where TModule : class, IModule
    {
      var result = TryGet<TModule>(filter);
      if (result==null)
        throw new AzosException(StringConsts.APP_MODULE_GET_BY_TYPE_ERROR.Args(typeof(TModule).DisplayNameWithExpandedGenericArgs()));

      return result;
    }

    public virtual TModule TryGet<TModule>(Func<TModule, bool> filter) where TModule : class, IModule
    {
      foreach(var module in m_Children.OrderedValues)
      {
        var tm = module as TModule;
        if (tm==null) continue;
        if (filter!=null && !filter(tm)) continue;
        return tm;
      }
      return null;
    }

    public virtual TModule Get<TModule>(string name) where TModule : class, IModule
    {
      var result = TryGet<TModule>(name);
      if (result==null)
        throw new AzosException(StringConsts.APP_MODULE_GET_BY_NAME_ERROR.Args(name, typeof(TModule).DisplayNameWithExpandedGenericArgs()));

      return result;
    }

    public virtual TModule TryGet<TModule>(string name) where TModule : class, IModule
    {
      if (name.IsNullOrWhiteSpace()) throw new AzosException(StringConsts.ARGUMENT_ERROR + "Module.TryGet(name==null|empty");
      var result = m_Children[name] as TModule;
      return result;
    }

    bool IApplicationInjection.InjectApplication(IApplicationDependencyInjector injector) => DoInjectApplication(injector);

    void IModuleImplementation.ApplicationAfterInit()
    {
      var handled = DoApplicationAfterInit();
      if (!handled)
        m_Children.OrderedValues.ForEach( c => ((IModuleImplementation)c).ApplicationAfterInit());
    }

    void IModuleImplementation.ApplicationBeforeCleanup()
    {
      var msTimeout = this.ExpectedShutdownDurationMs;
      if (msTimeout < 1) msTimeout = App.ExpectedComponentShutdownDurationMs;
      if (msTimeout < 1) msTimeout = CommonApplicationLogic.DFLT_EXPECTED_COMPONENT_SHUTDOWN_DURATION_MS;

      var handled =  TimedCall.Run( ct =>  DoApplicationBeforeCleanup(),
                                    msTimeout,
                                    () => WriteLog(MessageType.WarningExpectation,
                                                   nameof(IModuleImplementation.ApplicationBeforeCleanup),
                                                   "Module '{0}' finalization is taking longer than expected {1:n} ms".Args(Name, msTimeout))
                                  );
      if (!handled)
      {
        m_Children.OrderedValues
                  .Reverse()
                  .Cast<IModuleImplementation>()//explicitly def interface
                  .ForEach( c => c.ApplicationBeforeCleanup() );
      }
    }

    void IConfigurable.Configure(IConfigSectionNode node) => DoConfigureInScope(node);

    /// <summary>
    /// Override to add crosscutting concerns such as establish SecurityFlowScope
    /// around configuration code block
    /// </summary>
    protected virtual void DoConfigureInScope(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
      DoConfigureChildModules(node);
      DoConfigure(node);
    }


    IEnumerable<KeyValuePair<string, Type>> IExternallyParameterized.ExternalParameters
      => DoGetExternalParameters();

    bool IExternallyParameterized.ExternalGetParameter(string name, out object value, params string[] groups)
      => DoExternalGetParameter(name, out value, groups);

    IEnumerable<KeyValuePair<string, Type>> IExternallyParameterized.ExternalParametersForGroups(params string[] groups)
      => DoGetExternalParametersForGroups(groups);

    bool IExternallyParameterized.ExternalSetParameter(string name, object value, params string[] groups)
      => DoExternalSetParameter(name, value, groups);

    public override string ToString()
      => "Module {0}(@{1}, '{2}', [{3}])".Args(GetType().DisplayNameWithExpandedGenericArgs(), ComponentSID, Name, Order);

    /// <summary> Override to configure the instance </summary>
    protected virtual void DoConfigure(IConfigSectionNode node) { }

    /// <summary>
    /// Override to perform custom population/registration of modules
    /// </summary>
    protected virtual void DoConfigureChildModules(IConfigSectionNode node)
    {
      cleanupChildren(false);
      if (node==null || !node.Exists) return;

      var allModules = DoGetAllChildModuleConfigNodes(node);
      foreach(var mnode in allModules)
      {
        var module = FactoryUtils.MakeAndConfigureDirectedComponent<ModuleBase>(this, mnode);
        if (!m_Children.Register(module))
          throw new AzosException(StringConsts.APP_MODULE_DUPLICATE_CHILD_ERROR.Args(this, module));
      }
    }

    protected virtual IEnumerable<IConfigSectionNode> DoGetAllChildModuleConfigNodes(IConfigSectionNode node)
    {
      if (node==null || !node.Exists) return Enumerable.Empty<IConfigSectionNode>();

      node = node[CommonApplicationLogic.CONFIG_MODULES_SECTION];// module{.. props...... modules{ ....... } }

      return node.Children.Where(c => c.IsSameName(CommonApplicationLogic.CONFIG_MODULE_SECTION));
    }

    protected virtual IEnumerable<KeyValuePair<string, Type>> DoGetExternalParameters()
      => ExternalParameterAttribute.GetParameters(this);

    protected virtual bool DoExternalGetParameter(string name, out object value, params string[] groups)
      => ExternalParameterAttribute.GetParameter(App, this, name, out value, groups);

    protected virtual bool DoExternalSetParameter(string name, object value, params string[] groups)
      => ExternalParameterAttribute.SetParameter(App, this, name, value, groups);

    protected virtual IEnumerable<KeyValuePair<string, Type>> DoGetExternalParametersForGroups(params string[] groups)
      => ExternalParameterAttribute.GetParameters(this, groups);

    /// <summary>
    /// Override to perform custom DI, the default implementation injects content into all child modules.
    /// The DI is done before DoApplicationAfterInit() call by app chassis
    /// </summary>
    protected virtual bool DoInjectApplication(IApplicationDependencyInjector injector)
    {
      ChildModules.OrderedValues.ForEach( m => injector.InjectInto(m) );//Inject into all children
      return false;//injection not completed, let the system keep processing [Inject] attributes
    }

    /// <summary>
    /// Override to perform this module-specific actions after app container init.
    /// The DI has already taken place.
    /// Return true only when the system should not continue to call all child modules, false to let the system call all child modules.
    /// The call is used to perform initialization tasks such as inter-service dependency fixups by code,
    /// initial data loads (e.g. initial cache fetch etc..) after everything has loaded in the application container.
    /// The implementation is expected to handle internal exceptions gracefully (i.e. use log etc.)
    /// </summary>
    protected virtual bool DoApplicationAfterInit()=> false;

    /// <summary>
    /// Override to perform this module-specific actions before app container shutdown.
    /// Return true only when the system should not continue to call all child modules, false let the system call all child modules.
    /// The call is used to perform finalization tasks such as inter-service dependency tears and flushes before
    /// everything is about to be shutdown in the application container.
    /// The implementation is expected to handle internal exceptions gracefully (i.e. use log etc.)
    /// </summary>
    protected virtual bool DoApplicationBeforeCleanup() => false;

  }
}
