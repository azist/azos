/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Azos.Collections;

namespace Azos.Apps.Injection
{
  /// <summary>
  /// Decorates fields that should be injected with app-rooted services (for example log or data store).
  /// A call to IApplication.DependencyInjector.InjectInto(instance) performs injection.
  /// Framework code invokes this method automatically for glue servers and MVC objects.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
  public class InjectAttribute : Attribute
  {
    /// <summary>
    /// When set, provides name of entity to inject, e.g. Module instance name
    /// </summary>
    public string Name {  get; set; }

    /// <summary>
    /// When set, provides the expected type of injected instance, e.g. module type
    /// </summary>
    public Type Type { get; set; }


    /// <summary>
    /// When set to true (default is false), ignores system's inability to inject the dependency.
    /// This is useful in some cases like feature-detection which can optionally rely
    /// on a dependency then test whether certain feature was injected.
    /// The best practice is to not use optional dependencies but for cases when a class provides
    /// an extra service when hosting container has that extra dependency and breaking-out the logic
    /// in multiple classes would have been impractical
    /// </summary>
    /// A typical example is the re-use of the same data Forms (data docs)
    /// on both client and server tiers of the system - a client may not have all of the extra validation services
    /// deployed, hence the form can detect the extra dependency and use it accordingly in its Validate() method
    /// <remarks>
    /// </remarks>
    public bool Optional {  get; set; }


    public override string ToString()
      => "{0}(Type: {1}, Name: {2})".Args(GetType().Name, Type?.Name ?? CoreConsts.NULL_STRING, Name ?? CoreConsts.NULL_STRING);


    /// <summary>
    /// System code not intended to be used by business apps.
    /// Applies attribute with auto-scoping rules - the system tries to detect
    /// what is trying to be injected based on the supplied type of the field.
    /// Return true if assignment was made
    /// </summary>
    public bool Apply(object target, FieldInfo fInfo, IApplicationDependencyInjector injector)
    {
      var tf = fInfo.FieldType;

      if (Type!=null && !tf.IsAssignableFrom(Type))
        throw new DependencyInjectionException(StringConsts.DI_ATTRIBUTE_TYPE_INCOMPATIBILITY_ERROR.Args(
                      Type.DisplayNameWithExpandedGenericArgs(),
                      target.GetType().DisplayNameWithExpandedGenericArgs(),
                      fInfo.ToDescription()));
      try
      {
        return DoApply(target, fInfo, injector);
      }
      catch(Exception error)
      {
        throw new DependencyInjectionException(StringConsts.DI_ATTRIBUTE_APPLY_ERROR.Args(
                      GetType().Name,
                      target.GetType().DisplayNameWithExpandedGenericArgs(),
                      fInfo.ToDescription(),
                      error.ToMessageWithType()), error);
      }
    }

    protected virtual bool DoApply(object target, FieldInfo fInfo, IApplicationDependencyInjector injector)
    {
      var tf = fInfo.FieldType;

      //0. Inject application itself
      if (tf==typeof(IApplication))
      {
        fInfo.SetValue(target, injector.App);
        return true;
      }

      //1. Inject module
      if (typeof(IModule).IsAssignableFrom(tf)) return TryInjectModule(target, fInfo, injector);

      //2. Try app root objects
      if (TryInjectAppRootObjects(target, fInfo, injector)) return true;

      return false;
    }


    /// <summary>
    /// Tries to perform module injection by name or type, returning true if assignment was made
    /// </summary>
    protected virtual bool TryInjectModule(object target, FieldInfo fInfo, IApplicationDependencyInjector injector)
    {
      var needType = Type==null ? fInfo.FieldType : Type;

      IModule module = null;
      if (Name.IsNotNullOrWhiteSpace())
      {
        module = injector.App.ModuleRoot.ChildModules[Name];
        if (module==null) return false;
        if (!needType.IsAssignableFrom(module.GetType())) return false;//type mismatch
      }
      else
      {
        module = injector.App.ModuleRoot.ChildModules
                                 .OrderedValues
                                 .FirstOrDefault( m => needType.IsAssignableFrom(m.GetType()) );
        if (module == null) return false;
      }

      fInfo.SetValue(target, module);
      return true;
    }

    /// <summary>
    /// Tries to inject app root object (such as Log, Glue etc.) returning true on a successful assignment.
    /// The default implementation trips on a first match
    /// </summary>
    protected virtual bool TryInjectAppRootObjects(object target, FieldInfo fInfo, IApplicationDependencyInjector injector)
    {
      var tf = fInfo.FieldType;
      var needType = Type==null ? tf : Type;

      foreach(var appRoot in GetApplicationRoots(injector))
      {
        if (needType.IsAssignableFrom(appRoot.GetType()))//trips on first match
        {
          if (Name.IsNotNullOrWhiteSpace() && appRoot is INamed named)
            if (!Name.EqualsIgnoreCase(named.Name)) continue;

          fInfo.SetValue(target, appRoot);
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Enumerates app injectable roots (root application chassis objects)
    /// </summary>
    protected virtual IEnumerable<object> GetApplicationRoots(IApplicationDependencyInjector injector)
      => injector.GetApplicationRoots(); //the default clones roots from the injector

  }

  /// <summary>
  /// Performs application module injection
  /// </summary>
  public class InjectModuleAttribute : InjectAttribute
  {
    protected override bool DoApply(object target, FieldInfo fInfo, IApplicationDependencyInjector injector)
      => TryInjectModule(target, fInfo, injector);
  }

  /// <summary>
  /// Performs application singleton instance injection based on the type. If Name is specified and
  /// a singleton instance is INamed, also check for name match
  /// </summary>
  public class InjectSingletonAttribute : InjectAttribute
  {
    protected override bool DoApply(object target, FieldInfo fInfo, IApplicationDependencyInjector injector)
      => TryInjectAppRootObjects(target, fInfo, injector);

    protected override IEnumerable<object> GetApplicationRoots(IApplicationDependencyInjector injector)
    {
      foreach (var singleton in injector.App.Singletons)
        yield return singleton;
    }
  }

  //class MyClass
  //{
  //  [Inject] private ICardRenderingModule m_CardRedering;
  //  [InjectModule] private IBuzzerModule m_Buzzer;
  //  [Inject] private ILog m_Logger;
  //  [InjectSingleton] private MySingleton m_Singleton;
  //}
}
