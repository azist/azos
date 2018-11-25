/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Web.Social
{
    /// <summary>
    /// Defines constants and helper methods that facilitate Facebook functionality
    /// </summary>
    public class Facebook : SocialNetwork
    {
      #region CONSTS
        //do not localize

        public const string DEFAULT_API_OAUTH_VERSION = "v2.12";
        public const string DEFAULT_API_GRAPH_VERSION = "v2.11";

        public const string FACEBOOK_PUB_SERVICE_URL = "https://www.facebook.com";

        private const string LOGIN_LINK_TEMPLATE = "https://www.facebook.com/{0}/dialog/oauth?client_id={1}&redirect_uri={2}&state={3}&response_type=code&scope={4}";

        private const string ACCESSTOKEN_BASEURL = "https://graph.facebook.com/{0}/oauth/access_token";
        private const string ACCESSTOKEN_CODE_PARAMNAME = "code";
        private const string ACCESSTOKEN_CLIENTID_PARAMNAME = "client_id";
        private const string ACCESSTOKEN_CLIENTSECRET_PARAMNAME = "client_secret";
        private const string ACCESSTOKEN_REDIRECTURL_PARAMNAME = "redirect_uri";
        private const string ACCESSTOKEN_PARAMNAME = "access_token";
        private const string FBEXCHANGETOKEN_PARAMNAME = "fb_exchange_token";
        private const string GRANTTYPE_PARAMNAME = "grant_type";
        private const string GRANTTYPE_FBEXCHANGETOKEN_PARAMVALUE = FBEXCHANGETOKEN_PARAMNAME;

        private const string GETUSERINFO_BASEURL = "https://graph.facebook.com/{0}/me";

        private const string GETUSERINFO_FIELDS_PARAMNAME = "fields";

        private const string USER_ID_PARAMNAME = "id";
        private const string USER_EMAIL_PARAMNAME = "email";
        private const string USER_NAME_PARAMNAME = "name"; // person's full name
        private const string USER_FIRSTNAME_PARAMNAME = "first_name";
        private const string USER_LASTNAME_PARAMNAME = "last_name";
        private const string USER_MIDDLENAME_PARAMNAME = "middle_name";
        private const string USER_GENDER_PARAMNAME = "gender";
          private const string USER_GENDER_MALE = "male";
          private const string USER_GENDER_FEMALE = "female";
        private const string USER_BIRTHDAY_PARAMNAME = "birthday"; // person's birthday in the format MM/DD/YYYY.
        private const string USER_LOCALE_PARAMNAME = "locale";
        private const string USER_TIMEZONE_PARAMNAME = "timezone"; // int, user's current timezone offset from UTC
        private const string USER_PICTURE_PARAMNAME = "picture";
          private const string USER_PICTURE_DATA_PARAMNAME = "data";
          private const string USER_PICTURE_URL_PARAMNAME = "url";

        private const string PUBLISH_BASEURL_PATTERN = "https://graph.facebook.com/{0}/{1}/feed";
        private const string GET_USER_PICTURE_URL_PATTERN = "https://graph.facebook.com/{0}/{1}/picture?type=large&redirect=false";

        private const string MESSAGE_PARAMNAME = "message";

        private static readonly string FILL_USER_INFO_FIELDS = string.Join(",", USER_ID_PARAMNAME, USER_EMAIL_PARAMNAME, USER_NAME_PARAMNAME, USER_FIRSTNAME_PARAMNAME,
            USER_LASTNAME_PARAMNAME, USER_MIDDLENAME_PARAMNAME, USER_GENDER_PARAMNAME, USER_BIRTHDAY_PARAMNAME,
            USER_LOCALE_PARAMNAME, USER_TIMEZONE_PARAMNAME, USER_PICTURE_PARAMNAME);

      #endregion

      #region Static

        public static string EncodeState(string query)
        {
          var result = Convert.ToBase64String(Encoding.UTF8.GetBytes(query ?? string.Empty));
          result.TrimEnd('=').Replace('/', '_').Replace('+', '-');
          return result;
        }

        public static JSONDataMap DecodeState(string state)
        {
          try
          {
            var str = state.Replace('_', '/').Replace('-', '+');
            switch(state.Length % 4)
            {
              case 2: str += "=="; break;
              case 3: str += "="; break;
            }
            var url = Encoding.UTF8.GetString(Convert.FromBase64String(str));
            return JSONDataMap.FromURLEncodedString(url);
          }
          catch (Exception)
          {
            return new JSONDataMap();
          }
        }

    #endregion

      public Facebook(IApplication app) : base(app) { }
      public Facebook(IApplicationComponent director) : base(director) { }

      #region Fields

      private string m_APIOAuthVersion = DEFAULT_API_OAUTH_VERSION;
      private string m_APIGraphVersion = DEFAULT_API_GRAPH_VERSION;

      #endregion

      #region Properties

        [Config]
        public string APIOAuthVersion
        {
          get { return m_APIOAuthVersion ?? DEFAULT_API_OAUTH_VERSION; }
          set { m_APIOAuthVersion = value; }
        }

        [Config]
        public string APIGraphVersion
        {
          get { return m_APIGraphVersion ?? DEFAULT_API_GRAPH_VERSION; }
          set { m_APIGraphVersion = value; }
        }

        /// <summary>
        /// Globally uniquelly identifies social network architype
        /// </summary>
        public sealed override SocialNetID ID { get { return SocialNetID.FBK; } }

        [Config] public string AppID { get; set; }

        [Config] public string ClientSecret { get; set; }

      #endregion

      #region Protected

        public override SocialUserInfo CreateSocialUserInfo(SocialUserInfoToken? existingToken = null)
        {
          return new FacebookSocialUserInfo(this);
        }

        public override string GetExternalLoginReference(string returnURL)
        {
          var sb = new StringBuilder();
          if (GrantViewEmail)     sb.Append("email+");
          if (GrantPost)          sb.Append("publish_actions+");
          if (GrantAccessProfile) sb.Append("public_profile+");
          if (GrantAccessFriends) sb.Append("user_friends+");

          returnURL = PrepareReturnURLParameter(returnURL, false);
          var uri = new Uri(returnURL);
          var redirectURL = Uri.EscapeDataString(uri.GetLeftPart(UriPartial.Path));
          var state = EncodeState(uri.Query.Substring(1));

          return LOGIN_LINK_TEMPLATE.Args(APIOAuthVersion, AppID, redirectURL, state, sb.Length > 0 ? sb.ToString(0, sb.Length - 1) : string.Empty);
        }

        protected override void DoObtainTokens(SocialUserInfo userInfo, JSONDataMap request, string returnPageURL)
        {
          var code = request[ACCESSTOKEN_CODE_PARAMNAME].AsString();

          if (code.IsNullOrWhiteSpace())
            throw new SocialException( StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetUserInfo(request should contain code)");

          FacebookSocialUserInfo fbUserInfo = userInfo as FacebookSocialUserInfo;

          var uri = new Uri(returnPageURL);
          fbUserInfo.AccessToken = getAccessToken(code, Uri.EscapeDataString(uri.GetLeftPart(UriPartial.Path)));
        }

        protected override void DoRetrieveLongTermTokens(SocialUserInfo userInfo)
        {
          var fbUserInfo = userInfo as FacebookSocialUserInfo;
          fbUserInfo.LongTermAccessToken = getLongTermAccessToken(fbUserInfo.AccessToken);
        }

        protected override void DoRetrieveUserInfo(SocialUserInfo userInfo)
        {
          var fbUserInfo = userInfo as FacebookSocialUserInfo;
          fillUserInfo(fbUserInfo);
        }

        protected override void DoPostMessage(string text, SocialUserInfo userInfo)
        {
          if (userInfo.LoginState != SocialLoginState.LoggedIn)
            return;

          FacebookSocialUserInfo fbUserInfo = userInfo as FacebookSocialUserInfo;

          publish( fbUserInfo.ID, fbUserInfo.AccessToken, text);
        }

        /// <summary>
        /// Returns service description
        /// </summary>
        public override string Description { get { return "Facebook";} }

        /// <summary>
        /// Specifies how service takes user credentials
        /// </summary>
        public override CredentialsEntryMethod CredentialsEntry {  get { return CredentialsEntryMethod.Browser; } }

        /// <summary>
        /// Returns the root public URL for the service
        /// </summary>
        public override string ServiceURL { get { return FACEBOOK_PUB_SERVICE_URL; } }

        public override bool CanPost { get { return true;} }

      #endregion

      #region .pvt. impl.

        private string getAccessToken(string code, string redirectURI)
        {
          var response = WebClient.GetJson(ACCESSTOKEN_BASEURL.Args(APIGraphVersion), new WebClient.RequestParams(this)
          {
            Method = HTTPRequestMethod.GET,
            QueryParameters = new Dictionary<string, string>() {
              {ACCESSTOKEN_CLIENTID_PARAMNAME, AppID},
              {ACCESSTOKEN_REDIRECTURL_PARAMNAME, redirectURI},
              {ACCESSTOKEN_CLIENTSECRET_PARAMNAME, ClientSecret},
              {ACCESSTOKEN_CODE_PARAMNAME, code}
            }
          });

          return response[ACCESSTOKEN_PARAMNAME].AsString();
        }

        private void fillUserInfo(FacebookSocialUserInfo userInfo)
        {
          var responseObj = WebClient.GetJson(GETUSERINFO_BASEURL.Args(APIGraphVersion), new WebClient.RequestParams(this)
          {
            Method = HTTPRequestMethod.GET,
            QueryParameters = new Dictionary<string, string>() {
              {ACCESSTOKEN_PARAMNAME, userInfo.AccessToken},
              {GETUSERINFO_FIELDS_PARAMNAME, FILL_USER_INFO_FIELDS}
            }
          });

          userInfo.LoginState = SocialLoginState.LoggedIn;
          userInfo.ID = responseObj[USER_ID_PARAMNAME].AsString();
          userInfo.UserName = responseObj[USER_NAME_PARAMNAME].AsString();
          userInfo.Email = responseObj[USER_EMAIL_PARAMNAME].AsString();
          userInfo.FirstName = responseObj[USER_FIRSTNAME_PARAMNAME].AsString();
          userInfo.LastName = responseObj[USER_LASTNAME_PARAMNAME].AsString();
          userInfo.MiddleName = responseObj[USER_MIDDLENAME_PARAMNAME].AsString();

          var genderStr = responseObj[USER_GENDER_PARAMNAME].AsString();
          if (genderStr == USER_GENDER_MALE)
            userInfo.Gender = Gender.MALE;
          else if (genderStr == USER_GENDER_FEMALE)
            userInfo.Gender = Gender.FEMALE;

          var birthDateStr = responseObj[USER_BIRTHDAY_PARAMNAME].AsString();
          DateTime birthDate;
          if (DateTime.TryParseExact(birthDateStr, "MM/DD/YYYY",
            System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out birthDate))
              userInfo.BirthDate = birthDate;

          userInfo.Locale = responseObj[USER_LOCALE_PARAMNAME].AsString();
          userInfo.TimezoneOffset = ((int)responseObj[USER_TIMEZONE_PARAMNAME]) * 60 * 60;

          var picObj = WebClient.GetJson(GET_USER_PICTURE_URL_PATTERN.Args(APIGraphVersion, userInfo.ID), new WebClient.RequestParams(this)
          {
            Method = HTTPRequestMethod.GET,
            QueryParameters = new Dictionary<string, string>() {
              {ACCESSTOKEN_PARAMNAME, userInfo.AccessToken}
            }
          });
          var imgInfo = picObj[USER_PICTURE_DATA_PARAMNAME] as JSONDataMap;
          if (imgInfo != null)
            userInfo.PictureLink = imgInfo[USER_PICTURE_URL_PARAMNAME].AsString();
        }

        private string getLongTermAccessToken(string accessToken)
        {
          var response = WebClient.GetJson(ACCESSTOKEN_BASEURL.Args(APIGraphVersion), new WebClient.RequestParams(this)
          {
            QueryParameters = new Dictionary<string, string>() {
              {GRANTTYPE_PARAMNAME, GRANTTYPE_FBEXCHANGETOKEN_PARAMVALUE},
              {ACCESSTOKEN_CLIENTID_PARAMNAME, AppID},
              {ACCESSTOKEN_CLIENTSECRET_PARAMNAME, ClientSecret},
              {FBEXCHANGETOKEN_PARAMNAME, accessToken}
            }
          });

          return response[ACCESSTOKEN_PARAMNAME].AsString();
        }

        // you can post a new wall post on current user's wall by issuing a POST request to https://graph.facebook.com/[USER_ID]/feed:
        private void publish(string userId, string accessToken, string message)
        {
          string url = PUBLISH_BASEURL_PATTERN.Args(APIGraphVersion, userId);

          dynamic responseObj = WebClient.GetJsonAsDynamic(url, new WebClient.RequestParams(this)
          {
            Method = HTTPRequestMethod.POST,
            QueryParameters = new Dictionary<string, string>() {
              {MESSAGE_PARAMNAME, message},
              {ACCESSTOKEN_PARAMNAME, accessToken}
            }
          });
        }

      #endregion

    }




  [Serializable]
  public class FacebookSocialUserInfo: SocialUserInfo
  {
    public FacebookSocialUserInfo(Facebook issuer, SocialUserInfoToken? existingToken = null) : base(issuer) { }

    /// <summary>
    /// Facebook user name field
    /// </summary>
    public string UserName { get; internal set; }

    /// <summary>
    /// Token to perform Facebook operations like post (CAN expire in 20 minutes)
    /// </summary>
    public string AccessToken { get; internal set; }

    /// <summary>
    /// Token to perform Facebook operations like post (expires in two months)
    /// </summary>
    public string LongTermAccessToken
    {
      get;
      internal set;
    }

    public override string LongTermProviderToken
    {
      get { return LongTermAccessToken; }
      internal set
      {
        LongTermAccessToken = value;
        base.LongTermProviderToken = value;
      }
    }

    public override string DisplayName { get { return "{0}".Args(UserName);}}

    public override string DebugInfo { get { return "{0}, {1}, {2}".Args(ID, AccessToken, LongTermAccessToken);} }

    #region Object overrides

      public override bool Equals(object obj)
      {
        return base.Equals(obj) && UserName == ((FacebookSocialUserInfo)obj).UserName;
      }

      public override int GetHashCode()
      {
        return UserName.GetHashCode();
      }

    #endregion
  }


}
