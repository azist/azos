
using System.Collections.Generic;

using System.Data.SqlClient;

namespace Azos.Data.Access.MsSql
{
  public sealed class MsSqlCursor : Cursor
  {
    internal MsSqlCursor(MySqlCRUDQueryExecutionContext context,
                         SqlCommand command,
                         SqlDataReader reader,
                         IEnumerable<Doc> source) : base(source)
    {
      m_Context = context;
      m_Command = command;
      m_Reader = reader;
    }

    protected override void Destructor()
    {
      base.Destructor();

      DisposableObject.DisposeAndNull(ref m_Reader);
      DisposableObject.DisposeAndNull(ref m_Command);

      if (m_Context.Transaction==null)
        m_Context.Connection.Dispose();
    }

    private MySqlCRUDQueryExecutionContext m_Context;
    private SqlCommand m_Command;
    private SqlDataReader m_Reader;
  }
}
