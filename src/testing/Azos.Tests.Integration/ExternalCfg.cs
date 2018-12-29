/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Web;
using Azos.Conf;
using Azos.Platform;
using SVN_CONN_PARAMS = Azos.IO.FileSystem.SVN.SVNFileSystemSessionConnectParams;

namespace Azos.Tests.Integration
{
  public class ExternalCfg
  {
    #region LACONF

      protected string LACONF = typeof(ExternalCfg).GetText("ExternalCfg.laconf");

    #endregion

    public const string NFX_SVN = "NFX_SVN";

    protected string SVN_ROOT;
    protected string SVN_UNAME;
    protected string SVN_UPSW;

    #region Tax

      protected const string NFX_TAX = "NFX_TAX";

      protected const string NFX_TAX_CALCULATOR_TAXJAR = "TaxJar";

      protected const string NFX_TAX_DFLT_SESS_PRMS = "default-session-connect-params";

    #endregion

    protected const string NFX_STRIPE = "NFX_STRIPE";
    protected const string NFX_PAYPAL = "NFX_PAYPAL";

    protected string STRIPE_SECRET_KEY;
    protected string STRIPE_PUBLISHABLE_KEY;

    protected string PAYPAL_SERVER_URL;
    protected string PAYPAL_EMAIL;
    protected string PAYPAL_CLIENT_ID;
    protected string PAYPAL_CLIENT_SECRET;

    public ExternalCfg()
    {
            //initSocial();
            //initSVNConsts();
            //initS3V4Consts();
            //initGoogleDriveConsts();
            //initStripeConsts();
    }


    private void initSVNConsts()
    {
      try
      {
        string envVarsStr = System.Environment.GetEnvironmentVariable(NFX_SVN);

        var cfg = Configuration.ProviderLoadFromString(envVarsStr, Configuration.CONFIG_LACONIC_FORMAT).Root;

        SVN_ROOT = cfg.AttrByName(SVN_CONN_PARAMS.CONFIG_SERVERURL_ATTR).Value;
        SVN_UNAME = cfg.AttrByName(SVN_CONN_PARAMS.CONFIG_UNAME_ATTR).Value;
        SVN_UPSW = cfg.AttrByName(SVN_CONN_PARAMS.CONFIG_UPWD_ATTR).Value;

        LACONF = LACONF.Replace("[SVN_SERVER_URL]", SVN_ROOT)
          .Replace("[SVN_USER_NAME]", SVN_UNAME)
          .Replace("[SVN_USER_PASSWORD]", SVN_UPSW);
      }
      catch (Exception ex)
      {
        throw new Exception( string.Format(
            "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added",
              NFX_SVN,
              "svn{ server-url='https://somehost.com/svn/XXX' user-name='XXXX' user-password='XXXXXXXXXXXX' }"),
          ex);
      }
    }


  }
}
