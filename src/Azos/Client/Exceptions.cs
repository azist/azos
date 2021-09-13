/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Client
{
  /// <summary>
  /// Base exception thrown by the Azos.Client functionality
  /// </summary>
  [Serializable]
  public class ClientException : AzosException
  {
    public ClientException() { }

    public ClientException(string message) : base(message) { }

    public ClientException(string message, Exception inner) : base(message, inner) { }

    protected ClientException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
