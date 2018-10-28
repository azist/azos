/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace MySqlConnector.Logging
{
	/// <summary>
	/// <see cref="NoOpLogger"/> is an implementation of <see cref="IMySqlConnectorLogger"/> that does nothing.
	/// </summary>
	/// <remarks>This is the default logging implementation unless <see cref="MySqlConnectorLogManager.Provider"/> is set.</remarks>
	public sealed class NoOpLogger : IMySqlConnectorLogger
	{
		/// <summary>
		/// Returns <c>false</c>.
		/// </summary>
		public bool IsEnabled(MySqlConnectorLogLevel level) => false;

		/// <summary>
		/// Ignores the specified log message.
		/// </summary>
		public void Log(MySqlConnectorLogLevel level, string message, object[] args = null, Exception exception = null)
		{
		}

		/// <summary>
		/// Returns a singleton instance of <see cref="NoOpLogger"/>.
		/// </summary>
		public static IMySqlConnectorLogger Instance { get; } = new NoOpLogger();
	}
}
