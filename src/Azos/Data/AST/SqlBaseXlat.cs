/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Azos.Conf;

namespace Azos.Data.AST
{
  /// <summary>
  /// Implements basic expression translation for Sql-based databases by
  /// generating Sql and parameters
  /// </summary>
  public abstract class SqlBaseXlat : Xlat<SqlXlatContext>
  {
    protected SqlBaseXlat(string name = null) : base(name) { }
    protected SqlBaseXlat(IConfigSectionNode conf): base(conf) { }

#pragma warning disable 0649
    [Config] private bool m_IsCaseSensitive;
#pragma warning restore 0649

    public override bool IsCaseSensitive => m_IsCaseSensitive;

    /// <summary>
    /// Assigns an optional functor which gets called during identifier validation,
    /// this is handy in many business-specific cases to
    /// limit the column names without having to override this class
    /// </summary>
    public Func<IdentifierExpression, bool> IdentifierFilter { get; set; }

    /// <summary>
    /// Returns a stream of supported unary operators
    /// </summary>
    public override IEnumerable<string> UnaryOperators
    {
      get
      {
        yield return "not";
        yield return "in";
        yield return "exists";
        yield return "+";
        yield return "-";
      }
    }

    /// <summary>
    /// Returns a stream of supported binary operators
    /// </summary>
    public override IEnumerable<string> BinaryOperators
    {
      get
      {
        yield return "and";
        yield return "or";
        yield return "=";
        yield return "<>";
        yield return "<";
        yield return ">";
        yield return "<=";
        yield return ">=";
        yield return "like";

        yield return "-";
        yield return "+";
        yield return "*";
        yield return "/";
        yield return "%";
      }
    }

  }



  /// <summary>
  /// Provides abstraction for sql-based translation contexts
  /// </summary>
  public abstract class SqlXlatContext : XlatContext<SqlBaseXlat>
  {
    protected SqlXlatContext(SqlBaseXlat xlat) : base(xlat)
    {
    }

    protected StringBuilder m_Sql = new StringBuilder(512);
    private List<IDataParameter> m_Parameters = new List<IDataParameter>(16);


    /// <summary>
    /// Returns built SQL
    /// </summary>
    public string SQL => m_Sql.ToString();

    /// <summary>
    /// Returns list of SQL parameters created during translation
    /// </summary>
    public IEnumerable<IDataParameter> Parameters => m_Parameters;

    /// <summary>
    /// Override to create a parameter, assigning (and converting) value.
    /// You can throw ASTException if some unsupported value was passed-in
    /// </summary>
    protected abstract IDataParameter MakeAndAssignParameter(ValueExpression value);

    /// <summary>
    /// Null literal
    /// </summary>
    protected virtual string NullLiteral => "NULL";

    /// <summary>
    /// Master target object(table) alias, `T1` is used by default
    /// </summary>
    protected virtual string MasterAlias => "T1";

    /// <summary>
    /// Identifier quote, default tis doublequote
    /// </summary>
    protected virtual char IdentifierQuote => '"';

    /// <summary>
    /// Parameter open span, such as `@` for msSQL
    /// </summary>
    protected abstract string ParameterOpenSpan{ get; }

    /// <summary>
    /// Parameter close span, most databases do not use it
    /// </summary>
    protected virtual string ParameterCloseSpan => "";

    /// <summary>
    /// Maps unary operator e.g. "not" -> "NOT"
    /// </summary>
    protected virtual string MapUnaryOperator(string oper) => oper.ToUpperInvariant();

    /// <summary>
    /// Maps binary operator, if right hand side is null then emits IS NULL by default
    /// </summary>
    protected virtual string MapBinaryOperator(string oper, bool rhsNull) => rhsNull ? "IS" :  oper.ToUpperInvariant();

    /// <summary>
    /// Constant set of types qualified as "primitive" which are inlined in SQL and do not generate parameters
    /// </summary>
    protected static readonly HashSet<Type> DEFAULT_PRIMITIVE_TYPES = new HashSet<Type>{ typeof(byte), typeof(short), typeof(int), typeof(long) };

    /// <summary>
    /// Returns true if the ValueExpression represents a primitive
    /// </summary>
    protected virtual bool HandlePrimitiveValue(ValueExpression expr)
    {
      var tv = expr.Value.GetType();
      return DEFAULT_PRIMITIVE_TYPES.Contains(tv);
    }


    public override void Visit(ValueExpression value)
    {
      value.NonNull(nameof(value));

      if (value.Value==null)
      {
        m_Sql.Append(NullLiteral);
        return;
      }

      if (HandlePrimitiveValue(value))
      {
        m_Sql.Append(value.Value.ToString());
        return;
      }

      var p = MakeAndAssignParameter(value);
      m_Sql.Append(ParameterOpenSpan);
      m_Sql.Append(p.ParameterName);
      m_Sql.Append(ParameterCloseSpan);
      m_Parameters.Add(p);
    }

    public override void Visit(ArrayValueExpression array)
    {
      array.NonNull(nameof(array));

      if (array.ArrayValue == null)
      {
        m_Sql.Append(NullLiteral);
        return;
      }

      m_Sql.Append('(');
      var first = true;

      foreach(var elm in array.ArrayValue)
      {
        if (!first) m_Sql.Append(',');

        if (elm==null)
          m_Sql.Append(NullLiteral);
        else
          elm.Accept(this);

        first = false;
      }

      m_Sql.Append(')');
    }

    public override void Visit(IdentifierExpression id)
    {
      id.NonNull(nameof(id)).Identifier.NonBlank(nameof(id.Identifier));


      //check that id is accepted
      var f = Translator.IdentifierFilter;
      if (f!=null && !f(id)) throw new ASTException(StringConsts.AST_BAD_IDENTIFIER_ERROR.Args(id.Identifier));

      m_Sql.Append(MasterAlias);
      m_Sql.Append('.');
      m_Sql.Append(IdentifierQuote);
      m_Sql.Append(id.Identifier);
      m_Sql.Append(IdentifierQuote);
    }

    public override void Visit(UnaryExpression unary)
    {
      unary.NonNull(nameof(unary));

      m_Sql.Append("(");


      if (!unary.Operator.NonBlank(nameof(unary.Operator)).IsOneOf(Translator.UnaryOperators))
        throw new ASTException(StringConsts.AST_UNSUPPORTED_UNARY_OPERATOR_ERROR.Args(unary.Operator));

      m_Sql.Append(MapUnaryOperator(unary.Operator));

      unary.Operand.Accept(this);

      m_Sql.Append(")");
    }

    public override void Visit(BinaryExpression binary)
    {
      binary.NonNull(nameof(binary));

      m_Sql.Append("(");

      binary.LeftOperand.Accept(this);

      m_Sql.Append(" ");


      var isNull = binary.RightOperand == null || binary.RightOperand is ValueExpression ve && ve.Value == null;

      if (!binary.Operator.NonBlank(nameof(binary.Operator)).IsOneOf(Translator.BinaryOperators))
        throw new ASTException(StringConsts.AST_UNSUPPORTED_BINARY_OPERATOR_ERROR.Args(binary.Operator));

      m_Sql.Append(MapBinaryOperator(binary.Operator, isNull));

      m_Sql.Append(" ");

      if (isNull)
        m_Sql.Append(NullLiteral);
      else
        binary.RightOperand.Accept(this);

      m_Sql.Append(")");
    }
  }
}
