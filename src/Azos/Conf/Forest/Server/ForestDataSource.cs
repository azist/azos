/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Collections;
using Azos.Data;
using Azos.Data.Access;
using Azos.Instrumentation;
using Azos.Pile;

namespace Azos.Conf.Forest.Server
{
  /// <summary>
  /// Acts as a config forest data hub/dispatcher organizing data stores for forest/tree
  /// </summary>
  public sealed class ForestDataSource : DaemonWithInstrumentation<IApplicationComponent>, IForestDataSource
  {
    public const string CONFIG_FOREST_SECTION = "forest";
    public const string CONFIG_TREE_SECTION = "tree";
    public const string CONFIG_CACHE_SECTION = "cache";
    public const string CONFIG_PILE_SECTION = "pile";

    private sealed class _forest : INamed
    {
      internal _forest(Atom name, Registry<IDataStoreImplementation> trees)
      {
        Name = name;
        Trees = trees;
      }

      internal readonly Atom Name;
      string INamed.Name => Name.Value;
      internal readonly Registry<IDataStoreImplementation> Trees;
    }


    public ForestDataSource(IForestSetupLogic logic) : base(logic) { }

    protected override void Destructor()
    {
      cleanup();
      base.Destructor();
    }

    private Registry<_forest> m_Forests = new Registry<_forest>();
    private IEnumerable<IDataStoreImplementation> allStores => m_Forests.SelectMany(f => f.Trees);
    private IPileImplementation m_Pile;
    private ICacheImplementation m_Cache;

    private bool m_InstrumentationEnabled;
    public override string ComponentLogTopic => CoreConsts.CONF_TOPIC;

    /// <summary>
    /// Implements IInstrumentable
    /// </summary>
    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled; }
      set { m_InstrumentationEnabled = value; }
    }

    /// <summary>
    /// Cache or null
    /// </summary>
    public ICache Cache => m_Cache.NonNull(nameof(Cache));

    /// <summary>
    /// Tries to return all trees for the forest or null enumerable if the forest is not found
    /// </summary>
    public Task<IEnumerable<Atom>> TryGetAllForestTreesAsync(Atom idForest)
    {
      if (!Running) return Task.FromResult<IEnumerable<Atom>>(null);
      var forest = m_Forests[idForest.Value];
      if (forest == null)
        throw new ValidationException("Forest not found"){ HttpStatusCode = 404, HttpStatusDescription = "Forest not found"};
      return Task.FromResult(forest.Trees.Select( t => Atom.Encode(t.Name) ));
    }

    /// <summary>
    /// Returns a context for the specified forest tree or null if not found
    /// </summary>
    public IDataStore TryGetTreeDataStore(Atom idForest, Atom idTree)
    {
      if (!Running) return null;
      var forest = m_Forests[idForest.Value];
      if (forest == null) return null;
      var tree = forest.Trees[idTree.Value];
      return tree;
    }


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node == null) return;

      cleanup();

      //build forests
      foreach (var nforest in node.ChildrenNamed(CONFIG_FOREST_SECTION))
      {
        var idForest = nforest.Of(Configuration.CONFIG_NAME_ATTR, "id").ValueAsAtom(Atom.ZERO);
        if (idForest.IsZero || !idForest.IsValid)
          throw new ConfigException($"{nameof(ForestDataSource)} config `forest` section is missing a valid atom `$id`");

        var trees = new Registry<IDataStoreImplementation>();
        var forest = new _forest(idForest, trees);

        if (!m_Forests.Register(forest))
          throw new ConfigException($"{nameof(ForestDataSource)} config duplicate section: ./forest[name='{forest.Name}']");

        //build trees
        foreach(var ntree in nforest.ChildrenNamed(CONFIG_TREE_SECTION))
        {
          var idTree = ntree.Of(Configuration.CONFIG_NAME_ATTR).ValueAsAtom(Atom.ZERO);
          if (idTree.IsZero || !idTree.IsValid)
            throw new ConfigException($"{nameof(ForestDataSource)} config `tree` section is missing a valid atom `$id`");

          var tree = FactoryUtils.MakeAndConfigureDirectedComponent<IDataStoreImplementation>(this, ntree);

          if (!trees.Register(tree))
            throw new ConfigException($"{nameof(ForestDataSource)} config duplicate section: ./forest[name='{forest.Name}']/tree['{tree.Name}']");
        }
      }

      //Build CACHE
      var ncache = node[CONFIG_CACHE_SECTION];
      m_Cache = FactoryUtils.MakeAndConfigureDirectedComponent<ICacheImplementation>(this,
                                                                         ncache,
                                                                         typeof(LocalCache),
                                                                         new[] { "Cache::{0}::{1}".Args(nameof(ForestDataSource), Name) });
      if (m_Cache is LocalCache lcache)
      {
        var npile = node[CONFIG_PILE_SECTION];
        m_Pile = FactoryUtils.MakeAndConfigureDirectedComponent<IPileImplementation>(this,
                                npile,
                                typeof(DefaultPile),
                                new[] { "Pile::{0}::{1}".Args(nameof(ForestDataSource), Name) });
        lcache.Pile = m_Pile;
      }
    }

    protected override void DoStart()
    {
      base.DoStart();

      m_Pile.NonNull("pile");
      m_Cache.NonNull("cache");
      (m_Forests.Count > 0).IsTrue("Forest > 0");

      if (m_Pile is Daemon dp) dp.Start();
      if (m_Cache is Daemon d) d.Start();

      allStores.OfType<Daemon>().ForEach(c => this.DontLeak(() => c.Start()));
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      this.DontLeak(() => m_Cache.SignalStop());
      this.DontLeak(() => m_Pile.SignalStop());
      allStores.OfType<Daemon>().ForEach(c => this.DontLeak(() => c.SignalStop()));
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      this.DontLeak(() => m_Cache.SignalStop());
      this.DontLeak(() => m_Pile.SignalStop());
      allStores.OfType<Daemon>().ForEach(c => this.DontLeak(() => c.WaitForCompleteStop()));
    }

    protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
    {
      base.DoAcceptManagerVisit(manager, managerNow);
      allStores.OfType<Daemon>().ForEach(d => this.DontLeak(() => d.AcceptManagerVisit(this, managerNow)));
    }

    private void cleanup()
    {
      var all = allStores.ToArray();
      m_Forests.Clear();
      all.ForEach( tree => this.DontLeak(() => tree.Dispose()) );

      DisposeAndNull(ref m_Cache);
      DisposeAndNull(ref m_Pile);
    }

  }
}
