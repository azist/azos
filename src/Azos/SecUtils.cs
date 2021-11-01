/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using Azos.Security;
using Azos.Serialization.JSON;

namespace Azos
{
  /// <summary>
  /// Provides security utility functions used by the majority of projects
  /// </summary>
  public static class SecUtils
  {
    /// <summary>
    /// Concatenates multiple permissions in Enumerable
    /// </summary>
    public static IEnumerable<Permission> And(this Permission permission, Permission another)
    {
      if (permission!=null) yield return permission;
      if (another!=null) yield return another;
    }

    /// <summary>
    /// Concatenates multiple permissions in Enumerable
    /// </summary>
    public static IEnumerable<Permission> And(this IEnumerable<Permission> permissions, Permission another)
     => permissions ?? Enumerable.Empty<Permission>().AddOneAtEnd(another);

    /// <summary>
    /// Checks a single specified permission in the calling scope security context
    /// </summary>
    /// <param name="app">Non-null app context</param>
    /// <param name="permission">Permission instance to check</param>
    /// <param name="caller">The caller name is derived from CallerMemebr compiler info</param>
    /// <param name="session">The caller session scope or null in which case an ambient caller scope will be used</param>
    /// <remarks>
    /// Use case:
    /// <code>
    /// app.Authroize(new A());
    /// </code>
    /// </remarks>
    public static void Authorize(this IApplication app, Permission permission, [CallerMemberName] string caller = null, Apps.ISession session = null)
     => Permission.AuthorizeAndGuardAction(app.NonNull(nameof(app)).SecurityManager, permission, caller, session ?? Ambient.CurrentCallSession);

    /// <summary>
    /// Checks the specified permissions in the calling scope security context
    /// </summary>
    /// <param name="app">Non-null app context</param>
    /// <param name="permissions">Permission instances to check</param>
    /// <param name="caller">The caller name is derived from CallerMemebr compiler info</param>
    /// <param name="session">The caller session scope or null in which case an ambient caller scope will be used</param>
    /// <remarks>
    /// Use case:
    /// <code>
    /// app.Authroize(Permission.All(new A(), new B());
    /// app.Authorize(new A().And(new B()).And(new C()));
    /// </code>
    /// </remarks>
    public static void Authorize(this IApplication app, IEnumerable<Permission> permissions, [CallerMemberName] string caller = null, Apps.ISession session = null)
     => Permission.AuthorizeAndGuardAction(app.NonNull(nameof(app)).SecurityManager, permissions, caller, session ?? Ambient.CurrentCallSession);
  }
}
