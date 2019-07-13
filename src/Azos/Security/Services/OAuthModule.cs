/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Security.Tokens;

namespace Azos.Security.Services
{
  /// <summary>
  /// Provides OAuth20 security services. This is a dependency for a OAuthControllerBase-derived classes
  /// </summary>
  public sealed class OAuthModule : ModuleBase, IOAuthModuleImplementation
  {
    public OAuthModule(IApplication application) : base(application) { }
    public OAuthModule(IModule parent) : base(parent) { }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    private ISecurityManagerImplementation m_ClientSecurity;
    private ITokenRingImplementation m_TokenRing;

    public ISecurityManager ClientSecurity => m_ClientSecurity;
    public ITokenRing TokenRing => m_TokenRing;


  }
}
