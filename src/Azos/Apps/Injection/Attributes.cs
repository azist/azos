using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace Azos.Apps.Injection
{

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
    /// System code not intended to be used by business apps.
    /// Applies attribute with auto-scoping rules - the system tries to detect
    /// what is trying to be injected based on the supplied type of the field.
    /// Return true if assignment was made
    /// </summary>
    public bool Apply(object target, FieldInfo fInfo, IApplication app)
    {
      var tf = fInfo.FieldType;

      if (Type!=null && !tf.IsAssignableFrom(Type))
        throw new AzosException("Incompatible injection types. Injection type expectation of '{0}' is not assignable into the field '{1}: {2}'");
//todo peredelat exception na DI exception

      return DoApply(target, fInfo, app);
    }

    protected virtual bool DoApply(object target, FieldInfo fInfo, IApplication app)
    {
      var tf = fInfo.FieldType;

      //0. Inject application itself
      if (tf==typeof(IApplication))
      {
        fInfo.SetValue(target, app);
        return true;
      }

      //1. Inject module
      if (typeof(IModule).IsAssignableFrom(tf)) return TryInjectModule(target, fInfo, app);

      //2. Try app root objects
      if (TryInjectAppRootObjects(target, fInfo, app)) return true;

      return false;
    }


    /// <summary>
    /// Performs module injection by name or field type
    /// </summary>
    protected virtual bool TryInjectModule(object target, FieldInfo fInfo, IApplication app)
    {
      var needType = Type==null ? fInfo.FieldType : Type;

      IModule module = null;
      if (Name.IsNotNullOrWhiteSpace())
      {
        module = app.ModuleRoot.ChildModules[Name];
        if (module==null) return false;
        if (!needType.IsAssignableFrom(module.GetType())) return false;//type mismatch
      }
      else
      {
        module = app.ModuleRoot.ChildModules
                                 .OrderedValues
                                 .FirstOrDefault( m => needType.IsAssignableFrom(m.GetType()) );
        if (module == null) return false;
      }

      fInfo.SetValue(target, module);
      return true;
    }

    protected virtual bool TryInjectAppRootObjects(object target, FieldInfo fInfo, IApplication app)
    {
      var tf = fInfo.FieldType;

      foreach(var appRoot in GetApplicationRoots(app))
      {
        if (tf.IsAssignableFrom(appRoot.GetType()))//trips on first match
        {
          fInfo.SetValue(target, appRoot);
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Enumerates app injectable roots (root application chassis objects)
    /// </summary>
    protected virtual IEnumerable<object> GetApplicationRoots(IApplication app)
    {
      yield return app;
      yield return app.Log;
      yield return app.DataStore;
      yield return app.Instrumentation;
      yield return app.SecurityManager;
      yield return app.Random;
      yield return app.TimeSource;
      yield return app.Glue;
      yield return app.ObjectStore;
      yield return app.EventTimer;
    }
  }

  /// <summary>
  /// Performs application module injection
  /// </summary>
  public class InjectModuleAttribute : InjectAttribute
  {
    protected override bool DoApply(object target, FieldInfo fInfo, IApplication app)
    {
      return TryInjectModule(target, fInfo, app);
    }
  }

  /// <summary>
  /// Performs application singleton instance injection by field type only
  /// </summary>
  public class InjectSingletonAttribute : InjectAttribute
  {
    protected override bool DoApply(object target, FieldInfo fInfo, IApplication app)
    {
      return TryInjectAppRootObjects(target, fInfo, app);
    }

    protected override IEnumerable<object> GetApplicationRoots(IApplication app)
    {
      foreach (var singleton in app.Singletons)
        yield return singleton;
    }
  }



  class Jaba
  {
    [Inject] private ICardRenderingModule m_CardRedering;
    [Inject] private ILog m_Logger;
  }


}
