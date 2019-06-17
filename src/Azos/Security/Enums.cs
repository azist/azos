/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace Azos.Security
{
  /// <summary>
  /// Denotes types of identities: Users, Processes, Groups etc...
  /// </summary>
  public enum IdentityType
  {
    Other= 0,
    /// <summary>Identity of a particular system User</summary>
    User,
    /// <summary>Identity of a user group</summary>
    Group,
    /// <summary>Identity of a system component such as a Process</summary>
    Process,
    /// <summary>Identity of business entity such as a company, supplier, bank etc.</summary>
    Business
  }

  /// <summary>
  /// User status enumeration -  super-permission levels
  /// </summary>
  public enum UserStatus
  {
    /// <summary>
    /// Invalid user, not authenticated and not authorized
    /// </summary>
    Invalid = 0,

    /// <summary>
    /// The lowest level of a user, bound by permissions inside their domain and domain section (such as company/organization/facility)
    /// </summary>
    User = 1,
    Usr = User,

    /// <summary>
    /// Administrators may run administration console, but always bound by their domain
    /// </summary>
    Administrator = 1000,

    Admin = Administrator,
    Adm = Administrator,

    /// <summary>
    /// Cross domain user, all restrictions are lifted, all permission checks pass
    /// </summary>
    System = 1000000,
    Sys = System,
  }


  /// <summary>
  /// Defines what actions should be logged by the system
  /// </summary>
  [Flags]
  public enum SecurityLogMask
  {
    Off = 0,

    Custom               = 1 << 0,
    Authentication       = 1 << 1,

    Authorization        = 1 << 2,

    Gate                 = 1 << 3,
    Login                = 1 << 4,
    Logout               = 1 << 5,
    LoginChange          = 1 << 6,

    UserCreate           = 1 << 7,
    UserDestroy          = 1 << 8,
    UserSuspend          = 1 << 9,
    UserResume           = 1 << 10,

    All = -1
  }

  /// <summary>
  /// Describes types of actions relating to application security
  /// </summary>
  public enum SecurityLogAction
  {
    Custom = 0,
    Authentication,
    Authorization,
    Gate,
    Login,
    Logout,
    LoginChange,
    UserCreate,
    UserDestroy,
    UserSuspend,
    UserResume
  }

  public static class EnumUtils
  {
    public static SecurityLogMask ToMask(this SecurityLogAction action)
    {
      switch (action)
      {
        case SecurityLogAction.Custom:         return SecurityLogMask.Custom;
        case SecurityLogAction.Authentication: return SecurityLogMask.Authentication;
        case SecurityLogAction.Authorization:  return SecurityLogMask.Authorization;
        case SecurityLogAction.Gate:           return SecurityLogMask.Gate;
        case SecurityLogAction.Login:          return SecurityLogMask.Login;
        case SecurityLogAction.Logout:         return SecurityLogMask.Logout;
        case SecurityLogAction.LoginChange:    return SecurityLogMask.LoginChange;
        case SecurityLogAction.UserCreate:     return SecurityLogMask.UserCreate;
        case SecurityLogAction.UserDestroy:    return SecurityLogMask.UserDestroy;
        case SecurityLogAction.UserSuspend:    return SecurityLogMask.UserSuspend;
        case SecurityLogAction.UserResume:     return SecurityLogMask.UserResume;
        default: return SecurityLogMask.Off;
      }
    }
  }
}