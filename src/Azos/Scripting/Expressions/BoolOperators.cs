/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Scripting.Expressions
{
  public class BoolAnd<TContext> : BinaryOperator<TContext, bool, bool, bool>
  {
    public override bool Evaluate(TContext context)
    {
      var left = LeftOperand.NonNull(nameof(LeftOperand));
      var right = RightOperand.NonNull(nameof(RightOperand));
      return left.Evaluate(context) && right.Evaluate(context);
    }
  }

  public class BoolOr<TContext> : BinaryOperator<TContext, bool, bool, bool>
  {
    public override bool Evaluate(TContext context)
    {
      var left = LeftOperand.NonNull(nameof(LeftOperand));
      var right = RightOperand.NonNull(nameof(RightOperand));
      return left.Evaluate(context) || right.Evaluate(context);
    }
  }

  public class BoolXor<TContext> : BinaryOperator<TContext, bool, bool, bool>
  {
    public override bool Evaluate(TContext context)
    {
      var left = LeftOperand.NonNull(nameof(LeftOperand));
      var right = RightOperand.NonNull(nameof(RightOperand));
      return left.Evaluate(context) ^ right.Evaluate(context);
    }
  }

  public class BoolEquals<TContext> : BinaryOperator<TContext, bool, bool, bool>
  {
    public override bool Evaluate(TContext context)
    {
      var left = LeftOperand.NonNull(nameof(LeftOperand));
      var right = RightOperand.NonNull(nameof(RightOperand));
      return left.Evaluate(context) == right.Evaluate(context);
    }
  }

  public class BoolNotEquals<TContext> : BinaryOperator<TContext, bool, bool, bool>
  {
    public override bool Evaluate(TContext context)
    {
      var left = LeftOperand.NonNull(nameof(LeftOperand));
      var right = RightOperand.NonNull(nameof(RightOperand));
      return left.Evaluate(context) != right.Evaluate(context);
    }
  }

  public class BoolObjectEquals<TContext> : BinaryOperator<TContext, bool, object, object>
  {
    public override bool Evaluate(TContext context)
    {
      var left = LeftOperand.NonNull(nameof(LeftOperand));
      var right = RightOperand.NonNull(nameof(RightOperand));

      var lv = left.Evaluate(context);
      var rv = right.Evaluate(context);
      if (lv == null && rv == null) return true;
      if (lv == null || rv == null) return false;

      return lv.Equals(rv);
    }
  }

  public class BoolObjectNotEquals<TContext> : BinaryOperator<TContext, bool, object, object>
  {
    public override bool Evaluate(TContext context)
    {
      var left = LeftOperand.NonNull(nameof(LeftOperand));
      var right = RightOperand.NonNull(nameof(RightOperand));

      var lv = left.Evaluate(context);
      var rv = right.Evaluate(context);
      if (lv == null && rv == null) return true;
      if (lv == null || rv == null) return false;

      return !lv.Equals(rv);
    }
  }

  public class BoolNot<TContext> : UnaryOperator<TContext, bool, bool>
  {
    public override bool Evaluate(TContext context)
      => !Operand.NonNull(nameof(Operand)).Evaluate(context);
  }

}
