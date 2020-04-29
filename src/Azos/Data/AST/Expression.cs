/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

namespace Azos.Data.AST
{
  /// <summary>
  /// Provides general ancestor for abstract syntax tree nodes
  /// </summary>
  [ExpressionJsonHandler]
  public abstract class Expression : AmorphousTypedDoc
  {
    /// <summary>
    /// Implements a visitor pattern in XlatContext (translation context)
    /// </summary>
    public abstract void Accept(XlatContext ctx);
  }

  /// <summary>
  /// Provides abstraction for operators
  /// </summary>
  public abstract class OperatorExpression : Expression
  {
    [Field(required: true, MinLength = 1)]
    public string Operator { get; set; }
  }


  /// <summary>
  /// Represents a value, such as a constant literal
  /// </summary>
  public class ValueExpression : Expression
  {
    [Field]
    public object Value { get; set; }//may contain null, json array or json data map as-is

    public override void Accept(XlatContext ctx)
     => ctx.Visit(this);
  }

  /// <summary>
  /// Represents an array of values, such as the one used with IN operator in SQL
  /// </summary>
  public class ArrayValueExpression : Expression
  {
    [Field]
    public IEnumerable<ValueExpression> ArrayValue { get; set; }

    public override void Accept(XlatContext ctx)
     => ctx.Visit(this);
  }

  /// <summary>
  /// Represents an identifier such as column/field name
  /// </summary>
  public class IdentifierExpression : Expression
  {
    [Field(required: true, MinLength = 1)]
    public string Identifier { get; set; }

    public override void Accept(XlatContext ctx)
     => ctx.Visit(this);
  }


  /// <summary>
  /// Represents an operator that has a single operand, e.g. a negation or "not" operator
  /// </summary>
  public class UnaryExpression : OperatorExpression
  {
    [Field(required: true)]
    public Expression Operand { get; set; }

    public override void Accept(XlatContext ctx) => ctx.Visit(this);
  }

  /// <summary>
  /// Represents an operator that has two operands: left and right
  /// </summary>
  public class BinaryExpression : OperatorExpression
  {
    [Field(required: true)] public Expression LeftOperand {  get; set; }
    [Field(required: true)] public Expression RightOperand { get; set; }

    public override void Accept(XlatContext ctx) => ctx.Visit(this);
  }


}
