using System;

using Azos.Data;

namespace Azos.IAM.Server.Data
{
/*
   Account LOCK-OUT for  X wrong log-in attempts
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

    /// <summary>
    /// Group assignment
    /// </summary>
    [Field(required: false,
           description: "Points to group which this account belongs to",
           metadata: "idx{name='grp' dir=asc}")]
    [Field(typeof(Account), nameof(Group), TMONGO, backendName: "grp")]
    public GDID Group{  get; set;}

    [Field(description: "Canonical name, such as PAN/User ID or EMail")]
    public string CName {  get; set; }


    /// <summary>
    /// Account Name/Title. For human users this is set to FirstName+LastName
    /// </summary>
    [Field(required: false, description: "Account Name/Title. For human users this is set to FirstName+LastName")]
    [Field(typeof(Account), nameof(Title), TMONGO, backendName: "ttl")]
    public string Title {  get; set; }

    /// <summary>
    /// Human, Process, Robot, Org, System
    /// </summary>
    [Field(required: false, valueList: ACCOUNT_TYPE_VALUE_LIST, description: "Account type")]
    [Field(typeof(Account), nameof(Type), TMONGO, backendName: "tp")]
    public char? Type { get; set; }
  }


  /// <summary>
  /// Account Login
  /// </summary>
  public sealed class Login : EntityWithRights
  {
    /// <summary> System id: GDID</summary>
    public const string TYPE_SID = "sid";

    /// <summary> Screenname e.g.: "alex1980"; screen names are used in systems that provide email e.g. "alex1980@myservice.com"</summary>
    public const string TYPE_SCREENNAME = "scrn";

    /// <summary> Email address e.g. "alex1980@myservice.com"</summary>
    public const string TYPE_EMAIL = "eml";

    /// <summary> Telephone number e.g. "8885552223413"</summary>
    public const string TYPE_PHONE = "tel";

    /// <summary> Login via OAuth/OpenID connect through 3rd party (e.g. Twitter account)</summary>
    public const string TYPE_OAUTH = "oauth";

    /// <summary> Bearer token issued by the system </summary>
    public const string TYPE_BEARER = "bear";


    [Field(required: false,
           description: "Points to group which this account belongs to",
           metadata: "idx{name='account' dir=asc}")]
    [Field(typeof(Login), nameof(Account), TMONGO, backendName: "acc")]
    public GDID Account{ get; set;}

    public string Type {  get; set; }
    public string Provider { get; set; }//FBK, Twitter, Instagram, Or your own system if it issues OAuth token


    [Field(required: true,
           description: "Unique account ID/Token body",
           metadata: "idx{name='id' unique=true dir=asc}")]
    [Field(typeof(Login), nameof(ID), TMONGO, backendName: "id")]
    public string ID{ get; set;}

    [Field(required: true,
           description: "Password hash")]
    [Field(typeof(Login), nameof(Password), TMONGO, backendName: "pwd")]
    public string Password {  get; set;}

    [Field(required: false,
           description: "Utc timestamp when account login was confirmed, or null")]
    [Field(typeof(Login), nameof(ConfirmDate), TMONGO, backendName: "cdt")]
    public DateTime? ConfirmDate {  get; set;}

  }

}
