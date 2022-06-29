/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Data.Heap.Implementation
{
  /// <summary>
  /// Default implementation of Node selector selects best suiting nodes for call based
  /// on node status and proximity
  /// </summary>
  public sealed class DefaultNodeSelector : ApplicationComponent<IArea>, INodeSelector
  {
    public DefaultNodeSelector(IArea area, IConfigSectionNode cfg) : base(area)
    {
    }

    public IArea Area => ComponentDirector;
    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;
    public IEnumerable<INode> ForLocal => RelativeTo(null);


    public IEnumerable<INode> RelativeTo(string hostName)
    {
      return Area.Nodes;
    }
  }
}
