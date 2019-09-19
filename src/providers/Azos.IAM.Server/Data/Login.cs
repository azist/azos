using System;

using Azos.Data;
using Azos.IAM.Protocol;
using Azos.Serialization.JSON;

namespace Azos.IAM.Server.Data
{
  /// <summary>
  /// Account Login
  /// </summary>
  public sealed class Login : EntityWithRights
  {
    [Field(required: true,
           description: "Points to account which this login is for",
           metadata: "idx{name='account' dir=asc}")]
    [Field(typeof(Login), nameof(G_Account), TMONGO, backendName: "g_a")]
    public GDID G_Account{ get; set;}


    /// <summary>
    /// Access level Archetype which may only narrow-down the account level: Invalid,User,Admin,System
    /// </summary>
    [Field(required: false, valueList: ValueLists.ACCOUNT_LEVEL_VALUE_LIST, description: "Login access level archetype which can only restrict the Account level")]
    [Field(typeof(Login), nameof(Level), TMONGO, backendName: "lvl")]
    public char? Level { get; set; }

    [Field(required: true,
           description: "Login type: email, screenname etc.",
           metadata: "idx{name='type' dir=asc}")]
    [Field(typeof(Login), nameof(Type), TMONGO, backendName: "tp")]
    public string Type {  get; set; }

    [Field(required: false,
           maxLength: Sizes.NAME_MAX,
           description: "Populated for external providers, such as Twitter/FaceBook etc. used for identity federation")]
    [Field(typeof(Login), nameof(ExternalProvider), TMONGO, backendName: "extp")]
    public string ExternalProvider { get; set; }//FBK, Twitter, Instagram, Or your own system if it issues OAuth token

    [Field(required: false,
           description: "Stores data specific to external provider, such as Social Net User ID, User Name, Link to primary image, Long Token, Refresh Token etc.")]
    [Field(typeof(Login), nameof(ExternalProviderData), TMONGO, backendName: "extdata")]
    public JsonDataMap ExternalProviderData {  get; set;}

    [Field(required: true,
           minLength: Sizes.LOGIN_ID_MIN,
           maxLength: Sizes.LOGIN_ID_MAX,
           description: "Unique account ID/Token body (e.g. Bearer). For external providers stores its user ID suffixed with provider type, e.g. `3109119011@twitter`",
           metadata: "idx{name='id' unique=true dir=asc}")]
    [Field(typeof(Login), nameof(ID), TMONGO, backendName: "id")]
    public string ID{ get; set; }


    //{"alg":"KDF","fam":"Text","h":"fTGpaEmHmVnWLz-IYrMBFR81Rq-9axLUPNZ1O5wTH3E","s":"hNtB2MTobIf-ijutHsPfPcioGH_DaILoOxjE_aCejnLZXs"}
    [Field(required: true,
           minLength: Sizes.LOGIN_PWD_MIN,
           maxLength: Sizes.LOGIN_PWD_MAX,
           description: "Password hash object. For external providers stores empty object or provider-specific options")]
    [Field(typeof(Login), nameof(Password), TMONGO, backendName: "pwd")]
    public string Password {  get; set; }



    [Field(required: true,
           maxLength: Sizes.LOGIN_PWD_HISTORY_COUNT_MAX,
           description: "History of older Password hashes. The policy governs how many are kept")]
    [Field(typeof(Login), nameof(PasswordHistory), TMONGO, backendName: "pwdh")]
    public string[] PasswordHistory {  get; set; }


    [Field(required: false,
           description: "Utc timestamp when the password was changed for the last time")]
    [Field(typeof(Login), nameof(PasswordChangeDate), TMONGO, backendName: "pwddt")]
    public DateTime? PasswordChangeDate { get; set; }

    [Field(required: false,
           description: "Utc timestamp when account login was confirmed, or null")]
    [Field(typeof(Login), nameof(ConfirmDate), TMONGO, backendName: "cdt")]
    public DateTime? ConfirmDate {  get; set; }

    [Field(required: false,
           maxLength: Sizes.DESCRIPTION_MAX,
           description: "Brief description of confirmation method")]
    [Field(typeof(Login), nameof(ConfirmMethod), TMONGO, backendName: "cmtd")]
    public string ConfirmMethod { get; set; }


    public override Exception Validate(string targetName)
    {
      var ve = base.Validate(targetName);
      if (ve != null) return ve;

      if (ExternalProviderData == null) return null;

      foreach (var kvp in ExternalProviderData)
      {
        if (kvp.Key.Length > Sizes.EXT_PROVIDER_NAME_MAX)
          return new FieldValidationException(this, kvp.Key.TakeLastChars(32, ".."),
                            StringConsts.EXT_PROVIDER_NAME_LONG_ERROR.Args(Sizes.EXT_PROVIDER_NAME_MAX));

        if (kvp.Value != null && kvp.Value.AsString().Length > Sizes.EXT_PROVIDER_VALUE_MAX)
          return new FieldValidationException(this, kvp.Key.TakeLastChars(32, ".."),
                            StringConsts.EXT_PROVIDER_VALUE_LONG_ERROR.Args(Sizes.EXT_PROVIDER_VALUE_MAX));
      }

      return null;
    }
  }



  /// <summary>
  /// LoginStatus keeps Login volatile information to lessen the replication load.
  /// The GDID is the same as that of Login's record, consequently this is an extension entity, such that cardinality is
  /// Login:LoginStatus = 1:1
  /// </summary>
  public sealed class LoginStatus : Entity
  {
    [Field(required: true, description: "How many consecutive times an account login attempt was incorrect")]
    [Field(typeof(LoginStatus), nameof(IncorrectLoginCount), TMONGO, backendName: "icty")]
    public int? IncorrectLoginCount {  get; set; }

    [Field(required: true, description: "When the agent tried to perform incorrect account login for the last time")]
    [Field(typeof(LoginStatus), nameof(IncorrectLastLoginDate), TMONGO, backendName: "idt")]
    public DateTime? IncorrectLastLoginDate { get; set; }

    [Field(required: true,
           maxLength: Sizes.HOST_MAX,
           description: "Last network address from where the user tried to log in")]
    [Field(typeof(LoginStatus), nameof(IncorrectLastLoginAddr), TMONGO, backendName: "iadr")]
    public string IncorrectLastLoginAddr { get; set; }

    [Field(required: true,
           maxLength: Sizes.USER_AGENT_MAX,
           description: "User agent/device used for the last invalid log in")]
    [Field(typeof(LoginStatus), nameof(IncorrectLastLoginAgent), TMONGO, backendName: "iua")]
    public string IncorrectLastLoginAgent { get; set; }


    [Field(required: true, description: "How many consecutive times an account login attempt was successful")]
    [Field(typeof(LoginStatus), nameof(IncorrectLoginCount), TMONGO, backendName: "scty")]
    public int? LoginCount { get; set; }

    [Field(required: true, description: "Last successful login timestamp")]
    [Field(typeof(LoginStatus), nameof(LastLoginDate), TMONGO, backendName: "sdt")]
    public DateTime? LastLoginDate { get; set; }

    [Field(required: true,
           maxLength:  Sizes.HOST_MAX,
           description: "Last successful login timestamp")]
    [Field(typeof(LoginStatus), nameof(LastLoginAddr), TMONGO, backendName: "sadr")]
    public string LastLoginAddr { get; set; }

    [Field(required: true,
           maxLength: Sizes.USER_AGENT_MAX,
           description: "Last successful login User agent/device used for the last invalid log in")]
    [Field(typeof(LoginStatus), nameof(LastLoginAgent), TMONGO, backendName: "sua")]
    public string LastLoginAgent { get; set; }
  }

}
