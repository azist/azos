using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Azos.Data.AST;
using Azos.Scripting;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class ASTSqlTests
  {
    [Run]
    public void Test1()
    {
      var ast = new BinaryExpression
      {
        LeftOperand = new IdentifierExpression{  Identifier = "Name" },
        Operator = "=",
        RightOperand = new ValueExpression{  Value = "Kozlovich"}
      };

      ast.See("AST content");

      var xlat = new TeztXlat();
      var ctx = xlat.TranslateInContext(ast);
      ctx.SQL.See();
      ctx.Parameters.See();

      Aver.AreEqual("(T1.\"Name\" = ??P0)", ctx.SQL.ToString());
    }


    public class TeztXlat : SqlBaseXlat
    {

      public override SqlXlatContext TranslateInContext(Expression expression)
      {
        var result = new TeztContext(this);
        expression.Accept(result);
        return result;
      }
    }

    public class TeztContext : SqlXlatContext
    {
      public TeztContext(TeztXlat xlat) : base(xlat)
      {
      }

      private int m_ParamCount;

      protected override string ParameterOpenSpan => "??";

      protected override IDataParameter MakeAndAssignParameter(ValueExpression value)
      {
        var p = new TeztParam{ParameterName = "P{0}".Args(m_ParamCount++), Value = value.Value};
        return p;
      }
    }

    public class TeztParam : IDataParameter
    {
      public DbType DbType { get; set;}
      public ParameterDirection Direction { get ; set; }

      public bool IsNullable => true;

      public string ParameterName { get; set; }
      public string SourceColumn { get; set; }
      public DataRowVersion SourceVersion { get; set; }
      public object Value { get; set; }
    }

  }


}
