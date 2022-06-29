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
    public abstract object Accept(XlatContext ctx);
  }


  /// <summary>
  /// Provides abstraction for operators
  /// </summary>
  public abstract class OperatorExpression : Expression
  {
    [Field(required: true, MinLength = 1, Description = "Operator, such as '==', '!=' etc...")]
    public string Operator { get; set; }
  }


  /// <summary>
  /// Represents a value, such as a constant literal
  /// </summary>
  public class ValueExpression : Expression
  {
    [Field(description: "Provides a value such as null, a string, scalar, or array/map")]
    public object Value { get; set; }

    [Field(description: "Provides an optional type hint for the value, typically type system CNAME(Canonical Name) for example a `utctime` hint may be used to decorate a string value to treat is as a UTC DateTime value")]
    public string TypeHint{  get; set; }

    public override object Accept(XlatContext ctx)
     => ctx.Visit(this);
  }


  /// <summary>
  /// Represents an array of values, such as the one used with IN operator in SQL
  /// </summary>
  public class ArrayValueExpression : Expression
  {
    [Field(description: "An array of ValueExpressions; as the ones used with IN in SQL etc")]
    public IEnumerable<ValueExpression> ArrayValue { get; set; }

    public override object Accept(XlatContext ctx)
     => ctx.Visit(this);
  }


  /// <summary>
  /// Represents an identifier such as column/field name
  /// </summary>
  public class IdentifierExpression : Expression
  {
    [Field(required: true, MinLength = 1, Description = "Represents an identifier, such as a column/field name")]
    public string Identifier { get; set; }

    public override object Accept(XlatContext ctx)
     => ctx.Visit(this);
  }


  /// <summary>
  /// Represents an operator that has a single operand, e.g. a negation or "not" operator
  /// </summary>
  public class UnaryExpression : OperatorExpression
  {
    [Field(required: true, Description = "A single operand of the unary operator")]
    public Expression Operand { get; set; }

    public override object Accept(XlatContext ctx) => ctx.Visit(this);
  }


  /// <summary>
  /// Represents an operator that has two operands: left and right
  /// </summary>
  public class BinaryExpression : OperatorExpression
  {
    [Field(required: true, Description = "Left operand of the binary operator")]
    public Expression LeftOperand {  get; set; }

    [Field(required: true, Description = "Right operand of the binary operator")]
    public Expression RightOperand { get; set; }

    public override object Accept(XlatContext ctx) => ctx.Visit(this);
  }

}
