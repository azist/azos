/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace MySqlConnector.Core
{
	/// <summary>
	/// Specifies whether to perform synchronous or asynchronous I/O.
	/// </summary>
	internal enum ConnectionType
	{
		/// <summary>
		/// Connection is a TCP connection.
		/// </summary>
		Tcp,

		/// <summary>
		/// Connection is a Unix Domain Socket.
		/// </summary>
		Unix,
	}
}
