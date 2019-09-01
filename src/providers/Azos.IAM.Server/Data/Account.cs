using System;

using Azos.Data;

namespace Azos.IAM.Server.Data
{
/*
   Policies = settings - they get applied to Groups
   Password change every X days, last pwd change
   Account LOCK-OUT for  X wrong log-in attempts
   Number of log-in attempts
   2 factor authentication
   Should not be able to re-use LOGIN/EMAIL after it is inactivated
   Can not re-use X old passwords
   Password change schedule
   Password edit distance

   Permissions valid in a date/time span - maybe add this to root permission (`sd`,`ed` along with `level`) -
   or maybe this should be delegated to specific app
*/
  /// <summary>
  /// Represents an account. Account represent users, processes, organizations and other entities.
  /// </summary>
  public sealed class Account : EntityWithRights
  {
    public const string ACCOUNT_TYPE_VALUE_LIST = "H:Human,S:Service,G:Group,O:Organization,S:System";
    public const string ACCOUNT_LEVEL_VALUE_LIST = "I:Invalid,U:User,A:Admin,S:System";

    /// <summary>
    /// Group assignment. All accounts belong to a specific group
    /// </summary>
    [Field(required: true,
           description: "Points to group which this account belongs to",
           metadata: "idx{name='grp' dir=asc}")]
    [Field(typeof(Account), nameof(G_Group), TMONGO, backendName: "g_grp")]
    public GDID G_Group{  get; set;}


    /// <summary>
    /// Account Name/Title. For human users this is set to FirstName+LastName
    /// </summary>
    [Field(required: false, description: "Account Name/Title. For human users this is set to FirstName+LastName")]
    [Field(typeof(Account), nameof(Title), TMONGO, backendName: "ttl")]
    public string Title {  get; set; }

    /// <summary>
    /// Human, Process, Robot, Org, System
    /// </summary>
    [Field(required: true, valueList: ACCOUNT_TYPE_VALUE_LIST, description: "Account type")]
    [Field(typeof(Account), nameof(Type), TMONGO, backendName: "tp")]
    public char? Type { get; set; }

    /// <summary>
    /// Access level Archetype: Invalid,User,Admin,System
    /// </summary>
    [Field(required: true, valueList: ACCOUNT_LEVEL_VALUE_LIST, description: "Access level Archetype: Invalid,User,Admin,System")]
    [Field(typeof(Account), nameof(Level), TMONGO, backendName: "lvl")]
    public char? Level { get; set; }


    [Field(required: false, description: "Points to policy affecting this account; if null then effective group policy is assumed")]
    [Field(typeof(Account), nameof(G_Policy), TMONGO, backendName: "g_pol")]
    public GDID G_Policy { get; set; }
  }

}
