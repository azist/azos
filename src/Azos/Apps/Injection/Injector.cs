using System;
using System.Reflection;
using System.Collections.Generic;

namespace Azos.Apps.Injection
{

  public delegate void InjectorAction(object target, IApplication app);

  public class Injector
  {
    public Injector(Type type)
    {
      T = type;
      (m_Attrs, m_Action) = Build();
    }

    public  readonly Type T;
    private readonly (FieldInfo fi, InjectAttribute attr)[] m_Attrs;
    private readonly InjectorAction m_Action;

    public void Inject(object target, IApplication app)
    {
      if (m_Action == null) return;
      m_Action(target, app); //todo: Surround by exception guard
    }

    public void DefaultApply(object target, IApplication app)
    {
      for(var i=0; i<m_Attrs.Length; i++)
      {
        var entry = m_Attrs[i];
        entry.attr.Apply(target, entry.fi, app);
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
