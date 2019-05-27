/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos
{
  /// <summary>
  /// Provides configuration-related utility extensions
  /// </summary>
  public static class ConfUtils
  {

    /// <summary>
    /// Tries to convert object to laconic config content and parse it. This is a shortcut to ObjectValueConversion.AsLaconicConfig(object)
    /// </summary>
    public static ConfigSectionNode AsLaconicConfig(this string val,
                                                    ConfigSectionNode dflt = null,
                                                    string wrapRootName = "azos",
                                                    Azos.Data.ConvertErrorHandling handling = Azos.Data.ConvertErrorHandling.ReturnDefault)
    => Azos.Data.ObjectValueConversion.AsLaconicConfig(val, dflt, wrapRootName, handling);


    /// <summary>
    /// Returns the content of config node as terse laconic string capped at the specified max len (64 by default).
    /// Warning: the returned laconic is mostly used for logging and error reporting, it is not possible to
    /// read it back into config node as it is for info purposes only
    /// </summary>
    public static string AsTextSnippet(this IConfigSectionNode node, int len = 0, string ellipsis = null)
    {
      if (node==null || !node.Exists) return string.Empty;
      if (len<=0) len = 64;
      if (ellipsis==null) ellipsis = "...";
      return node.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact)
                 .TakeFirstChars(len, ellipsis);
    }


    /// <summary>
    /// Converts dictionary into configuration where every original node gets represented as a sub-section of config's root
    /// </summary>
    public static Configuration ToConfigSections(this IDictionary<string, object> dict)
    {
      var result = new MemoryConfiguration();
      result.Create();
      foreach (var pair in dict)
        result.Root.AddChildNode(pair.Key, pair.Value);

      return result;
    }

    /// <summary>
    /// Converts dictionary into configuration where every original node gets represented as an attribute of config's root
    /// </summary>
    public static Configuration ToConfigAttributes(this IDictionary<string, object> dict)
    {
      var result = new MemoryConfiguration();
      result.Create();
      foreach (var pair in dict)
        result.Root.AddAttributeNode(pair.Key, pair.Value);

      return result;
    }

    /// <summary>
    ///  Evaluates variables in a context of optional configuration supplied in XML format
    /// </summary>
    public static string EvaluateVarsInXMLConfigScope(this string line, string xmlScope = null, IEnvironmentVariableResolver envResolver = null, IMacroRunner macroRunner = null)
    {
      Configuration config = null;
      if (!string.IsNullOrWhiteSpace(xmlScope))
      {
        config = XMLConfiguration.CreateFromXML(xmlScope);
        config.EnvironmentVarResolver = envResolver;
        config.MacroRunner = macroRunner;
      }
      return line.EvaluateVarsInConfigScope(config);
    }

    /// <summary>
    ///  Evaluates variables in a context of optional configuration supplied as dictionary which is converted to attributes
    /// </summary>
    public static string EvaluateVarsInDictionaryScope(this string line, IDictionary<string, object> dict = null, IEnvironmentVariableResolver envResolver = null, IMacroRunner macroRunner = null)
    {
      if (dict == null)
        return line.EvaluateVarsInConfigScope(null);
      else
      {
        var config = dict.ToConfigAttributes();
        config.EnvironmentVarResolver = envResolver;
        config.MacroRunner = macroRunner;
        return line.EvaluateVarsInConfigScope(config);
      }
    }


    /// <summary>
    ///  Evaluates variables in a context of optional variable resolver and macro runner
    /// </summary>
    public static string EvaluateVars(this string line, IEnvironmentVariableResolver envResolver = null, IMacroRunner macroRunner = null)
    {
      var config = new MemoryConfiguration();
      config.Create();
      config.EnvironmentVarResolver = envResolver;
      config.MacroRunner = macroRunner;
      return EvaluateVarsInConfigScope(line, config);
    }

    /// <summary>
    ///  Evaluates variables in a context of optional configuration supplied as config object
    /// </summary>
    public static string EvaluateVarsInConfigScope(this string line, Configuration scopeConfig = null)
    {
      if (scopeConfig == null)
      {
        scopeConfig = new MemoryConfiguration();
        scopeConfig.Create();
      }

      return scopeConfig.Root.EvaluateValueVariables(line);
    }

    /// <summary>
    /// Puts the specified named attributes into JsonMap. If named attribute is not found it is written with null if keepAbsent=true or skipped.
    /// You can specify optional rename pattern like so "src->target" e.g. : node.ToMapOfAttrs("run-id->id","description->d");
    /// </summary>
    public static JsonDataMap ToMapOfAttrs(this IConfigSectionNode node, bool keepAbsent, params string[] attrNames)
    {
      var result = new JsonDataMap(false);
      if (node!=null && attrNames!=null)
        attrNames.ForEach( a =>
        {
          var target = a;
          var source = a;
          var i = a.IndexOf("->");
          if (i > 0 && i< a.Length-2)
          {
            source = a.Substring(0, i);
            target = a.Substring(i+2);//len of ->
          }
          var attr = node.AttrByName(source);
          if (attr.Exists || keepAbsent) result[target] = attr.Value;
        });

      return result;
    }

    /// <summary>
    /// Puts the specified named attributes into JsonDynamicObject. If attribute is not found it is skipped.
    /// You can specify optional rename pattern like so "src->target" e.g. : node.ToMapOfAttrs("run-id->id","description->d");
    /// </summary>
    public static dynamic ToDynOfAttrs(this IConfigSectionNode node, params string[] attrNames)
    {
      var map = node.ToMapOfAttrs(keepAbsent: true, attrNames: attrNames);
      return new JsonDynamicObject(map);
    }

    /// <summary>
    /// A shortcut to node.NonNull(nameof(node)).AttrByName(attrName); You can retrieve section by name using indexer
    /// </summary>
    public static IConfigAttrNode Of(this IConfigSectionNode node, string attrName) => node.NonNull(nameof(node)).AttrByName(attrName);

    /// <summary>
    /// A shortcut to node.Of(attrName).Value
    /// </summary>
    public static string ValOf(this IConfigSectionNode node, string attrName) => node.Of(attrName).Value;

  }
}
