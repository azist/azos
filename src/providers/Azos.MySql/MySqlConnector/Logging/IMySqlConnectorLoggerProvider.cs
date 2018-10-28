/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace MySqlConnector.Logging
{
	/// <summary>
	/// Implementations of <see cref="IMySqlConnectorLoggerProvider"/> create logger instances.
	/// </summary>
	public interface IMySqlConnectorLoggerProvider
	{
		/// <summary>
		/// Creates a logger with the specified name. This method may be called from multiple threads and must be thread-safe.
		/// </summary>
		IMySqlConnectorLogger CreateLogger(string name);
	}
}
