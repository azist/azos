/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Implements MySqlCursor
  /// </summary>
  public sealed class MySqlCursor : Cursor
  {
    internal MySqlCursor(MySqlCRUDQueryExecutionContext context,
                         MySqlCommand command,
                         MySqlDataReader reader,
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
    private MySqlCommand m_Command;
    private MySqlDataReader m_Reader;
  }
}
