using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Azos.Conf;
using Azos.Data;
using Azos.Platform;
using Azos.Text;

namespace Azos
{
  /// <summary>
  /// Defines levels of metadata detalization which is important for controlling the level of disclosure to foreign/public consumers,
  /// such as API documentation endpoints and the like
  /// </summary>
  public enum MetadataDetailLevel
  {
    /// <summary>
    /// Minimum possible metadata should be given, e.g. do not expose internal CLR entity names, internal GUIDs for types, parameter names etc.
    /// </summary>
    Minimum = int.MinValue,

    /// <summary>
    /// Default level = Public API is the most likely consumer, do not disclose detailed internal CLR type names and other system-specific internals
    /// </summary>
    Default = 0,
    /// <summary>
    /// Same as default, purposed for Public API consumption. Do not disclose system-internal data such as CLR type names
    /// </summary>
    Public = Default,

    /// <summary>
    /// Metadata is for internal system use, may disclose all internal type names etc.
    /// </summary>
    Internal = 100,

    /// <summary>
    /// Maximum level - include as much metadata detail is possible
    /// </summary>
    Maximum = int.MaxValue
  }


  /// <summary>
  /// Denotes entities that generate metadata information into structured data vector.
  /// You should be careful and set DetailLevel property to the minimum level for the consuming party
  /// in order not to disclose system-internal information which may be sensitive (e.g. internal permission names)
  /// </summary>
  public interface IMetadataGenerator
  {
    /// <summary>
    /// Application context
    /// </summary>
    IApplication App { get;}

    /// <summary>
    /// Defines the level of metadata details disclosed during metadata generation to consumer.
    /// Be careful not to disclose system internal type names and other sensitive information to the
    /// public consumer, such as an API documentation facade
    /// </summary>
    MetadataDetailLevel DetailLevel {  get; }

    /// <summary>
    /// Default target name used for extraction of targeted metadata such as database backend target name used in data documents/ schemas
    /// </summary>
    string DefaultDataTargetName { get; }

    /// <summary>
    /// Name of public metadata config section, typically `pub`
    /// </summary>
    string PublicMetadataSection { get; }


    /// <summary>
    /// A list of Type pattern matches that must be ignored during metadata discovery, e.g. "System.Threading.*"
    /// </summary>
    List<string> IgnoreTypePatterns{ get; }

    /// <summary>
    /// Generates metadata into ConfigSectionNode structure
    /// </summary>
    ConfigSectionNode Generate();


    /// <summary>
    /// Returns true if the specified type is well-known and should not need to be described, e.g. a 'string' does not need to be described
    /// </summary>
    bool IsWellKnownType(Type type);

    /// <summary>
    /// Gets data target name for the specified schema/doc type, and its optional instance.
    /// Default implementations typically just return DefaultDataTargetName.
    /// This mechanism is used to get proper target names in call context, for example
    /// you may need to get a different metadata depending on a call context such as Session.DataContextName etc.
    /// </summary>
    string GetSchemaDataTargetName(Schema schema, IDataDoc instance);

    /// <summary>
    /// Adds a type with an optional instance to be described, this is typically used to register Permissions and Doc schemas
    /// </summary>
    /// <param name="type">A type to describe</param>
    /// <param name="instance">Optional instance of the type</param>
    /// <returns>An instance unique ID identifying the type, if the type is already added to the set (and of the same instance), returns its existing id</returns>
    string AddTypeToDescribe(Type type, object instance = null);

    /// <summary>
    /// Writes error to the generator, e.g. using a log
    /// </summary>
    void ReportError(Log.MessageType type, Exception error);
  }


  /// <summary>
  /// Implemented by classes which provide metadata from their instances (not just from their types), e.g. a `Permission` may need to provide
  /// metadata based on its instance state such as requested access level
  /// </summary>
  public interface IInstanceCustomMetadataProvider
  {
    /// <summary>
    /// Returns true when the instance metadata should be used in the specified generator context
    /// </summary>
    /// <param name="context">IMetadataGenerator context in which the metadata acquisition takes place</param>
    /// <param name="dataRoot">Root data node under which THIS entity is supposed to create its sub-node to provide its metadata into</param>
    /// <returns>Returns false to excuse the instance from metadata generation</returns>
    bool ShouldProvideInstanceMetadata(IMetadataGenerator context, ConfigSectionNode dataRoot);

    /// <summary>
    /// Called by various metadata consumers to get additional metadata about the decorated type
    /// </summary>
    /// <param name="context">IMetadataGenerator context in which the metadata acquisition takes place</param>
    /// <param name="dataRoot">Root data node under which THIS entity is supposed to create its sub-node to provide its metadata into</param>
    /// <param name="overrideRules">Config node override rules to use for structured merging, or null to use the defaults</param>
    /// <returns>A new data node that this provider has written into, such as a new node which is a child of dataRoot or null if nothing was written</returns>
    ConfigSectionNode ProvideInstanceMetadata(IMetadataGenerator context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null);
  }


  /// <summary>
  /// Decorates members such as Types and Methods that provide custom metadata.
  /// The metadata is harvested into structured ConfigSectionNode, you can either specify config content in Laconic format or
  /// provide a type of CustomMetadataProvider-derived class which performs metadata acquisition imperatively in the scope of the calling context
  /// </summary>
  [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
  public sealed class CustomMetadataAttribute : Attribute
  {
    public const string CONFIG_META_SECTION = "meta";

    /// <summary>
    /// SKU are tags used to identify entities with globally-unique asset IDs (e.g. Types, etc..)
    /// </summary>
    public const string CONFIG_SKU_ATTR = "sku";


    public CustomMetadataAttribute(Type providerType)
      => ProviderType = providerType.IsOfType<CustomMetadataProvider>(nameof(providerType));

    public CustomMetadataAttribute(string laconicMetadata) : this(MetadataDetailLevel.Public, laconicMetadata)
    {
    }

    public CustomMetadataAttribute(MetadataDetailLevel detailLevel, string laconicMetadata)
    {
      ContentDetailLevel =  detailLevel;
      try
      {
        Content = (CONFIG_META_SECTION+'{'+laconicMetadata.NonBlank(nameof(laconicMetadata))+'}').AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      }
      catch(Exception error)
      {
        throw new AzosException(StringConsts.METADATA_CTOR_CONTENT_ERROR.Args(
                                 nameof(CustomMetadataAttribute),
                                 nameof(laconicMetadata),
                                 error.ToMessageWithType(), error));
      }
    }


    /// <summary>
    /// Type of CustomMetadataProvider-derivative used for dynamic invocation to get custom metadata.
    /// If this is set then Content=null
    /// </summary>
    public readonly Type ProviderType;

    /// <summary>
    /// Custom metadata for the decorated type. If this is set then ProviderType=null
    /// </summary>
    public readonly ConfigSectionNode Content;

    /// <summary>
    /// Specifies the level of detail when this attribute Content should be applied
    /// </summary>
    public readonly MetadataDetailLevel ContentDetailLevel;

    /// <summary>
    /// Applies the attribute - performs metadata acquisition by either dynamic invocation of referenced CustomMetadataProvider derivative
    /// or merging the config content
    /// </summary>
    /// <param name="target">MemberInfo such as type or MethodInfo that the attribute is being applied to</param>
    /// <param name="instance">
    /// Optional instance of Type when member represents a type, this way the metadata may depend on instance,
    /// e.g. when generating permissions the instance contains the required access level
    /// </param>
    /// <param name="context">Optional operation context</param>
    /// <param name="data">The data root to output metadata into</param>
    /// <param name="overrideRules">Config node override rules to apply while evaluating attribute inheritance chains</param>
    public static bool Apply(MemberInfo target, object instance, IMetadataGenerator context, ConfigSectionNode data, NodeOverrideRules overrideRules = null)
    {
      var imp = instance as IInstanceCustomMetadataProvider;
      if (imp != null)
      {
        if (!imp.ShouldProvideInstanceMetadata(context, data)) return false;
        data = imp.ProvideInstanceMetadata(context, data, overrideRules);//the instance-level may redefine data output root
        if (data==null) return false;//if null then nothing should be written from any attributes
      }

      var chain = new List<CustomMetadataAttribute>();

      var info = target.NonNull(nameof(target));
      while(info!=null)
      {
        var atr = info.GetCustomAttributes<CustomMetadataAttribute>(inherit: false);
        if (atr!=null) atr.Where(a => context==null || a.ContentDetailLevel <= context.DetailLevel )
                          .ForEach(a => chain.Add(a));

        if (info is Type infoType) info = infoType.BaseType;
        else if (info is MethodInfo infoMethod)
        {
          var baseInfo = infoMethod.FindImmediateBaseForThisOverride();
          if (baseInfo==null) break;
          info = baseInfo;
        }
        else break;
      }

     // if (chain.Count==0) return false;

      //from parent->child = tail->head
      for(var i=chain.Count-1; i>-1; i--)
      {
        var atr = chain[i];
        if (atr.Content!=null)
        {
          data.MergeAttributes(atr.Content, overrideRules);
          data.MergeSections(atr.Content, overrideRules);
        }
        else
        {
          var provider = (CustomMetadataProvider)Activator.CreateInstance(atr.ProviderType);
          try
          {
            provider.ProvideMetadata(target, instance, context, data, overrideRules);
          }
          finally
          {
            if (provider is IDisposable disp) disp.Dispose();
          }
        }
      }

      //Default SKU is added for types only taking type name (which may not be globally unique),
      //so set `SKU` attribute on the public types, or use [MetadataTypeSkuNamespaceMappingAttribute]
      //to map CLR namespaces to metadata sku type names
      if (target is Type typeTarget)
      {
        if (data.AttrByName(CONFIG_SKU_ATTR).Value.IsNullOrWhiteSpace())
        {
          var ns = MetadataTypeSkuNamespaceMappingAttribute.GetTypeSku(typeTarget);
          data.AddAttributeNode(CONFIG_SKU_ATTR, ns.Default(typeTarget.Name));
        }

        //enumerated types get handled automatically
        if (typeTarget.IsEnum)
        {
          var nenum = data.AddChildNode("enum");
          foreach(var v in typeTarget.GetEnumValues())
          {
            nenum.AddAttributeNode(typeTarget.GetEnumName(v), (int)v);
          }
        }

      }

      return true;
    }
  }


  /// <summary>
  /// Implemented by types which provide custom metadata for another MemberInfos, such as types or methods.
  /// The derivatives of CustomMetadataProvider are referenced by CustomMetadataAttribute.
  /// This pattern is needed because classes do not support static method polymorphism, therefore
  /// the system needs to call a virtual method on a Type (not instance).
  /// The custom metadata is output into ConfigSectionNode as a universal dynamic data bag with structured override behavior
  /// </summary>
  public abstract class CustomMetadataProvider
  {
    /// <summary>
    /// Called by various metadata consumers to get additional metadata about the decorated type
    /// </summary>
    /// <param name="member">A member (e.g. a type or a method) which is being described</param>
    /// <param name="instance">
    /// Optional instance of Type when member represents a type, this way the metadata may depend on instance,
    /// e.g. when generating permissions the instance contains the required access level
    /// </param>
    /// <param name="context">IMetadataGenerator context in which the metadata acquisition takes place</param>
    /// <param name="dataRoot">Root data node under which THIS entity is supposed to create its sub-node to provide its metadata into</param>
    /// <param name="overrideRules">Config node override rules to use for structured merging, or null to use the defaults</param>
    /// <returns>A new data node that this provider has written into, such as a new node which is a child of dataRoot</returns>
    public abstract ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, IMetadataGenerator context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null);
  }


  /// <summary>
  /// Thrown for metadata-related problems
  /// </summary>
  [Serializable]
  public class CustomMetadataException : AzosException
  {
    public CustomMetadataException(string message) : base(message) { }
    public CustomMetadataException(string message, Exception inner) : base(message, inner) { }
    protected CustomMetadataException(SerializationInfo info, StreamingContext context) : base(info, context) { Code = info.GetInt32(CODE_FLD_NAME); }
  }


  /// <summary>
  /// Applied to assemblies, maps namespace names for types to metadata namespace names used for documentation.
  /// This is needed not to disclose real CLR namespace names to the metadata consumer
  /// </summary>
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
  public sealed class MetadataTypeSkuNamespaceMappingAttribute : Attribute
  {
    public const string VAR_NS = "$ns$";
    public const string VAR_NAME = "$name$";
    public const string VAR_ASM = "$asm$";


    private static FiniteSetLookup<Type, string> s_Cache = new FiniteSetLookup<Type, string>( type => {
      var asm = type.Assembly;
      var atrs = asm.GetCustomAttributes<MetadataTypeSkuNamespaceMappingAttribute>().OrderBy(a => a.Order);
      foreach (var atr in atrs)
      {
        if (atr.ClrNamespaces.Any(nsp => type.Namespace.MatchPattern(nsp, senseCase: true)))
        {
          return (atr.Sku ?? string.Empty).Replace(VAR_NS, type.Namespace)
                                          .Replace(VAR_NAME, type.Name)
                                          .Replace(VAR_ASM, System.IO.Path.GetFileNameWithoutExtension(asm.Location.Default(string.Empty)));
        }
      }
      return string.Empty;
    });

    /// <summary>
    /// Gets SKU namespace prefix for a type. The prefix mappings are defined using [MetadataTypeSkuNamespaceMappingAttribute]
    /// attribute decorations on an assembly which contains the type of interest
    /// </summary>
    public static string GetTypeSku(Type type) => s_Cache[type.NonNull(nameof(type))];

    /// <summary>
    /// MetadataTypeSkuNamespaceMappingAttribute .ctor which maps possibly multiple pattern matches applied to CLR namespace names
    /// for types, to metadata type SKU prefixes
    /// </summary>
    /// <param name="order">The relative order of attribute evaluation among other assembly attributes</param>
    /// <param name="clrNamespaces">The pattern match expression(s) applied to CLR namespace names. Separate multiple values with ';' or '|' character</param>
    /// <param name="sku">
    /// The SKU prefix mapped to. Use `$name$` for target type name;
    /// `$ns$` for original namespace string;
    /// `$asm$` for assembly file name w/o extension and path
    /// </param>
    public MetadataTypeSkuNamespaceMappingAttribute(int order, string clrNamespaces, string sku)
    {
      Order = order;
      Sku = sku.NonBlank(nameof(sku));
      ClrNamespaces = clrNamespaces.NonBlank(nameof(clrNamespaces))
                                  .Split(';','|')
                                  .Where(s => s.IsNotNullOrWhiteSpace())
                                  .ToArray();
    }

    /// <summary>
    /// Specifies the order of attribute evaluation when multiple attributes are defined on the assembly
    /// </summary>
    public int Order{  get; private set; }

    /// <summary>
    /// A list of CLR namespace pattern matches, e.g. "My.Application.Controllers.*". The patterns are case-sensitive
    /// </summary>
    public string[] ClrNamespaces{  get; private set; }

    /// <summary>
    /// SKU used for name, use `$name$` for target type name; `$ns$` for original namespace string; `$asm$` for assembly file name w/o extension and path
    /// </summary>
    public string Sku {  get; private set; }
  }


  /// <summary>
  /// Utilities for working with custom metadata
  /// </summary>
  public static class MetadataUtils
  {
    public const string CONFIG_RUN_METADATA_ID_ATTR = "run-id";

    /// <summary>
    /// Generates string ID based on MetadataTokes: module-member
    /// </summary>
    public static string GetMetadataTokenId(MemberInfo info)
    {
      if (info==null) return "0-0";
      return "{0:x2}-{1:x2}".Args(info.Module.MetadataToken, info.MetadataToken);
    }

    /// <summary>
    /// Adds metadata token attribute to config node
    /// </summary>
    public static ConfigSectionNode AddMetadataTokenIdAttribute(ConfigSectionNode node, MemberInfo info)
    {
      node.NonEmpty(nameof(node)).AddAttributeNode(CONFIG_RUN_METADATA_ID_ATTR, GetMetadataTokenId(info));
      return node;
    }
  }

}
