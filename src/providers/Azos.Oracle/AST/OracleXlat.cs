using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Azos.Data.AST
{
  public class OracleXlat : SqlBaseXlat
  {

    public override SqlXlatContext TranslateInContext(Expression expression)
    {
      var result = new OracleXlatContext(this);
      expression.Accept(result);
      return result;
    }
  }

  public class OracleXlatContext : SqlXlatContext
  {
    public OracleXlatContext(OracleXlat xlat) : base(xlat)
    {
    }

    private int m_ParamCount;

    protected override string ParameterOpenSpan => "";

    protected override IDataParameter MakeAndAssignParameter(ValueExpression value)
    {
      var p = new Oracle.ManagedDataAccess.Client.OracleParameter("@P{0}".Args(m_ParamCount++), value.Value);
      return p;
    }
  }
}
