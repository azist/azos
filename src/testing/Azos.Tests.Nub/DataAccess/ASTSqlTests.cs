/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Azos.Data;
using Azos.Data.AST;
using Azos.Scripting;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class ASTSqlTests
  {
    [Run]
    public void SimpleBinary()
    {
      var ast = new BinaryExpression
      {
        LeftOperand = new IdentifierExpression { Identifier = "Name" },
        Operator = "=",
        RightOperand = new ValueExpression { Value = "Kozlovich" }
      };

      ast.See("AST content");

      var xlat = new TeztXlat();
      var ctx = xlat.TranslateInContext(ast);
      ctx.SQL.See();
      ctx.Parameters.See();

      Aver.AreEqual("(T1.\"Name\" = ??P0)", ctx.SQL.ToString());
      Aver.AreEqual("P0", ctx.Parameters.First().ParameterName);
      Aver.AreEqual("Kozlovich", ctx.Parameters.First().Value.AsString());
    }


    [Run]
    public void SimpleBinaryWithUnary()
    {
      var ast = new BinaryExpression
      {
        LeftOperand = new IdentifierExpression{  Identifier = "Latitude" },
        Operator = ">=",
        RightOperand = new UnaryExpression{ Operator = "-", Operand = new ValueExpression{  Value = 22.3d}}
      };

      ast.See("AST content");

      var xlat = new TeztXlat();
      var ctx = xlat.TranslateInContext(ast);
      ctx.SQL.See();
      ctx.Parameters.See();

      Aver.AreEqual("(T1.\"Latitude\" >= (-??P0))", ctx.SQL.ToString());
      Aver.AreEqual("P0", ctx.Parameters.First().ParameterName);
      Aver.AreEqual(22.3d, ctx.Parameters.First().Value.AsDouble());
    }

    [Run]
    public void SimpleBinaryWithUnaryPrimitive()
    {
      var ast = new BinaryExpression
      {
        LeftOperand = new IdentifierExpression { Identifier = "Latitude" },
        Operator = ">=",
        RightOperand = new UnaryExpression { Operator = "-", Operand = new ValueExpression { Value = 1080 } }
      };

      ast.See("AST content");

      var xlat = new TeztXlat();
      var ctx = xlat.TranslateInContext(ast);
      ctx.SQL.See();

      Aver.AreEqual("(T1.\"Latitude\" >= (-1080))", ctx.SQL.ToString());
      Aver.IsFalse(ctx.Parameters.Any());
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
