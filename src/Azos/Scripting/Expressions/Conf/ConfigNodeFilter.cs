/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Scripting.Expressions.Conf
{
  /// <summary>
  /// Filters expressions in the <see cref="IConfigNode"/> context
  /// </summary>
  public class ConfigNodeFilter : BoolFilter<IConfigSectionNode>
  {
    public Expression<IConfigSectionNode, bool> Condition { get; set; }

    /// <summary>
    /// Override to perform custom logic. Base implementation evaluates Tree if it is not null
    /// </summary>
    public override bool Evaluate(IConfigSectionNode context)
    {
      var condition = Condition;
      if (condition == null) return false;//FALSE by default
      return condition.Evaluate(context);
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      Condition = Make<Expression<IConfigSectionNode, bool>>(node, nameof(Condition));
    }
  }
}
