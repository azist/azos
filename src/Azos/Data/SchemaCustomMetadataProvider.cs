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

      var ndoc = dataRoot.AddChildNode("schema");

      if (context.DetailLevel > MetadataDetailLevel.Public)
      {
        ndoc.AddAttributeNode("name", schema.Name);
      }
      else
        ndoc.AddAttributeNode("name", schema.TypedDocType?.Name ?? schema.Name);

      ndoc.AddAttributeNode("read-only", schema.ReadOnly);

      TypedDoc doc = null;

      if (schema.TypedDocType != null)
      {
        ndoc.AddAttributeNode("typed-doc-type", context.AddTypeToDescribe(schema.TypedDocType));

        try
        { //this may fail because there may be constructor incompatibility, then we just can get instance-specific metadata
          doc = Activator.CreateInstance(schema.TypedDocType, true) as TypedDoc;
          context.App.InjectInto(doc);
        }
        catch { }
      }

      foreach (var def in schema)
      {
        var nfld = ndoc.AddChildNode("field");
        try
        {
          field(def, context, nfld, doc);
        }
        catch(Exception error)
        {
          throw new CustomMetadataException(StringConsts.METADATA_GENERATION_SCHEMA_FIELD_ERROR.Args(schema.Name, def.Name, error.ToMessageWithType()), error);
        }
      }

      return ndoc;
    }

    private void field(Schema.FieldDef def, IMetadataGenerator context, ConfigSectionNode data, TypedDoc doc)
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

      //add values from field attribute .ValueList property
      var nvlist = new Lazy<ConfigSectionNode>(()=> data.AddChildNode("value-list"));
      if (fatr.HasValueList)
        fatr.ParseValueList().ForEach(item=> nvlist.Value.AddAttributeNode(item.Key, item.Value));

      //if doc!=null call doc.GetClientFieldValueList on the instance to get values from Database lookups etc...
      if (doc!=null)
      {
        var lookup = doc.GetClientFieldValueList(def, context.DataTargetName, null);
        if (lookup != null)
         lookup.ForEach( item => nvlist.Value.AddAttributeNode(item.Key, item.Value));
      }

    }
  }
}
