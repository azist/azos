/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Instrumentation
{
  /// <summary>
  /// Thrown to indicate log archive related problems
  /// </summary>
  [Serializable]
  public class TelemetryArchiveException : SkyException
  {
    public TelemetryArchiveException() : base() {}
    public TelemetryArchiveException(string message) : base(message) {}
    public TelemetryArchiveException(string message, Exception inner) : base(message, inner) { }
    protected TelemetryArchiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
