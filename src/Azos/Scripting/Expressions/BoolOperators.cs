/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Scripting.Expressions
{
  public class BoolAndOperator<TContext> : BinaryOperator<TContext, bool, bool, bool>
  {
    public override bool Evaluate(TContext context)
     => LeftOperand.Evaluate(context) && RightOperand.Evaluate(context);
  }

  public class BoolOrOperator<TContext> : BinaryOperator<TContext, bool, bool, bool>
  {
    public override bool Evaluate(TContext context)
     => LeftOperand.Evaluate(context) || RightOperand.Evaluate(context);
  }

  public class BoolXorOperator<TContext> : BinaryOperator<TContext, bool, bool, bool>
  {
    public override bool Evaluate(TContext context)
     => LeftOperand.Evaluate(context) ^ RightOperand.Evaluate(context);
  }

  public class BoolEqualsOperator<TContext> : BinaryOperator<TContext, bool, bool, bool>
  {
    public override bool Evaluate(TContext context)
     => LeftOperand.Evaluate(context) == RightOperand.Evaluate(context);
  }

  public class BoolNotEqualsOperator<TContext> : BinaryOperator<TContext, bool, bool, bool>
  {
    public override bool Evaluate(TContext context)
     => LeftOperand.Evaluate(context) != RightOperand.Evaluate(context);
  }

  public class BoolObjectEqualsOperator<TContext> : BinaryOperator<TContext, bool, object, object>
  {
    public override bool Evaluate(TContext context)
    {
      var left = LeftOperand.Evaluate(context);
      var right = RightOperand.Evaluate(context);
      if (left == null && right == null) return true;
      if (left == null || right == null) return false;

      return left.Equals(right);
    }
  }

  public class BoolObjectNotEqualsOperator<TContext> : BinaryOperator<TContext, bool, object, object>
  {
    public override bool Evaluate(TContext context)
    {
      var left = LeftOperand.Evaluate(context);
      var right = RightOperand.Evaluate(context);
      if (left == null && right == null) return false;
      if (left == null || right == null) return true;

      return !left.Equals(right);
    }
  }

  public class BoolNotOperator<TContext> : UnaryOperator<TContext, bool, bool>
  {
    public override bool Evaluate(TContext context)
      => !Operand.Evaluate(context);
  }

}
