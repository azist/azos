/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Web.Social
{
  /// <summary>
  /// Defines constants and helper methods that facilitate Facebook functionality
  /// </summary>
  public class LinkedIn : SocialNetwork
  {
    #region CONSTS
      //do not localize

      public const string LINKEDIN_PUB_SERVICE_URL = "https://www.linkedin.com";

      private const string LOGIN_LINK_TEMPLATE = "https://www.linkedin.com/uas/oauth2/authorization?response_type=code&client_id={0}&scope=r_basicprofile+r_emailaddress+rw_nus&state={1}&redirect_uri={2}";

      private const string ACCESSTOKEN_BASEURL = "https://www.linkedin.com/uas/oauth2/accessToken";
        //?grant_type=authorization_code
        //                                 &code=AUTHORIZATION_CODE
        //                                 &redirect_uri=YOUR_REDIRECT_URI
        //                                 &client_id=YOUR_API_KEY
        //                                 &client_secret=YOUR_SECRET_KEY";

      private const string ACCESSTOKEN_GRANTTYPE_PARAMNAME = "grant_type";
      private const string ACCESSTOKEN_GRANTTYPE_PARAMVALUE = "authorization_code";
      private const string ACCESSTOKEN_CODE_PARAMNAME = "code";
      private const string ACCESSTOKEN_CLIENTID_PARAMNAME = "client_id";
      private const string ACCESSTOKEN_CLIENTSECRET_PARAMNAME = "client_secret";
      private const string ACCESSTOKEN_REDIRECTURL_PARAMNAME = "redirect_uri";
      private const string ACCESSTOKEN_PARAMNAME = "access_token";

      private const string USERINFO_PERSON_PARAMNAME = "person";
      private const string USERINFO_ID_PARAMNAME = "id";
      private const string USERINFO_EMAIL_PARAMNAME = "email-address";
      private const string USERINFO_FIRSTNAME_PARAMNAME = "first-name";
      private const string USERINFO_LASTNAME_PARAMNAME = "last-name";

      private static readonly string GETUSERINFO_BASEURL = "https://api.linkedin.com/v1/people/~:({0})"
        .Args(String.Join(",", USERINFO_ID_PARAMNAME, USERINFO_FIRSTNAME_PARAMNAME, USERINFO_LASTNAME_PARAMNAME, USERINFO_EMAIL_PARAMNAME));
      private const string OAUTH2ACCESSTOKEN_PARAMNAME = "oauth2_access_token";

      private const string SHARE_BASEURL = "https://api.linkedin.com/v1/people/~/shares";

      private const string SHAREBODY_TEMPLATE = @"<share>
<comment>{0}</comment>
<content>
  <title>LinkedIn Developers Documentation On Using the Share API</title>
  <description>Leverage the Share API to maximize engagement on user-generated content on LinkedIn</description>
  <submitted-url>https://developer.linkedin.com/documents/share-api</submitted-url>
  <submitted-image-url>http://m3.licdn.com/media/p/3/000/124/1a6/089a29a.png</submitted-image-url>
</content>
<visibility>
  <code>anyone</code>
</visibility>
</share>";

    #endregion

    #region Static


    public LinkedIn(IApplication app) : base(app) { }
    public LinkedIn(IApplicationComponent director) : base(director) { }

    #endregion

    #region Public

    public override SocialUserInfo CreateSocialUserInfo(SocialUserInfoToken? existingToken = null)
      {
        return new LinkedInSocialUserInfo(this);
      }

      public override string GetExternalLoginReference(string returnURL)
      {
        return LOGIN_LINK_TEMPLATE.Args(ApiKey, GenerateNonce(), PrepareReturnURLParameter(returnURL));
      }

    #endregion

    #region Properties

      /// <summary>
      /// Globally uniquelly identifies social network architype
      /// </summary>
      public sealed override SocialNetID ID { get { return SocialNetID.LIN; } }

      [Config]
      public string ApiKey { get; set; }

      [Config]
      public string SecretKey { get; set; }

      /// <summary>
      /// Returns service description
      /// </summary>
      public override string Description { get { return "Linked In"; } }

      /// <summary>
      /// Specifies how service takes user credentials
      /// </summary>
      public override CredentialsEntryMethod CredentialsEntry { get { return CredentialsEntryMethod.Browser; } }

      /// <summary>
      /// Returns the root public URL for the service
      /// </summary>
      public override string ServiceURL { get { return LINKEDIN_PUB_SERVICE_URL; } }

      public override bool CanPost { get { return true; } }

    #endregion

    #region Protected

      protected override void DoObtainTokens(SocialUserInfo userInfo, JSONDataMap request, string returnPageURL)
      {
        var code = request[ACCESSTOKEN_CODE_PARAMNAME].AsString();

        if (code.IsNullOrWhiteSpace())
          throw new SocialException( StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetUserInfo(request should contain code)");

        var liUserInfo = userInfo as LinkedInSocialUserInfo;

        string returnURL = PrepareReturnURLParameter(returnPageURL);

        liUserInfo.AccessToken = getAccessToken( code, returnURL);
      }

      protected override void DoRetrieveLongTermTokens(SocialUserInfo userInfo) {}

      protected override void DoRetrieveUserInfo(SocialUserInfo userInfo)
      {
        var liUserInfo = userInfo as LinkedInSocialUserInfo;
        getUserInfo(liUserInfo);
      }

      protected override void DoPostMessage(string text, SocialUserInfo userInfo)
      {
        if (userInfo.LoginState != SocialLoginState.LoggedIn)
          return;

        LinkedInSocialUserInfo liUserInfo = userInfo as LinkedInSocialUserInfo;

        share(liUserInfo.AccessToken, liUserInfo, text);
      }

    #endregion

    #region .pvt. impl.

      private string getAccessToken(string code, string redirectURI)
      {
        dynamic responseObj = WebClient.GetJsonAsDynamic(ACCESSTOKEN_BASEURL, new WebClient.RequestParams(this)
        {
          Method = HTTPRequestMethod.GET,
          QueryParameters = new Dictionary<string, string>() {
            {ACCESSTOKEN_GRANTTYPE_PARAMNAME, ACCESSTOKEN_GRANTTYPE_PARAMVALUE},
            {ACCESSTOKEN_CODE_PARAMNAME, code},
            {ACCESSTOKEN_REDIRECTURL_PARAMNAME, redirectURI},
            {ACCESSTOKEN_CLIENTID_PARAMNAME, ApiKey},
            {ACCESSTOKEN_CLIENTSECRET_PARAMNAME, SecretKey}
          }
        });

        return responseObj[ACCESSTOKEN_PARAMNAME];
      }

      private void getUserInfo(LinkedInSocialUserInfo liUserInfo)
      {
        var responseDoc = WebClient.GetXML(GETUSERINFO_BASEURL, new WebClient.RequestParams(this)
        {
          Method = HTTPRequestMethod.GET,
          Headers = new Dictionary<string, string>() {
            {OAUTH2ACCESSTOKEN_PARAMNAME, liUserInfo.AccessToken}
          }
        });

        XElement person = responseDoc.Element(USERINFO_PERSON_PARAMNAME);
        XElement id = person.Element(USERINFO_ID_PARAMNAME);
        XElement email = person.Element(USERINFO_EMAIL_PARAMNAME);
        XElement firstName = person.Element(USERINFO_FIRSTNAME_PARAMNAME);
        XElement lastName = person.Element(USERINFO_LASTNAME_PARAMNAME);

        liUserInfo.LoginState = SocialLoginState.LoggedIn;
        liUserInfo.ID = id.Value;
        liUserInfo.Email = email.Value;
        liUserInfo.FirstName = firstName.Value;
        liUserInfo.LastName = lastName.Value;
      }

      private void share(string accessToken, LinkedInSocialUserInfo liUserInfo, string text)
      {
        string body = SHAREBODY_TEMPLATE.Args(text);

        var responseDoc = WebClient.GetXML(SHARE_BASEURL, new WebClient.RequestParams(this)
        {
          Headers = new Dictionary<string, string>() {{OAUTH2ACCESSTOKEN_PARAMNAME, accessToken}},
          Body = body
        });
      }

    #endregion
  }

  [Serializable]
  public class LinkedInSocialUserInfo: SocialUserInfo
  {
     public LinkedInSocialUserInfo(LinkedIn issuer) : base(issuer) {}

    /// <summary>
    /// Token to perform Twitter operations like post
    /// </summary>
    public string AccessToken { get; internal set; }

    ///// <summary>
    ///// LinkedIn first name field
    ///// </summary>
    //public string FirstName { get; internal set; }

    ///// <summary>
    ///// LinkedIn last name field
    ///// </summary>
    //public string LastName { get; internal set; }

    public override string DisplayName { get { return "{0}, {1}".Args(LastName, FirstName);}}

    public override string LongTermProviderToken
    {
      get { return AccessToken; }
      internal set
      {
        AccessToken = value;
        base.LongTermProviderToken = value;
      }
    }
  }

}
