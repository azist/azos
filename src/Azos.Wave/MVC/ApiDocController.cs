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


    [Action(Name = "index"), HttpGet]
    public object Index(string uriPattern = null)
    {
      var data = Data.Children
          .Where(nscope => nscope.IsSameName("scope") &&
                           (uriPattern.IsNullOrWhiteSpace() || nscope.AttrByName("uri-base").Value.MatchPattern(uriPattern)))
          .OrderBy(nscope => nscope.AttrByName("uri-base").Value )
          .Select(nscope => new JsonDataMap{
              { "uri", nscope.AttrByName("uri-base").Value },
              { "id", nscope.AttrByName("run-id").Value },
              { "title", nscope.AttrByName("title").Value },
              { "description", nscope.AttrByName("description").Value },
              { "endpoints", nscope.Children
                                   .Where(nep => nep.IsSameName("endpoint"))
                                   .Select(nep => nep.ToMapOfAttrs("uri", "title", "description"))
                                   .ToArray()
              },
          });

      if (WorkContext.RequestedJSON) return new JSONResult(data, JsonWritingOptions.PrettyPrintRowsAsMap);

      return IndexView(data);
    }

    //[Action(Name = "index"), HttpGet]
    //public object Index(string uriPattern = null)
    //{
    //  var data = Data.Children
    //      .Where(nscope => nscope.IsSameName("scope") &&
    //                       (uriPattern.IsNullOrWhiteSpace() || nscope.AttrByName("uri-base").Value.MatchPattern(uriPattern)))
    //      .Select(nscope => new JsonDataMap{
    //          { "title", nscope.AttrByName("title").Value },
    //          { "description", nscope.AttrByName("description").Value },
    //          { "endpoints", nscope.Children
    //                               .Where(nep => nep.IsSameName("endpoint"))
    //                               .Select(nep => nep.ToMapOfAttrs("uri", "title", "description"))
    //                               .ToArray()
    //          },
    //      });

    //  if (WorkContext.RequestedJSON) return new JSONResult(data, JsonWritingOptions.PrettyPrintRowsAsMap);

    //  return IndexView(data);
    //}

    protected virtual object IndexView(IEnumerable<JsonDataMap> data)
     => new Templatization.StockContent.ApiDoc_Index(data);


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
