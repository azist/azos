/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace MySqlConnector.Protocol
{
	internal enum CommandKind
	{
		Quit = 1,
		InitDatabase = 2,
		Query = 3,
		Ping = 14,
		ChangeUser = 17,
		ResetConnection = 31,
	}
}
