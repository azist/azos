using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Azos.Conf;

namespace Azos.Security
{
  /// <summary>
  /// Described permissions
  /// </summary>
  public sealed class PermissionCustomMetadataProvider : CustomMetadataProvider
  {
    public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, IMetadataGenerator context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
    {
      var tperm = member.NonNull(nameof(member)) as Type;
      if (tperm==null) return null;
      var node = dataRoot.AddChildNode("permission");
      node.AddAttributeNode("id", MetadataUtils.GetMetadataTokenId(tperm));
      if (instance is Permission perm)
      {
        node.AddAttributeNode("name", perm.Name);
        node.AddAttributeNode("path", perm.Path);
        node.AddAttributeNode("description", perm.Description);
        node.AddAttributeNode("level", perm.Level);
      }
      else
      {
        node.AddAttributeNode("name", tperm.Name.Replace("Permission", string.Empty));
        node.AddAttributeNode("ns", tperm.Namespace);
      }
      return dataRoot;
    }
  }
}
