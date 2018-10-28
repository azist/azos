/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace MySqlConnector.Core
{
	internal static class ServerVersions
	{
		// https://dev.mysql.com/doc/refman/5.7/en/mysql-reset-connection.html
		public static readonly Version SupportsResetConnection = new Version(5, 7, 3);

		// http://dev.mysql.com/doc/refman/5.5/en/parameters-table.html
		public static readonly Version SupportsProcedureCache = new Version(5, 5, 3);
	}
}
