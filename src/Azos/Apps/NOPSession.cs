/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Azos.Apps
{
  /// <summary>
  /// Represents a session that does nothing and returns fake user
  /// </summary>
  public sealed class NOPSession : ISession
  {
    private static NOPSession s_Instance = new NOPSession();

    private static Guid s_ID = new Guid("DA132A02-0D36-47D3-A9D7-10BC64741A6E");

    private NOPSession()
    {

    }

    /// <summary>
    /// Returns a singleton instance of the NOPSession
    /// </summary>
    public static NOPSession Instance => s_Instance;


    public Guid ID => s_ID;

    public ulong IDSecret => 0;

    public Guid? OldID => null;

    public bool IsNew => false;

    public bool IsJustLoggedIn => false;

    public DateTime? LastLoginUTC => null;

    public SessionLoginType LastLoginType => SessionLoginType.Unspecified;

    public bool IsEnded => false;

    public Security.User User
    {
      get { return Security.User.Fake; }
      set { }
    }

    public Atom LanguageISOCode => CoreConsts.ISOA_LANG_ENGLISH;


    public string DataContextName { get => null; set { } }


    public IDictionary<object, object> Items => new Dictionary<object, object>();


    public object this[object key]
    {
      get { return null; }
      set { }
    }

    public void End()
    {

    }

    public void Acquire()
    {

    }

    public void Release()
    {

    }

    public void HasJustLoggedIn(SessionLoginType loginType, DateTime utcNow)
    {

    }

    public void RegenerateID()
    {

    }

    IIdentity IPrincipal.Identity => User;
    bool IPrincipal.IsInRole(string role) => User.IsInRole(role);
  }
}
