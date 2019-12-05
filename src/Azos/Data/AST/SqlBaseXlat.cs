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
      }
    }

    public override IEnumerable<string> BinaryOperators
    {
      get
      {
        yield return "and";
        yield return "or";
        yield return "=";
        yield return "!=";
        yield return "<";
        yield return ">";
        yield return "<=";
        yield return ">=";
        yield return "like";
      }
    }

  }

  public abstract class SqlXlatContext : XlatContext<SqlBaseXlat>
  {
    protected SqlXlatContext(SqlBaseXlat xlat) : base(xlat)
    {
    }

    private StringBuilder m_Sql = new StringBuilder(512);
    private List<IDataParameter> m_Parameters = new List<IDataParameter>(16);


    /// <summary>
    /// Returns built SQL
    /// </summary>
    public string SQL => m_Sql.ToString();

    /// <summary>
    /// Returns list of parameters
    /// </summary>
    public IEnumerable<IDataParameter> Parameters => m_Parameters;

    protected abstract IDataParameter MakeParameter(ValueExpression value);
    protected virtual string NullLiteral => "NULL";
    protected virtual string MasterAlias => "T1";
    protected virtual char IdentifierQuote => '"';

    protected virtual string MapUnaryOperator(string oper) => oper.ToUpperInvariant();
    protected virtual string MapBinaryOperator(string oper, bool rhsNull) => rhsNull ? "IS" :  oper.ToUpperInvariant();


    public override void Visit(ValueExpression value)
    {
      if (value.Value==null)
      {
        m_Sql.Append(NullLiteral);
        return;
      }

      var p = MakeParameter(value);
      m_Sql.Append(p);
    }

    public override void Visit(IdentifierExpression id)
    {
      m_Sql.Append(MasterAlias);
      m_Sql.Append('.');
      m_Sql.Append(IdentifierQuote);
      m_Sql.Append(id.Identifier);
      m_Sql.Append(IdentifierQuote);
    }

    public override void Visit(UnaryExpression unary)
    {
      m_Sql.Append("(");

      //check to see that operators are in the accepted range
      m_Sql.Append(MapUnaryOperator(unary.Operator));

      unary.Operand.Accept(this);

      m_Sql.Append(")");
    }

    public override void Visit(BinaryExpression binary)
    {
      m_Sql.Append("(");

      binary.LeftOperand.Accept(this);

      m_Sql.Append(" ");


      var isNull = binary.RightOperand == null || binary.RightOperand is ValueExpression ve && ve.Value == null;

      //check to see that operators are in the accepted range
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
