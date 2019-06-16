using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Serialization.JSON;
using Azos.Text;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Serves Api documentation produced by ApiDocGenerator
  /// </summary>
  [NoCache]
  public abstract class ApiDocController : Controller
  {
    protected static Dictionary<Type, ConfigSectionNode> s_Data = new Dictionary<Type, ConfigSectionNode>();

    /// <summary>
    /// Factory method for ApiDocgenerator. Sets generation locations
    /// </summary>
    protected abstract ApiDocGenerator MakeDocGenerator();

    /// <summary>
    /// Override to generate data by calling ApiDocGenerator ad/or caching the result as necessary
    /// </summary>
    protected virtual ConfigSectionNode Data
    {
      get
      {
        var t = GetType();//of the caller instance
        lock(s_Data)
        {
          if (s_Data.TryGetValue(t, out var data)) return data;
          var gen = MakeDocGenerator();
          data = gen.Generate();
          s_Data[t] = data;
          return data;
        }
      }
    }



    [Action(Name = "all"), HttpGet]
    public object All()
    {
      if (WorkContext.RequestedJSON) return new JSONResult(Data, JsonWritingOptions.PrettyPrintRowsAsMap);
      return Data.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint);
    }


    [Action(Name = "toc"), HttpGet]
    public object Toc(string uriPattern = null, string typePattern = null)
    {

      dynamic data = JsonDynamicObject.NewMap();
      data.scopes = Data.Children
          .Where(nscope => nscope.IsSameName("scope") &&
                           (uriPattern.IsNullOrWhiteSpace() || nscope.AttrByName("uri-base").Value.MatchPattern(uriPattern)))
          .OrderBy(nscope => nscope.AttrByName("uri-base").Value )
          .Select(nscope =>
          {
            dynamic d = JsonDynamicObject.NewMap();
            d.uri    = nscope.ValOf("uri-base");
            d.id     = nscope.ValOf("run-id");
            d.title  = nscope.ValOf("title");
            d.descr  = nscope.ValOf("description");

            d.endpoints= nscope.Children
                          .Where(nep => nep.IsSameName("endpoint"))
                          .Select(nep => nep.ToDynOfAttrs("uri", "title", "run-id->id"))
                          .ToArray();

            return d;
          });

      data.docs = Data["type-schemas"].Children
          .Where(nts =>
          {
            var sku = nts.AttrByName("sku").Value;
            if (sku.IsNullOrWhiteSpace()) return false;
            if (!nts["data-doc"].Exists) return false;
            return (typePattern.IsNullOrWhiteSpace() || sku.MatchPattern(typePattern));
          })
          .OrderBy(nts => nts.AttrByName("sku").Value)
          .Select(nts =>
          {
            dynamic d = JsonDynamicObject.NewMap();
            d.id = nts.Name;
            d.sku = nts.ValOf("sku");

            return d;
          });

      data.perms = Data["type-schemas"].Children
          .Where(nts =>
          {
            var sku = nts.AttrByName("sku").Value;
            if (sku.IsNullOrWhiteSpace()) return false;
            if (!nts["permission"].Exists) return false;
            return (typePattern.IsNullOrWhiteSpace() || sku.MatchPattern(typePattern));
          })
          .OrderBy(nts => nts.AttrByName("sku").Value)
          .Select(nts =>
          {
            dynamic d = JsonDynamicObject.NewMap();
            d.id = nts.Name;
            d.sku = nts.ValOf("sku");

            return d;
          });

      if (WorkContext.RequestedJSON) return new JSONResult(data, JsonWritingOptions.PrettyPrintRowsAsMap);

      return TocView(data);
    }



    protected virtual object TocView(dynamic data)
     => new Templatization.StockContent.ApiDoc_Toc(data);


    [Action(Name = "schema"), HttpGet]
    public object Schema(string id)
    {
      const string TSCH = "type-schemas";
      const string TSKU = "type-skus";
      if (id.IsNullOrWhiteSpace()) throw HTTPStatusException.BadRequest_400("No id");

      IConfigSectionNode[] data = Data[TSCH][id].ConcatArray();
      if (!data[0].Exists)
      {
        data = Data[TSKU].Attributes
                         .Where(c => c.IsSameName(id) && c.Value.IsNotNullOrWhiteSpace())
                         .Select(c => Data[TSCH][c.Value])
                         .ToArray();
      }

      if (data.Length==0) return new Http404NotFound();

      if (WorkContext.RequestedJSON) return data;

      return SchemaView(data);
    }

    protected virtual object SchemaView(IEnumerable<IConfigSectionNode> data)
     => new Templatization.StockContent.ApiDoc_Schema(data);

    [Action(Name = "scope"), HttpGet]
    public object Scope(string id)
    {
      var data = Data.Children.FirstOrDefault(c => c.IsSameName("scope") && c.ValOf("run-id").EqualsOrdIgnoreCase(id));
      if (data==null) return new Http404NotFound();
      if (WorkContext.RequestedJSON) return data;

      var scopeContent = data.ValOf("doc-content");
      var excision = MarkdownUtils.ExciseSection(scopeContent, "## Endpoints");

      var epContent = new StringBuilder();
      epContent.AppendLine("## Endpoints");

      foreach(var nep in data.Children.Where( c=> c.IsSameName("endpoint")))
      {
        var epdoc = nep.ValOf("doc-content-tpl");
        var hadContent = false;
        if (epdoc.IsNotNullOrWhiteSpace())
        {
          hadContent = true;
          //epoint content in ep section scope
          epdoc = MarkdownUtils.EvaluateVariables(epdoc, (v) =>
          {
            if (v.IsNullOrWhiteSpace()) return v;
            //Escape: ``{{a}}`` -> `{a}`
            if (v.StartsWith("{") && v.EndsWith("}")) return v.Substring(1, v.Length - 2);
            if (v.StartsWith("@")) return $"`{{{v}}}`";//do not expand TYPE spec here

            //else navigate config path
            return nep.Navigate(v).Value;
          });
          epContent.AppendLine(epdoc);
        }
        else
        {
          //otherwise build EPOINT documentation here
          epContent.AppendLine("### {0} - {1}".Args(nep.ValOf("uri"), nep.ValOf("title")));
          var d = nep.ValOf("description");
          if (d.IsNotNullOrWhiteSpace()) epContent.AppendLine(d);
        }

        //Add type map sections:
        var refTypes = nep.Attributes.Where(a => a.IsSameName("tp-ref"));
        if (refTypes.Any())
        {
          epContent.AppendLine("##### Referenced Types");
          foreach(var tref in refTypes)
          {
            var nt = Data["type-schemas"][tref.Value];
            var sku = nt.ValOf("sku");
            if (sku.IsNullOrWhiteSpace()) continue;
            hadContent = true;
            epContent.AppendLine("* <a href=\"schema?id={0}\">{1}</a>".Args(tref.Value, sku));
          }
        }

        if (hadContent) epContent.AppendLine("##### Definition Metadata");

        epContent.AppendLine("```");
        epContent.AppendLine(nep.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint));
        epContent.AppendLine("```");
        epContent.AppendLine("---");

      }//foreach endpoint



      //eval variables
      var finalMarkdown = "{0}\n{1}\n{2}".Args(excision.content.Substring(0, excision.iexcision),
                                               epContent.ToString(),
                                               excision.content.Substring(excision.iexcision));

      //eval type references
      finalMarkdown = MarkdownUtils.EvaluateVariables(finalMarkdown, v =>
      {
        if (v.IsNotNullOrWhiteSpace() && v.StartsWith("@"))
        {
          v = v.Substring(1);
          return "<a href=\"{0}\">{1} Schema</a>".Args("schema?id={0}".Args(v), v);
        }
        return v;
      });

      var html = MarkdownUtils.MarkdownToHtml(finalMarkdown);
      return ScopeView(html);
    }

    protected virtual object ScopeView(string html)
     => new Templatization.StockContent.ApiDoc_Scope(html);

  }
}
