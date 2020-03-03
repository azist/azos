/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Conf;
using Azos.Data;

namespace Azos.Scripting.Expressions
{
  /// <summary>
  /// Implements a type-safe binary operator with the concrete left and right operand types
  /// </summary>
  public abstract class BinaryOperator<TContext, TResult, TLeftOperand, TRightOperand> : Expression<TContext, TResult>, IBinaryOperator
  {
    public const string CONFIG_LEFT_SECT = "left";
    public const string CONFIG_RIGHT_SECT = "right";

    public Expression<TContext, TLeftOperand> LeftOperand { get; set; }
    public Expression<TContext, TRightOperand> RightOperand { get; set; }

    IExpression IBinaryOperator.LeftOperand => LeftOperand;
    IExpression IBinaryOperator.RightOperand => RightOperand;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      LeftOperand = Make<Expression<TContext, TLeftOperand>>(node, CONFIG_LEFT_SECT);
      RightOperand = Make<Expression<TContext, TRightOperand>>(node, CONFIG_RIGHT_SECT);
    }
  }

  /// <summary>
  /// Implements a type-safe unary operator of a concrete operand type
  /// </summary>
  public abstract class UnaryOperator<TContext, TResult, TOperand> : Expression<TContext, TResult>, IUnaryOperator
  {
    public const string CONFIG_OPERAND_SECT = "operand";

    public Expression<TContext, TOperand> Operand { get; set; }
    IExpression IUnaryOperator.Operand => Operand;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      Operand = Make<Expression<TContext, TOperand>>(node, CONFIG_OPERAND_SECT);
    }
  }
}
