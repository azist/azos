using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Azos.Conf;

namespace Azos.Data.AST
{
  /// <summary>
  /// Defines SqlTranslation for ORACLE database
  /// </summary>
  public class OracleXlat : SqlBaseXlat
  {
    public OracleXlat(string name = null) : base(name) { }
    public OracleXlat(IConfigSectionNode conf) : base(conf) { }

    /// <summary>
    /// Translates root AST expression for ORACLE RDBMS
    /// </summary>
    public override SqlXlatContext TranslateInContext(Expression expression)
    {
      var result = new OracleXlatContext(this);
      expression.Accept(result);
      return result;
    }
  }

  /// <summary>
  /// Defines Xlat context specific to ORACLE RDBMS
  /// </summary>
  public class OracleXlatContext : SqlXlatContext
  {
    public OracleXlatContext(OracleXlat xlat) : base(xlat)
    {
    }

    private int m_ParamCount;

    /// <summary>
    /// Oracle parameters start with `:`
    /// </summary>
    protected override string ParameterOpenSpan => ":";

    /// <summary>
    /// Override to provide custom name for ORACLE parameter prefix, the default is `ORAXLATP_`
    /// </summary>
    protected virtual string ParameterNamePrefix => "ORAXLATP_";

    /// <summary>
    /// Creates ORA specific parameter
    /// </summary>
    protected override IDataParameter MakeAndAssignParameter(ValueExpression value)
    {
      var p = new Oracle.ManagedDataAccess.Client.OracleParameter($"{ParameterNamePrefix}{m_ParamCount++}", value.Value);
      return p;
    }
  }
}
