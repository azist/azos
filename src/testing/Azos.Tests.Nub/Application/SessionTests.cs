using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Scripting;
using Azos.Security;

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class SessionTests
  {
    [Run("dcn='z d e g' norm='d,e,g,z'")]
    [Run("dcn='zIMa,peTer,AleX' norm='alex,peter,zima'")]
    [Run("dcn='zIMa;peter,  peTer;AleX      ' norm='alex,peter,peter,zima'")]
    public void DataContextNorm(string dcn, string norm)
    {
      var session = new BaseSession(Guid.NewGuid(), 0);
      session.DataContextName = dcn;
      var got = session.GetNormalizedDataContextName();
      Aver.AreEqual(norm, got);
    }

    [Run("it=15 delay=50")]
    [Run("it=50 delay=5")]
    public async Task AmbientSessionContextFlow(int it, int delay)
    {
      var session = new BaseSession(Guid.NewGuid(), 0);
      var user = new User(new IDPasswordCredentials("a","b"), new SysAuthToken(), "User1", Rights.None);
      session.DataContextName = "abcd";
      session.User = user;
      ExecutionContext.__SetThreadLevelSessionContext(session);

      checkAmbientSession("User1");

      for(var i = 0; i < it; i++)
      {
        await Task.Delay(delay)//note: Task.Yield() yields on the same thread if there is no load
                  .ContinueWith( a =>  TaskUtils.LoadAllCoresFor(delay))
                  .ContinueWith( a => checkAmbientSession("User1"));
      }

      //---------- switch user in-place --------------------
      "Switching user in-place".See();

      session.User = new User(new IDPasswordCredentials("a", "b"), new SysAuthToken(), "User2", Rights.None);
      checkAmbientSession("User2");

      for (var i = 0; i < it; i++)
      {
        await Task.Delay(delay)
                  .ContinueWith(a => TaskUtils.LoadAllCoresFor(delay))
                  .ContinueWith(a => checkAmbientSession("User2"));
      }
    }

    private static void checkAmbientSession(string usr)
    {
      "Ambient session context on physical thread: {0}".SeeArgs(System.Threading.Thread.CurrentThread.ManagedThreadId);
      var session = Ambient.CurrentCallSession;
      Aver.AreEqual("abcd", session.DataContextName);
      Aver.AreEqual(usr, session.User.Name);
    }

  }
}
