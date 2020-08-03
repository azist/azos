using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Azos.Apps;

namespace Azos.Tools.Crun.Logic
{
  /// <summary>
  /// The command dto model used in mapping methods to
  /// targeted CmdAttribute decorated methods.
  /// </summary>
  public class Cmd
  {
    /// <summary>The attribute details of a command method</summary>
    public CmdAttribute Attribute { get; set; }

    /// <summary>The associated method info command logic to invoke</summary>
    public MethodInfo Method { get; set; }
  }

  /// <summary>
  /// The Attribute class for decorating CRUN sub command methods to be
  /// executed invoked by the ProgramBody run method.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
  public sealed class CmdAttribute : Attribute
  {
    /// <summary>
    /// The default CRUN sub command attribute constructor
    /// </summary>
    /// <param name="commandName">The name of the sub command (args[0] value)</param>
    /// <param name="description">A short description of the method's purpose</param>
    /// <param name="helpFile">Alternate help file resource (default: "Help.txt")</param>
    /// <param name="welcomeFile">Alternate welcom file resource (default: "Welcome.txt")</param>
    public CmdAttribute(string commandName, string description = null, string helpFile = null, string welcomeFile = null)
    {
      CommandName = commandName;
      Description = description;
      HelpFile = helpFile ?? "Help.txt";
      WelcomeFile = welcomeFile ?? "Welcome.txt";
    }

    /// <summary>The command name matching args[0]</summary>
    public string CommandName { get; private set; }

    /// <summary>A short description of the method's purpose</summary>
    public string Description { get; private set; }

    /// <summary>Alternate help file resource (default: "Help.txt")</summary>
    public string HelpFile { get; private set; }

    /// <summary>Alternate welcom file resource (default: "Welcome.txt")</summary>
    public string WelcomeFile { get; private set; }

    /// <summary>
    /// Returns a command name / Cmd <![CDATA[Dictionary<string, Cmd>]]> for attributed methods.
    /// </summary>
    public static Dictionary<string, Cmd> GetCommandDictionary(Type t)
    {
      var rv = new Dictionary<string, Cmd>();
      foreach (var c in getCmdsByType(t))
      {
        rv.Add(c.Attribute.CommandName, c);
      }
      return rv;
    }

    private static IEnumerable<Cmd> getCmdsByType(Type t)
    {
      return t.GetMethods(BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance)
        .Where(y => y.GetParameters()?.Length == 1 && y.GetParameters().First().ParameterType == typeof(AzosApplication))
        .SelectMany(x => getCmdsByMethod(x));
    }

    private static IEnumerable<Cmd> getCmdsByMethod(MethodInfo method)
    {
      return method.GetCustomAttributes<CmdAttribute>().Cast<CmdAttribute>()
        .Select(x => new Cmd() { Attribute = x, Method = method });
    }
  }
}
