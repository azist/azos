/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Security.Principal;

using Azos.Data;

namespace Azos.Security
{
  /// <summary>
  /// Provides base user functionality. Particular security manager implementations typically return users derived from this class
  /// </summary>
  [Serializable]
  public class User : IIdentityDescriptor, IIdentity, IPrincipal
  {
    #region Static
    /// <summary>
    /// Returns an instance of the fake user that has no rights
    /// </summary>
    public static User Fake => new User(BlankCredentials.Instance,
                                        new SysAuthToken(),
                                        UserStatus.Invalid,
                                          "John Doe",
                                          "Fake user",
                                          Rights.None,
                                          Ambient.UTCNow);
    #endregion

    #region .ctor
    private User() { } //for quicker serialization

    public User(Credentials credentials,
                SysAuthToken token,
                UserStatus status,
                string name,
                string descr,
                Rights rights,
                DateTime? utcNow = null,
                ConfigVector props = null)
    {
      m_Credentials = credentials;
      m_AuthenticationToken = token;
      m_Status = status;
      m_Name = name;
      m_Description = descr;
      m_Rights = rights;
      m_StatusTimeStampUTC = utcNow ?? Ambient.UTCNow;
      m_Props = props;
    }

    public User(Credentials credentials,
                SysAuthToken token,
                string name,
                Rights rights,
                DateTime? utcNow = null,
                ConfigVector props = null) : this(credentials, token, UserStatus.User, name, null, rights, utcNow, props)
    {
    }
    #endregion

    #region Fields

    private DateTime m_StatusTimeStampUTC;

    private Credentials m_Credentials;
    private SysAuthToken m_AuthenticationToken;

    private UserStatus m_Status;
    private string m_Name;

    private string m_Description;

    [NonSerialized]//Important, rights are NOT serializable
    private Rights m_Rights;

    [NonSerialized]//Important, props are NOT serializable
    private ConfigVector m_Props;

    #endregion

    #region Properties

    /// <summary>
    /// Captures timestamp when this user was set to current status (created/set rights)
    /// Security managers may elect to re-fetch user rights after some period
    /// </summary>
    public DateTime StatusTimeStampUTC => m_StatusTimeStampUTC;

    public Credentials Credentials => m_Credentials ?? BlankCredentials.Instance;

    /// <summary>
    /// System authentication token - this token is issued by the security manager and
    /// used "inside" the system perimeter - it should never be disclosed to the public/outside
    /// consuming parties, as external callers should use appropriate credentials (e.g. Bearer)
    /// in place of this token.
    /// </summary>
    public SysAuthToken AuthToken => m_AuthenticationToken;

    /// <summary>
    /// User name/id which uniquely identifies the user in the system
    /// </summary>
    public string Name => m_Name ?? string.Empty;

    public string Description => m_Description ?? string.Empty;

    public UserStatus Status => m_Status;

    /// <summary>
    /// Returns data bag that contains user rights. This is a framework-only internal property
    ///  which should not be used by application developers. This bag may get populated fully-or-partially
    ///   by ISecurityManager implementation. Use User[permission] indexer or Application.SecurityManager.Authorize()
    ///    to obtain AccessLevel
    /// </summary>
    public Rights Rights => m_Rights;

    /// <summary>
    /// Returns principal properties or NULL if none were set
    /// </summary>
    public ConfigVector Props => m_Props;

    #endregion

    #region Public

    /// <summary>
    /// Makes user invalid
    /// </summary>
    public void Invalidate() => m_Status = UserStatus.Invalid;

    /// <summary>
    /// Framework-internal. Do not call
    /// </summary>
    public void ___update_status(
                  UserStatus status,
                  string name,
                  string descr,
                  Rights rights,
                  DateTime utcNow)
    {
        m_Status = status;
        m_Name = name;
        m_Description = descr;
        m_Rights = rights;
        m_StatusTimeStampUTC = utcNow;
    }

    public override string ToString() => "[{0}]{1},{2}".Args(Status, Name, Description);

    #endregion

    #region IIdentity Members
    /// <summary>
    /// Implementation of IIdentity interface
    /// </summary>
    public string AuthenticationType => m_AuthenticationToken.Realm ?? string.Empty;

    /// <summary>
    /// Implementation of IIdentity interface. Checks to see if user is not in invalid status
    /// </summary>
    public bool IsAuthenticated  => m_Status > UserStatus.Invalid;
    #endregion

    #region IPrincipal Members

    /// <summary>
    /// Implementation of IPrincipal interface
    /// </summary>
    public IIdentity Identity => this;

    /// <summary>
    /// Determines whether the current principal belongs to the specified role.
    /// This method implements IPrincipal and has little application in Azos framework context
    /// as Azos permissions are more granular than just boolean. This method really checks user kind (User/Admin/Sys).
    /// Confusion comes from the fact that what Microsoft calls role really is just a single named permission -
    ///  a role is a named permission set in Azos.
    /// </summary>
    public bool IsInRole(string role) => m_Status == role.AsEnum(UserStatus.Invalid);

    #endregion

    #region IIdentityDescriptor
    public virtual IdentityType IdentityDescriptorType  => IdentityType.User;
    public virtual string IdentityDescriptorName => this.Name;
    public virtual object IdentityDescriptorID => AuthToken;
    #endregion

  }
}
