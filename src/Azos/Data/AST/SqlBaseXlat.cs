using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Azos.Data.AST
{
  /// <summary>
  /// Implements basic expression translation for Sql-based databases by
  /// generating Sql and parameters
  /// </summary>
  public abstract class SqlBaseXlat : Xlat<SqlXlatContext>
  {
    public override bool IsCaseSensitive => throw new NotImplementedException();

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
    /// Returns list of parameters
    /// </summary>
    public IEnumerable<IDataParameter> Parameters => m_Parameters;

    /// <summary>
    /// Override to create a parameter, assigning (and converting) value.
    /// You can throw ASTException if some unsupported value was passed-in
    /// </summary>
    protected abstract IDataParameter MakeAndAssignParameter(ValueExpression value);

    protected virtual string NullLiteral => "NULL";
    protected virtual string MasterAlias => "T1";
    protected virtual char IdentifierQuote => '"';
    protected abstract string ParameterOpenSpan{ get;}
    protected virtual string ParameterCloseSpan => "";

    protected virtual string MapUnaryOperator(string oper) => oper.ToUpperInvariant();
    protected virtual string MapBinaryOperator(string oper, bool rhsNull) => rhsNull ? "IS" :  oper.ToUpperInvariant();


    protected static readonly HashSet<Type> DEFAULT_PRIMITIVE_TYPES = new HashSet<Type>{ typeof(byte), typeof(short), typeof(int), typeof(long) };

    protected virtual bool HandlePrimitiveValue(ValueExpression expr)
    {
      var tv = expr.Value.GetType();
      return DEFAULT_PRIMITIVE_TYPES.Contains(tv);
    }

    /// <summary>
    /// Assigns an optional functor which gets called during identifier validation,
    /// this is handy in many business-specific cases to
    /// limit the column names without having to override this class
    /// </summary>
    public Func<IdentifierExpression, bool> IdentifierFilter { get; set;}


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

      if (array.Value == null)
      {
        m_Sql.Append(NullLiteral);
        return;
      }

      m_Sql.Append('(');
      var first = true;

      foreach(var elm in array.Value)
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
      var f = IdentifierFilter;
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
