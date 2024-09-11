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

      /// <summary>
      /// Passing this header forces all DateTime values to be transmitted as UTC WITHOUT
      /// conversion, that is as if these dates were already UTC.
      /// This is needed to avoid any date conversion for local dates during transmission, as
      /// the logically "local" utc offset is only known at the receiving side and the same server needs to
      /// send dates in logically different times zones (which it does not know about) as-is.
      /// When callers pass this header, they already know that the "UTC" values coming back are
      /// really fake, and need to be re-coded AS LOCAL AS-OF SPECIFIC LOGICAL TIMEZONE on a CASE-BY-CASE basis
      /// </summary>
      public const string UTC_MASQUERADE = "utc-masquerade";
    }
  }
}