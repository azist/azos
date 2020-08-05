/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Conf;
using Azos.Data.Idgen;

namespace Azos.Sky.Identification
{
  /// <summary>
  /// Sets contract for hosting IGdidProvider as an app module
  /// </summary>
  public interface IGdidProviderModule : IModule
  {
    IGdidProvider Provider { get; }
  }

  /// <summary>
  /// Provides default implementation for GdidProviderModule which uses GdidGenerator with remote accessor or LocalGdidGenerator
  /// </summary>
  public sealed class GdidProviderModule : ModuleBase, IGdidProviderModule
  {
    public const string CONFIG_ACCESSOR_SECT = "accessor";
    public const string CONFIG_GENERATOR_SECT = "generator";
    public const string CONFIG_LOCAL_ATTR = "use-local-authority";

    public GdidProviderModule(IApplication application) : base(application) { }
    public GdidProviderModule(IModule parent) : base(parent) { }


    private IGdidProvider m_Generator;

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.TOPIC_ID_GEN;

    public IGdidProvider Provider => m_Generator;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      node.NonEmpty(nameof(GdidProviderModule)+".conf");
      base.DoConfigure(node);

      var isLocal = node.Of(CONFIG_LOCAL_ATTR).ValueAsBool(false);

      var naccessor = node[CONFIG_ACCESSOR_SECT];
      if (!isLocal && naccessor.Exists)
      {
        var accessor = FactoryUtils.MakeAndConfigureDirectedComponent<IGdidAuthorityAccessor>(this, naccessor);
        m_Generator = new GdidGenerator(this, nameof(GdidProviderModule), accessor);
      }
      else
      {
        if (isLocal)
          m_Generator = new LocalGdidGenerator(this);
        else
          m_Generator = new GdidGenerator(this, nameof(GdidProviderModule));
      }

      var ngen = node[CONFIG_GENERATOR_SECT];
      if (ngen.Exists && m_Generator is IConfigurable configurable) configurable.Configure(ngen);
    }

  }
}
