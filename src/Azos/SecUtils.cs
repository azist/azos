/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

using Azos.Security;

namespace Azos
{
  /// <summary>
  /// Provides security utility functions used by the majority of projects
  /// </summary>
  public static class SecUtils
  {
    /// <summary>
    /// Checks all of the permissions decorating the calling method in the scope of current call context session.
    /// This is shortcut to Permission.AuthorizeAndGuardAction(MethodInfo).
    /// WARNING: in async methods, this call must be before the first AWAIT statement, otherwise the stack can not be unwound.
    /// Use case: `app.CheckThisCallPermissions()`
    /// </summary>
    public static void CheckThisCallPermissions(this IApplication app,  [CallerMemberName]string callingMethodName = null)
    {
      app.NonNull(nameof(app));
      callingMethodName.NonBlank(nameof(callingMethodName));

      MethodBase method = null;
      for (var i = 1; ; i++)
      {
        var frame = new StackFrame(i, false);
        method = frame.GetMethod();

        if (method==null)
          throw new SecurityException(StringConsts.SECURITY_CHECKTHISCALLPERMISSIONS_STACK_ERROR.Args(nameof(CheckThisCallPermissions), callingMethodName));

        if (method.Name == callingMethodName) break;
      }

      var session = Ambient.CurrentCallSession;
      Permission.AuthorizeAndGuardAction(app, method.DeclaringType, session);
      Permission.AuthorizeAndGuardAction(app, method, session);
    }


    ///// <summary>
    ///// Checks all of the permissions decorating the calling method in the scope of current call context session.
    ///// This is shortcut to Permission.AuthorizeAndGuardAction(MethodInfo).
    ///// Use case: `MethodBase.GetCurrentMethod().CheckPermissions(app)`
    ///// </summary>
    //public static async Task<MethodBase> CheckPermissionsAsync(this MethodBase method, IApplication app)
    //{
    //  app.NonNull(nameof(app));
    //  await Permission.AuthorizeAndGuardActionAsync(app, method, Ambient.CurrentCallSession);
    //  return method;
    //}

  }
}
