/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Scripting;
using Azos.Web.Social;

namespace Azos.Tests.Unit.Social
{
  [Runnable(TRUN.BASE, 4)]
  public class SocialSerialization : IRunnableHook
  {
    [Run]
    public void SerializeDeserializeFB()
    {
      using(var app = new TestApplication(m_RootCfg))
      {
        var ui = new FacebookSocialUserInfo(Facebook.Instance);

        initSocialUserInfo(ui);

        ui.UserName = "Pupkin Vasya";

        var s = ui.SerializeToString();
        var ui1 = SocialUserInfo.DeserializeFromString<FacebookSocialUserInfo>(s);
        Aver.AreObjectsEqual(ui, ui1);
      }
    }

    [Run]
    public void SerializeDeserializeTWT()
    {
      using (var app = new TestApplication(m_RootCfg))
      {
        var ui = new TwitterSocialUserInfo(Twitter.Instance);

        initSocialUserInfo(ui);

        ui.UserName = "Pupkin Vasya";
        ui.OAuthRequestToken = "rMd67-J5c";
        ui.OAuthRequestTokenSecret = "brDM-00zeq6";
        ui.OAuthVerifier = "J78iOPz";

        var s = ui.SerializeToString();
        var ui1 = SocialUserInfo.DeserializeFromString<TwitterSocialUserInfo>(s);
        Aver.AreObjectsEqual(ui, ui1);
      }
    }

    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      m_RootCfg = initConf();
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error) { return false; }

    private void initSocialUserInfo(SocialUserInfo ui)
    {
      ui.Email = "pupkin@mail.com";
      ui.FirstName = "Vasya";
      ui.LastName = "Pupkin";
      ui.MiddleName = "S";
      ui.Gender = Gender.MALE;
      ui.BirthDate = new DateTime(1980, 01, 10);
      ui.Locale = "en-us";
      ui.TimezoneOffset = -120;
      ui.LongTermProviderToken = "aBX567-mm78Da/Yhj78-iik";
      ui.LoginState = SocialLoginState.LongTermTokenObtained;
      ui.LastError = new AzosException("ex-outer", new NullReferenceException());
    }

    #region LACONF

      private const string NFX_SOCIAL = "NFX_SOCIAL";

      private ConfigSectionNode initConf()
      {


//social
//{
//  provider {
//    name="Gooogle+"
//    type='Azos.Web.Social.GooglePlus, Azos.Web'
//    client-code='111111111111' client-secret='11111111111-111111111111'
//    web-service-call-timeout-ms='20000' keep-alive='false' pipelined='false'}
//  provider {
//    name="Facebook"
//    type='Azos.Web.Social.Facebook, Azos.Web'
//    client-code='1111111111111111' client-secret='11111111111111111111111111111111' app-accesstoken='1111111111111111|111111111111111111111111111'}
//  provider {
//    name="Twitter"
//    type='Azos.Web.Social.Twitter, Azos.Web' client-code='1111111111111111111111' client-secret='111111111111111111111111111111111111111111'}
//  provider {
//    name="VKontakte"
//    type='Azos.Web.Social.VKontakte, Azos.Web' client-code='1111111' client-secret='11111111111111111111'}
//  provider {
//    name="LinkedIn"
//    type='Azos.Web.Social.LinkedIn, Azos.Web' api-key='11111111111111' secret-key='1111111111111111'}
//}

        string envVarStr;
        try
        {
          envVarStr = System.Environment.GetEnvironmentVariable(NFX_SOCIAL);
        }
        catch (Exception ex)
        {
          throw new Exception(
            "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added".Args(
              NFX_SOCIAL,
              "social { provider {type='Azos.Web.Social.GooglePlus, Azos.Web' client-code='111111111111' client-secret='agh222ppppp-1pppppppPppp' web-service-call-timeout-ms='20000' keep-alive='false' pipelined='false' } }"),
          ex);
        }

        var laconfStr = "nfx {{ web-settings {{ {0} }} }}".Args(envVarStr);
        var cfg = Configuration.ProviderLoadFromString(laconfStr, Configuration.CONFIG_LACONIC_FORMAT);
        return cfg.Root;
      }

      private ConfigSectionNode m_RootCfg;

    #endregion
  }
}
