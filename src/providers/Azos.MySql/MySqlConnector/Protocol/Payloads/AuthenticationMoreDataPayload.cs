/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Protocol.Payloads
{
	internal sealed class AuthenticationMoreDataPayload
	{
		public byte[] Data { get; }

		public const byte Signature = 0x01;

		public static AuthenticationMoreDataPayload Create(PayloadData payload)
		{
			var reader = new ByteArrayReader(payload.ArraySegment);
			reader.ReadByte(Signature);
			return new AuthenticationMoreDataPayload(reader.ReadByteArray(reader.BytesRemaining));
		}

		private AuthenticationMoreDataPayload(byte[] data) => Data = data;
	}
}
