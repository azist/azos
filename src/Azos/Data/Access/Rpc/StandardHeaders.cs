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
    /// If passed, suppresses result rowset schema object, false by default = include schema
    /// </summary>
    public const string NO_SCHEMA = "no-schema";

    /// <summary>
    /// If true, serializes rows as map with field name, otherwise as an array (default)
    /// </summary>
    public const string ROWS_AS_MAP = "rows-as-map";

    /// <summary>
    /// If true, makes result look pretty by using line breaks and spacing, default is false
    /// </summary>
    public const string PRETTY = "pretty";


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