/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.Business;
using Azos.Security;
using Azos.Serialization.Bix;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azos.AuthKit
{
  /// <summary>
  /// Describes filter form object for user account search
  /// </summary>
  [Bix("a392a84b-9979-4972-b136-34497a724922")]
  [Schema(Description = "Describes filter form object for user account search")]
  public sealed class UserListFilter : FilterModel<IEnumerable<UserInfo>>
  {
    /// <summary>
    /// Realm is an implicit ambient context which drives security checks,
    /// therefore here we provide a convenience accessor only.
    /// </summary>
    public Atom Realm => Ambient.CurrentCallSession.GetAtomDataContextName();

    [Field(description: @"User GDID (immutable primary key)")]
    public GDID Gdid { get; set; }

    [Field(description: @"User Guid (secondary immutable primary key)")]
    public Guid?  Guid { get; set; }

    [Field(description: @"User login id")]
    public string LoginId { get; set; }

    [Field(description: @"User status level: Invalid|User|Admin|System")]
    public UserStatus? Level { get; set; }

    [Field(description: @"User name query. Use a '*' for matching")]
    public string Name { get; set; }

    [Field(description: @"User organization unit")]
    public string OrgUnit { get; set; }

    [Field(description: @"As of what point in time to assess status. If null, then current Utc is assumed")]
    public DateTime? AsOfUtc {  get; set; }

    [Field(description: @"Null returns both active and inactive, false returns inactive only, true returns active-only accounts as of the Utc")]
    public bool? Active { get; set; }

    [Field(description: @"Null returns both locked and unlocked, false returns unlocked only, true returns locked-only accounts as of the Utc")]
    public bool? Locked { get; set; }

    [Field(description: "Optional tag filter expression tree which is overlaid on top of other filters supplied")]
    public Data.AST.Expression TagFilter { get; set; }

    [Field(description: "When true, keeps config vector as-is as text, e.g. in Laconic format, otherwise (default) converts them to JSON")]
    public bool KeepConfigVectorsAsIs{ get; set; }

    [Inject] IIdpUserCoreLogic m_Logic;

    protected async override Task<SaveResult<IEnumerable<UserInfo>>> DoSaveAsync()
      => new SaveResult<IEnumerable<UserInfo>>(await m_Logic.GetUserListAsync(this).ConfigureAwait(false));
  }
}
