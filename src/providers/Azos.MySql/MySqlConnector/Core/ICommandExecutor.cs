/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Core
{
	internal interface ICommandExecutor
	{
		Task<int> ExecuteNonQueryAsync(string commandText, MySqlParameterCollection parameterCollection, IOBehavior ioBehavior, CancellationToken cancellationToken);

		Task<object> ExecuteScalarAsync(string commandText, MySqlParameterCollection parameterCollection, IOBehavior ioBehavior, CancellationToken cancellationToken);

		Task<DbDataReader> ExecuteReaderAsync(string commandText, MySqlParameterCollection parameterCollection, CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken);
	}
}
