/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Sky.Contracts;

namespace Azos.Sky.Identification.Server
{
  /// <summary>
  /// Hosts IGdidAuthority implementor as an app module
  /// </summary>
  public interface IGdidAuthorityModule : IGdidAuthority, IModule
  {
  }

  /// <summary>
  /// Provides default implementation for IGdidAuthorityModule which uses GdidAuthorityService.
  /// WARNING: This module should ONLY be used in GDID authority server applications!!!
  /// You should not run authority servers in your business applications, instead
  /// you should use GdidProviderModule which interfaces with well-defined ID generation
  /// servers (which mount GdidAuthorityModule class) using Http/Glue
  /// </summary>
  public sealed class GdidAuthorityModule : ModuleBase, IGdidAuthorityModule
  {
    public const string CONFIG_AUTHORITY_SECT = "authority";

    public GdidAuthorityModule(IApplication application) : base(application) { }
    public GdidAuthorityModule(IModule parent) : base(parent) { }


    private GdidAuthorityService m_Svc;

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.TOPIC_ID_GEN;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      node.NonEmpty(nameof(GdidAuthorityModule) + ".conf");

      base.DoConfigure(node);

      var nauth = node[CONFIG_AUTHORITY_SECT];
      nauth.NonEmpty("cfg section `{0}`".Args(CONFIG_AUTHORITY_SECT));

      DisposeAndNull(ref m_Svc);
      m_Svc = FactoryUtils.MakeAndConfigureDirectedComponent<GdidAuthorityService>(this, nauth, typeof(GdidAuthorityService));
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Svc.NonNull(nameof(m_Svc)).Start();
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      m_Svc.NonNull(nameof(m_Svc)).WaitForCompleteStop();
      return base.DoApplicationBeforeCleanup();
    }

    public GdidBlock AllocateBlock(string scopeName, string sequenceName, int blockSize, ulong? vicinity = 1152921504606846975)
     => m_Svc.AllocateBlock(scopeName, sequenceName, blockSize, vicinity);
  }
}
