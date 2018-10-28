/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Text;

namespace MySqlConnector.Protocol.Payloads
{
	internal sealed class QueryPayload
	{
		public static PayloadData Create(string query)
		{
			var length = Encoding.UTF8.GetByteCount(query);
			var payload = new byte[length + 1];
			payload[0] = (byte) CommandKind.Query;
			Encoding.UTF8.GetBytes(query, 0, query.Length, payload, 1);
			return new PayloadData(payload);
		}
	}
}
