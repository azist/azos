/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Scripting;
using Azos.Security;
using Azos.Security.MinIdp;

namespace Azos.Tests.Nub.Security
{
  [Runnable]
  public class MinIdpTests : IRunnableHook
  {
    internal class MockStore : Daemon, IMinIdpStoreImplementation
    {
      public const string PWD1 = "{alg:'MD5',fam:'Text',hash:'WtaG\\/XLsvvrC5CCpmm92Aw==',salt:'g7sni3\\/uh08Ttb2Yar9optRPtd3aIQaDe89UTA==}";//thejake
      public const string PWD2 = "{alg:'KDF',fam:'Text',h:'3xg0BzA4wCZ9CXsfBZUKIbtPEylWoAXV1ecMJgY98Hs',s:'k2P0NzALo4eIOmpHwNTrh-0iaEaab6dOSiniyNEDej8'}";//awsedr

      public MockStore(IApplicationComponent dir) : base(dir){ }

      public override string ComponentLogTopic => "tezt";

      public Task<MinIdpUserData> GetByIdAsync(Atom realm, string id)
      {
        if (realm.Value == "r1" && id=="user1")
          return Task.FromResult(new MinIdpUserData{ CreateUtc = DateTime.Now, Name = "R1User1", Status = UserStatus.User, LoginId = "user1", LoginPassword = PWD1 });

        if (realm.Value == "r2" && id == "user1")
          return Task.FromResult(new MinIdpUserData { CreateUtc = DateTime.Now, Name = "R2User1", Status = UserStatus.User, LoginId = "user1", LoginPassword = PWD2 });

        return null;
      }

      public Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken)
      {
        if (realm.Value == "r1" && sysToken == "t1")
          return Task.FromResult(new MinIdpUserData { CreateUtc = DateTime.Now, Name = "R1User1", Status = UserStatus.User });

        if (realm.Value == "r2" && sysToken == "t1")
          return Task.FromResult(new MinIdpUserData { CreateUtc = DateTime.Now, Name = "R2User1", Status = UserStatus.User });

        return null;
      }

      public Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri)
      {
        if (realm.Value == "r1" && uri == "uri1")
          return Task.FromResult(new MinIdpUserData { CreateUtc = DateTime.Now, Name = "R1User1", Status = UserStatus.User });

        if (realm.Value == "r2" && uri == "uri1")
          return Task.FromResult(new MinIdpUserData { CreateUtc = DateTime.Now, Name = "R2User1", Status = UserStatus.User });

        return null;
      }
    }



    private static string conf =
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
    private AzosApplication m_App;

    private void impersonate(Credentials credentials)
    {
      var session = new BaseSession(Guid.NewGuid(), 1234);
      session.User = m_App.SecurityManager.Authenticate(credentials);
      Azos.Apps.ExecutionContext.__SetThreadLevelSessionContext(session);
    }

    void IRunnableHook.Prologue(Runner runner, FID id)
     => m_App = new AzosApplication(null, conf.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_App);
      return false;
    }


    [Run]
    public void Authenticate_BadUser()
    {
      var credentials = new IDPasswordCredentials("sadfsafsa", "wqerwqerwqer");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status== UserStatus.Invalid);
    }

    [Run]
    public void Authenticate_BadUser_UriCredentials()
    {
      var credentials = new EntityUriCredentials("sadfsafsa");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }

    [Run]
    public void Authenticate_RegularUser()
    {
      var credentials = new IDPasswordCredentials("user1", "thejake");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("R1User1", user.Name);
    }


  }
}


