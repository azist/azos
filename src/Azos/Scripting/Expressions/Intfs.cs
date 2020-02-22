/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Scripting.Expressions
{
  /// <summary>
  /// Denotes an abstract expression, which can possibly be an operator.
  /// You can access expressions polymorphically using EvaludateObject()
  /// </summary>
  /// <remarks>
  /// This technology is very efficient as it is based on a type-safe expressions and static compilation
  /// with dynamic composition (e.g. via configuration).
  /// A typical filter expression tree (e.g. in log filtering) with 2-3 nodes yields multi
  /// million ops/sec performance on a single thread.
  /// As there is no locking and no global state, performance scales linearly on multiple threads.
  /// The expression evaluation implementation must be thread-safe (e.g. take all args in local variables in calling method)
  /// </remarks>
  public interface IExpression : IConfigurable
  {
    /// <summary>
    /// Evaluates the expression in object context. The context has to be assignment-compatible
    /// with the underlying concrete type (which depends on reifing type in case of generic implementation)
    /// </summary>
    object EvaluateObject(object context);
  }

  /// <summary>
  /// Denotes a unary operator expression with a single polymorphic operand
  /// </summary>
  public interface IUnaryOperator : IExpression
  {
    /// <summary>
    /// Provides polymorphic access to a single operand of a unary operator expression
    /// </summary>
    IExpression Operand { get; }
  }

  /// <summary>
  /// Denotes a binary operator expression with two polymorphic operands
  /// </summary>
  public interface IBinaryOperator : IExpression
  {
    /// <summary>
    /// Provides polymorphic access to the left operand of a binary operator expression
    /// </summary>
    IExpression LeftOperand { get;}

    /// <summary>
    /// Provides polymorphic access to the right operand of a binary operator expression
    /// </summary>
    IExpression RightOperand { get; }
  }

}
