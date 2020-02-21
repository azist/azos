/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Scripting.Expressions;

namespace Azos.Log.Filters
{
  /// <summary>
  /// Derive class to override Evaluate(Message) with custom filtering logic.
  /// </summary>
  /// <remarks>
  /// A typical filter expression tree with 2-3 nodes yields multi million ops/sec performance on a single thread.
  /// As there is no locking and no global state, performance scales linearly on multiple threads
  /// </remarks>
  public class LogMessageFilter : BoolFilter<Message>
  {
    public const string CONFIG_TREE_SECTION = "tree";

    public Expression<Message, bool> Tree{ get; set; }

    /// <summary>
    /// Override to perform custom logic. Base implementation evaluates Tree if it is not null
    /// </summary>
    public override bool Evaluate(Message context)
    {
      var root = Tree;
      if (root == null) return true;//passes filter by default
      return root.Evaluate(context);
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      var nTree = node[CONFIG_TREE_SECTION];
      if (nTree.Exists)
       Tree = FactoryUtils.MakeAndConfigure<Expression<Message, bool>>(nTree);
    }
  }
}
