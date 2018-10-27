/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Decorates controller classes or actions that set NoCache headers in response. By default Force = true
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
  public sealed class NoCacheAttribute : ActionFilterAttribute
  {
    public NoCacheAttribute() : base(0)
    {
      Force = true;
    }

    public NoCacheAttribute(bool force, int order) : base(order)
    {
      Force = force;
    }

    public bool Force{ get; private set; }


    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      return false;
    }

    protected internal override bool AfterActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      work.Response.SetNoCacheHeaders(Force);
      return false;
    }

    protected internal override void ActionInvocationFinally(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {

    }

  }
}
