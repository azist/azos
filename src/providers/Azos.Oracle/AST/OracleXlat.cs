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

    protected override IDataParameter MakeParameter(ValueExpression value)
    {
      throw new NotImplementedException();
    }
  }
}
