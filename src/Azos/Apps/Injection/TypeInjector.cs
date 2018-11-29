using System;
using System.Reflection;
using System.Collections.Generic;

namespace Azos.Apps.Injection
{
  /// <summary>
  /// Framework internal delegate type participating in dependency injection
  /// </summary>
  public delegate void InjectorAction(object target, IApplicationDependencyInjector appInjector);

  /// <summary>
  /// Framework-internal type which performs dependency injection on the specified type.
  /// Business app developers - do not use.
  /// Advanced: you can derive from this type in case of custom dependency injection implementation
  /// (e.g. use precompiled lambdas instead of reflection)
  /// </summary>
  public class TypeInjector
  {
    public TypeInjector(Type type)
    {
      T = type;
      (m_Attrs, m_Action) = Build();
    }

    public  readonly Type T;
    private readonly (FieldInfo fi, InjectAttribute attr)[] m_Attrs;
    private readonly InjectorAction m_Action;

    public void Inject(object target, IApplicationDependencyInjector appInjector)
    {
      if (m_Action == null) return;
      m_Action(target, appInjector); //todo: Surround by exception guard
    }

    public void DefaultApply(object target, IApplicationDependencyInjector appInjector)
    {
      for(var i=0; i<m_Attrs.Length; i++)
      {
        var entry = m_Attrs[i];
        var injected = entry.attr.Apply(target, entry.fi, appInjector);
        if (!injected)
          throw new DependencyInjectionException(StringConsts.DI_UNSATISIFED_INJECTION_ERROR.Args(
                      target.GetType().DisplayNameWithExpandedGenericArgs(),
                      entry.fi.ToDescription(),
                      entry.attr));
      }
    }

    /// <summary>
    /// Override to perform custom injection, may use expression tree/code gen for speed
    /// </summary>
    public virtual ((FieldInfo, InjectAttribute)[], InjectorAction) Build()
    {
      var allFields = T.GetFields(BindingFlags.Instance |
                                  BindingFlags.Public |
                                  BindingFlags.NonPublic);

      List<(FieldInfo, InjectAttribute)> lst = null;
      foreach(var f in allFields)
      {
        var attr = f.GetCustomAttribute<InjectAttribute>(true);
        if (attr==null) continue;
        if (lst==null) lst =new List<(FieldInfo, InjectAttribute)>();
        lst.Add((f, attr));
      }

      if (lst==null) return (null, null);
      return (lst.ToArray(), DefaultApply);
    }



  }
}
