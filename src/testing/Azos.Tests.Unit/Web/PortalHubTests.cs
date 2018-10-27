/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using Azos.Scripting;
using System;
using System.Collections.Generic;

using Azos.Conf;
using Azos.Data;
using Azos.Apps;
using Azos.Wave;


namespace Azos.Tests.Unit.Web
{
  [Runnable(TRUN.BASE, 6)]
  public class PortalHubTests
  {
    private const string CONF1=@"
app{
    starters
    {
       starter
       {
         name='PortalHub'
         type='Azos.Wave.PortalHub, Azos.Wave'

         content-file-system
         {
            type='Azos.IO.FileSystem.Local.LocalFileSystem, NFX'
            connect-params{}
            root-path=$'c:\'
         }

              portal
              {
                name='Paris' type='Azos.Tests.Unit.Web.MockPortalFrench, Azos.Tests.Unit' 
                primary-root-uri='http://paris.for.me'
                default=true

                theme{ name='Eiffel'  default=true  type='Azos.Tests.Unit.Web.EuroTheme, Azos.Tests.Unit' resource-path='paris'}
              }
              portal
              {
                name='Berlin' type='Azos.Tests.Unit.Web.MockPortalGerman, Azos.Tests.Unit' 
                primary-root-uri='http://berlin.for.me'

                theme{ name='Merkel'  default=true  type='Azos.Tests.Unit.Web.EuroTheme, Azos.Tests.Unit' resource-path='ausgang'}
              }
       }//PortalHub
    }

}//app
";


    [Run]
    public void Test1()
    {
      using(var app = new ServiceBaseApplication(null, CONF1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
      {
          Aver.IsNotNull(PortalHub.Instance);

          var paris = PortalHub.Instance.Portals["PARIS"];
          Aver.IsNotNull(paris);

          var berlin = PortalHub.Instance.Portals["BERLIN"];
          Aver.IsNotNull(berlin);

          var onlineDefault = PortalHub.Instance.DefaultOnline;
          Aver.IsNotNull(onlineDefault);
          Aver.IsTrue( onlineDefault.Name.EqualsOrdIgnoreCase("PARIS"));

          Aver.IsTrue(paris is MockPortalFrench);
          Aver.IsTrue(berlin is MockPortalGerman);

          Aver.AreEqual(CoreConsts.ISO_LANG_FRENCH, paris.DefaultLanguageISOCode);
          Aver.AreEqual(CoreConsts.ISO_LANG_GERMAN, berlin.DefaultLanguageISOCode);

          Aver.IsNotNull(paris.DefaultTheme);
          Aver.AreEqual("Eiffel", paris.DefaultTheme.Name);

          Aver.IsNotNull(berlin.DefaultTheme);
          Aver.AreEqual("Merkel", berlin.DefaultTheme.Name);
      }
    }

  }


  public class MockPortalFrench : Portal
  {

    protected MockPortalFrench(IConfigSectionNode conf) : base(conf){}

    public override string DefaultISOCountry { get { return CoreConsts.ISO_COUNTRY_USA; } }

    public override string DefaultLanguageISOCode { get{ return CoreConsts.ISO_LANG_FRENCH;}}

    public override string DefauISOCurrency { get{ return CoreConsts.ISO_CURRENCY_EUR;}}

    public override string CountryISOCodeToLanguageISOCode(string countryISOCode)
    {
      return CoreConsts.ISO_LANG_FRENCH;
    }

    public override string AmountToString(Azos.Financial.Amount amount, Portal.MoneyFormat format = MoneyFormat.WithCurrencySymbol, ISession session = null)
    {
      return amount.Value.ToString();
    }

    public override string DateTimeToString(DateTime dt, Portal.DateTimeFormat format = DateTimeFormat.LongDateTime, ISession session = null)
    {
      return dt.ToString();
    }

    protected override Dictionary<string, string> GetLocalizableContent()
    {
      return new Dictionary<string,string>
      {
        {"Hello", "Bonjour"}
      };
    }
  }

  public class MockPortalGerman : Portal
  {

    protected MockPortalGerman(IConfigSectionNode conf) : base(conf){}

    public override string DefaultISOCountry { get { return CoreConsts.ISO_COUNTRY_USA; } }

    public override string DefaultLanguageISOCode { get{ return CoreConsts.ISO_LANG_GERMAN;}}

    public override string DefauISOCurrency { get{ return CoreConsts.ISO_CURRENCY_EUR;}}

    public override string CountryISOCodeToLanguageISOCode(string countryISOCode)
    {
      return CoreConsts.ISO_LANG_GERMAN;
    }

    public override string AmountToString(Azos.Financial.Amount amount, Portal.MoneyFormat format = MoneyFormat.WithCurrencySymbol, ISession session = null)
    {
      return amount.Value.ToString();
    }

    public override string DateTimeToString(DateTime dt, Portal.DateTimeFormat format = DateTimeFormat.LongDateTime, ISession session = null)
    {
      return dt.ToString();
    }

    protected override Dictionary<string, string> GetLocalizableContent()
    {
      return new Dictionary<string,string>
      {
        {"Hello", "Hello"}
      };
    }
  }


  public class EuroTheme : Theme<Portal>
  {
    protected EuroTheme(Portal portal, IConfigSectionNode conf) : base(portal, conf){ }
  }
}
