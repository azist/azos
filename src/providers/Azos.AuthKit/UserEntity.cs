/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.Bix;

namespace Azos.AuthKit
{
  /// <summary>
  /// Provides persisted model for user account data
  /// </summary>
  [Bix("ac75cd1f-75d4-471b-879d-17235c1241e5")]
  [Schema(Description = "Provides persisted model for user account data")]
  [UniqueSequence(Constraints.ID_NS_AUTHKIT, Constraints.ID_SEQ_USER)]
  public sealed class UserEntity : PersistedEntity<IIdpUserCoreLogic, ChangeResult>
  {
    public override EntityId Id => throw new NotImplementedException();


    protected override async Task<ValidState> DoAfterValidateOnSaveAsync(ValidState state)
    {
      var result = await base.DoAfterValidateOnSaveAsync(state).ConfigureAwait(false);
      if (result.ShouldContinue)
      {
        state = await m_SaveLogic.ValidateUserAsync(this, state).ConfigureAwait(false);
      }
      return result;
    }

    protected override async Task<ChangeResult> SaveBody(IIdpUserCoreLogic logic)
     => await logic.SaveUserAsync(this).ConfigureAwait(false);
  }
}
