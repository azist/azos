/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps;

namespace Azos.Security
{
  /// <summary>
  /// Provides security manager implementation that does nothing and always returns fake user instance
  /// </summary>
  public sealed class NOPSecurityManager : ApplicationComponent, ISecurityManagerImplementation
  {
    private NOPSecurityManager() : base()
    {
      m_PasswordManager = new DefaultPasswordManager(this);
      m_PasswordManager.Start();
    }

    protected override void Destructor()
    {
      m_PasswordManager.Dispose();
      base.Destructor();
    }

    private IPasswordManagerImplementation m_PasswordManager;

    private static NOPSecurityManager s_Instance = new NOPSecurityManager();

    public static NOPSecurityManager Instance { get { return s_Instance; } }

    public IPasswordManager PasswordManager { get { return m_PasswordManager; } }

    public User Authenticate(Credentials credentials) { return User.Fake; }

    public void Configure(Environment.IConfigSectionNode node) {}

    public User Authenticate(AuthenticationToken token) { return User.Fake; }

    public void Authenticate(User user) {}

    public AccessLevel Authorize(User user, Permission permission) { return new AccessLevel(user, permission, Rights.None.Root); }

    public SecurityLogMask LogMask{ get; set;}
    public Log.MessageType LogLevel { get; set;}


    public Environment.IConfigSectionNode GetUserLogArchiveDimensions(IIdentityDescriptor identity)
    {
      return null;
    }

    public void LogSecurityMessage(SecurityLogAction action, Log.Message msg, IIdentityDescriptor identity = null)
    {

    }
  }
}
