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
using Azos.Security;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.AuthKit
{
  /// <summary>
  /// Provides persisted model for user login data
  /// </summary>
  [Bix("ac75cd1f-75d4-471b-879d-17235c1241e5")]
  [Schema(Description = "Provides persisted model for user login data")]
  [UniqueSequence(Constraints.ID_NS_AUTHKIT, Constraints.ID_SEQ_USER)]
  public sealed class LoginEntity : EntityBase<IIdpUserCoreLogic, ChangeResult>
  {
    public override EntityId Id => new EntityId(Constraints.SYS_AUTHKIT,
                                                Constraints.ETP_LOGIN,
                                                Constraints.SCH_GDID, Gdid.ToString());
    /// <summary>
    /// User login realm set only on insert. Must be null/not supplied for update
    /// </summary>
    [Field(required: true, Description = "User login realm set only on insert. Must be null/not supplied for update")]
    public Atom? Realm { get; set; }

    /// <summary>
    /// User account Gdid
    /// </summary>
    [Field(required: true, Description = "User login Gdid")]
    public GDID UserId { get; set; }

    /// <summary>
    /// Login access level demotion (level down)
    /// </summary>
    [Field(Description = "Login access level demotion (level down)")]
    public UserStatus? LevelDemotion { get; set; }

    /// <summary>
    /// Login ID, or provider key
    /// </summary>
    [Field(required: true, maxLength: Constraints.LOGIN_ID_MAX_LEN, Description = "Login ID, or provider key")]
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
    [Field(maxLength: Constraints.LOGIN_PWD_MAX_LEN, Description = "Password vector, or NULL for providers who dont need it")]
    public string Password { get; set; }

    /// <summary>
    /// Optional extra provider -specific JSON vector
    /// </summary>
    [Field(maxLength:Constraints.PROVIDER_DATA_MAX_LEN, Description = "Optional extra provider -specific JSON vector")]
    public string ProviderData { get; set; }

    /// <summary>
    /// When login privilege takes effect
    /// </summary>
    [Field(required: true, Description = "When login privilege takes effect")]
    public DateRange ValidSpanUtc { get; set; }

    /// <summary>
    /// Properties such as tree connections (e.g. roles) and claims
    /// </summary>
    [Field(minLength: Constraints.PROPS_MIN_LEN,
           maxLength: Constraints.PROPS_MAX_LEN,
           Description = "Properties such as tree connections (e.g. roles) and claims")]
    public ConfigVector Props { get; set; }

    /// <summary>
    /// Login-specific Rights override or null for default rights
    /// </summary>
    [Field(minLength: Constraints.RIGHTS_MIN_LEN,
           maxLength: Constraints.RIGHTS_MAX_LEN,
           Description = "Login-specific Rights override or null for default rights")]
    public ConfigVector Rights { get; set; }

    protected override async Task<ValidState> DoAfterValidateOnSaveAsync(ValidState state)
    {
      var result = await base.DoAfterValidateOnSaveAsync(state).ConfigureAwait(false);
      if (!result.ShouldContinue) return result;

      state = await m_SaveLogic.ValidateLoginAsync(this, state).ConfigureAwait(false);

      if (FormMode == FormMode.Update)
      {
        if (Realm.HasValue && !Realm.Value.IsZero)
        {
          return new ValidState(state, new FieldValidationException(this, nameof(Realm),
             "`Realm` field value may not be provided for entity UPDATES as it is immutable. " +
             "If you are trying to re-use the same `UserEtity` instance for an update, " +
             "set its `Realm` field value to null first to signify the intent"));
        }
      }

      return result;
    }

    protected override async Task<ChangeResult> SaveBody(IIdpUserCoreLogic logic)
     => await logic.SaveLoginAsync(this).ConfigureAwait(false);
  }
}
