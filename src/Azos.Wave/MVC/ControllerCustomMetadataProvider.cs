using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Azos.Conf;
using Azos.Data;
using Azos.Platform;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Extracts custom metadata from Controller class and its derivatives
  /// </summary>
  public sealed class ControllerCustomMetadataProvider : CustomMetadataProvider
  {
    public override ConfigSectionNode ProvideMetadata(MemberInfo member, object context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
    {
      if (member is Type tController && context is ApiDocGenerator.ControllerContext apictx)
      {
        var apiAttr = tController.GetCustomAttribute<ApiControllerDocAttribute>();
        if (apiAttr!=null)
        return describe(tController, apictx, dataRoot, overrideRules);
      }

      return null;
    }

    private ConfigSectionNode describe(Type tController, ApiDocGenerator.ControllerContext apictx, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules)
    {
      var cdata = dataRoot.AddChildNode("scope");
      var cattr = apictx.ApiDocAttr;
      (var drequest, var dresponse) = writeCommon(apictx.Generator, cattr, cdata);
      cdata.AddAttributeNode("uri-base", cattr.BaseUri);
      cdata.AddAttributeNode("doc-file", cattr.DocFile);
      var docContent = tController.GetText(cattr.DocFile);
      cdata.AddAttributeNode("doc-content", tController.GetText(cattr.DocFile));
      cdata.AddAttributeNode("auth", cattr.Authentication);

      var allMethodContexts = apictx.Generator.GetApiMethods(tController, apictx.ApiDocAttr);
      foreach(var mctx in allMethodContexts)
      {
        var edata = cdata.AddChildNode("endpoint");
        writeCommon(apictx.Generator, mctx.ApiEndpointDocAttr, edata);

        var epuri = mctx.ApiEndpointDocAttr.Uri.AsString().Trim();
        if (epuri.IsNullOrWhiteSpace())
        {
          // infer from action attribute
          var action = mctx.Method.GetCustomAttributes<ActionAttribute>().FirstOrDefault();
          if (action!=null)
             epuri = action.Name;
          if (epuri.IsNullOrWhiteSpace()) epuri = mctx.Method.Name;
        }


        if (!epuri.StartsWith("/"))
        {
          var bu = cattr.BaseUri.Trim();
          if (!bu.EndsWith("/")) bu += "/";
          epuri = bu + epuri;
        }

        edata.AddAttributeNode("uri", epuri);
        writeCollection(mctx.ApiEndpointDocAttr.Methods, "method", drequest, ':');
        //todo doc anchor....(parse out of docContent);

        //todo Get app parameters look for Docs and register them and also permissions
      }

      return cdata;
    }

    private (ConfigSectionNode request, ConfigSectionNode response) writeCommon(ApiDocGenerator gen, ApiDocAttribute attr, ConfigSectionNode data)
    {
      data.AddAttributeNode("title", attr.Title);
      var drequest = data.AddChildNode("request");
      var dresponse = data.AddChildNode("response");

      writeCollection(attr.RequestHeaders, "header", drequest, ':');
      writeCollection(attr.RequestQueryParameters, "param", drequest, '=');
      writeCollection(attr.ResponseHeaders, "header", dresponse, ':');

      if (attr.Connection.IsNotNullOrWhiteSpace())
        data.AddAttributeNode("connection", attr.Connection);

      if (attr.RequestBody.IsNotNullOrWhiteSpace())
        drequest.AddAttributeNode("body", attr.RequestBody);

      if (attr.ResponseContent.IsNotNullOrWhiteSpace())
        dresponse.AddAttributeNode("content", attr.ResponseContent);

      writeTypeCollection(attr.TypeSchemas, "schema", data, gen);

      return (drequest, dresponse);
    }

    private void writeCollection(string[] items, string iname, ConfigSectionNode data, char delim)
    {
      if (items != null && items.Length > 0)
      {
        foreach (var item in items.Where(itm => itm.IsNotNullOrWhiteSpace())
                                                   .Select(itm => itm.SplitKVP(delim)))
        {
          var node = data.AddChildNode(iname);
          node.AddAttributeNode("name", item.Key.Trim());
          node.AddAttributeNode("value", item.Value.Trim());
        }
      }
    }

    private void writeTypeCollection(Type[] items, string iname, ConfigSectionNode data, ApiDocGenerator gen)
    {
      if (items != null && items.Length > 0)
      {
        foreach (var type in items.Where(t => t!=null))
        {
          var id = gen.AddTypeToDescribe(type).ToString();
          var node = data.Children.FirstOrDefault(c => c.IsSameName(iname) && c.AttrByName("id").Value.EqualsIgnoreCase(id));
          if (node!=null) continue;//already exists
          node = data.AddChildNode(iname);
          node.AddAttributeNode("id", id);
          node.AddAttributeNode("name", type.DisplayNameWithExpandedGenericArgs());
        }
      }
    }
  }
}
