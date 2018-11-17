/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Apps
{
  /// <summary>
  /// Provides base for implementation of IModule
  /// </summary>
  public abstract class ModuleBase : ApplicationComponent, IModuleImplementation
  {
    /// <summary>
    /// Creates a root module without a parent
    /// </summary>
    protected ModuleBase(IApplication application) : base(application){ }

    /// <summary>
    /// Creates a module under a parent module, such as HubModule
    /// </summary>
    protected ModuleBase(IModule parent) : base(parent) { }

    /// <summary>
    /// Creates a module under a parent module with the specified order, such as HubModule
    /// </summary>
    protected ModuleBase(IApplication application, IModule parent, int order) : base(application, parent) { m_Order = order; }

    protected override void Destructor()
    {
      cleanupChildren(true);
      base.Destructor();
    }

    private void cleanupChildren(bool all)
    {
      var toClean = m_Children.Where(c => c.ParentModule==this && (all || !c.IsHardcodedModule)).ToList();
      toClean.ForEach( c =>
                       {
                         c.Dispose();
                         m_Children.Unregister(c);
                       });
    }

    [Config] private string m_Name;
    [Config] private int m_Order;

    protected Collections.OrderedRegistry<ModuleBase> m_Children = new Collections.OrderedRegistry<ModuleBase>();

    public override string ComponentCommonName => m_Name.IsNotNullOrWhiteSpace() ? m_Name : GetType().FullName;

    public IModule ParentModule { get{ return ComponentDirector as IModule;} }

    public Collections.IOrderedRegistry<IModule> ChildModules { get{ return m_Children;} }

    public string Name { get{ return m_Name;} }

    public int Order { get{ return m_Order;} }

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

    void IModuleImplementation.ApplicationAfterInit(IApplication application)
    {
      var handled = DoApplicationAfterInit(application);
      if (!handled)
        m_Children.OrderedValues.ForEach( c => ((IModuleImplementation)c).ApplicationAfterInit(application));
    }

    void IModuleImplementation.ApplicationBeforeCleanup(IApplication application)
    {
      var handled = DoApplicationBeforeCleanup(application);
      if (!handled)
        m_Children.OrderedValues.Reverse().ForEach( c => ((IModuleImplementation)c).ApplicationBeforeCleanup(application));
    }

    void IConfigurable.Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
      if (m_Name.IsNullOrWhiteSpace()) m_Name = this.GetType().Name;
      DoConfigureChildModules(node);
      DoConfigure(node);
    }

    IEnumerable<KeyValuePair<string, Type>> IExternallyParameterized.ExternalParameters
    {
      get { return DoGetExternalParameters(); }
    }

    bool IExternallyParameterized.ExternalGetParameter(string name, out object value, params string[] groups)
    {
      return DoExternalGetParameter(name, out value, groups);
    }

    IEnumerable<KeyValuePair<string, Type>> IExternallyParameterized.ExternalParametersForGroups(params string[] groups)
    {
      return DoGetExternalParametersForGroups(groups);
    }

    bool IExternallyParameterized.ExternalSetParameter(string name, object value, params string[] groups)
    {
      return DoExternalSetParameter(name, value, groups);
    }

    public override string ToString()
    {
      return "Module {0}(@{1}, '{2}', [{3}])".Args(GetType().DisplayNameWithExpandedGenericArgs(), ComponentSID, Name, Order);
    }

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
        var module = FactoryUtils.MakeAndConfigureModuleComponent<ModuleBase>(App, this, mnode);
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
    {
      return ExternalParameterAttribute.GetParameters(this);
    }


    protected virtual bool DoExternalGetParameter(string name, out object value, params string[] groups)
    {
      return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
    }

    protected virtual bool DoExternalSetParameter(string name, object value, params string[] groups)
    {
      return ExternalParameterAttribute.SetParameter(this, name, value, groups);
    }

    protected virtual IEnumerable<KeyValuePair<string, Type>> DoGetExternalParametersForGroups(params string[] groups)
    {
      return ExternalParameterAttribute.GetParameters(this, groups);
    }


    /// <summary>
    /// Override to perform this module-specific actions after app container init.
    /// Return true only when the system should not continue to call all child modules, false to let the system call all child modules.
    /// The call is used to perform initialization tasks such as inter-service dependency fixups,
    /// initial data loads (e.g. initial cache fetch etc..) after everything has loaded in the application container.
    /// The implementation is expected to handle internal exceptions gracefully (i.e. use log etc.)
    /// </summary>
    protected virtual bool DoApplicationAfterInit(IApplication application)
    {
      return false;
    }

    /// <summary>
    /// Override to perform this module-specific actions before app container shutdown.
    /// Return true only when the system should not continue to call all child modules, false let the system call all child modules.
    /// The call is used to perform finalization tasks such as inter-service dependency tears and flushes before
    /// everything is about to be shutdown in the application container.
    /// The implementation is expected to handle internal exceptions gracefully (i.e. use log etc.)
    /// </summary>
    protected virtual bool DoApplicationBeforeCleanup(IApplication application)
    {
      return false;
    }

  }
}
