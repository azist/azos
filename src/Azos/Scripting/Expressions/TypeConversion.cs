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
  /// Performs a type cast of TOperand into TResult
  /// </summary>
  public abstract class TypeCast<TContext, TResult, TOperand> : UnaryOperator<TContext, TResult, TOperand>
  {
    public sealed override TResult Evaluate(TContext context)
     => (TResult)(object)Operand.NonNull(nameof(Operand)).Evaluate(context);
  }


}
