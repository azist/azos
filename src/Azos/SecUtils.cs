/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Security;

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

    /// <summary>
    /// Get authorization <see cref="AccessLevel"/> for specified permission in the scope of the app
    /// </summary>
    public static AccessLevel GetAccessLevel(this IApplication app, Permission permission, User user = null)
     => app.NonNull(nameof(app)).SecurityManager.Authorize(user ?? Ambient.CurrentCallUser, permission);

    /// <summary>
    /// Get authorization <see cref="AccessLevel"/> for specified permission in the scope of the app
    /// </summary>
    public static async Task<AccessLevel> GetAccessLevelAsync(this IApplication app, Permission permission, User user = null)
     => await app.NonNull(nameof(app)).SecurityManager.AuthorizeAsync(user ?? Ambient.CurrentCallUser, permission).ConfigureAwait(false);

    /// <summary>
    /// Runs a function impersonated by a user with the specified credentials.
    /// NOTE: this is a special functionality not intended to be used in normal business applications as
    /// security is handled by the system code and this method should be rarely used when you need to impersonate a piece of code
    /// execution as some other user. For example, this may be needed in public services when internal logic should
    /// run on behalf of some system-anonymous user instead
    /// </summary>
    public static async Task<T> CallImpersonatedAsync<T>(this ISecurityManager secman, Func<Task<T>> fBody, Credentials credentials, Func<ISession> fSessionFactory = null)
    {
      secman.NonNull(nameof(secman));
      fBody.NonNull(nameof(fBody));
      credentials.NonNull(nameof(credentials));

      var user = await secman.AuthenticateAsync(credentials).ConfigureAwait(false);
      var result = await fBody.CallImpersonatedAsync(user, fSessionFactory).ConfigureAwait(false);
      return result;
    }

    /// <summary>
    /// Runs a function impersonated by the specified user.
    /// NOTE: this is a special functionality not intended to be used in normal business applications as
    /// security is handled by the system code and this method should be rarely used when you need to impersonate a piece of code
    /// execution as some other user. For example, this may be needed in public services when internal logic should
    /// run on behalf of some system-anonymous user instead
    /// </summary>
    public static async Task<T> CallImpersonatedAsync<T>(this Func<Task<T>> fBody, User user, Func<ISession> fSessionFactory = null)
    {
      fBody.NonNull(nameof(fBody));
      user.NonNull(nameof(user));

      var session = (fSessionFactory?.Invoke()) ?? new BaseSession(Guid.NewGuid(), Ambient.Random.NextRandomUnsignedLong);
      session.User = user;
      var result = await fBody.CallInSessionAsync(session).ConfigureAwait(false);
      return result;
    }

    /// <summary>
    /// Runs a function impersonated by a user of the specified session.
    /// NOTE: this is a special functionality not intended to be used in normal business applications as
    /// security is handled by the system code and this method should be rarely used when you need to impersonate a piece of code
    /// execution as some other user. For example, this may be needed in public services when internal logic should
    /// run on behalf of some system-anonymous user instead
    /// </summary>
    public static async Task<T> CallInSessionAsync<T>(this Func<Task<T>> fBody, ISession session)
    {
      fBody.NonNull(nameof(fBody));
      session.NonNull(nameof(session));

      var originalSession = ExecutionContext.Session;
      try
      {
        //-----
        ExecutionContext.__SetThreadLevelSessionContext(session);
        return await fBody().ConfigureAwait(false);
        //-----
      }
      finally
      {
        ExecutionContext.__SetThreadLevelSessionContext(originalSession);
      }
    }

  }
}
