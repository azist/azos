/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;


namespace Azos.Sky.Chronicle
{
  /// <summary>
  /// Thrown to indicate chronicle-related problems: log/instrumentation archive
  /// </summary>
  [Serializable]
  public class ChronicleException : SkyException
  {
    public ChronicleException() : base() {}
    public ChronicleException(string message) : base(message) {}
    public ChronicleException(string message, Exception inner) : base(message, inner) { }
    protected ChronicleException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
