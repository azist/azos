using System;

using Azos.Data;
using Azos.IAM.Protocol;

namespace Azos.IAM.Server.Data
{
  /// <summary>
  /// Represents an account. Account represent users, processes, organizations and other entities.
  /// </summary>
  public sealed class Account : EntityWithRights
  {
    /// <summary>
    /// Group assignment. All accounts belong to a specific group
    /// </summary>
    [Field(required: true,
           description: "Points to group which this account belongs to",
           metadata: "idx{name='grp' dir=asc}")]
    [Field(typeof(Account), nameof(G_Group), TMONGO, backendName: "g_grp")]
    public GDID G_Group { get; set; }


    /// <summary>
    /// Account Name/Title. For human users this is set to FirstName+LastName
    /// </summary>
    [Field(required: true,
           maxLength: Sizes.ACCOUNT_TITLE_MAX,
           description: "Account Name/Title. For human users this is set to FirstName+LastName")]
    [Field(typeof(Account), nameof(Title), TMONGO, backendName: "title")]
    public string Title {  get; set; }

    /// <summary>
    /// Human, Process, Robot, Org, System
    /// </summary>
    [Field(required: true, valueList: ValueLists.ACCOUNT_TYPE_VALUE_LIST, description: "Account type: Human/Group etc.")]
    [Field(typeof(Account), nameof(Type), TMONGO, backendName: "tp")]
    public char? Type { get; set; }

    /// <summary>
    /// Access level Archetype: Invalid,User,Admin,System
    /// </summary>
    [Field(required: true, valueList: ValueLists.ACCOUNT_LEVEL_VALUE_LIST, description: "Access level Archetype: Invalid,User,Admin,System")]
    [Field(typeof(Account), nameof(Level), TMONGO, backendName: "lvl")]
    public char? Level { get; set; }


    [Field(required: false, description: "Points to policy affecting this account; if null then effective group policy is assumed")]
    [Field(typeof(Account), nameof(G_Policy), TMONGO, backendName: "g_pol")]
    public GDID G_Policy { get; set; }
  }

}
