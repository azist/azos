/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Globalization;

using Azos.Data;
using Azos.Time;
using Azos.Apps;
using System.Linq;

namespace Azos.Conf
{
  /// <summary>
  /// Represents an entity that runs config var macros
  /// </summary>
  public interface IMacroRunner
  {
    /// <summary>
    /// Runs macro
    /// </summary>
    string Run(IConfigSectionNode node, string inputValue, string macroName, IConfigSectionNode macroParams, object context = null);
  }


  /// <summary>
  /// Provides default implementation for configuration variable macros.
  /// NOTE: When serialized a new instance is created which will not equal by reference to static.Instance property
  /// </summary>
  [Serializable]
  public class DefaultMacroRunner : IMacroRunner
  {
    #region CONSTS

    public const string AS_PREFIX = "as-";

    #endregion

    #region STATIC

    private static DefaultMacroRunner s_Instance = new DefaultMacroRunner();

    private DefaultMacroRunner() { }

    /// <summary>
    /// Returns a singleton class instance
    /// </summary>
    public static DefaultMacroRunner Instance
    {
      get { return s_Instance; }
    }

    /// <summary>
    /// Returns a string value converted to desired type with optional default and format
    /// </summary>
    /// <param name="value">String value to convert</param>
    /// <param name="type">A type to convert string value into i.e. "decimal"</param>
    /// <param name="dflt">Default value which is used when conversion of original value can not be made</param>
    /// <param name="fmt">Format string that formats the converted value. Example: 'Goods: {0}'. The '0' index is the value</param>
    /// <returns>Converted value to desired type then back to string, using optional formatting and default if conversion did not succeed</returns>
    public static string GetValueAs(string value, string type, string dflt = null, string fmt = null)
    {
      var mn = "As" + type.CapitalizeFirstChar();

      var mi = typeof(StringValueConversion).GetMethods()
                                            .FirstOrDefault(i => i.IsStatic && i.IsPublic && i.Name == mn && i.GetParameters().Length == 2);

      object result;
      if (!string.IsNullOrWhiteSpace(dflt))
      {
        var dval = mi.Invoke(null, new object[] { dflt, null });

        result = mi.Invoke(null, new object[] { value, dval });
      }
      else
        result = mi.Invoke(null, new object[] { value, null });


      if (result == null) return string.Empty;

      if (!string.IsNullOrWhiteSpace(fmt))
        return string.Format(fmt, result);
      else
        return result.ToString();
    }

    #endregion

    public virtual string Run(IConfigSectionNode node, string inputValue, string macroName, IConfigSectionNode macroParams, object context = null)
    {

      if (macroName.StartsWith(AS_PREFIX, StringComparison.InvariantCultureIgnoreCase) && macroName.Length > AS_PREFIX.Length)
      {
        var type = macroName.Substring(AS_PREFIX.Length);

        return GetValueAs(inputValue,
                          type,
                          macroParams.Navigate("$dflt|$default").Value,
                          macroParams.Navigate("$fmt|$format").Value);

      }
      else if (string.Equals(macroName, "now", StringComparison.InvariantCultureIgnoreCase))
      {
        var utc = macroParams.AttrByName("utc").ValueAsBool(false);

        var fmt = macroParams.Navigate("$fmt|$format").ValueAsString();

        var valueAttr = macroParams.AttrByName("value");


        var now = Ambient.UTCNow;
        if (!utc)
        {
          ILocalizedTimeProvider timeProvider = context as ILocalizedTimeProvider;
          if (timeProvider == null && context is IApplicationComponent cmp)
          {
            timeProvider = cmp.ComponentDirector as ILocalizedTimeProvider;
            if (timeProvider == null) timeProvider = cmp.App;
          }

          now = timeProvider != null ? timeProvider.LocalizedTime : now.ToLocalTime();
        }

        // We inspect the "value" param that may be provided for testing purposes
        if (valueAttr.Exists)
          now = valueAttr.Value.AsDateTimeFormat(now, fmt,
                   utc ? DateTimeStyles.AssumeUniversal : DateTimeStyles.AssumeLocal);

        return fmt == null ? now.ToString() : now.ToString(fmt);
      }
      else if (string.Equals(macroName, "ctx-name", StringComparison.InvariantCultureIgnoreCase))
      {
        if (context is Collections.INamed)
          return ((Collections.INamed)context).Name;
      }


      return inputValue;
    }
  }

}
