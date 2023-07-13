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
using System.Security.Cryptography;
using System.Threading.Tasks;
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


    ///// <summary>
    ///// A one-way function which turns a pass-phrase such as a s password into a key of the specified length.
    ///// The function uses KDF derivation to come up with a key derived form a string of Unicode characters
    ///// </summary>
    ///// <param name="passphrase">A passphrase to turn into a key, may not be null or empty</param>
    ///// <returns>A key of desired length: 16 (128bits), 32(256bits), or 64(512 bits) bytes </returns>
    //public static byte[] PassphraseStringToKey(string passphrase)
    //{
    //  passphrase.NonBlank(nameof(passphrase));
    //  var emptySalt = Array.Empty<byte>();
    //  var iterations = 1000;
    //  var desiredKeyLength = 16; // 16 bytes equal 128 bits.
    //  var hashMethod = HashAlgorithmName.SHA384;
    //  //new Rfc2898DeriveBytes(passphrase, )

    //  return Rfc2898DeriveBytes. .Pbkdf2(Encoding.Unicode.GetBytes(password),
    //                                   emptySalt,
    //                                   iterations,
    //                                   hashMethod,
    //                                   desiredKeyLength);
    //}
  }
}
