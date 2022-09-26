/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.AuthKit
{
  /// <summary>
  /// Sets/resets lock-related field on account or login target
  /// </summary>
  [Bix("a49fd780-7415-442d-ae0b-07a00030aa38")]
  [Schema(Description = "Sets/resets lock-related field on account or login target")]
  public sealed class LockStatus : PersistedModel<ChangeResult>
  {
    /// <summary>
    /// Target entity, such as a UserAccount or Login, to apply lock status to
    /// </summary>
    [Field(required: true, Description = "Target entity, such as a UserAccount or Login, to apply lock status to")]
    public EntityId TargetEntity { get; set; }

    /// <summary>
    /// Requires a target entity address of GDID
    /// </summary>
    public GDID TargetEntityGdid => Constraints.AsValidLockEntityId(TargetEntity).HasRequiredValue();

    /// <summary>
    /// Lock timestamp range, if set the login is inactive past that timestamp, until LOCK_END_UTC if null then there is no lock on the entity
    /// </summary>
    [Field(Description = "Lock timestamp range, if set the account is inactive past that timestamp, until LOCK_END_UTC, if null then there is no lock on the entity")]
    public DateRange? LockSpanUtc { get; set; }

    /// <summary>
    /// Who locked the user account or login
    /// </summary>
    [Field(Description = "Who locked the user account or login")]
    public EntityId? LockActor { get; set; }

    /// <summary>
    /// Short note explaining lock reason/status
    /// </summary>
    [Field(Description = "Short note explaining lock reason/status")]
    public string LockNote { get; set; }


    [Inject] IIdpUserCoreLogic m_Logic;

    /// <inheritdoc/>
    public override ValidState Validate(ValidState state, string scope = null)
    {
      state =  base.Validate(state, scope);
      if (state.ShouldStop) return state;

      var gtarget = Constraints.AsValidLockEntityId(TargetEntity);
      if (gtarget.IsZero)
      {
        state = new ValidState(state, new FieldValidationException(this, nameof(TargetEntity), "The field must contain valid auth kit lockable entity id"));
      }

      return state;
    }

    /// <inheritdoc/>
    protected async override Task<SaveResult<ChangeResult>> DoSaveAsync()
      => new SaveResult<ChangeResult>(await m_Logic.SetLockStatusAsync(this).ConfigureAwait(false));
  }
}
