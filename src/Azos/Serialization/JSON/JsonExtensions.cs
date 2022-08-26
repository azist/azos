/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.CodeAnalysis.Source;
using Azos.Conf;
using Azos.Web;

namespace Azos.Serialization.JSON
{
  /// <summary>
  /// Provides JSON extension methods
  /// </summary>
  public static class JsonExtensions
  {
    /// <summary>
    ///  Deserializes JSON content into dynamic JSON object
    /// </summary>
    public static dynamic JsonToDynamic(this string json, bool caseSensitiveMaps = true)
    {
        return JsonReader.DeserializeDynamic(json, caseSensitiveMaps);
    }

    /// <summary>
    ///  Deserializes JSON content into dynamic JSON object
    /// </summary>
    public static dynamic JsonToDynamic(this Stream json, Encoding encoding = null, bool caseSensitiveMaps = true)
    {
        return JsonReader.DeserializeDynamic(json, encoding, caseSensitiveMaps);
    }


    /// <summary>
    ///  Deserializes JSON content into dynamic JSON object
    /// </summary>
    public static dynamic JsonToDynamic(this ISourceText json, bool caseSensitiveMaps = true)
    {
        return JsonReader.DeserializeDynamic(json, caseSensitiveMaps);
    }


    /// <summary>
    ///  Deserializes JSON content into IJSONDataObject
    /// </summary>
    public static IJsonDataObject JsonToDataObject(this string json, bool caseSensitiveMaps = true)
    {
        return JsonReader.DeserializeDataObject(json, caseSensitiveMaps);
    }

    /// <summary>
    ///  Deserializes JSON content into IJSONDataObject
    /// </summary>
    public static IJsonDataObject JsonToDataObject(this Stream json, Encoding encoding = null, bool caseSensitiveMaps = true)
    {
        return JsonReader.DeserializeDataObject(json, encoding, caseSensitiveMaps);
    }

    /// <summary>
    ///  Deserializes JSON content into object
    /// </summary>
    public static Task<object> JsonToObjectAsync(this Stream json, Encoding encoding = null, bool caseSensitiveMaps = true)
     => JsonReader.DeserializeAsync(json, encoding, caseSensitiveMaps);

    /// <summary>
    ///  Deserializes JSON content into IJSONDataObject
    /// </summary>
    public static Task<object> JsonToObjectAsync(this ISourceText json, bool caseSensitiveMaps = true)
      => JsonReader.DeserializeAsync(json, caseSensitiveMaps);

    /// <summary>
    ///  Serializes object into JSON string
    /// </summary>
    public static string ToJson(this object root, JsonWritingOptions options = null)
    {
        return JsonWriter.Write(root, options);
    }

    /// <summary>
    ///  Serializes object into JSON format using provided TextWriter
    /// </summary>
    public static void ToJson(this object root, TextWriter wri, JsonWritingOptions options = null)
    {
        JsonWriter.Write(root, wri, options);
    }

    /// <summary>
    ///  Serializes object into JSON format using provided stream and optional encoding
    /// </summary>
    public static void ToJson(this object root, Stream stream, JsonWritingOptions options = null, Encoding encoding = null)
    {
        JsonWriter.Write(root, stream, options, encoding);
    }

    public const string JSON_INCLUDE_PRAGMA = "@";


    /// <summary>
    /// Creates an object which is a deep copy of the specified IJsonDataObject with includePragmas processed from a local file system
    /// </summary>
    /// <remarks>
    ///   https://github.com/azist/azos/issues/684
    /// </remarks>
    public static IJsonDataObject ProcessJsonLocalFileIncludes(this IJsonDataObject root,
                                                               IApplication app,
                                                               string rootPath,
                                                               string includePragma = null,
                                                               bool recurse = true,
                                                               string commonFilePathPragma = null)
    {
      var scope = commonFilePathPragma.IsNotNullOrWhiteSpace() ? new List<string>() : null;

      bool iscd(string p) => p.IsNotNullOrWhiteSpace() && (p.StartsWith("./") || p.StartsWith(".\\"));

      var result = ProcessJsonIncludes(root, cfg =>
      {
         var fn = cfg.ValOf("file").NonBlank("$file");

         if (scope != null)
         {
           if (!iscd(fn))
           {
              var path = "";
              foreach(var seg in scope)
              {
                if (seg.IsNullOrWhiteSpace()) continue;
                if (iscd(seg))
                {
                  path = seg;
                  continue;
                }
                path = Path.Combine(path, seg);
              }
              fn = Path.Combine(path, fn);
           }
         }

         var fullPath = rootPath.IsNullOrWhiteSpace() ? fn : Path.Combine(rootPath, fn);
         var fExt = Path.GetExtension(fn);
         var ctp = cfg.ValOf("content-type");

         var mappings = ContentType.GetContentTypeMappings(app.NonNull(nameof(app)));
         var mapping = ctp.IsNotNullOrWhiteSpace() ? mappings.MapContentType(ctp).FirstOrDefault()
                                                   : mappings.MapFileExtension(fExt);

         if (mapping == null) mapping = ContentType.Mapping.GENERIC_BINARY;

         if (mapping.IsText)
         {
           var text = File.ReadAllText(fullPath);
           if (mapping.ContentType.EqualsOrdIgnoreCase(ContentType.JSON))
             return JsonReader.DeserializeDataObject(text);
           else
             return text;
         }
         else
         {
           var bin = File.ReadAllBytes(fullPath);
           return bin;
         }

      }, includePragma, recurse,
      (isSet, map) =>
      {
        if (scope == null) return;

        if (isSet)
        {
          var cpath = map[commonFilePathPragma] as string;
          scope.Add(cpath);
        }
        else
        {
          scope.RemoveAt(scope.Count - 1);
        }
      });

      return result;
    }


    /// <summary>
    /// Creates an object which is a deep copy of the specified IJsonDataObject with includePragmas processed
    /// </summary>
    /// <remarks>
    ///   https://github.com/azist/azos/issues/684
    /// </remarks>
    public static IJsonDataObject ProcessJsonIncludes(this IJsonDataObject root,
                                                      Func<IConfigSectionNode, object> fReplace,
                                                      string includePragma = null,
                                                      bool recurse = true,
                                                      Action<bool, JsonDataMap> fScope = null)
    {
      root.NonNull(nameof(root));
      fReplace.NonNull(nameof(fReplace));
      includePragma = includePragma.Default(JSON_INCLUDE_PRAGMA);

      var callDepth = 0;

      return processJsonIncludes(ref callDepth, root, fReplace, fScope, includePragma, recurse);
    }

    private static IJsonDataObject processJsonIncludes(ref int depth,
                                                      IJsonDataObject root,
                                                      Func<IConfigSectionNode, object> fReplace,
                                                      Action<bool, JsonDataMap> fScope,
                                                      string includePragma,
                                                      bool recurse)
    {
      const int MAX_GRAPH_DEPTH = 64;

      try
      {
        if (depth++ > MAX_GRAPH_DEPTH) throw new JSONException("{0} possible circular reference".Args(nameof(ProcessJsonIncludes)));

        if (root is JsonDataMap mapRoot)
        {
          if (fScope != null) fScope(true, mapRoot);
          try
          {
            var result = new JsonDataMap(mapRoot.CaseSensitive);

            foreach (var kvp in mapRoot)
            {
              var val = kvp.Value;
              val = processIncludeValue(ref depth, val, fReplace, fScope, includePragma, recurse);
              result.Add(kvp.Key, val);
            }

            return result;
          }
          finally
          {
            if (fScope != null) fScope(false, mapRoot);
          }
        }
        else if (root is JsonDataArray arrRoot)
        {
          var result = new JsonDataArray(arrRoot.Count);
          for (var i = 0; i < arrRoot.Count; i++)
          {
            var val = arrRoot[i];
            val = processIncludeValue(ref depth, val, fReplace, fScope, includePragma, recurse);
            result.Add(val);
          }
          return result;
        }
      }
      finally
      {
        depth--;
      }

      return root;
    }//processJsonIncludes


    private static object processIncludeValue(ref int depth,
                                              object val,
                                              Func<IConfigSectionNode, object> fReplace,
                                              Action<bool, JsonDataMap> fScope,
                                              string includePragma,
                                              bool recurse)
    {
      var result = val;
      if (val is string sval && sval.StartsWith(includePragma))
      {
        var cfg = sval.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
        result = fReplace(cfg);
      }
      if (result is IJsonDataObject jdo && recurse)
      {
        result = processJsonIncludes(ref depth, jdo, fReplace, fScope, includePragma, recurse);
      }
      return result;
    }//processIncludeValue
  }
}
