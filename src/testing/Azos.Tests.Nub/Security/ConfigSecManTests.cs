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
using Azos.Scripting;
using Azos.Security;

namespace Azos.Tests.Nub.Security
{
  [Runnable]
  public class ConfigSecManTests : IRunnableHook
  {
      private static string conf =
      @"
app
{
  security
  {
    users
    {
      user
      {
        name='User1'
        description='Just a User'
        status='User'
        id='user1'
        //password = thejake
        password='{""alg"":""MD5"",""fam"":""Text"",""hash"":""WtaG\\/XLsvvrC5CCpmm92Aw=="",""salt"":""g7sni3\\/uh08Ttb2Yar9optRPtd3aIQaDe89UTA==""}'
        rights
        {
           Azos{ Tests{ Nub{ Security{
             TeztPermissionA{ level = 2 }
             TeztPermissionC{ level = 3 }
           }}}}
        }
      }

      user
      {
        name='User2'
        description='Just a User'
        status='User'
        id='user2'
        //password = thejake
        password='{""alg"":""MD5"",""fam"":""Text"",""hash"":""WtaG\\/XLsvvrC5CCpmm92Aw=="",""salt"":""g7sni3\\/uh08Ttb2Yar9optRPtd3aIQaDe89UTA==""}'
        rights
        {
           Azos{ Tests{ Nub{ Security{
             TeztPermissionA{ level = 5 }
             TeztPermissionB{ level = 5 }
             TeztPermissionC{ level = 5 }
           }}}}
        }
      }

      user
      {
        name='UserSystem'
        description='User System'
        status='System'
        id='sys'
        //password = thejake
        password='{""alg"":""MD5"",""fam"":""Text"",""hash"":""\\/jps4fYHDMpfjbJag\\/\\/yhQ=="",""salt"":""EHIMX9k8V0rtPwnLqeBO6eUe""}'
      }


      user
      {
        name='UserKDF1'
        description='User1 with KDF password'
        status='User'
        id='ukdf1'
        //password = thejake
        password='{""alg"":""KDF"",""fam"":""Text"",""h"":""ZiZBXDp1HlN6J0XpXlTx31UPtaHZShRZr2nlJsEKIJc"",""s"":""1k1E2tS-equdggnLdaJKQhEkckPgDfnzmA31kVCOzZg""}'
      }

      user
      {
        name='UserKDF2'
        description='User2 with KDF password'
        status='User'
        id='ukdf2'
        //password = thejake
        password='{""alg"":""KDF"",""fam"":""Text"",""h"":""5BCbpaurWHlWsGyNGw4gp-O7A2sCemzct0homL-2oNM"",""s"":""q72tF6uMRHi44BESeMAgNpLhSBPvUxbrjd4qFXlMBJg""}'
      }

      user
      {
        name='UserKDF3'
        description='User3 with KDF password'
        status='User'
        id='ukdf3'
        //password = zizi-kaka12345
        password='{""alg"":""KDF"",""fam"":""Text"",""h"":""VsTnGyPjBzVOr_5fpMlua15dRN4OeT7hW5eep342hjw"",""s"":""-iuNdH1R6OXbeKbRh73MyhYtCb9retMKYMG2-SV5Nns""}'
      }

    }//users
  }//security
}//app
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
      Aver.AreEqual("User1", user.Name);
      Aver.AreEqual("Just a User", user.Description);
    }

    [Run]
    public void Authenticate_Reauthenticate_RegularUser()
    {
      void ensure(User u)
      {
        Aver.IsTrue(u.Status == UserStatus.User);
        Aver.AreEqual("User1", u.Name);
        Aver.AreEqual("Just a User", u.Description);
      }

      var credentials = new IDPasswordCredentials("user1", "thejake");
      var user = m_App.SecurityManager.Authenticate(credentials);
      ensure(user);

      var token = user.AuthToken;
      var user2 = m_App.SecurityManager.Authenticate(token);
      ensure(user2);

      m_App.SecurityManager.Authenticate(user2);//re-authenticate in-place
      ensure(user2);
    }

    [Run]
    public void Authenticate_RegularUser_UriCredentials()
    {
      var credentials = new EntityUriCredentials("user1");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("User1", user.Name);
      Aver.AreEqual("Just a User", user.Description);
    }

    [Run]
    public void Authenticate_Reauthenticate_RegularUser_UriCredentials()
    {
      void ensure(User u)
      {
        Aver.IsTrue(u.Status == UserStatus.User);
        Aver.AreEqual("User1", u.Name);
        Aver.AreEqual("Just a User", u.Description);
      }

      var credentials = new EntityUriCredentials("user1");
      var user = m_App.SecurityManager.Authenticate(credentials);
      ensure(user);

      var token = user.AuthToken;
      var user2 = m_App.SecurityManager.Authenticate(token);
      ensure(user2);

      m_App.SecurityManager.Authenticate(user2);//re-authenticate in-place
      ensure(user2);
    }


    [Run]
    public void Authenticate_SystemUser()
    {
      var credentials = new IDPasswordCredentials("sys", "thejake");
      var user = m_App.SecurityManager.Authenticate(credentials);

      Aver.IsTrue(user.Status == UserStatus.System);
      Aver.AreEqual("UserSystem", user.Name);
      Aver.AreEqual("User System", user.Description);
    }

    [Run, Run, Run, Run, Run, Run, Run]
    public async Task FlowAsyncContext()
    {
      impersonate( new IDPasswordCredentials("user1", "thejake"));


      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      await Task.Delay(Ambient.Random.NextScaledRandomInteger(10, 70));
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

      Console.WriteLine(Ambient.CurrentCallUser.ToString());

      Aver.IsTrue(Ambient.CurrentCallUser.Status == UserStatus.User);
      Aver.AreEqual("User1", Ambient.CurrentCallUser.Name);
      Aver.AreEqual("Just a User", Ambient.CurrentCallUser.Description);
      await Task.Yield();
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      await Task.Delay(Ambient.Random.NextScaledRandomInteger(10, 120));
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      Aver.IsTrue(Ambient.CurrentCallUser.Status == UserStatus.User);
      Aver.AreEqual("User1", Ambient.CurrentCallUser.Name);
      Aver.AreEqual("Just a User", Ambient.CurrentCallUser.Description);
      await Task.Delay(Ambient.Random.NextScaledRandomInteger(10, 70));

      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

      impersonate(new IDPasswordCredentials("sys", "thejake"));

      await Task.Delay(Ambient.Random.NextScaledRandomInteger(10, 99));
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      Aver.IsTrue(Ambient.CurrentCallUser.Status == UserStatus.System);
      Aver.AreEqual("UserSystem", Ambient.CurrentCallUser.Name);
      Aver.AreEqual("User System", Ambient.CurrentCallUser.Description);
      await Task.Delay(Ambient.Random.NextScaledRandomInteger(10, 150));
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      Aver.IsTrue(Ambient.CurrentCallUser.Status == UserStatus.System);
      Aver.AreEqual("UserSystem", Ambient.CurrentCallUser.Name);
      Aver.AreEqual("User System", Ambient.CurrentCallUser.Description);
      await Task.Delay(Ambient.Random.NextScaledRandomInteger(10, 250));
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      Aver.IsTrue(Ambient.CurrentCallUser.Status == UserStatus.System);
      Aver.AreEqual("UserSystem", Ambient.CurrentCallUser.Name);
      Aver.AreEqual("User System", Ambient.CurrentCallUser.Description);
      await Task.Delay(Ambient.Random.NextScaledRandomInteger(10, 100));
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      await Task.Yield();
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      Aver.IsTrue(Ambient.CurrentCallUser.Status == UserStatus.System);
      await Task.Delay(Ambient.Random.NextScaledRandomInteger(10, 100));
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      Aver.AreEqual("UserSystem", Ambient.CurrentCallUser.Name);
      await Task.Yield();
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      Aver.AreEqual("User System", Ambient.CurrentCallUser.Description);
      await Task.Delay(Ambient.Random.NextScaledRandomInteger(10, 150));
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      Aver.IsTrue(Ambient.CurrentCallUser.Status == UserStatus.System);
      await Task.Delay(Ambient.Random.NextScaledRandomInteger(10, 100));
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      Aver.AreEqual("UserSystem", Ambient.CurrentCallUser.Name);
      await Task.Delay(Ambient.Random.NextScaledRandomInteger(10, 300));
      Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
      Aver.AreEqual("User System", Ambient.CurrentCallUser.Description);
    }



    [Run]
    public void CheckMethodPermissions_1()
    {
      impersonate(new IDPasswordCredentials("user1", "thejake"));
      m_App.Authorize(new TeztPermissionA(1));
      m_App.Authorize(new[]{ new TeztPermissionA(1) });
    }

    [Run]
    public async Task CheckMethodPermissions_1_AsyncCaller()
    {
      impersonate(new IDPasswordCredentials("user1", "thejake"));
      m_App.Authorize(new TeztPermissionA(1));
      m_App.Authorize(new[] { new TeztPermissionA(1) });
      await Task.Delay(50);
    }

    [Run]
    [Aver.Throws(ExceptionType = typeof(SecurityException))]
    public void CheckMethodPermissions_2()
    {
      impersonate(new IDPasswordCredentials("user1", "thejake"));
      m_App.Authorize(new TeztPermissionA(4));//does not have 4
    }

    [Run]
    [Aver.Throws(ExceptionType = typeof(SecurityException))]
    public async Task CheckMethodPermissions_2_AsyncCaller()
    {
      impersonate(new IDPasswordCredentials("user1", "thejake"));
      await Task.Delay(50);
      m_App.Authorize(new TeztPermissionA(4));//does not have 4
    }

    [Run]
    public void CheckMethodPermissions_3()
    {
      impersonate(new IDPasswordCredentials("user2", "thejake"));
      m_App.Authorize(new TeztPermissionA(4));//does have 5
    }

    [Run]
    [Aver.Throws(ExceptionType = typeof(SecurityException))]
    public void CheckMethodPermissions_4()
    {
      impersonate(new IDPasswordCredentials("user1", "thejakewrongpassword"));
      m_App.Authorize(new TeztPermissionA(1));
    }

    [Run]
    [Aver.Throws(ExceptionType = typeof(SecurityException))]
    public void CheckMethodPermissions_5()
    {
      impersonate(new IDPasswordCredentials("user1", "thejake"));
      m_App.Authorize(Permission.All(
         new TeztPermissionA(1),
         new TeztPermissionB(1),//user1 does not have this one,
         new TeztPermissionC(1)
      ));
    }

    [Run]
    public void CheckMethodPermissions_6()
    {
      impersonate(new IDPasswordCredentials("user2", "thejake"));
      m_App.Authorize(
         new TeztPermissionA(1)
         .And( new TeztPermissionB(1) )//user2 does have this one,
         .And( new TeztPermissionC(1))
      );
    }

    [Run]
    [Aver.Throws(ExceptionType = typeof(SecurityException))]
    public void CheckMethodPermissions_7()
    {
      impersonate(new IDPasswordCredentials("user2", "thejake"));
      m_App.Authorize(
         new TeztPermissionA(6)
         .And(new TeztPermissionB(6))
         .And(new TeztPermissionC(6))
      );
    }

    [Run]
    public void CheckMethodPermissions_8()
    {
      impersonate(new IDPasswordCredentials("sys", "thejake"));
      m_App.Authorize(
         new TeztPermissionA(6)
         .And(new TeztPermissionB(6))//sys bypasses all security altogether
         .And(new TeztPermissionC(6))
      );
    }


    [Run]
    public void Authenticate_RegularUser_1_KDFPassword()
    {
      var credentials = new IDPasswordCredentials("ukdf1", "thejake");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("UserKDF1", user.Name);
      Aver.AreEqual("User1 with KDF password", user.Description);
    }

    [Run]
    public void Authenticate_RegularUser_2_KDFPassword()
    {
      var credentials = new IDPasswordCredentials("ukdf2", "thejake");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("UserKDF2", user.Name);
      Aver.AreEqual("User2 with KDF password", user.Description);
    }

    [Run]
    public void Authenticate_RegularUser_1_Invalid_KDFPassword()
    {
      var credentials = new IDPasswordCredentials("ukdf1", "zizi-kaka12345");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }

    [Run]
    public void Authenticate_RegularUser_2_Invalid_KDFPassword()
    {
      var credentials = new IDPasswordCredentials("ukdf2", "zizi-kaka12345");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }

    [Run]
    public void Authenticate_RegularUser_3_KDFPassword()
    {
      var credentials = new IDPasswordCredentials("ukdf3", "zizi-kaka12345");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("UserKDF3", user.Name);
      Aver.AreEqual("User3 with KDF password", user.Description);
    }

  }
}


