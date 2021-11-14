/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Data.Access.Rpc
{
  /// <summary>
  /// Declares standard headers and their values for Commands, for example certain
  /// Sql-based implementations of <see cref="IRpcHandler"/> need to know whether supplied <see cref="Command.Text"/>
  /// is a Sql text block or stored procedure name
  /// </summary>
  public static class StandardHeaders
  {
    /// <summary>
    /// Headers related to RDBMS/SQL technologies
    /// </summary>
    public static class Sql
    {
      public const string COMMAND_TYPE = "sql-command-type";
      public const string COMMAND_TYPE_VALUE_TEXT = "text";
      public const string COMMAND_TYPE_VALUE_PROC = "proc";
    }
  }
}
