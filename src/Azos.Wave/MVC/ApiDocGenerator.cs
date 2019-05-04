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



    public ApiDocGenerator(IApplication app){ m_App = app.NonNull(nameof(app)); }

    private IApplication m_App;

    private class instanceList : List<(object item, bool wasDescribed)>
    {
      public instanceList() { Guid = Guid.NewGuid(); }
      public readonly Guid Guid;
    }

    private Dictionary<Type, instanceList> m_TypesToDescribe = new Dictionary<Type, instanceList>();


    public IApplication App => m_App;

    /// <summary>
    /// A list of locations where system looks for controllers to generate Api docs from
    /// </summary>
    public List<ControllerLocation> Locations { get; } = new List<ControllerLocation>();


    /// <summary>
    /// Controls the level of detail for generated metadata
    /// </summary>
    public MetadataDetailLevel DetailLevel { get; set;}

    /// <summary>
    /// Specifies target name used for data docs/schema targeted metadata extraction
    /// </summary>
    public string DataTargetName { get; set;}

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
      var skuSection = data.AddChildNode("type-skus");
      bool found;
      do
      {
        found = false;
        foreach(var kvp in m_TypesToDescribe.ToArray())//make a copy as dictionary may be mutated as we describe more items
        {
          var listValueCount = kvp.Value.Count;//the new types will be added to this list at the end
          for (var i=0; i< listValueCount; i++)
          {
            var rec = kvp.Value[i];
            if (rec.wasDescribed) continue;

            kvp.Value[i] = (rec.item, true);
            found = true;

            //add type id
            var typeSection = typesSection.AddChildNode("{0}-{1}".Args(kvp.Value.Guid, i));
            CustomMetadataAttribute.Apply(kvp.Key, rec.item, this, typeSection);

            //add reverse index for SKU -> type id
            var sku = typeSection.AttrByName(CustomMetadataAttribute.CONFIG_SKU_ATTR);
            if (sku.Exists)
              skuSection.AddAttributeNode(sku.Value, typeSection.Name);
          }
        }
      }while(found);//as we describe types they may be adding other types into the loop

      return data;
    }

    public string AddTypeToDescribe(Type type, object instance = null)
    {
      if (type == typeof(object)) return "object";
      if (type == typeof(string)) return "string";
      if (type == typeof(decimal)) return "decimal";
      if (type.IsPrimitive) return "{0}".Args(type.Name.ToLowerInvariant());
      if (type.IsArray) return "{0}[]".Args(AddTypeToDescribe(type.GetElementType()));
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return "{0}?".Args(AddTypeToDescribe(type.GetGenericArguments()[0]));
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return "{0}[]".Args(AddTypeToDescribe(type.GetGenericArguments()[0]));

      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        return "map<{0},{1}>".Args(AddTypeToDescribe(type.GetGenericArguments()[0]), AddTypeToDescribe(type.GetGenericArguments()[1]));


      instanceList list;
      if (!m_TypesToDescribe.TryGetValue(type, out list))
      {
        list = new instanceList();
        list.Add((null, false));//always at index #0
        m_TypesToDescribe.Add(type, list);
      }

      var idx = list.FindIndex( litem => object.ReferenceEquals(litem.item, instance));
      if (idx<0)
      {
        list.Add((instance, false));
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
