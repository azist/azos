/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Business;
using Azos.Security;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.AuthKit
{
  /// <summary>
  /// Describes user account
  /// </summary>
  [Bix("3485e189-106e-449f-b34e-892a74a01ec7")]
  [Schema(Description = "Describes user account")]
  public sealed class UserInfo : TransientModel
  {
    /// <summary>
    /// User account realm
    /// </summary>
    [Field(required: true, Description = "User account realm")]
    public Atom Realm { get; set; }

    /// <summary>
    /// User account Gdid
    /// </summary>
    [Field(required: true, Description = "User account Gdid")]
    public GDID Gdid { get; set; }

    /// <summary>
    /// User account Gdid
    /// </summary>
    [Field(required: true, Description = "User account Guid")]
    public Guid Guid { get; set; }

    /// <summary>
    /// Name/Title of user account
    /// </summary>
    [Field(required: true, Description = "Name/Title of user account")]
    public string Name { get; set; }

    /// <summary>
    /// User access level
    /// </summary>
    [Field(required: true, Description = "User access level")]
    public UserStatus Level { get; set; }

    /// <summary>
    /// User description
    /// </summary>
    [Field(required: true, Description = "User description")]
    public string Description { get; set; }

    /// <summary>
    /// When user privilege takes effect
    /// </summary>
    [Field(required: true, Description = "When user privilege takes effect")]
    public DateRange ValidSpanUtc { get; set; }

    /// <summary>
    /// Tree path for org unit. So the user list may be searched by it
    /// </summary>
    [Field(required: false, Description = "Tree path for org unit. So the user list may be searched by it")]
    public EntityId? OrgUnit { get; set; }

    /// <summary>
    /// Properties such as tree connections (e.g. roles) and claims
    /// </summary>
    [Field(required: true, Description = "Properties such as tree connections (e.g. roles) and claims")]
    public ConfigVector Props { get; set; }

    /// <summary>
    /// User-specific Rights override or null for default rights
    /// </summary>
    [Field(Description = "User-specific Rights override or null for default rights")]
    public ConfigVector Rights { get; set; }

    /// <summary>
    /// Free form text notes associated with the account
    /// </summary>
    [Field(Description = "Free form text notes associated with the account")]
    public string Note { get; set; }

    /// <summary>
    /// Creation version (UTC, Actor, Origin)
    /// </summary>
    [Field(required: true, Description = "Creation version (UTC, Actor, Origin)")]
    public VersionInfo CreateVersion { get; set; }

    /// <summary>
    /// Version of this data record
    /// </summary>
    [Field(required: true, Description = "Version of this data record")]
    public VersionInfo DataVersion { get; set; }

    /// <summary>
    /// Lock timestamp range, if set the account is inactive past that timestamp, until LOCK_END_UTC
    /// </summary>
    [Field(Description = "Lock timestamp range, if set the account is inactive past that timestamp, until LOCK_END_UTC")]
    public DateRange? LockSpanUtc { get; set; }

    /// <summary>
    /// Who locked the user account
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
