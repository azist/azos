/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Azos.IO.Net.Gate
{
  /// <summary>
  /// Throw by NetGate
  /// </summary>
  [Serializable]
  public class NetGateException : AzosException
  {
    public NetGateException() { }
    public NetGateException(string message) : base(message) { }
    public NetGateException(string message, Exception inner) : base(message, inner) { }
    protected NetGateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
