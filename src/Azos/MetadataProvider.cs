using System;
using System.Collections.Generic;
using System.Reflection;

using Azos.Conf;

namespace Azos
{
  /// <summary>
  /// Decorates members such as Types and Methods that provide custom metadata.
  /// The metadata is harvested into structured ConfigSectionNode, you can either specify config content in Laconic format or
  /// provide a type of CustomMetadataProvider-derived class which performs metadata acquisition imperatively in the scope of the calling context
  /// </summary>
  [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
  public sealed class CustomMetadataAttribute : Attribute
  {
    public const string CONFIG_META_SECTION = "meta";

    public CustomMetadataAttribute(Type providerType)
      => ProviderType = providerType.IsOfType<CustomMetadataProvider>(nameof(providerType));

    public CustomMetadataAttribute(string laconicMetadata)
    {
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
    /// Applies the attribute - performs metadata acquisition by either dynamic invocation of referenced CustomMetadataProvider derivative
    /// or merging the config content
    /// </summary>
    /// <param name="target">MemebrInfo such as type or MethodInfo that the attribute is being applied to</param>
    /// <param name="context">Optional operation context</param>
    /// <param name="data">The data root to output metadata into</param>
    /// <param name="overrideRules">Config node override rules to apply while evaluating attribute inheritance chains</param>
    public static bool Apply(MemberInfo target, object context, ConfigSectionNode data, NodeOverrideRules overrideRules = null)
    {
      var chain = new List<CustomMetadataAttribute>();

      var info = target.NonNull(nameof(target));
      while(info!=null)
      {
        var atr = info.GetCustomAttribute<CustomMetadataAttribute>(inherit: false);
        if (atr!=null) chain.Add(atr);

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
            provider.ProvideMetadata(target, context, data, overrideRules);
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
    /// <param name="context">Optional context in which the metadata acquisition takes place</param>
    /// <param name="dataRoot">Root data node under which THIS entity is supposed to create its sub-node to provide its metadata into</param>
    /// <param name="overrideRules">Config node override rules to use for structured merging, or null to use the defaults</param>
    /// <returns>A new data node that this provider has written into, such as a new node which is a child of root</returns>
    public abstract ConfigSectionNode ProvideMetadata(MemberInfo member, object context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null);
  }
}
