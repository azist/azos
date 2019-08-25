using System;

using Azos.Data;

namespace Azos.IAM.Data
{
  public sealed class Account : EntityWithRights
  {

    [Field(description: "Canonical name, such as PAN/User ID or EMail")]
    public string CName {  get; set; }

    /// <summary>
    /// Account Name/Title. For human users this is set to FirstName+LastName
    /// </summary>
    public string Title {  get; set; }

    /// <summary>
    /// Human, Process, Robot, Org, System
    /// </summary>
    public string Type { get; set; }

  }



  public sealed class Login : EntityWithRights
  {
    public const string TYPE_SCREENNAME = "scrn";
    public const string TYPE_EMAIL = "eml";
    public const string TYPE_PHONE = "tel";
    public const string TYPE_OAUTH = "oauth";
    public const string TYPE_BEARER = "bear";

    public string Type {  get; set; }
    public string Provider { get; set; }//FBK, Twitter, Instagram, Or your own system if it issues OAuth token
    public GDID Account{ get; set;}

    public string ID{ get; set;}
    public string Password {  get; set;}


    public DateTime? StartDate{ get; set;}//start of validity
    public DateTime? EndDate { get; set; }//end of validity


  }

}
