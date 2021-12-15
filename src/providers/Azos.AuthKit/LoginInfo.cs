/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using Azos.Data.Business;
using Azos.Security;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.AuthKit
{
  /// <summary>
  /// Describes user login
  /// </summary>
  [Bix("05B1A10E-9A0C-45F4-AA0B-BB673FF7CAED")]
  [Schema(Description = "Describes user login")]
  public sealed class LoginInfo : TransientModel
  {
    /// <summary>
    /// User login realm
    /// </summary>
    [Field(required: true, Description = "User login realm")]
    public Atom Realm { get; set; }

    /// <summary>
    /// User login Gdid
    /// </summary>
    [Field(required: true, Description = "User login Gdid")]
    public GDID Gdid { get; set; }

    /// <summary>
    /// User account Gdid
    /// </summary>
    [Field(required: true, Description = "User account Gdid")]
    public GDID UserId { get; set; }

    /// <summary>
    /// User access level
    /// </summary>
    [Field(Description = "Login access level")] // should this be same as User model - ddl statement be not null and required????
    public UserStatus? Level { get; set; }

    /// <summary>
    /// Login ID, or provider key
    /// </summary>
    [Field(required: true, Description = "Login ID, or provider key")]
    public string LoginId { get; set; }

    /// <summary>
    /// Login Type Atom
    /// </summary>
    [Field(required: true, Description = "Login Type Atom")]
    public Atom LoginType { get; set; }

    /// <summary>
    /// Login provider, e.g.  AZOS, FBK, TWT, AD, SYSTEM X etc.. or Atom.ZERO for not eternal provider
    /// </summary>
    [Field(required: true, Description = "Login provider, e.g.  AZOS, FBK, TWT, AD, SYSTEM X etc.. or Atom.ZERO for not eternal provider")]
    public Atom Provider { get; set; }

    /// <summary>
    /// Password vector, or NULL for providers who dont need it
    /// </summary>
    [Field(Description = "Password vector, or NULL for providers who dont need it")]
    public string Password { get; set; }

    /// <summary>
    /// Optional extra provider -specific JSON vector
    /// </summary>
    [Field(Description = "Optional extra provider -specific JSON vector")]
    public string ProviderData { get; set; }

    /// <summary>
    /// When login privilege takes effect
    /// </summary>
    [Field(required: true, Description = "When login privilege takes effect")]
    public DateRange ValidSpanUtc { get; set; }

    /// <summary>
    /// Properties such as tree connections (e.g. roles) and claims
    /// </summary>
    [Field(Description = "Properties such as tree connections (e.g. roles) and claims")]
    public ConfigVector Props { get; set; }

    /// <summary>
    /// Login-specific Rights override or null for default rights
    /// </summary>
    [Field(Description = "Login-specific Rights override or null for default rights")]
    public ConfigVector Rights { get; set; }

    /// <summary>
    /// Creation version (UTC, Actor, Origin)
    /// </summary>
    [Field(required: true, Description = "Creation version (UTC, Actor, Origin)")]
    public VersionInfo CreateVersion { get; set; }

    /// <summary>
    /// Version of this login data record
    /// </summary>
    [Field(required: true, Description = "Version of this data record")]
    public VersionInfo DataVersion { get; set; }

    /// <summary>
    /// Lock timestamp range, if set the login is inactive past that timestamp, until LOCK_END_UTC
    /// </summary>
    [Field(Description = "Lock timestamp range, if set the account is inactive past that timestamp, until LOCK_END_UTC")]
    public DateRange? LockSpanUtc { get; set; }

    /// <summary>
    /// Who locked the login
    /// </summary>
    [Field(Description = "Who locked the user account")]
    public EntityId? LockActor    { get; set; }

    /// <summary>
    /// Short note explaining lock reason/status
    /// </summary>
    [Field(Description = "Short note explaining lock reason/status")]
    public string    LockNote     { get; set; }
  }
}
