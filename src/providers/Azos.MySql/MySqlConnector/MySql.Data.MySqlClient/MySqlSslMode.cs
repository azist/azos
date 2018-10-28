/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// SSL connection options.
	/// </summary>
	public enum MySqlSslMode
	{
		/// <summary>
		/// Do not use SSL.
		/// </summary>
		None,

		/// <summary>
		/// Use SSL if the server supports it.
		/// </summary>
		Preferred,

		/// <summary>
		/// Always use SSL. Deny connection if server does not support SSL.
		/// </summary>
		Required,

		/// <summary>
		///  Always use SSL. Validate the Certificate Authority but tolerate name mismatch.
		/// </summary>
		VerifyCA,

		/// <summary>
		/// Always use SSL. Fail if the host name is not correct.
		/// </summary>
		VerifyFull,
	}
}
