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

      if (context.DetailLevel > MetadataDetailLevel.Public)
      {
        ndoc.AddAttributeNode("name", schema.Name);
      }
      else
        ndoc.AddAttributeNode("name", schema.TypedDocType?.Name ?? schema.Name);

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
      var fname = def.GetBackendNameForTarget(context.DataTargetName, out var fatr);
      if (fatr==null) return;

      if (context.DetailLevel> MetadataDetailLevel.Public)
      {
        data.AddAttributeNode("prop-name", def.Name);
        data.AddAttributeNode("prop-type", def.Type.AssemblyQualifiedName);
        data.AddAttributeNode("non-ui", fatr.NonUI);
        data.AddAttributeNode("is-arow", fatr.IsArow);
        data.AddAttributeNode("store-flag", fatr.StoreFlag);
        data.AddAttributeNode("backend-type", fatr.BackendType);
        if (fatr.Metadata != null) data.AddChildNode(fatr.Metadata);
      }

      data.AddAttributeNode("name", fname);
      data.AddAttributeNode("type", context.AddTypeToDescribe(def.Type));
      data.AddAttributeNode("order", def.Order);

      if (fatr.Description.IsNotNullOrWhiteSpace()) data.AddAttributeNode("description", fatr.Description);
      data.AddAttributeNode("key", fatr.Key);

      if (def.Type==typeof(string))
        data.AddAttributeNode("kind", fatr.Kind);

      data.AddAttributeNode("required", fatr.Required);
      data.AddAttributeNode("visible", fatr.Required);
      data.AddAttributeNode("case", fatr.CharCase);
      if (fatr.Default != null) data.AddAttributeNode("default", fatr.Default);
      if (fatr.DisplayFormat.IsNotNullOrWhiteSpace()) data.AddAttributeNode("display-format", fatr.DisplayFormat);
      if (fatr.FormatRegExp.IsNotNullOrWhiteSpace()) data.AddAttributeNode("format-reg-exp", fatr.FormatRegExp);
      if (fatr.FormatDescription.IsNotNullOrWhiteSpace()) data.AddAttributeNode("format-description", fatr.FormatDescription);
      if (fatr.Max != null) data.AddAttributeNode("max", fatr.Max);
      if (fatr.Min != null) data.AddAttributeNode("min", fatr.Min);
      if (fatr.MinLength > 0 || fatr.MaxLength > 0) data.AddAttributeNode("min-len", fatr.MinLength);
      if (fatr.MinLength > 0 || fatr.MaxLength > 0) data.AddAttributeNode("max-len", fatr.MaxLength);


      if (fatr.HasValueList)
      {
        var vl = data.AddChildNode("value-list");
        foreach(var vitem in fatr.ParseValueList())
        {
          vl.AddAttributeNode(vitem.Key, vitem.Value);
        }
      }
    }
  }
}
