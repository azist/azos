/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Platform.Abstraction.NetCore
{
  /// <summary>
  /// Base exception thrown for the PAL-related issues
  /// </summary>
  [Serializable]
  public class NetCorePALException : PALException
  {
    public NetCorePALException() {}
    public NetCorePALException(string message) : base(message) {}
    public NetCorePALException(string message, Exception inner) : base(message, inner) {}
    protected NetCorePALException(SerializationInfo info, StreamingContext context) : base(info, context) {}
  }

}