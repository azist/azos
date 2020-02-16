/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Scripting.Expressions
{
  public abstract class Expression : IExpression
  {
    public void Configure(IConfigSectionNode node)
    {
      if (node==null || !node.Exists) return;
      ConfigAttribute.Apply(this, node);
      DoConfigure(node);
    }

    protected virtual void DoConfigure(IConfigSectionNode node)
    {
    }

    public abstract object EvaluateObject(object context);
  }

  public abstract class Expression<TContext, TResult> : Expression
  {
    public sealed override object EvaluateObject(object context)
    => Evaluate((TContext)context);

    public abstract TResult Evaluate(TContext context);
  }


}
