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
  public sealed class AccessLevel
  {
    #region CONSTS

      public const int DENIED = 0;
      public const int VIEW = 1;
      public const int VIEW_CHANGE = 2;
      public const int VIEW_CHANGE_DELETE = 3;

      public const string CONFIG_LEVEL_ATTR = "level";

      public static readonly IConfigSectionNode DENIED_CONF = "p{level=0}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

    #endregion

    #region .ctor

      public static AccessLevel DeniedFor(User user, Permission permission)
      {
        return new AccessLevel(user, permission, DENIED_CONF);
      }

      public AccessLevel(User user, Permission permission, IConfigSectionNode data)
      {
          m_User = user;
          m_Permission = permission;
          m_Data = data;
      }

    #endregion


    #region Fields

      private User m_User;
      private Permission m_Permission;
      private IConfigSectionNode m_Data;

    #endregion


    #region Properties

      /// <summary>
      /// Returns user that this access level is for
      /// </summary>
      public User User
      {
          get { return m_User;}
      }

      /// <summary>
      /// Returns permission that this access level is for
      /// </summary>
      public Permission Permission
      {
          get { return m_Permission;}
      }

      /// <summary>
      /// Returns security data for this level
      /// </summary>
      public IConfigSectionNode Data
      {
          get { return m_Data ?? Rights.None.Root;}
      }

      /// <summary>
      /// Returns security level attribute from Data
      /// </summary>
      public int Level
      {
          get { return m_Data.AttrByName(CONFIG_LEVEL_ATTR).ValueAsInt(DENIED);}
      }

      /// <summary>
      /// Indicates whether access is denied
      /// </summary>
      public bool Denied
      {
        get { return Level == DENIED; }
      }

    #endregion

  }//access level

}
