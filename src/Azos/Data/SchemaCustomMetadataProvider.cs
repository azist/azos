using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Azos.Conf;

namespace Azos.Data
{
  /// <summary>
  /// Provides custom metadata for Schema
  /// </summary>
  public sealed class SchemaCustomMetadataProvider : CustomMetadataProvider
  {
    public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, IMetadataGenerator context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
    {
      var schema = instance as Schema;//is a sealed class by design

      if (schema==null) return null;

      var ndoc = dataRoot.AddChildNode("data-schema");

      ndoc.AddAttributeNode("name", schema.Name);
      ndoc.AddAttributeNode("read-only", schema.ReadOnly);

      if (schema.TypedDocType != null)
        ndoc.AddAttributeNode("typed-doc-type", context.AddTypeToDescribe(schema.TypedDocType));

      //todo Targeted TableAttrs
      //todo Targeted FieldDefs
      //context.DataTargetName

      foreach (var def in schema)
      {
        var nfld = ndoc.AddChildNode("field");
        field(def, context, nfld);
      }

      return ndoc;
    }

    private void field(Schema.FieldDef def, IMetadataGenerator context, ConfigSectionNode data)
    {
      var fname = def.GetBackendNameForTarget(context.DataTargetName, out var fldAttr);
//      if (fldAttr==null) return;

      if (context.DetailLevel> MetadataDetailLevel.Public)
      {
        data.AddAttributeNode("prop-name", def.Name);
        data.AddAttributeNode("non-ui", def.NonUI);
      }

      data.AddAttributeNode("name", fname);
      data.AddAttributeNode("order", def.Order);
return;
      data.AddAttributeNode("key", fldAttr.Key);
      data.AddAttributeNode("kind", fldAttr.Kind);

      if (fldAttr.HasValueList)
      {
        var vl = data.AddChildNode("value-list");
        foreach(var vitem in fldAttr.ParseValueList())
        {
          vl.AddAttributeNode(vitem.Key, vitem.Value);
        }
      }
    }
  }
}
