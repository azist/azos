/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Data.Common;

namespace MySql.Data.MySqlClient
{
	public sealed class MySqlClientFactory : DbProviderFactory
	{
		public static readonly MySqlClientFactory Instance = new MySqlClientFactory();

		private MySqlClientFactory()
		{
		}

		public override DbCommand CreateCommand()
			=> new MySqlCommand();

		public override DbConnection CreateConnection()
			=> new MySqlConnection();

		public override DbConnectionStringBuilder CreateConnectionStringBuilder()
			=> new MySqlConnectionStringBuilder();

		public override DbParameter CreateParameter()
			=> new MySqlParameter();
	}
}
