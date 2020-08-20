/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Security;

namespace Azos.Tests.Integration.Security
{
  [Runnable]
  public class MinIdpSqlTests  : IRunnableHook
  {

    private static string confR1 =
      @"
app
{
  security
  {
    type='Azos.Security.MinIdp.MinIdpSecurityManager, Azos'
    realm = '1r'
    store
    {
       type='Azos.Security.MinIdp.CacheLayer, Azos'
       //max-cache-age-sec=0
       store
       {
         type='Azos.Security.MinIdp.MinIdpSqlStore, Azos.MsSql'
         connect-string='Data Source=$(~App.HOST);Initial Catalog=MINIDP;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False'
       }
    }
  }
}
";
    private AzosApplication m_App;


    public void Prologue(Runner runner, FID id)
    {
      m_App = new AzosApplication(null, confR1.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
    }

    public bool Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_App);
      return false;
    }


    [Run]
    public void Authenticate_BadUserPassword()
    {
      var credentials = new IDPasswordCredentials("user1", "wqerwqerwqer");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status== UserStatus.Invalid);
    }

    [Run]
    public void Authenticate_BadUser_SysToken1()
    {
      var tok = new SysAuthToken("1r", "23423423423423");
      var user = m_App.SecurityManager.Authenticate(tok);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }

    [Run]
    public void Authenticate_BadUser_SysToken2()
    {
      var tok = new SysAuthToken("4535r1", "5001");
      var user = m_App.SecurityManager.Authenticate(tok);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }

    [Run]
    public void Authenticate_BadUser_UriCredentials()
    {
      var credentials = new EntityUriCredentials("usr12222222");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }


    [Run]
    public void Authenticate_IDPasswordCredentials()
    {
      var credentials = new IDPasswordCredentials("user1", "awsedr");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("User1", user.Name);
    }

    [Run]
    public void Authenticate_SysToken()
    {
      var tok = new SysAuthToken("1r", "5001");
      var user = m_App.SecurityManager.Authenticate(tok);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("User1", user.Name);
    }

    [Run]
    public void Authenticate_UriCredentials()
    {
      var credentials = new EntityUriCredentials("usr1");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("User1", user.Name);
    }


  }
}


