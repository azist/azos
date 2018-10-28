/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using MySqlConnector.Utilities;

namespace MySql.Data.MySqlClient
{
	public sealed class MySqlProtocolException : InvalidOperationException
	{
		internal static MySqlProtocolException CreateForPacketOutOfOrder(int expectedSequenceNumber, int packetSequenceNumber)
		{
			return new MySqlProtocolException("Packet received out-of-order. Expected {0}; got {1}.".FormatInvariant(expectedSequenceNumber, packetSequenceNumber));
		}

		private MySqlProtocolException(string message)
			: base(message)
		{
		}
	}
}
