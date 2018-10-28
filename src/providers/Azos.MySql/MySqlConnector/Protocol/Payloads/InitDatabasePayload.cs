/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Text;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Protocol.Payloads
{
	internal sealed class InitDatabasePayload
	{
		public static PayloadData Create(string databaseName)
		{
			var writer = new PayloadWriter();

			writer.WriteByte((byte) CommandKind.InitDatabase);
			writer.Write(Encoding.UTF8.GetBytes(databaseName));

			return writer.ToPayloadData();
		}
	}
}
