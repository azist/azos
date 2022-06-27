/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Collections;
using Azos.Conf;
using Azos.Security;
using Azos.Text;
using Azos.Data;
using Azos.Serialization.Bix;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Creates a configuration object by examining API controller classes and action methods decorated by ApiDoc* via reflection.
  /// You can derive from this class to generate more detailed results
  /// </summary>
  public class ApiDocGenerator : IMetadataGenerator
  {
    public const string DEFAULT_PUBLIC_METADATA_SECTION = "pub";

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
      /// returns all controller types regardless of ApiDoc decorations and ignore patterns
      /// </summary>
      public IEnumerable<Type> AllControllerTypes
      {
        get
        {
          var asm = Assembly.LoadFrom(AssemblyName);
          return asm.GetTypes().Where(t => !t.IsAbstract &&
                                            t.IsSubclassOf(typeof(Controller)) &&
                                            t.GetCustomAttribute<NoApiDoc>(false) == null &&
                                            t.Namespace.MatchPattern(Namespace)).ToArray();
        }
      }
    }



    public ApiDocGenerator(IApplication app){ m_App = app.NonNull(nameof(app)); }

    private IApplication m_App;
    private string m_PublicMetadataSection = DEFAULT_PUBLIC_METADATA_SECTION;

    public IApplication App => m_App;


    private class instanceList : List<(object item, bool wasDescribed)>
    {
      public instanceList(string id) => ID = id;
      public readonly string ID;
    }

    private Dictionary<Type, instanceList> m_TypesToDescribe = new Dictionary<Type, instanceList>();



    /// <summary>
    /// A list of locations where system looks for controllers to generate Api docs from
    /// </summary>
    public List<ControllerLocation> Locations { get; } = new List<ControllerLocation>();

    /// <summary>
    /// A list of Type pattern matches that must be ignored during metadata discovery, e.g. "System.Threading.*"
    /// </summary>
    public List<string> IgnoreTypePatterns { get; } = new List<string>();


    /// <summary>
    /// Controls the level of detail for generated metadata
    /// </summary>
    public MetadataDetailLevel DetailLevel { get; set;}

    /// <summary>
    /// Name of public metadata config section, `pub` by default
    /// </summary>
    public string PublicMetadataSection
    {
      get => m_PublicMetadataSection ?? DEFAULT_PUBLIC_METADATA_SECTION;
      set => m_PublicMetadataSection = value;
    }

    /// <summary>
    /// Specifies target name used for data docs/schema targeted metadata extraction
    /// </summary>
    public string DefaultDataTargetName { get; set; }

    /// <summary>
    /// An optional callback used by GetSchemaDataTargetName().
    /// Return a tuple: `(string name, bool useFieldNames)` true to use targeted field backend names
    /// </summary>
    public Func<Schema, IDataDoc, (string name, bool useFieldNames)> SchemaDataTargetNameCallback { get; set;}


    /// <summary>
    /// Gets data target name for the specified schema/type, and its optional instance.
    /// Base implementation tries to delegate to DataTargetNameCallback if it is set, otherwise returning DefaultDataTargetName.
    /// This mechanism is used to get proper target names in call context, for example
    /// you may need to get a different metadata depending on a call context such as Session.DataContextName etc.
    /// </summary>
    public virtual (string name, bool useFieldNames) GetSchemaDataTargetName(Schema schema, IDataDoc instance)
    {
      var callback = SchemaDataTargetNameCallback;

      return callback==null ? (DefaultDataTargetName, false)
                            : callback(schema, instance);
    }

    /// <summary>
    /// Generates the resulting config object
    /// </summary>
    public virtual ConfigSectionNode Generate()
    {
      m_TypesToDescribe.Clear();

      var data = MakeConfig();

      var allControllers = Locations.SelectMany(loc => loc.AllControllerTypes)
                 .Where    ( t => !IgnoreTypePatterns.Any(ignore => t.FullName.MatchPattern(ignore) ))
                 .Select   ( t => (tController: t, aController: t.GetCustomAttribute<ApiControllerDocAttribute>() ))
                 .Where    ( tpl => FilterControllerType(tpl.tController, tpl.aController))
                 .OrderBy  ( tpl => OrderControllerType(tpl.tController, tpl.aController));

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
            var typeSection = typesSection.AddChildNode("{0}:{1}".Args(kvp.Value.ID, i));
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

    private static readonly HashSet<Type> s_KnownTypes = new HashSet<Type>
    {
      typeof(object), typeof(string),
      typeof(decimal), typeof(DateTime), typeof(TimeSpan),
      typeof(GDID), typeof(Atom), typeof(Guid),
      typeof(sbyte),  typeof(byte),
      typeof(short),  typeof(ushort),
      typeof(int),    typeof(uint),
      typeof(long),   typeof(ulong),
      typeof(float),
      typeof(double),
      typeof(bool),
      typeof(char),

      typeof(decimal?), typeof(DateTime?), typeof(TimeSpan?),
      typeof(sbyte?),  typeof(byte?),
      typeof(short?),  typeof(ushort?),
      typeof(int?),    typeof(uint?),
      typeof(long?),   typeof(ulong?),
      typeof(float?),
      typeof(double?),
      typeof(bool?),
      typeof(char?)
    };

    public bool IsWellKnownType(Type type)
    {
      if (type==null) return true;
      return s_KnownTypes.Contains(type);
    }

    public string AddTypeToDescribe(Type type, object instance = null)
    {
      if (type == typeof(object)) return "object";
      if (type == typeof(string)) return "string";
      if (type == typeof(decimal)) return "decimal";
      if (type == typeof(DateTime)) return "datetime";
      if (type == typeof(TimeSpan)) return "timespan";
      if (type == typeof(GDID)) return "gdid";
      if (type == typeof(Atom)) return "atom";
      if (type == typeof(Guid)) return "gdid";
      if (type.IsPrimitive) return "{0}".Args(type.Name.ToLowerInvariant());
      if (type.IsArray) return "array({0})".Args(AddTypeToDescribe(type.GetElementType()));
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return "nullable({0})".Args(AddTypeToDescribe(type.GetGenericArguments()[0]));
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return "array({0})".Args(AddTypeToDescribe(type.GetGenericArguments()[0]));

      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        return "map({0},{1})".Args(AddTypeToDescribe(type.GetGenericArguments()[0]), AddTypeToDescribe(type.GetGenericArguments()[1]));

      if (IgnoreTypePatterns.Any(ignore => type.FullName.MatchPattern(ignore))) return DetailLevel > MetadataDetailLevel.Public ? type.Name : "sys";

      instanceList list;
      if (!m_TypesToDescribe.TryGetValue(type, out list))
      {
        list = new instanceList("@{0:x2}-{1}".Args(m_TypesToDescribe.Count, MetadataUtils.GetMetadataTokenId(type)));
        list.Add((null, false));//always at index #0
        m_TypesToDescribe.Add(type, list);
      }

      var idx = list.FindIndex( litem => object.ReferenceEquals(litem.item, instance));
      if (idx < 0)
      {
        list.Add((instance, false));
        idx = list.Count-1;
      }

      return "{0}:{1}".Args(list.ID, idx);
    }

    public virtual ConfigSectionNode MakeConfig() => Configuration.NewEmptyRoot(GetType().Name);

    public virtual bool FilterControllerType(Type tController, ApiControllerDocAttribute attr)
      => !tController.IsAbstract && attr != null && tController.GetCustomAttribute<NoApiDoc>(false) == null;

    public virtual object OrderControllerType(Type tController, ApiControllerDocAttribute attr) => attr.BaseUri;

    public virtual IEnumerable<EndpointContext> GetApiMethods(Type tController, ApiControllerDocAttribute attr)
     => tController.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                   .Where(mi => mi.GetCustomAttribute<ApiEndpointDocAttribute>() != null && mi.GetCustomAttribute<NoApiDoc>(false) == null)
                   .Select(mi => new EndpointContext(this, tController, attr, mi.GetCustomAttribute<ApiEndpointDocAttribute>(), mi));

    public virtual void PopulateController(ConfigSectionNode data, Type ctlType, ApiControllerDocAttribute ctlAttr)
     => CustomMetadataAttribute.Apply(ctlType, new ControllerContext(this, ctlAttr), this, data);

    /// <summary>
    /// Writes error to the generator, e.g. using a log
    /// </summary>
    public void ReportError(Log.MessageType type, Exception error)
    {
      if (error==null) return;

      App.Log.Write(new Log.Message{
        Type = type,
        Topic = CoreConsts.DOC_TOPIC,
        From = GetType().Name,
        Text = error.ToMessageWithType(),
        Exception = error
      });
    }
  }
}
