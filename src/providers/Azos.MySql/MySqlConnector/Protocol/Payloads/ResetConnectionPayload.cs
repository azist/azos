/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace MySqlConnector.Protocol.Payloads
{
	internal sealed class ResetConnectionPayload
	{
		public static PayloadData Create() => new PayloadData(new[] { (byte) CommandKind.ResetConnection });
	}
}
