/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;

using Azos.Conf;

namespace Azos.Data
{
  /// <summary>
  /// Provides custom metadata for DataDocuments
  /// </summary>
  public sealed class DocCustomMetadataProvider : CustomMetadataProvider
  {
    public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, IMetadataGenerator context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
    {
      var tdoc = member.NonNull(nameof(member)) as Type;
      if (tdoc == null || !typeof(Doc).IsAssignableFrom(tdoc)) return null;

      var typed = tdoc.IsSubclassOf(typeof(TypedDoc));

      var ndoc = dataRoot.AddChildNode("data-doc");
      Schema schema;
      if (instance is Doc doc) schema = doc.Schema;
      else if (typed) schema = Schema.GetForTypedDoc(tdoc);
      else schema = null;

      MetadataUtils.AddMetadataTokenIdAttribute(ndoc, tdoc);
      ndoc.AddAttributeNode("kind", typed ? "typed" : "dynamic");

      CustomMetadataAttribute.Apply(typeof(Schema), schema, context, ndoc, overrideRules);

      return ndoc;
    }
  }
}
