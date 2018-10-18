/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

using Azos.Conf;

namespace Azos
{
  /// <summary>
  /// Provides configuration-related utility extensions
  /// </summary>
  public static class ConfUtils
  {

    /// <summary>
    /// Tries to convert object to laconic config content and parse i. This is a shortcut to ObjectValueConversion.AsLaconicConfig(object)
    /// </summary>
    public static ConfigSectionNode AsLaconicConfig(this string val,
                                                    ConfigSectionNode dflt = null,
                                                    string wrapRootName = "azos",
                                                    Azos.Data.ConvertErrorHandling handling = Azos.Data.ConvertErrorHandling.ReturnDefault)
    => Azos.Data.ObjectValueConversion.AsLaconicConfig(val, dflt, wrapRootName, handling);



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


  }
}
