/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace MySqlConnector.Protocol.Serialization
{
	/// <summary>
	/// Specifies how to handle protocol errors.
	/// </summary>
	internal enum ProtocolErrorBehavior
	{
		/// <summary>
		/// Throw an exception when there is a protocol error. This is the default.
		/// </summary>
		Throw,

		/// <summary>
		/// Ignore any protocol errors.
		/// </summary>
		Ignore,
	}
}
