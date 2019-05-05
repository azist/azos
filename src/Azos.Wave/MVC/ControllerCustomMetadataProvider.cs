using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Azos.Conf;
using Azos.Data;
using Azos.Platform;
using Azos.Text;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Extracts custom metadata from Controller class and its derivatives
  /// </summary>
  public sealed class ControllerCustomMetadataProvider : CustomMetadataProvider
  {
    public const string TYPE_REF = "tp-ref";

    public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, IMetadataGenerator context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
    {
      if (member is Type tController && instance is ApiDocGenerator.ControllerContext apictx)
      {
        var apiAttr = tController.GetCustomAttribute<ApiControllerDocAttribute>();
        if (apiAttr!=null)
        return describe(tController, instance, apictx, dataRoot, overrideRules);
      }

      return null;
    }

    private ConfigSectionNode describe(Type tController, object instance, ApiDocGenerator.ControllerContext apictx, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules)
    {
      var cdata = dataRoot.AddChildNode("scope");
      var cattr = apictx.ApiDocAttr;
      var docContent = tController.GetText(cattr.DocFile ?? "{0}.md".Args(tController.Name));
      var ctlTitle = MarkdownUtils.GetTitle(docContent);
      var ctlDescription = MarkdownUtils.GetTitleDescription(docContent);

      (var drequest, var dresponse) = writeCommon(ctlTitle, ctlDescription, tController, apictx.Generator, cattr, cdata);
      cdata.AddAttributeNode("uri-base", cattr.BaseUri);
      cdata.AddAttributeNode("auth", cattr.Authentication);

      cdata.AddAttributeNode("doc-content", docContent);

      var allMethodContexts = apictx.Generator.GetApiMethods(tController, apictx.ApiDocAttr);
      foreach(var mctx in allMethodContexts)
      {
        var edata = cdata.AddChildNode("endpoint");
        (var mrequest, var mresponse) = writeCommon(docContent, null, mctx.Method, apictx.Generator, mctx.ApiEndpointDocAttr, edata);

        var epuri = mctx.ApiEndpointDocAttr.Uri.AsString().Trim();
        if (epuri.IsNullOrWhiteSpace())
        {
          // infer from action attribute
          var action = mctx.Method.GetCustomAttributes<ActionBaseAttribute>().FirstOrDefault();
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
        writeCollection(mctx.ApiEndpointDocAttr.Methods, "method", mrequest, ':');

        //docAnchor
        var docAnchor = mctx.ApiEndpointDocAttr.DocAnchor.Default("### "+epuri);
        edata.AddAttributeNode("doc-content", MarkdownUtils.GetSectionContent(docContent, docAnchor));

        //Get all method attributes except ApiDoc
        var epattrs = mctx.Method
                          .GetCustomAttributes(true)
                          .Where(a => !(a is ApiDocAttribute) && !(a is ActionBaseAttribute));

        writeInstanceCollection(epattrs.Where(a => !(a is IInstanceCustomMetadataProvider) ||
                                                    (a is IInstanceCustomMetadataProvider cip &&
                                                     cip.ShouldProvideInstanceMetadata(apictx.Generator, edata))).ToArray(), TYPE_REF, edata, apictx.Generator);

        writeTypeCollection(epattrs.Select(a => a.GetType()).Distinct().ToArray(), TYPE_REF, edata, apictx.Generator);//distinct attr types

        //todo Get app parameters look for Docs and register them and also permissions
        var epargs = mctx.Method.GetParameters()
                         .Where(pi => !pi.IsOut && !pi.ParameterType.IsByRef && !pi.ParameterType.IsPrimitive )
                         .Select(pi => pi.ParameterType).ToArray();
        writeTypeCollection(epargs, TYPE_REF, edata, apictx.Generator);
      }

      return cdata;
    }

    private (Lazy<ConfigSectionNode> request, Lazy<ConfigSectionNode> response) writeCommon(
                                              string defTitle,
                                              string defDescription,
                                              MemberInfo info,
                                              ApiDocGenerator gen,
                                              ApiDocAttribute attr,
                                              ConfigSectionNode data)
    {
      MetadataUtils.AddMetadataTokenIdAttribute(data, info);

      var title = attr.Title.Default(defTitle);
      if (title.IsNotNullOrWhiteSpace())
        data.AddAttributeNode("title", title);

      var descr = attr.Description.Default(defDescription);
      if (descr.IsNotNullOrWhiteSpace())
        data.AddAttributeNode("description", descr);

      var drequest = new Lazy<ConfigSectionNode>( () => data.AddChildNode("request"));
      var dresponse = new Lazy<ConfigSectionNode>( () => data.AddChildNode("response"));

      writeCollection(attr.RequestHeaders, "header", drequest, ':');
      writeCollection(attr.RequestQueryParameters, "param", drequest, '=');
      writeCollection(attr.ResponseHeaders, "header", dresponse, ':');

      if (attr.Connection.IsNotNullOrWhiteSpace())
        data.AddAttributeNode("connection", attr.Connection);

      if (attr.RequestBody.IsNotNullOrWhiteSpace())
        drequest.Value.AddAttributeNode("body", attr.RequestBody);

      if (attr.ResponseContent.IsNotNullOrWhiteSpace())
        dresponse.Value.AddAttributeNode("content", attr.ResponseContent);

      writeTypeCollection(attr.TypeSchemas, TYPE_REF, data, gen);

      return (drequest, dresponse);
    }

    private void writeCollection(string[] items, string iname, Lazy<ConfigSectionNode> data, char delim)
    {
      if (items != null && items.Length > 0)
      {
        foreach (var item in items.Where(itm => itm.IsNotNullOrWhiteSpace())
                                                   .Select(itm => itm.SplitKVP(delim)))
        {
          var node = data.Value.AddChildNode(iname);
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
          var id = gen.AddTypeToDescribe(type);
          var node = data.Children.FirstOrDefault(c => c.IsSameName(iname) && c.AttrByName("id").Value.EqualsIgnoreCase(id));
          if (node!=null) continue;//already exists
          data.AddAttributeNode(iname, id);
        }
      }
    }

    private void writeInstanceCollection(object[] items, string iname, ConfigSectionNode data, ApiDocGenerator gen)
    {
      if (items != null && items.Length > 0)
      {
        foreach (var item in items.Where(i => i != null))
        {
          var titem = item.GetType();
          var id = gen.AddTypeToDescribe(titem, item);
          var node = data.Children.FirstOrDefault(c => c.IsSameName(iname) && c.AttrByName("id").Value.EqualsIgnoreCase(id));
          if (node != null) continue;//already exists
          data.AddAttributeNode(iname, id);
        }
      }
    }
  }
}
