using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Collections;
using Azos.Conf;
using Azos.Security;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Creates a configuration object by examining API controller classes and action methods decorated by ApiDoc* via reflection.
  /// You can derive from this class to generate more detailed results
  /// </summary>
  public class ApiDocGenerator
  {

    public class ControllerLocation
    {
      public ControllerLocation()
      {

      }

      public readonly string AssemblyName;
      public readonly string Namespace;

      /// <summary>
      /// returns all controller types regardless of ApiDoc decorations
      /// </summary>
      public IEnumerable<Type> AllControllerTypes => null;
    }



    public ApiDocGenerator(){ }

    private HashSet<Type> m_TypesToDescribe = new HashSet<Type>();

    public List<ControllerLocation> Locations { get; } = new List<ControllerLocation>();


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
      {
        var ctlData = data.AddChildNode("controller");
        PopulateController(ctlData, controller.tController, controller.aController);
      }

      foreach(var type in m_TypesToDescribe)
      {
        CustomMetadataAttribute.Apply(type, this, data);
        //ConfigSectionNode typData;

        ////if ()
        //typeof(Permission).IsAssignableFrom(type)
      }

      return data;
    }

    protected virtual ConfigSectionNode MakeConfig() => Configuration.NewEmptyRoot(GetType().Name);
    protected virtual bool FilterControllerType(Type tController, ApiControllerDocAttribute attr) => !tController.IsAbstract && attr != null;
    protected virtual object OrderControllerType(Type tController, ApiControllerDocAttribute attr) => attr.BaseUri;

    protected virtual IEnumerable<MethodInfo> GetApiMethods(Type tController, ApiControllerDocAttribute attr)
     => tController.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(mi => mi.GetCustomAttribute<ApiDocAttribute>()!=null);

    protected virtual void PopulateController(ConfigSectionNode data, Type ctlType, ApiControllerDocAttribute ctlAttr)
    {
      ctlAttr.Describe(this, data, ctlType);
      var apiMethods = GetApiMethods(ctlType, ctlAttr);
      foreach(var mi in apiMethods)
      {
        var epData = data.AddChildNode("endpoint");
        PopulateEndpoint(epData, ctlType, ctlAttr, mi);
      }
    }

    protected virtual void PopulateEndpoint(ConfigSectionNode data, Type ctlType, ApiControllerDocAttribute ctlAttr, MethodInfo miEndpoint)
    {

    }
  }
}
