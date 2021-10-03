/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Azos.Conf;
using Azos.Serialization.JSON;
using Azos.Text;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Serves Api documentation produced by ApiDocGenerator
  /// </summary>
  [NoCache]
  [ApiControllerDoc]
  public abstract class ApiDocController : Controller
  {
    /// <summary>
    /// This method is called before cache read and any data generation.
    /// Override to reject the supplied dataContext from processing. This is needed to filter-out contexts that are not supported.
    /// The string is normalized (e.g. names are sorted and trimmed).
    /// Throw a 400 exception for unsupported context. You can also check the caller security and throw 401/403
    /// for requests that require authorization as you may have public schemas and private schemas which should NOT be disclosed to
    /// unauthorized users. The base implementation does nothing
    /// </summary>
    protected virtual void CheckDataContext(string dataContext)
    {
    }

    /// <summary>
    /// Factory method for ApiDocgenerator. Sets generation locations and target depending on normalized data context string
    /// </summary>
    protected abstract ApiDocGenerator MakeDocGenerator(string dataContext);


    protected class docCacheSingleton : Dictionary<string, ConfigSectionNode> { }

    /// <summary>
    /// Returns a singleton cache dictionary static to App instance.
    /// The returned instance is NOT thread safe and needs to be synchronized on.
    /// </summary>
    protected docCacheSingleton GetAppStaticDataCache()
     => App.Singletons.GetOrCreate( () => new docCacheSingleton() ).instance;


    /// <summary>
    /// Override to generate data by calling ApiDocGenerator ad/or caching the result as necessary
    /// </summary>
    protected virtual ConfigSectionNode Data
    {
      get
      {
        var dctx = Ambient.CurrentCallSession.GetNormalizedDataContextName();
        //This will throw if context is unsupported (HTTP400) or caller does not have access (HTTP401/403)
        CheckDataContext(dctx);

        var tp = GetType();//of the caller instance, used just as a cache key for THIS controller
        var key = dctx + "::" + tp.AssemblyQualifiedName;//the dctx is sanitized, so arbitrary dctxs are not allowed

        var cache = GetAppStaticDataCache();

        lock (cache)
        {
          if (cache.TryGetValue(key, out var data)) return data;
          var gen = MakeDocGenerator(dctx);
          data = gen.Generate();
          cache[key] = data;
          return data;
        }
      }
    }


    [ApiEndpointDoc(
      Title ="Whole Documentation Set",
      Description ="Gets all metadata for all scopes and endpoints returning it as JSON or Laconic (easier to read)",
      Methods = new []{"GET=Gets full doc set"},
      ResponseContent = "JSON or Laconic containing Api doc data"
      )]
    [Action(Name = "all"), HttpGet]
    public object All()
    {
      if (WorkContext.RequestedJson) return new JsonResult(new {OK=true, data = Data}, JsonWritingOptions.PrettyPrintRowsAsMap);
      return Data.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint);
    }

    [ApiEndpointDoc(
      Title = "Table of Contents",
      Description = "Provides table of contents (TOC) generated as an outline of the whole documentation set",
      Methods = new[] { "GET=Gets the toc" },
      RequestQueryParameters = new[]{
         "uriPattern=Pattern  match string applied as a filter to Api endpoint URIs",
         "typePattern=Pattern  match string applied as a filter to type schemas used by Api"
      },
      ResponseContent = "HTML view or Json data of TOC"
      )]
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

      if (WorkContext.RequestedJson) return new JsonResult(new { OK = true, data }, JsonWritingOptions.PrettyPrintRowsAsMap);

      return TocView(data);
    }



    protected virtual object TocView(dynamic data)
     => new Templatization.StockContent.ApiDoc_Toc(data);


    [ApiEndpointDoc(
     Title = "Data Contract Schema",
     Description = "Provides schema listing for custom data contract - a type used by the Api",
     Methods = new[] { "GET=Gets the schema" },
     RequestQueryParameters = new[]{
         "id=Required type run-id or sku for type to be described",
     },
     ResponseContent = "HTML view or Json schema data"
     )]
    [Action(Name = "schema"), HttpGet]
    public object Schema(string id)
    {
      const string TSCH = "type-schemas";
      const string TSKU = "type-skus";
      if (id.IsNullOrWhiteSpace()) throw HTTPStatusException.BadRequest_400("No id");

      IConfigSectionNode data = Data[TSCH][id];
      if (!data.Exists)
      {
        data = Data[TSKU].Attributes
                         .Where(c => c.IsSameName(id) && c.Value.IsNotNullOrWhiteSpace())
                         .Select(c => Data[TSCH][c.Value])
                         .FirstOrDefault();
      }

      if (data==null || !data.Exists) return new Http404NotFound();

      if (WorkContext.RequestedJson) return new {OK = true, data};

      return SchemaView(data);
    }

    //map<int,string>
    //array<int>
    //array<@8343495-839>
    //array<array<@78343495-839>>
    //ref:3433-343
    protected string MapType(string t)
    {
      if (t.IsNullOrWhiteSpace()) return t;

      var i = t.IndexOf('(');
      if (i>=0)
      {
        var j = t.LastIndexOf(')');
        if (j>i)
        {
          var iraw = t.Substring(i + 1, j - i - 1);
          var inner = string.Join(", ", iraw.Split(',')
                                          .Where(s => s.IsNotNullOrWhiteSpace())
                                          .Select(s => MapType(s)));

          t = t.Substring(0, i) + "&lt;" + inner + "&gt;" + t.Substring(j+1);
        }
      }

      //try to parse type id
      if (t.StartsWith("@"))
      {
        //generate A tag with href to schema id looked-up by type id
        const string TSCH = "type-schemas";
        var tref = Data[TSCH][t];
        if (tref.Exists)
        {
          return $"<a href = \"schema?id={t}\">{tref.ValOf("sku")}</a>";
        }
      }

      return t; //  a href="schema?id=?[href]"> ?[d]</a>
    }

    protected virtual object SchemaView(IConfigSectionNode data)
      => new Templatization.StockContent.ApiDoc_Schema(data, MapType);


    [ApiEndpointDoc(
    Title = "Scope Details",
    Description = "Documentation page for a scope (aka controller) containing general help text along with inventory of all endpoints",
    Methods = new[] { "GET=Gets the scope HTML view or Json" },
    RequestQueryParameters = new[]{
         "id=Required type run-id for the scope to be described",
    },
    ResponseContent = "HTML view or Json scope data"
    )]
    [Action(Name = "scope"), HttpGet]
    public object Scope(string id)
    {
      var data = Data.Children.FirstOrDefault(c =>
          c.IsSameName("scope") &&
          (
            c.ValOf("run-id").EqualsOrdIgnoreCase(id) ||
            c.ValOf("sku").EqualsOrdIgnoreCase(id) //20201018 DKh
          ));
      if (data==null) return new Http404NotFound();
      if (WorkContext.RequestedJson) return new { OK = true, data };

      var scopeContent = data.ValOf("doc-content");
      var excision = MarkdownUtils.ExciseSection(scopeContent, "## Endpoints");
      var hasEndpointsBlock = excision.iexcision >= 0;
      var finalMarkdown = scopeContent;

      if (hasEndpointsBlock)
      {
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

        //stitch endpoint content back together
        finalMarkdown = "{0}\n{1}\n{2}".Args(excision.content?.Substring(0, excision.iexcision),
                                             epContent.ToString(),
                                             excision.content?.Substring(excision.iexcision));
      }//if hasEndpointBlock


      //eval type references
      finalMarkdown = MarkdownUtils.EvaluateVariables(finalMarkdown, v =>
      {
        if (v.IsNotNullOrWhiteSpace() && v.StartsWith("@"))
        {
          v = v.Substring(1);
          return "<a href=\"{0}\">{1}</a>".Args("schema?id={0}".Args(v), v);
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
