/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Azos.Apps
{
  /// <summary>
  /// Denotes types of session login
  /// </summary>
  public enum SessionLoginType
  {
     /// <summary>
     /// Not known
     /// </summary>
     Unspecified = 0,

     /// <summary>
     /// A human user has entered/supplied/provided the credentials
     /// </summary>
     Human,

     /// <summary>
     /// A robot/computer/process/API caller
     /// </summary>
     Robot
  }

  /// <summary>
  /// Describes user session
  /// </summary>
  public interface ISession : IEndableInstance, IPrincipal
  {
     /// <summary>
     /// Gets unique session identifier
     /// </summary>
     Guid ID { get; }


     /// <summary>
     /// References a user this session is for
     /// </summary>
     Security.User  User { get; set;}


     /// <summary>
     /// Returns user's language preference or null
     /// </summary>
     string LanguageISOCode{ get;}


     /// <summary>
     /// Indicates that session object was just created with current request processing cycle
     /// </summary>
     bool IsNew { get;}

     /// <summary>
     /// Indicates that user login happened in current request processing cycle. This flag is
     /// useful for long term token assignment on release
     /// </summary>
     bool IsJustLoggedIn { get;}

     /// <summary>
     /// Returns the UTC DateTime of the last login/when HasJustLoggedIn() was called for the last time within the lifetime of this session object or NULL
     /// </summary>
     DateTime? LastLoginUTC { get; }

     /// <summary>
     /// Returns last login type
     /// </summary>
     SessionLoginType LastLoginType { get; }

     /// <summary>
     /// References item dictionary that may be used to persist object graphs per session
     /// </summary>
     IDictionary<object, object> Items { get; }

     /// <summary>
     /// Shortcut to this.Items.TryGetValue(...). Returns null if key is not found
     /// </summary>
     object this[object key] { get; set;}


     /// <summary>
     /// When this parameter is not null then RegenerateID() was called and session provider may need to re-stow session object under a new ID
     /// </summary>
     Guid? OldID{get;}

     /// <summary>
     /// Acquires session for use
     /// </summary>
     void Acquire();


     /// <summary>
     /// Releases session after use
     /// </summary>
     void Release();

     /// <summary>
     /// Sets IsJustLoggedIn to true to indicate that user has supplied credentials/got checked via security manager.
     /// The caller of this method is implementation-specific and depends on what is considered to be "proof of users' identity"
     /// </summary>
     void HasJustLoggedIn(SessionLoginType loginType, DateTime utcNow);

     /// <summary>
     /// Generates new GUID and stores it in ID storing old ID value in OldID property which is not serializable.
     /// The implementations may elect to re-stow the existing session under the new ID.
     /// This method is useful for security, i.e. when user logs-in we may want to re-generate ID
     /// </summary>
     void RegenerateID();
  }

}
