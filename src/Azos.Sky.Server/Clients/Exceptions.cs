/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Clients
{
  /// <summary>
  /// Thrown to indicate problems that arise while using clients (ISkyServiceClient implementors)
  /// </summary>
  [Serializable]
  public class SkyClientException : SkyException
  {
    public SkyClientException() : base() { }
    public SkyClientException(string message) : base(message) { }
    public SkyClientException(string message, Exception inner) : base(message, inner) { }
    protected SkyClientException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
