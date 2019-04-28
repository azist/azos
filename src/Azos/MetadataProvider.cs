using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Azos.Conf;

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
    /// Defines the level of metadata details disclosed during metadata generation to consumer.
    /// Be careful not to disclose system internal type names and other sensitive information to the
    /// public consumer, such as an API documentation facade
    /// </summary>
    MetadataDetailLevel DetailLevel {  get; }

    /// <summary>
    /// Target name used for extraction of targeted metadata such as database backend target name used in data documents/ schemas
    /// </summary>
    string DataTargetName {  get; }

    /// <summary>
    /// Generates metadata into ConfigSectionNode structure
    /// </summary>
    ConfigSectionNode Generate();

    /// <summary>
    /// Adds a type with an optional instance to be described, this is typically used to register Permissions and Doc schemas
    /// </summary>
    /// <param name="type">A type to describe</param>
    /// <param name="instance">Optional instance of the type</param>
    /// <returns>An instance unique ID identifying the type, if the type is already added to the set (and of the same instance), returns its existing id</returns>
    string AddTypeToDescribe(Type type, object instance = null);
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
        Content = laconicMetadata.NonBlank(nameof(laconicMetadata)).AsLaconicConfig(wrapRootName: CONFIG_META_SECTION, handling: Data.ConvertErrorHandling.Throw);
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

      if (chain.Count==0) return false;

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

      return true;
    }
  }


  /// <summary>
  /// Implemented by types which provide custom metadata for another MemebrInfos, such as types or methods.
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
  /// Utilities for working with custom metadata
  /// </summary>
  public static class MetadataUtils
  {
    /// <summary>
    /// Generates string ID based on MetadataTokes: module-member
    /// </summary>
    public static string GetMetadataTokenId(MemberInfo info)
    {
      if (info==null) return "0-0";
      return "{0:x2}-{1:x2}".Args(info.Module.MetadataToken, info.MetadataToken);
    }
  }

}
