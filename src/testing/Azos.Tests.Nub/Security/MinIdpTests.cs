/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Security;
using Azos.Security.MinIdp;

namespace Azos.Tests.Nub.Security
{
  [Runnable]
  public class MinIdpTests  : IRunHook
  {
    internal class MockStore : Daemon, IMinIdpStoreImplementation
    {
      public const string PWD1 = "{alg:'MD5', fam:'Text', hash:'WtaG\\/XLsvvrC5CCpmm92Aw==', salt:'g7sni3\\/uh08Ttb2Yar9optRPtd3aIQaDe89UTA=='}";//thejake
      public const string PWD2 = "{alg:'KDF', fam:'Text', h:'3xg0BzA4wCZ9CXsfBZUKIbtPEylWoAXV1ecMJgY98Hs', s:'k2P0NzALo4eIOmpHwNTrh-0iaEaab6dOSiniyNEDej8'}";//awsedr

      public MockStore(IApplicationComponent dir) : base(dir){ }

      public override string ComponentLogTopic => "tezt";

      public ICryptoMessageAlgorithm MessageProtectionAlgorithm => null;

      public Task<MinIdpUserData> GetByIdAsync(Atom realm, string id, AuthenticationRequestContext ctx = null)
      {
        if (realm.Value == "r1" && id=="user1")
          return Task.FromResult(new MinIdpUserData{ SysId = 1, Realm = realm, CreateUtc = DateTime.UtcNow, StartUtc = DateTime.UtcNow.AddMinutes(-10), EndUtc = DateTime.UtcNow.AddMinutes(10), Name = "R1User1", Status = UserStatus.User, LoginId = "user1", LoginPassword = PWD1 });

        if (realm.Value == "r2" && id == "user1")
          return Task.FromResult(new MinIdpUserData { SysId = 2, Realm = realm, CreateUtc = DateTime.UtcNow, StartUtc = DateTime.UtcNow.AddMinutes(-10), EndUtc = DateTime.UtcNow.AddMinutes(10), Name = "R2User1", Status = UserStatus.User, LoginId = "user1", LoginPassword = PWD2 });

        return Task.FromResult<MinIdpUserData>(null);
      }

      public Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken, AuthenticationRequestContext ctx = null)
      {
        if (realm.Value == "r1" && sysToken == "t1")
          return Task.FromResult(new MinIdpUserData { SysId = 1, Realm = realm, CreateUtc = DateTime.UtcNow, StartUtc = DateTime.UtcNow.AddMinutes(-10), EndUtc = DateTime.UtcNow.AddMinutes(10), Name = "R1User1", Status = UserStatus.User });

        if (realm.Value == "r2" && sysToken == "t1")
          return Task.FromResult(new MinIdpUserData { SysId = 2, Realm = realm, CreateUtc = DateTime.UtcNow, StartUtc = DateTime.UtcNow.AddMinutes(-10), EndUtc = DateTime.UtcNow.AddMinutes(10), Name = "R2User1", Status = UserStatus.User });

        return Task.FromResult<MinIdpUserData>(null);
      }

      public Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri, AuthenticationRequestContext ctx = null)
      {
        if (realm.Value == "r1" && uri == "uri1")
          return Task.FromResult(new MinIdpUserData { SysId = 1, Realm = realm, CreateUtc = DateTime.UtcNow, StartUtc = DateTime.UtcNow.AddMinutes(-10), EndUtc = DateTime.UtcNow.AddMinutes(10), Name = "R1User1", Status = UserStatus.User });

        if (realm.Value == "r2" && uri == "uri1")
          return Task.FromResult(new MinIdpUserData { SysId = 2, Realm = realm, CreateUtc = DateTime.UtcNow, StartUtc = DateTime.UtcNow.AddMinutes(-10), EndUtc = DateTime.UtcNow.AddMinutes(10), Name = "R2User1", Status = UserStatus.User });

        return Task.FromResult<MinIdpUserData>(null);
      }
    }


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
         type='Azos.Tests.Nub.Security.MinIdpTests+MockStore, Azos.Tests.Nub'

       }
    }
  }
}
";

    private static string confR2 =
         @"
app
{
  security
  {
    type='Azos.Security.MinIdp.MinIdpSecurityManager, Azos'
    realm = 'r2'
    store
    {
       type='Azos.Security.MinIdp.CacheLayer, Azos'
       store
       {
         type='Azos.Tests.Nub.Security.MinIdpTests+MockStore, Azos.Tests.Nub'

       }
    }
  }
}
";
    private AzosApplication m_App;

    public bool Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
    {
      m_App = new AzosApplication(null, (args[0].AsInt()==1 ? confR1 : confR2).AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      return false;
    }

    public bool Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
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


