/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;

namespace Azos.Security
{
  /// <summary>
  /// Represents an entity that performs user authentication based on passed credentials and other security-related global tasks
  /// </summary>
  public interface ISecurityManager : IApplicationComponent
  {
    /// <summary>
    /// Returns true for security systems that support true asynchronous implementation (e.g. permissions checked via external calls)
    /// and the caller should favor async methods over sync ones. In some high-throughput systems the security manager
    /// may be implemented as 100% in-ram CPU bound code which yields much better performance and cause less GC pressure calling sync-only methods
    /// </summary>
    bool SupportsTrueAsynchrony { get; }

    /// <summary>
    /// References an entity that manages passwords such as: computes and verifies hash tokens
    /// and provides password strength verification
    /// </summary>
    IPasswordManager PasswordManager { get; }

    /// <summary>
    /// Provides cryptography services, such as message protection etc.
    /// </summary>
    ICryptoManager Cryptography{ get; }

    /// <summary>
    /// Authenticates user by checking the supplied credentials against the
    /// authentication store that this manager represents.
    /// If credential are invalid then UserKind.Invalid is returned.
    /// This method may populate user access rights completely or partially - depending on store implementation.
    /// If rights are computed on authentication than Authorize() just checks existing in-memory structure, otherwise
    ///  Authorize() may re-fetch permissions from store on every call or cache them for the specified interval in memory
    /// </summary>
    /// <param name="credentials">User credentials.
    /// Particular manager implementation may elect to support multiple credential types, i.e.
    /// IdPassword, Twitter, Facebook, OAuth, LegacySystemA/B/C etc.
    /// </param>
    /// <returns>
    /// User object. Check User.Status for UserStatus.Invalid flag to see if authentication succeeded
    /// </returns>
    User Authenticate(Credentials credentials);

    /// <summary>
    /// Authenticates user by checking the supplied credentials against the
    /// authentication store that this manager represents.
    /// If credential are invalid then UserKind.Invalid is returned.
    /// This method may populate user access rights completely or partially - depending on store implementation.
    /// If rights are computed on authentication than Authorize() just checks existing in-memory structure, otherwise
    ///  Authorize() may re-fetch permissions from store on every call or cache them for the specified interval in memory
    /// </summary>
    /// <param name="credentials">User credentials.
    /// Particular manager implementation may elect to support multiple credential types, i.e.
    /// IdPassword, Twitter, Facebook, OAuth, LegacySystemA/B/C etc.
    /// </param>
    /// <returns>
    /// User object. Check User.Status for UserStatus.Invalid flag to see if authentication succeeded
    /// </returns>
    Task<User> AuthenticateAsync(Credentials credentials);


    /// <summary>
    /// Authenticates user by checking the supplied token against the
    /// authentication store that this manager represents.
    /// If token is invalid then UserKind.Invalid is returned.
    /// This method may populate user access rights completely or partially - depending on store implementation.
    /// If rights are computed on authentication than Authorize() just checks existing in-memory structure, otherwise
    ///  Authorize() may re-fetch permissions from store on every call or cache them for the specified interval in memory
    /// </summary>
    /// <param name="token">User authentication token </param>
    /// <returns>
    /// User object. Check User.Status for UserStatus.Invalid flag to see if authentication succeeded
    /// </returns>
    User Authenticate(SysAuthToken token);

    /// <summary>
    /// Authenticates user by checking the supplied token against the
    /// authentication store that this manager represents.
    /// If token is invalid then UserKind.Invalid is returned.
    /// This method may populate user access rights completely or partially - depending on store implementation.
    /// If rights are computed on authentication than Authorize() just checks existing in-memory structure, otherwise
    ///  Authorize() may re-fetch permissions from store on every call or cache them for the specified interval in memory
    /// </summary>
    /// <param name="token">User authentication token </param>
    /// <returns>
    /// User object. Check User.Status for UserStatus.Invalid flag to see if authentication succeeded
    /// </returns>
    Task<User> AuthenticateAsync(SysAuthToken token);


    /// <summary>
    /// Authenticates user by checking the supplied user's token against the
    /// authentication store that this manager represents.
    /// This method is called by the framework after User object was deserialized and it's Rights need to be re-fetched.
    /// If token is invalid then UserStatus.Invalid is set.
    /// This method may populate user access rights completely or partially - depending on store implementation.
    /// If rights are computed on authentication than Authorize() just checks existing in-memory structure, otherwise
    ///  Authorize() may re-fetch permissions from store on every call or cache them for the specified interval in memory
    /// </summary>
    /// <param name="user">User object which is checked and updated</param>
    void Authenticate(User user);

    /// <summary>
    /// Authenticates user by checking the supplied user's token against the
    /// authentication store that this manager represents.
    /// This method is called by the framework after User object was deserialized and it's Rights need to be re-fetched.
    /// If token is invalid then UserStatus.Invalid is set.
    /// This method may populate user access rights completely or partially - depending on store implementation.
    /// If rights are computed on authentication than Authorize() just checks existing in-memory structure, otherwise
    ///  Authorize() may re-fetch permissions from store on every call or cache them for the specified interval in memory
    /// </summary>
    /// <param name="user">User object which is checked and updated</param>
    Task AuthenticateAsync(User user);

    /// <summary>
    /// Authorizes user by finding appropriate access level to permission by supplied path.
    /// Depending on particular implementation, rights may be fully or partially cached in memory.
    /// Note: this authorization call returns AccessLevel object that may contain a complex data structure.
    /// The final assertion of user's ability to perform a certain action is encapsulated in Permission.Check() method.
    /// Call Permission.AuthorizeAndGuardAction(MemberInfo, ISession) to guard classes and methods from unauthorized access
    /// </summary>
    /// <param name="user">A user to perform authorization for</param>
    /// <param name="permission">An instance of permission to get</param>
    /// <returns>AccessLevel granted to the specified permission</returns>
    AccessLevel Authorize(User user, Permission permission);

    /// <summary>
    /// Authorizes user by finding appropriate access level to permission by supplied path.
    /// Depending on particular implementation, rights may be fully or partially cached in memory.
    /// Note: this authorization call returns AccessLevel object that may contain a complex data structure.
    /// The final assertion of user's ability to perform a certain action is encapsulated in Permission.Check() method.
    /// Call Permission.AuthorizeAndGuardAction(MemberInfo, ISession) to guard classes and methods from unauthorized access
    /// </summary>
    /// <param name="user">A user to perform authorization for</param>
    /// <param name="permission">An instance of permission to get</param>
    /// <returns>AccessLevel granted to the specified permission</returns>
    Task<AccessLevel> AuthorizeAsync(User user, Permission permission);

    /// <summary>
    /// Performs a lookup of security-addressable subject/resource/entity, such as a user/group/room/circle/client app etc...
    /// The format of the URI is up to the provider. Warning: this method should never ever been used for authentication, rather
    /// it is to lookup existing entity. Be careful not to disclose publicly too much information which may be private to this entity
    /// </summary>
    /// <param name="uri">Implementation-specific identifier of the identity to look up, e.g. `user://mike12345`, `app://facebookwall`</param>
    /// <returns>Entity information, or null if such entity is not found</returns>
    /// <remarks>
    /// Not all security manager implement this functionality. Typically it is being implemented by complex
    /// security facades that support social/public users, and OAuth client applications
    /// </remarks>
    Task<IEntityInfo> LookupEntityAsync(string uri);

    /// <summary>
    /// Extracts values for archive dimensions to store the log message for the specified user descriptor.
    /// Depending on the system descriptor represents an entity that describes user (e.g. User, UserInfo, etc.).
    /// The method only fills the fields specific to user identity
    /// </summary>
    string GetUserLogArchiveDimensions(IIdentityDescriptor identity);

    /// <summary>
    /// Logs security-related message via security channel
    /// </summary>
    /// <param name="action">Action that was performed</param>
    /// <param name="msg">A message to log</param>
    /// <param name="identity">If msg.ArchiveDim is not set, sets to  GetUserLogArchiveDimensions(user | currentCallContext)</param>
    void LogSecurityMessage(SecurityLogAction action, Message msg, IIdentityDescriptor identity = null);
  }


  /// <summary>
  /// Represents an implementation of an entity that performs user authentication based on passed credentials and other security-related global tasks
  /// </summary>
  public interface ISecurityManagerImplementation : ISecurityManager, IDisposable, IConfigurable
  {
    /// <summary>
    /// Defines what security events should be sec-channel logged by the system
    /// </summary>
    SecurityLogMask SecurityLogMask{ get; set;}

    /// <summary>
    /// Defines the level above which the security channel messages are logged
    /// </summary>
    MessageType SecurityLogLevel{ get; set;}
  }
}
