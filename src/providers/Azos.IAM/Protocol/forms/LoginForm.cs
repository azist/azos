
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.IAM.Protocol
{

  public sealed class LoginForm : ChangeForm<LoginEntityBody>{ }

  public sealed class LoginEntityBody : EntityBodyWithRights
  {
    [Field(required: true,
           description: "Points to account which this login is for")]
    public GDID G_Account { get; set; }


    /// <summary>
    /// Access level Archetype which may only narrow-down the account level: Invalid,User,Admin,System
    /// </summary>
    [Field(required: false, valueList: ValueLists.ACCOUNT_LEVEL_VALUE_LIST, description: "Login access level archetype which can only restrict the Account level")]
    public char? Level { get; set; }

    [Field(required: true,
           description: "Login type: email, screenname etc.")]
    public string Type { get; set; }

    [Field(required: false,
           maxLength: Sizes.NAME_MAX,
           description: "Populated for external providers, such as Twitter/FaceBook etc. used for identity federation")]
    public string ExternalProvider { get; set; }//FBK, Twitter, Instagram, Or your own system if it issues OAuth token

    [Field(required: false,
           description: "Stores data specific to external provider, such as Social Net User ID, User Name, Link to primary image, Long Token, Refresh Token etc.")]
    public JsonDataMap ExternalProviderData { get; set; }

    [Field(required: true,
           minLength: Sizes.LOGIN_ID_MIN,
           maxLength: Sizes.LOGIN_ID_MAX,
           description: "Unique account ID/Token body (e.g. Bearer). For external providers stores its user ID suffixed with provider type, e.g. `3109119011@twitter`")]
    public string ID { get; set; }


    //{"alg":"KDF","fam":"Text","h":"fTGpaEmHmVnWLz-IYrMBFR81Rq-9axLUPNZ1O5wTH3E","s":"hNtB2MTobIf-ijutHsPfPcioGH_DaILoOxjE_aCejnLZXs"}
    [Field(required: true,
           minLength: Sizes.LOGIN_PWD_MIN,
           maxLength: Sizes.LOGIN_PWD_MAX,
           description: "Password hash object. For external providers stores empty object or provider-specific options")]
    public string Password { get; set; }

  }


}
