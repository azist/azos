
using Azos.Data;

namespace Azos.IAM.Protocol
{

  public sealed class AccountForm : ChangeForm<GroupEntityBody>{ }

  public sealed class AccountEntityBody : EntityBodyWithRights
  {
    /// <summary>
    /// Group assignment. All accounts belong to a specific group
    /// </summary>
    [Field(required: true, description: "Points to group which this account belongs to")]
    public GDID G_Group { get; set; }


    /// <summary>
    /// Account Name/Title. For human users this is set to FirstName+LastName
    /// </summary>
    [Field(required: true, maxLength: Sizes.ACCOUNT_TITLE_MAX,
           description: "Account Name/Title. For human users this is set to FirstName+LastName")]
    public string Title { get; set; }

    /// <summary>
    /// Human, Process, Robot, Org, System
    /// </summary>
    [Field(required: true, valueList: ValueLists.ACCOUNT_TYPE_VALUE_LIST, description: "Account type: Human/Group etc.")]
    public char? Type { get; set; }

    /// <summary>
    /// Access level Archetype: Invalid,User,Admin,System
    /// </summary>
    [Field(required: true, valueList: ValueLists.ACCOUNT_LEVEL_VALUE_LIST, description: "Access level Archetype: Invalid,User,Admin,System")]
    public char? Level { get; set; }


    [Field(required: false, description: "Points to policy affecting this account; if null then effective group policy is assumed")]
    public GDID G_Policy { get; set; }
  }


}
