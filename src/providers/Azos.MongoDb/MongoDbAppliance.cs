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
using Azos.Glue;

namespace Azos.Data.Access.MongoDb
{
  /// <summary>
  /// Outlines a contract for Mongo appliance-mode databases -bundled with application and operating in a fully automated mode
  /// </summary>
  public interface IMongoDbAppliance : IModule
  {
    /// <summary>
    /// Returns the actual server node connection specifier (without "appliance" binding)
    /// </summary>
    Glue.Node EffectiveServerNode{  get; }

    /// <summary>
    /// Returns locally-bundled mongoDB or Null if this process does not host mongodb.
    /// Keep in mind that a physical server may have multiple applications running, and
    /// only one of them may have BundledMongoDb instance running, which serves other processes
    /// on the same machine
    /// </summary>
    BundledMongoDb Bundled {  get; }
  }


  /// <summary>
  /// Provides default implementation for IMongoDbAppliance contract
  /// </summary>
  public sealed class MongoDbAppliance : ModuleBase, IMongoDbAppliance
  {
    public const string CONFIG_BUNDLED_MONGO_SECTION = "bundled-mongo";
    public const string CONFIG_SERVER_NODE_ATTR = "server-node";

    public MongoDbAppliance(IApplication app) : base(app){ }
    public MongoDbAppliance(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Bundled);
      base.Destructor();
    }

    private Node m_EffectiveServerNode;
    private BundledMongoDb m_Bundled;

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => MongoConsts.MONGO_TOPIC;


    public Node EffectiveServerNode => m_EffectiveServerNode;
    public BundledMongoDb Bundled => m_Bundled;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node==null) return;

      var nbundled = node[CONFIG_BUNDLED_MONGO_SECTION];
      if (nbundled.Exists)
      {
        DisposeAndNull(ref m_Bundled);
        m_Bundled = FactoryUtils.MakeAndConfigureDirectedComponent<BundledMongoDb>(this, nbundled, typeof(BundledMongoDb));

        m_EffectiveServerNode = m_Bundled.ServerNode;
      }
      else
      {
        var n = node.Of(CONFIG_SERVER_NODE_ATTR);
        if (!n.Exists || n.Value.IsNullOrWhiteSpace())
          throw new CallGuardException(nameof(MongoDbAppliance), CONFIG_SERVER_NODE_ATTR, "Attribute must be specified when no bundled Mongo instance hosted");

        m_EffectiveServerNode = new Node(n.Value);
      }
    }

    protected override bool DoApplicationAfterInit()
    {
      if (m_Bundled!=null) m_Bundled.Start();
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      if (m_Bundled != null) m_Bundled.WaitForCompleteStop();
      return base.DoApplicationBeforeCleanup();
    }

  }
}
