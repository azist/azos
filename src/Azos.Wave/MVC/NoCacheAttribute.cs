/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Decorates controller classes or actions that set NoCache headers in response. By default Force = true
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
  public sealed class NoCacheAttribute : ActionFilterAttribute
  {
    public NoCacheAttribute() { }

    public bool Force{ get; set; } = true;

    protected internal override ValueTask<(bool, object)> BeforeActionInvocationAsync(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, object result)
    {
      return new ValueTask<(bool, object)>((false, result));
    }

    protected internal override ValueTask<(bool, object)> AfterActionInvocationAsync(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, object result)
    {
      work.Response.SetNoCacheHeaders(Force);
      return new ValueTask<(bool, object)>((false, result));
    }

    protected internal override ValueTask<object> ActionInvocationFinallyAsync(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, object result)
      => new ValueTask<object>(result);
  }
}
