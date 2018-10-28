/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace MySqlConnector.Protocol.Payloads
{
    internal sealed class EmptyPayload
    {
        public static PayloadData Create()
        {
            return new PayloadData(new byte[0]);
        }
    }
}
