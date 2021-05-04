/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Reflection;

namespace Azos.CodeAnalysis.JSON
{
  /// <summary>
  /// Base class for JSON pattern matching
  /// </summary>
  [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
  public abstract class JsonPatternMatchAttribute : Attribute
  {
    /// <summary>
    /// Checks all pattern match attributes against specified member info until first match found
    /// </summary>
    public static bool Check(MemberInfo info, JsonLexer content)
    {
      var attrs = info.GetCustomAttributes(typeof(JsonPatternMatchAttribute), true).Cast<JsonPatternMatchAttribute>();
      foreach (var attr in attrs)
        if (attr.Match(content)) return true;

      return false;
    }

    /// <summary>
    /// Override to perform actual pattern matching, i.e. the one that uses FSM
    /// </summary>
    public abstract bool Match(JsonLexer content);
  }
}
