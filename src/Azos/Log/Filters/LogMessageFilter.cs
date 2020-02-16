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
  /// Derive class to override Evaluate(Message) with custom filtering logic
  /// </summary>
  public class LogMessageFilter : BoolFilter<Message>
  {
    public BoolFilter<Message> Tree{ get; set; }

    /// <summary>
    /// Override to perform custom logic. Base implementation evaluates Tree if it is not null
    /// </summary>
    public override bool Evaluate(Message context)
    {
      var tree = Tree;
      if (tree==null) return true;//pass by default
      return tree.Evaluate(context);
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      var nTree = node["tree"];
      if (nTree.Exists)
       Tree = FactoryUtils.MakeAndConfigure<BoolFilter<Message>>(nTree);
    }
  }
}
