/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;
using System.Threading.Tasks;
using Azos.Data;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Decorates controller classes or actions that need to check CSRF token on POST against the user session
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public sealed class SessionCSRFCheckAttribute : ActionFilterAttribute
  {
    public const string DEFAULT_TOKEN_NAME = CoreConsts.CSRF_TOKEN_NAME;


    public SessionCSRFCheckAttribute() {}


    public string TokenName{ get; set; } = DEFAULT_TOKEN_NAME;
    public bool OnlyExistingSession{ get; set; } = true;


    protected internal override async ValueTask<(bool, object)> BeforeActionInvocationAsync(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, object result)
    {
      if (work.IsGET) return (false, result);

      work.NeedsSession(OnlyExistingSession);

      var session = work.Session;
      var supplied = (await work.GetWholeRequestAsJsonDataMapAsync().ConfigureAwait(false))[TokenName].AsString();

      var bad = session==null;

      if (!bad && session.LastLoginType!=Apps.SessionLoginType.Robot)
         bad = !session.CSRFToken.EqualsOrdSenseCase(supplied);

      if (bad) throw new HTTPStatusException(WebConsts.STATUS_400, WebConsts.STATUS_400_DESCRIPTION, "CSRF failed");

      return (false, result);
    }

    protected internal override ValueTask<(bool, object)> AfterActionInvocationAsync(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, object result)
      => new ValueTask<(bool, object)>((false, result));

    protected internal override ValueTask<object> ActionInvocationFinallyAsync(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, object result)
      => new ValueTask<object>(result);
  }
}
