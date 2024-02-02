/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Conf;

namespace Azos.Scripting.Expressions.Data
{
  /// <summary>
  /// Performs value conversion using Convert.ChangeType API
  /// </summary>
  public class If : Expression<ScriptCtx, object>
  {
    public Expression<ScriptCtx, bool> Condition { get; set; }
    public Expression Then { get; set; }
    public Expression Else { get; set; }

    public override object Evaluate(ScriptCtx context)
    {
      Condition.NonNull(nameof(Condition));
      Then.NonNull(nameof(Then));
      Else.NonNull(nameof(Else));
      return Condition.Evaluate(context) ? Then.EvaluateObject(context) : Else.EvaluateObject(context);
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      Condition = Make<Expression<ScriptCtx, bool>>(node, nameof(Condition));
      Then = Make<Expression>(node, nameof(Then));
      Else = Make<Expression>(node, nameof(Else));
    }
  }

  /// <summary>
  /// Performs block call with exception handling, returning an optional value on error
  /// </summary>
  public class Guard : UnaryOperator<ScriptCtx, object, object>
  {
    public Expression OnError { get; set; }

    public override object Evaluate(ScriptCtx context)
    {
      Operand.NonNull(nameof(Operand));

      try
      {
        return Operand.Evaluate(context);
      }
      catch(Exception error)
      {
        context.SetError(error);
        return OnError.EvaluateObject(context);
      }
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      OnError = Make<Expression>(node, nameof(OnError));
    }
  }

  /// <summary>
  /// Returns current error or null
  /// </summary>
  public class Error : Expression<ScriptCtx, object>
  {
    public override object Evaluate(ScriptCtx context) => context.NonNull(nameof(context)).Error;
  }

}
