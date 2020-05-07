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
  public interface ISession : IPrincipal
  {
     /// <summary>
     /// Gets unique session identifier
     /// </summary>
     Guid ID { get; }

    /// <summary>
    /// Returns Session ID secret - the ulong that additionally identifies this session.
    /// This property is needed for cross-check upon GUID id lookup, so that
    /// Session ID GUID can not be forged by the client - a form of a "password"
    /// </summary>
     ulong IDSecret {get;}


    /// <summary>
    /// References a user this session is for
    /// </summary>
    Security.User  User { get; set;}


     /// <summary>
     /// Returns user's language preference or Atom.ZERO for none
     /// </summary>
     Atom LanguageISOCode{ get;}


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
     /// References item dictionary that may be used to persist object graphs per session.
     /// The property is lazily allocated on first access
     /// </summary>
     IDictionary<object, object> Items { get; }

     /// <summary>
     /// Shortcut to this.Items.TryGetValue(...). Returns null if key is not found
     /// </summary>
     object this[object key] { get; set;}


     /// <summary>
     /// Establishes an optional name(or names, using space or comma delimiters) for target data context that the session operates under.
     /// For example, this may be used to store target database instance name.
     /// Among other things, this property may be checked by permissions to provide context-aware security
     /// checks and data store may respect it to connect the session to the specific database instance (connect/string)
     /// </summary>
     /// <remarks>
     /// Usage of this property is way more efficient than using Items[key] pattern
     /// </remarks>
     string DataContextName { get; set; }


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


namespace Azos
{
  using Apps;

  /// <summary>
  /// Provides extension methods for ISession
  /// </summary>
  public static class SessionExtensions
  {
    /// <summary>
    /// Characters used as data context segment delimiters: {' ', ',', ';'}
    /// </summary>
    public static readonly char[] DATA_CTX_DELIMITERS = new []{' ', ',', ';'};

    /// <summary>
    /// Returns an enumerable of DataContextName segments parsed out of SessionDataContextName.
    /// Multiple segments are divided by the either of: ' ', ',', ';' characters/
    /// An empty sequence is returned for null session or null or empty context
    /// </summary>
    public static IEnumerable<string> GetDataContextNameSegments(this ISession session)
    {
      if (session == null || session.DataContextName.IsNullOrWhiteSpace())
        yield break;

      var segs = session.DataContextName.Split(DATA_CTX_DELIMITERS);

      foreach(var seg in segs)
      {
        if (seg.IsNotNullOrWhiteSpace())
          yield return seg.Trim();
      }
    }

    /// <summary>
    /// Returns a re-composed DataContextName string which is obtained and normalized out of Session object.
    /// The segments are parsed and sorted. Extra delimiters removed.
    /// The function ensures logical equality, e.g. "main,   data  ,business" will be normalized to "business,data,main".
    /// An empty string is returned for null session or null or empty context
    /// </summary>
    public static string GetNormalizedDataContextName(this ISession session)
    {
      var sb = new StringBuilder(48);
      var result = session.GetDataContextNameSegments()
             .OrderBy( s => s)
             .Aggregate(sb, (b, s) => (b.Length == 0 ? b : b.Append(',')).Append(s), b => b.ToString().ToLowerInvariant());

      return result;
    }
  }

}
