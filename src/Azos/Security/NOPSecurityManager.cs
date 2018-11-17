/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Log;

namespace Azos.Security
{
  /// <summary>
  /// Provides security manager implementation that does nothing and always returns fake user instance
  /// </summary>
  public sealed class NOPSecurityManager : ApplicationComponent, ISecurityManagerImplementation
  {
    public NOPSecurityManager(IApplication app) : base(app)
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

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    public IPasswordManager PasswordManager { get { return m_PasswordManager; } }

    public User Authenticate(Credentials credentials) => User.Fake;

    public void Configure(Conf.IConfigSectionNode node) {}

    public User Authenticate(AuthenticationToken token) => User.Fake;

    public void Authenticate(User user) {}

    public AccessLevel Authorize(User user, Permission permission) { return new AccessLevel(user, permission, Rights.None.Root); }

    public SecurityLogMask SecurityLogMask{ get; set;}
    public MessageType SecurityLogLevel { get; set; }

    public Conf.IConfigSectionNode GetUserLogArchiveDimensions(IIdentityDescriptor identity)
    {
      return null;
    }

    public void LogSecurityMessage(SecurityLogAction action, Log.Message msg, IIdentityDescriptor identity = null)
    {
    }
  }
}
