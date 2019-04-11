/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using System.Reflection;

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
    /// Use case: `MethodBase.GetCurrentMethod().CheckPermissions(app)`
    /// </summary>
    public static MethodBase CheckPermissions(this MethodBase method, IApplication app)
    {
      app.NonNull(nameof(app));
      Permission.AuthorizeAndGuardAction(app, method, Ambient.CurrentCallSession);
      return method;
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
