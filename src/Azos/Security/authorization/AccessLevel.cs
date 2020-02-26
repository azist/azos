/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using Azos.Conf;
using Azos.Data;

namespace Azos.Security
{
  /// <summary>
  /// A level of access granted to user for certain permission, i.e. if (level.Denied).....
  /// </summary>
  public struct AccessLevel
  {
    #region CONSTS
    /// <summary> Access denied level: bearer has no access/zero </summary>
    public const int DENIED = 0;

    /// <summary> View-only level: bearer has read-only access, so data can be viewed but can not be modified</summary>
    public const int VIEW = 1;

    /// <summary> Change level: bearer can view and change (add, edit) data but not delete it</summary>
    public const int VIEW_CHANGE = 2;

    /// <summary> Full CRUD level: bearer can view, add, update, and delete data</summary>
    public const int VIEW_CHANGE_DELETE = 3;

    /// <summary>
    /// Bearers have above the full control. In the scope of system administration permissions only:  this is typically used
    /// to protect irrevocable actions, direct data access, ability to launch arbitrary processes and other activities that
    /// might destabilize the system
    /// </summary>
    public const int ADVANCED = 1000;

    public const string CONFIG_LEVEL_ATTR = "level";

    public static readonly IConfigSectionNode DENIED_CONF = "p{level=0}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
    #endregion

    #region .ctor
    public static AccessLevel DeniedFor(User user, Permission permission)
     => new AccessLevel(user, permission, DENIED_CONF);

    public AccessLevel(User user, Permission permission, IConfigSectionNode data)
    {
      m_User = user.NonNull(nameof(user));
      m_Permission = permission.NonNull(nameof(permission));
      m_Data = data.NonNull(nameof(data));
    }
    #endregion

    #region Fields
    private User m_User;
    private Permission m_Permission;
    private IConfigSectionNode m_Data;
    #endregion

    #region Properties

    /// <summary>
    /// True if struct is assigned a value
    /// </summary>
    public bool IsAssigned => m_User!=null;

    /// <summary>
    /// Returns user that this access level is for
    /// </summary>
    public User User => m_User ?? User.Fake;

    /// <summary>
    /// Returns permission that this access level is for
    /// </summary>
    public Permission Permission => m_Permission;

    /// <summary>
    /// Returns security data for this level
    /// </summary>
    public IConfigSectionNode Data => m_Data ?? Rights.None.Root;

    /// <summary>
    /// Returns security level attribute from Data
    /// </summary>
    public int Level => m_Data.AttrByName(CONFIG_LEVEL_ATTR).ValueAsInt(DENIED);

    /// <summary>
    /// Indicates whether access is denied
    /// </summary>
    public bool Denied =>  Level == DENIED;
    #endregion
  }//access level

}
