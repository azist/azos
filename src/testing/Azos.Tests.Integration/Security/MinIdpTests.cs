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
  public class MinIdpTests  : IRunnableHook
  {

    private static string confR1 =
      @"
app
{
  security
  {
    type='Azos.Security.MinIdp.MinIdpSecurityManager, Azos'
    realm = 'r1'
    store
    {
       type='Azos.Security.MinIdp.CacheLayer, Azos'
       store
       {
         type='Azos.Security.MinIdp.MinIdpSqlStore, Azos.MsSql'
         connect-string='Data Source=OCTOD;Initial Catalog=MINIDP;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False'
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


    [Run("realm=1")]
    [Run("realm=2")]
    public void Authenticate_BadUserPassword(int realm)
    {
      var credentials = new IDPasswordCredentials("user1", "wqerwqerwqer");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status== UserStatus.Invalid);
    }

    [Run("realm=1")]
    [Run("realm=2")]
    public void Authenticate_BadUser_SysToken1(int realm)
    {
      var tok = new SysAuthToken("r{0}".Args(realm), "23423423423423");
      var user = m_App.SecurityManager.Authenticate(tok);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }

    [Run("realm=1")]
    public void Authenticate_BadUser_SysToken2(int realm)
    {
      var tok = new SysAuthToken("4535r1", "1");
      var user = m_App.SecurityManager.Authenticate(tok);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }

    [Run("realm=1")]
    [Run("realm=2")]
    public void Authenticate_BadUser_UriCredentials(int realm)
    {
      var credentials = new EntityUriCredentials("sadfsafsa");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }


    [Run("realm=1 name='R1User1' pwd='thejake'")]
    [Run("realm=2 name='R2User1' pwd='awsedr'")]
    public void Authenticate_IDPasswordCredentials(int realm, string name, string pwd)
    {
      var credentials = new IDPasswordCredentials("user1", pwd);
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual(name, user.Name);
    }

    [Run("realm=1 name='R1User1' data=t1")]
    [Run("realm=2 name='R2User1' data=t1")]
    public void Authenticate_SysToken(int realm, string name, string data)
    {
      var tok = new SysAuthToken("r{0}".Args(realm), data);
      var user = m_App.SecurityManager.Authenticate(tok);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual(name, user.Name);
    }

    [Run("realm=1 name='R1User1'")]
    [Run("realm=2 name='R2User1'")]
    public void Authenticate_UriCredentials(int realm, string name)
    {
      var credentials = new EntityUriCredentials("uri1");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual(name, user.Name);
    }


  }
}


