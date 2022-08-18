/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Sky.Messaging
{
  /// <summary>
  /// Message priorities: Urgent, Normal etc.
  /// </summary>
  public enum MsgPriority { Urgent = 0, Normal=1, BelowNormal=2, Slowest = BelowNormal}

  /// <summary>
  /// Message importance: Normal, Important, MostImportant etc.
  /// </summary>
  public enum MsgImportance{ Unimportant=-100, LessImportant=-10, Normal=0, Important=10, MoreImportant=100, MostImportant=1000}

  /// <summary>
  /// Denotes message chjannels - e.g EMail, SMS etc.
  /// </summary>
  [Flags]
  public enum MsgChannels
  {
    Unspecified = 0x00000000,
    All         = -1,


    EMail   = 1 << 0,
    SMS     = 1 << 1,
    MMS     = 1 << 2,
    Social  = 1 << 3,
    Call    = 1 << 4,
    Fax     = 1 << 5,
    Letter  = 1 << 6,
    Chat    = 1 << 7,

    /// <summary>
    /// System Users - the ones used in internal system auth scheme, such as admins, SYS (e.g. used in Glue authentication)
    /// </summary>
    SysUser = 1 << 8,

    /// <summary>
    /// Public Users - customers, the ones who can log in from external world
    /// </summary>
    PubUser = 1 << 9
  }

  /// <summary>
  /// Defines how a message send error should be handled: Ignored, Thrown etc.
  /// </summary>
  public enum SendMessageErrorHandling { Throw=0, Ignore }

}
