/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Scripting.Expressions.Conf
{
  /// <summary>
  /// Performs pattern set search on value gotten form config node at the specified path
  /// </summary>
  public sealed class ValuePattern : PatternSetFilter<IConfigSectionNode>
  {
    /// <summary>
    /// A relative path to get a value from. The value is then tested against the pattern set filter
    /// </summary>
    public string ValueExpression { get; set; }

    /// <summary>
    /// Keep processing until all vars evaluated recursively
    /// </summary>
    public bool Recurse { get; set; }

    protected override bool Default => false;

    protected override string GetValue(IConfigSectionNode context) => context?.EvaluateValueVariables(ValueExpression, Recurse);

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      ValueExpression = node.Of("expr", "val", "value").VerbatimValue;//Notice VERBATIM specifier - we do NOT want to evaluate the expression
    }
  }
}
