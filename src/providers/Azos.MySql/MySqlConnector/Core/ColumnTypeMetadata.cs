/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using MySql.Data.MySqlClient;

namespace MySqlConnector.Core
{
	internal sealed class ColumnTypeMetadata
	{
		public static string CreateLookupKey(string columnTypeName, bool isUnsigned, int length) => $"{columnTypeName}|{(isUnsigned ? "u" : "s")}|{length}";

		public ColumnTypeMetadata(string dataTypeName, DbTypeMapping dbTypeMapping, MySqlDbType mySqlDbType, bool isUnsigned = false, bool binary = false, int length = 0, string simpleDataTypeName = null, string createFormat = null, long columnSize = 0)
		{
			DataTypeName = dataTypeName;
			SimpleDataTypeName = simpleDataTypeName ?? dataTypeName;
			CreateFormat = createFormat ?? (dataTypeName + (isUnsigned ? " UNSIGNED" : ""));
			DbTypeMapping = dbTypeMapping;
			MySqlDbType = mySqlDbType;
			ColumnSize = columnSize;
			IsUnsigned = isUnsigned;
			Binary = binary;
			Length = length;
		}

		public string DataTypeName { get; }
		public string SimpleDataTypeName { get; }
		public string CreateFormat { get; }
		public DbTypeMapping DbTypeMapping { get; }
		public MySqlDbType MySqlDbType { get; }
		public bool Binary { get; }
		public long ColumnSize { get; }
		public bool IsUnsigned { get; }
		public int Length { get; }

		public string CreateLookupKey() => CreateLookupKey(DataTypeName, IsUnsigned, Length);
	}
}
