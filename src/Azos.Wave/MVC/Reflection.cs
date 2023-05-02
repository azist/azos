/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Azos.Collections;
using Azos.Serialization.JSON;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Provides reflection information about controller type.
  /// This is a framework internal method which is not intended to be used by business app developers
  /// </summary>
  public sealed class ControllerInfo : Collections.INamed
  {
    public static string TypeToKeyName(Type type)
    {
      return type.AssemblyQualifiedName;
    }

    public static IEnumerable<string> GetInvocationNames(MethodInfo mi)
    {
      var names = mi.GetCustomAttributes<ActionBaseAttribute>(false)
                    .Select(a => a.Name.IsNotNullOrWhiteSpace() ? a.Name : mi.Name)
                    .Where(n => n.IsNotNullOrWhiteSpace())
                    .Distinct(StringComparer.InvariantCultureIgnoreCase);
      return names;
    }

    public ControllerInfo(Type type)
    {
      string aname = null;
      try
      {
        Type = type;
        Name = TypeToKeyName(type);
        var groups = new Registry<ActionGroupInfo>();
        Groups = groups;

        JsonOptions = type.GetCustomAttribute<JsonReadingOptions>(false);

        var allMethods = GetAllActionMethods();

        foreach(var mi in allMethods)
        {
          var inames = GetInvocationNames(mi);
          foreach(var iname in inames)
          {
            aname = iname;
            var agi = groups[iname];
            if (agi==null)
            {
              agi = new ActionGroupInfo(this, iname);
              groups.Register(agi);
            }
            aname = null;
          }
        }
      }
      catch(Exception error)
      {
        throw new WaveException(StringConsts.MVC_CONTROLLER_REFLECTION_ERROR.Args(type.FullName, aname ?? CoreConsts.NULL_STRING, error.ToMessageWithType()), error);
      }
    }

    public string Name { get; }
    public readonly Type Type;
    public readonly JsonReadingOptions JsonOptions;
    public readonly IRegistry<ActionGroupInfo> Groups;

    internal IEnumerable<MethodInfo> GetAllActionMethods()
    {
      return Type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                  .Where(mi => Attribute.IsDefined(mi, typeof(ActionBaseAttribute), false) &&
                              !mi.ContainsGenericParameters &&
                              !mi.GetParameters().Any(mp=>mp.IsOut || mp.ParameterType.IsByRef)
                                );
    }
  }


  /// <summary>
  /// Provides reflection information about a group of action methods which all share the same action name(invocation name) within controller type.
  /// Invocation names are mapped to actual method names, as ActionAttribute may override the name of actual method that it decorates.
  /// This is a framework internal method which is not intended to be used by business logic developers
  /// </summary>
  public sealed class ActionGroupInfo : INamed
  {
    internal ActionGroupInfo(ControllerInfo controller, string actionInvocationName)
    {
      Controller = controller;
      Name = actionInvocationName;

      var allNamedMethods = controller.GetAllActionMethods()
                                      .Where(mi => ControllerInfo.GetInvocationNames(mi).Any( n => n.EqualsIgnoreCase(actionInvocationName)));

      var actions = new List<ActionInfo>();

      foreach(var mi in allNamedMethods)
      {
        var allAtrs = mi.GetCustomAttributes<ActionBaseAttribute>(false)
                        .Where( a => a.Name.IsNullOrWhiteSpace() || a.Name.EqualsIgnoreCase(actionInvocationName) );

        var miJsonReadingOptions = mi.GetCustomAttribute<JsonReadingOptions>(false);

        foreach(var atr in allAtrs)
          actions.Add(new ActionInfo(this, mi, atr, miJsonReadingOptions));
      }

      Actions = actions.OrderBy( ai => ai.Attribute.Order ).ToArray();

      //warm-up for possible errors
      foreach(var ai in actions)
      {
        var matches = ai.Attribute.Matches;//cause matches script to load, and bubble exceptions if it contains any
      }

    }

    /// <summary>
    /// Action invocation name- may be different from method name
    /// </summary>
    public string Name { get; }

    public readonly ControllerInfo Controller;

    /// <summary>
    /// Returns the actions in the order suitable for match making
    /// </summary>
    public IEnumerable<ActionInfo> Actions;
  }


  /// <summary>
  /// Provides reflection information about a particular action method of a controller type.
  /// This is a framework internal method which is not intended to be used by business logic developers
  /// </summary>
  public sealed class ActionInfo
  {
    internal ActionInfo(ActionGroupInfo group, MethodInfo method, ActionBaseAttribute atr, JsonReadingOptions jsonOptions)
    {
      Group = group;
      Method = method;
      Attribute = atr;
      JsonOptions = jsonOptions;
    }

    public readonly ActionGroupInfo Group;
    public readonly MethodInfo      Method;
    public readonly ActionBaseAttribute Attribute;
    public readonly JsonReadingOptions JsonOptions;
  }
}
