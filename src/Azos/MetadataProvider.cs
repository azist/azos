using System;
using System.Collections.Generic;
using System.Reflection;

using Azos.Conf;

namespace Azos
{
  /// <summary>
  /// Decorates classes that provide custom metadata. The attribute pattern is used because
  /// there is no way to attach a polymorphic behavior(virtual method) to a Type(not an object instance).
  /// The metadata is harvested into structured ConfigSectionNode
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public sealed class CustomMetadataAttribute : Attribute
  {
    public CustomMetadataAttribute(Type providerType)
      => ProviderType = providerType.IsOfType<CustomMetadataProvider>(nameof(providerType));

    public CustomMetadataAttribute(string laconic)
      => Content = laconic.NonBlank(nameof(laconic)).AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);


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
    /// <param name="targetType">The type being applied to</param>
    /// <param name="context">Optional operation context</param>
    public static bool Apply(Type targetType, object context, ConfigSectionNode data, NodeOverrideRules overrideRules = null)
    {
      var chain = new List<CustomMetadataAttribute>();

      var t = targetType.NonNull(nameof(targetType));
      while(t!=null)
      {
        var atr = t.GetCustomAttribute<CustomMetadataAttribute>();
        if (atr!=null) chain.Add(atr);
        t = t.BaseType;
      }

      if (chain.Count==0) return false;

      //from parent->child = tail->head
      for(var i=chain.Count-1; i>-1; i--)
      {
        var atr = chain[i];
        if (atr.Content!=null)
          data.OverrideBy(atr.Content, overrideRules);
        else
        {
          var provider = (CustomMetadataProvider)Activator.CreateInstance(atr.ProviderType);
          try
          {
            provider.ProvideMetadata(targetType, context, data);
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
  /// Implemented by types which provide custom metadata for another types.
  /// The derivatives of CustomMetadataProvider are referenced by CustomMetadataAttribute.
  /// This pattern is needed because classes do not support static method polymorphism, therefore
  /// the system needs to call a virtual method on a Type (not instance).
  /// The custom metadata is output into ConfigSectionNode as a universal dynamic data bag
  /// </summary>
  public abstract class CustomMetadataProvider
  {
    /// <summary>
    /// Called by various metadata consumers to get additional metadata about the decorated type
    /// </summary>
    /// <param name="type">A type which is being described</param>
    /// <param name="context">Optional context in which the metadata acquisition takes place</param>
    /// <param name="root">Root data node under which THIS entity is supposed to create its sub-node to provide its metadata into</param>
    /// <returns>A new data node that this provider has written into, such as a new node a child of root</returns>
    public abstract ConfigSectionNode ProvideMetadata(Type type, object context, ConfigSectionNode root);
  }
}
