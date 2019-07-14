/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Security.Tokens;

namespace Azos.Security.Services
{
  /// <summary>
  /// Provides OAuth20 security services. This is a dependency for a OAuthControllerBase-derived classes
  /// </summary>
  public sealed class OAuthModule : ModuleBase, IOAuthModuleImplementation
  {
    public const string CONFIG_CLIENT_SECURITY_SECTION = "client-security";
    public const string CONFIG_TOKEN_RING_SECTION = "token-ring";

    public OAuthModule(IApplication application) : base(application) { }
    public OAuthModule(IModule parent) : base(parent) { }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    private ISecurityManagerImplementation m_ClientSecurity;
    private ITokenRingImplementation m_TokenRing;

    public ISecurityManager ClientSecurity => m_ClientSecurity.NonNull($"{nameof(ClientSecurity)} is not available");
    public ITokenRing TokenRing => m_TokenRing.NonNull($"{nameof(TokenRing)} is not available");

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node==null || !node.Exists) return;

      var nsec = node[CONFIG_CLIENT_SECURITY_SECTION];
      if (nsec.Exists)
      {
        m_ClientSecurity = FactoryUtils.MakeAndConfigureDirectedComponent<ISecurityManagerImplementation>(this, nsec, typeof(ConfigSecurityManager));
        if (m_ClientSecurity is Daemon daemon) daemon.Start();
      }

      var nring = node[CONFIG_TOKEN_RING_SECTION];
      if (nring.Exists)
      {
        m_TokenRing = FactoryUtils.MakeAndConfigureDirectedComponent<ITokenRingImplementation>(this, nsec);
        if (m_TokenRing is Daemon daemon) daemon.Start();
      }
    }

    protected override bool DoApplicationAfterInit()
    {
      //if not configured, then shortcut to App.SecurityManager
      if (m_ClientSecurity==null) m_ClientSecurity = (ISecurityManagerImplementation)App.SecurityManager;

      //if not configured, then make default ClientTokenRing
      if (m_TokenRing==null) m_TokenRing = new ClientTokenRing(this);

      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      if (m_ClientSecurity.ComponentDirector==this)
        DisposeAndNull(ref m_ClientSecurity);//if directed, then dispose it
      else
        m_ClientSecurity = null;//if not directed by this, then just lose reference to it

      DisposeAndNull(ref m_TokenRing);

      return base.DoApplicationBeforeCleanup();
    }
  }
}
