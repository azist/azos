/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Azos
{
  /// <summary>
  /// Provides core reflection utility functions used by the majority of projects
  /// </summary>
  public static class ReflectionUtils
  {
    /// <summary>
    /// For the specified method tries to find the closest base which this method overrides.
    /// Returns null if there is no base different than this method itself or the supplied method is static or not override.
    /// In contrast to .GetBaseDefintion() which returns the first method in the method override chain, this one returns the
    /// last one which this method override directly, that is: given A, B, C, D classes all but C overriding .ToString(), this method would return MethodInfo
    /// for B.ToString() if called for D.ToString() and C.ToString() was not overridden. Standard FX.GetBaseDefintion() would return object.ToString()
    /// </summary>
    /// <param name="info">MethodInfo for the specified method to find base for</param>
    /// <returns>Base that this method overrides or null. Static and non-virtuals return null</returns>
    public static MethodInfo FindImmediateBaseForThisOverride(this MethodInfo info)
    {
      if (info.NonNull(nameof(info)).IsStatic) return null;

      var veryBase = info.GetBaseDefinition();

      if (veryBase == info) return null;

      var t = info.DeclaringType.BaseType;
      while(t!=null)
      {
        var miBase = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                      .FirstOrDefault(mi => mi.GetBaseDefinition() == veryBase);
        if (miBase!=null) return miBase;
        t = t.BaseType;
      }

      return null;
    }

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

    /// <summary>
    /// Returns true if the supplied method is an async method
    /// </summary>
    public static bool IsAsyncMethod(this MethodBase method)
    {
      if (method==null) return false;
      return Attribute.IsDefined(method, typeof(AsyncStateMachineAttribute), inherit: false);
    }

  }
}
