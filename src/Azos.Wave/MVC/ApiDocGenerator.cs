using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Collections;
using Azos.Conf;
using Azos.Security;
using Azos.Text;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Creates a configuration object by examining API controller classes and action methods decorated by ApiDoc* via reflection.
  /// You can derive from this class to generate more detailed results
  /// </summary>
  public class ApiDocGenerator : IMetadataGenerator
  {
    /// <summary>
    /// Represents a MVC controller scope during metadata generation
    /// </summary>
    public struct ControllerContext
    {
      public ControllerContext(ApiDocGenerator gen, ApiControllerDocAttribute attr)
      {
        Generator = gen;
        ApiDocAttr = attr;
      }
      public readonly ApiDocGenerator Generator;
      public readonly ApiControllerDocAttribute ApiDocAttr;
    }

    /// <summary>
    /// Represents an MVC action method scope within MVC controller scope during metadata generation
    /// </summary>
    public struct EndpointContext
    {
      public EndpointContext(ApiDocGenerator gen, Type tController, ApiControllerDocAttribute cattr, ApiEndpointDocAttribute eattr, MethodInfo method)
      {
        Generator = gen;
        TController = tController;
        ApiControllerDocAttr = cattr;
        ApiEndpointDocAttr = eattr;
        Method = method;
      }
      public readonly ApiDocGenerator Generator;
      public readonly Type TController;
      public readonly ApiControllerDocAttribute ApiControllerDocAttr;
      public readonly ApiEndpointDocAttribute ApiEndpointDocAttr;
      public readonly MethodInfo Method;
    }

    /// <summary>
    /// Provides the location of controllers
    /// </summary>
    public sealed class ControllerLocation
    {
      public ControllerLocation(string assemblyName, string namespaceName)
      {
        AssemblyName = assemblyName.NonBlank(nameof(assemblyName));
        Namespace = namespaceName.NonBlank(nameof(namespaceName));
      }

      public readonly string AssemblyName;
      public readonly string Namespace;

      /// <summary>
      /// returns all controller types regardless of ApiDoc decorations
      /// </summary>
      public IEnumerable<Type> AllControllerTypes
      {
        get
        {
          var asm = Assembly.LoadFrom(AssemblyName);
          return asm.GetTypes().Where(t => !t.IsAbstract &&
                                            t.IsSubclassOf(typeof(Controller)) &&
                                            t.Namespace.MatchPattern(Namespace)).ToArray();
        }
      }
    }



    public ApiDocGenerator(){ }

    private class instanceList : List<object>
    {
      public instanceList() { Guid = Guid.NewGuid(); }
      public readonly Guid Guid;
    }

    private Dictionary<Type, instanceList> m_TypesToDescribe = new Dictionary<Type, instanceList>();

    /// <summary>
    /// A list of locations where system looks for controllers to generate Api docs from
    /// </summary>
    public List<ControllerLocation> Locations { get; } = new List<ControllerLocation>();


    /// <summary>
    /// Controls the level of detail for generated metadata
    /// </summary>
    public MetadataDetailLevel DetailLevel { get; set;}

    /// <summary>
    /// Generates the resulting config object
    /// </summary>
    public virtual ConfigSectionNode Generate()
    {
      m_TypesToDescribe.Clear();

      var data = MakeConfig();

      var allControllers = Locations.SelectMany(loc => loc.AllControllerTypes)
                 .Select( t => (tController: t, aController: t.GetCustomAttribute<ApiControllerDocAttribute>() ))
                 .Where(tpl => FilterControllerType(tpl.tController, tpl.aController))
                 .OrderBy( tpl => FilterControllerType(tpl.tController, tpl.aController));

      foreach(var controller in allControllers)
        PopulateController(data, controller.tController, controller.aController);

      var typesSection = data.AddChildNode("type-schemas");
      foreach(var kvp in m_TypesToDescribe)
        for(var i=0; i< kvp.Value.Count; i++)
        {
          var typeSection = typesSection.AddChildNode("{0}-{1}".Args(kvp.Value.Guid, i));
          CustomMetadataAttribute.Apply(kvp.Key, kvp.Value[i], this, typeSection);
        }

      return data;
    }

    public string AddTypeToDescribe(Type type, object instance = null)
    {
      instanceList list;
      if (!m_TypesToDescribe.TryGetValue(type, out list))
      {
        list = new instanceList();
        list.Add(null);//always at index #0
        m_TypesToDescribe.Add(type, list);
      }

      var idx = list.FindIndex( i=>object.ReferenceEquals(i, instance));
      if (idx<0)
      {
        list.Add(instance);
        idx = list.Count-1;
      }

      return "{0}-{1}".Args(list.Guid, idx);
    }

    public virtual ConfigSectionNode MakeConfig() => Configuration.NewEmptyRoot(GetType().Name);
    public virtual bool FilterControllerType(Type tController, ApiControllerDocAttribute attr) => !tController.IsAbstract && attr != null;
    public virtual object OrderControllerType(Type tController, ApiControllerDocAttribute attr) => attr.BaseUri;

    public virtual IEnumerable<EndpointContext> GetApiMethods(Type tController, ApiControllerDocAttribute attr)
     => tController.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                   .Where(mi => mi.GetCustomAttribute<ApiEndpointDocAttribute>()!=null)
                   .Select(mi => new EndpointContext(this, tController, attr, mi.GetCustomAttribute<ApiEndpointDocAttribute>(), mi));

    public virtual void PopulateController(ConfigSectionNode data, Type ctlType, ApiControllerDocAttribute ctlAttr)
     => CustomMetadataAttribute.Apply(ctlType, new ControllerContext(this, ctlAttr), this, data);
  }
}
