/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Azos
{
  /// <summary>
  /// Provides core reflection utility functions used by the majority of projects
  /// </summary>
  public static class ReflectionUtils
  {
    /// <summary>
    /// Returns true when the method has the specified signature
    /// </summary>
    public static bool OfSignature(this MethodInfo method, params Type[] args)
    {
      if (method == null) return false;

      var pars = method.GetParameters();

      return (args == null || args.Length == 0) ?
              pars.Length == 0 :
              pars.Select(i => i.ParameterType).SequenceEqual(args);
    }

    /// <summary>
    /// Returns the index of a named typed method argument
    /// </summary>
    public static int IndexOfArg(this MethodInfo method, Type type, string name)
    {
      if (method == null || type == null) return -1;

      var pars = method.GetParameters();
      for (int i = 0; i < pars.Length; i++)
      {
        var par = pars[i];
        if (par.ParameterType == type && (name == null || name.EqualsSenseCase(par.Name)))
          return i;
      }

      return -1;
    }

    /// <summary>
    /// Returns MemberInfo described as short string
    /// </summary>
    public static string ToDescription(this MemberInfo mem)
    {
      var type = (mem is Type) ? ((Type)mem).FullName : (mem.DeclaringType != null) ? mem.DeclaringType.FullName : CoreConsts.UNKNOWN;
      return string.Format("{0}{{{1} '{2}'}}", type, mem.MemberType, mem.Name);
    }

  }
}
