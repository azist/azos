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
        password='{""algo"":""MD5"",""fam"":""Text"",""hash"":""WtaG\\/XLsvvrC5CCpmm92Aw=="",""salt"":""g7sni3\\/uh08Ttb2Yar9optRPtd3aIQaDe89UTA==""}'
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
        password='{""algo"":""MD5"",""fam"":""Text"",""hash"":""WtaG\\/XLsvvrC5CCpmm92Aw=="",""salt"":""g7sni3\\/uh08Ttb2Yar9optRPtd3aIQaDe89UTA==""}'
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
        password='{""algo"":""MD5"",""fam"":""Text"",""hash"":""\\/jps4fYHDMpfjbJag\\/\\/yhQ=="",""salt"":""EHIMX9k8V0rtPwnLqeBO6eUe""}'
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
    public void Authenticate_RegularUser()
    {
      var credentials = new IDPasswordCredentials("user1", "thejake");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("User1", user.Name);
      Aver.AreEqual("Just a User", user.Description);
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
    [TeztPermissionA(1)]
    public void CheckMethodPermissions_1()
    {
      impersonate(new IDPasswordCredentials("user1", "thejake"));
      MethodBase.GetCurrentMethod().CheckPermissions(m_App);
    }

    [Run]
    [Aver.Throws(ExceptionType = typeof(SecurityException))]
    [TeztPermissionA(4)]//does not have 4
    public void CheckMethodPermissions_2()
    {
      impersonate(new IDPasswordCredentials("user1", "thejake"));
      MethodBase.GetCurrentMethod().CheckPermissions(m_App);
    }

    [Run]
    [TeztPermissionA(4)]//does have 5
    public void CheckMethodPermissions_3()
    {
      impersonate(new IDPasswordCredentials("user2", "thejake"));
      MethodBase.GetCurrentMethod().CheckPermissions(m_App);
    }

    [Run]
    [Aver.Throws(ExceptionType = typeof(SecurityException))]
    [TeztPermissionA(1)]
    public void CheckMethodPermissions_4()
    {
      impersonate(new IDPasswordCredentials("user1", "thejakewrongpassword"));
      MethodBase.GetCurrentMethod().CheckPermissions(m_App);
    }

    [Run]
    [Aver.Throws(ExceptionType = typeof(SecurityException))]
    [TeztPermissionA(1)]
    [TeztPermissionB(1)] //user1 does not have this one
    [TeztPermissionC(1)]
    public void CheckMethodPermissions_5()
    {
      impersonate(new IDPasswordCredentials("user1", "thejake"));
      MethodBase.GetCurrentMethod().CheckPermissions(m_App);
    }

    [Run]
    [TeztPermissionA(1)]
    [TeztPermissionB(1)] //user2 does have this one
    [TeztPermissionC(1)]
    public void CheckMethodPermissions_6()
    {
      impersonate(new IDPasswordCredentials("user2", "thejake"));
      MethodBase.GetCurrentMethod().CheckPermissions(m_App);
    }

    [Run]
    [Aver.Throws(ExceptionType = typeof(SecurityException))]
    [TeztPermissionA(6)]
    [TeztPermissionB(6)] //user2 does have the required level
    [TeztPermissionC(6)]
    public void CheckMethodPermissions_7()
    {
      impersonate(new IDPasswordCredentials("user2", "thejake"));
      MethodBase.GetCurrentMethod().CheckPermissions(m_App);
    }

    [Run]
    [TeztPermissionA(6)]
    [TeztPermissionB(6)] //sys bypasses all security altogether
    [TeztPermissionC(6)]
    public void CheckMethodPermissions_8()
    {
      impersonate(new IDPasswordCredentials("sys", "thejake"));
      MethodBase.GetCurrentMethod().CheckPermissions(m_App);
    }


  }
}


