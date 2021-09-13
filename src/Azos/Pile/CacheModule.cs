/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Pile
{
  /// <summary>
  /// Hosts ICache as a module
  /// </summary>
  public interface ICacheModule : IModule
  {
    /// <summary>
    /// Provides cache services
    /// </summary>
    ICache Cache { get; }
  }

  /// <summary>
  /// Denotes an entity implementing ICache
  /// </summary>
  public interface ICacheModuleImplementation : ICacheModule, IModuleImplementation { }

  /// <summary>
  /// Provides default implementation for ICacheModuleImplementation
  /// </summary>
  public sealed class CacheModule : ModuleBase, ICacheModuleImplementation
  {
    public const string CONFIG_CACHE_SECTION = "cache";
    public const string CONFIG_PILE_SECTION = "pile";

    public CacheModule(IApplication application) : base(application) { }
    public CacheModule(IModule parent) : base(parent) { }

    private IPileImplementation m_Pile;
    private ICacheImplementation m_Cache;

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.CACHE_TOPIC;


    public ICache Cache => m_Cache.NonNull(nameof(m_Cache));

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node==null || !node.Exists) return;

      DisposeAndNull(ref m_Cache);
      DisposeAndNull(ref m_Pile);

      var ncache = node[CONFIG_CACHE_SECTION];
      m_Cache = FactoryUtils.MakeAndConfigureDirectedComponent<ICacheImplementation>(this,
                                                                         ncache,
                                                                         typeof(LocalCache),
                                                                         new[] { "Cache::{0}::{1}".Args(nameof(CacheModule), Name) });
      if (m_Cache is LocalCache lcache)
      {
        var npile = node[CONFIG_PILE_SECTION];
        m_Pile = FactoryUtils.MakeAndConfigureDirectedComponent<IPileImplementation>(this,
                                npile,
                                typeof(DefaultPile),
                                new[] { "Pile::{0}::{1}".Args(nameof(CacheModule), Name) });
        lcache.Pile = m_Pile;
      }
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Cache.NonNull("`{0}` not configured".Args(CONFIG_CACHE_SECTION));

      if (m_Pile is Daemon dp)
        dp.StartByApplication();

      if (m_Cache is Daemon d)
        d.StartByApplication();

      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      DisposeAndNull(ref m_Cache);
      DisposeAndNull(ref m_Pile);
      return base.DoApplicationBeforeCleanup();
    }
  }
}
