/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Data;

using MySqlConnector;

using Azos.Conf;

namespace Azos.Data.AST
{
  /// <summary>
  /// Defines SqlTranslation for MySql database
  /// </summary>
  public class MySqlXlat : SqlBaseXlat
  {
    public MySqlXlat(string name = null) : base(name) { }
    public MySqlXlat(IConfigSectionNode conf) : base(conf) { }

    /// <summary>
    /// Translates root AST expression for MySql RDBMS
    /// </summary>
    public override SqlXlatContext TranslateInContext(Expression expression)
    {
      var result = new MySqlXlatContext(this);
      expression.Accept(result);
      return result;
    }
  }

  /// <summary>
  /// Defines Xlat context specific to MySql RDBMS
  /// </summary>
  public class MySqlXlatContext : SqlXlatContext
  {
    public MySqlXlatContext(MySqlXlat xlat) : base(xlat)
    {
    }

    private int m_ParamCount;

    /// <summary>
    /// MySql parameters start with `?`
    /// </summary>
    protected override string ParameterOpenSpan => ":";

    /// <summary>
    /// Override to provide custom name for MySql parameter prefix, the default is `MYSQLXLATP_`
    /// </summary>
    protected virtual string ParameterNamePrefix => "MYSQLXLATP_";

    /// <summary>
    /// Creates MySql specific parameter
    /// </summary>
    protected override IDataParameter MakeAndAssignParameter(ValueExpression value)
    {
      var p = new MySqlParameter($"{ParameterNamePrefix}{m_ParamCount++}", value.Value);
      return p;
    }
  }
}
