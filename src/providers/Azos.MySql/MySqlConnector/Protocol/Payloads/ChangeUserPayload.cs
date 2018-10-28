/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Protocol.Payloads
{
	internal sealed class ChangeUserPayload
	{
		public static PayloadData Create(string user, byte[] authResponse, string schemaName, byte[] connectionAttributes)
		{
			var writer = new PayloadWriter();

			writer.WriteByte((byte) CommandKind.ChangeUser);
			writer.WriteNullTerminatedString(user);
			writer.WriteByte(checked((byte) authResponse.Length));
			writer.Write(authResponse);
			writer.WriteNullTerminatedString(schemaName ?? "");
			writer.WriteByte((byte) CharacterSet.Utf8Mb4Binary);
			writer.WriteByte(0);
			writer.WriteNullTerminatedString("mysql_native_password");
			if (connectionAttributes != null)
				writer.Write(connectionAttributes);

			return writer.ToPayloadData();
		}
	}
}
