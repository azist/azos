/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Data.Common;

namespace MySql.Data.MySqlClient
{
	public sealed class MySqlException : DbException
	{
		public int Number { get; }
		public string SqlState { get; }

		internal MySqlException(string message)
			: this(message, null)
		{
		}

		internal MySqlException(string message, Exception innerException)
			: this(0, null, message, innerException)
		{
		}

		internal MySqlException(int errorNumber, string sqlState, string message)
			: this(errorNumber, sqlState, message, null)
		{
		}

		internal MySqlException(int errorNumber, string sqlState, string message, Exception innerException)
			: base(message, innerException)
		{
			Number = errorNumber;
			SqlState = sqlState;
		}

		internal static MySqlException CreateForTimeout() => CreateForTimeout(null);

		internal static MySqlException CreateForTimeout(Exception innerException) =>
			new MySqlException((int) MySqlErrorCode.CommandTimeoutExpired, null, "The Command Timeout expired before the operation completed.");
	}
}
