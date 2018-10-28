/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace MySqlConnector.Logging
{
	/// <summary>
	/// Creates loggers that do nothing.
	/// </summary>
	public sealed class NoOpLoggerProvider : IMySqlConnectorLoggerProvider
	{
		/// <summary>
		/// Returns a <see cref="NoOpLogger"/>.
		/// </summary>
		public IMySqlConnectorLogger CreateLogger(string name) => NoOpLogger.Instance;
	}
}
