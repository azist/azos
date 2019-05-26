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

  }
}
