/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace MySqlConnector.Protocol
{
	internal struct PayloadData
	{
		public PayloadData(byte[] data) => ArraySegment = new ArraySegment<byte>(data);
		public PayloadData(ArraySegment<byte> data) => ArraySegment = data;

		public ArraySegment<byte> ArraySegment { get; }
		public byte HeaderByte => ArraySegment.Array[ArraySegment.Offset];
	}
}
