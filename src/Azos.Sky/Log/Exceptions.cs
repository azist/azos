/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;


namespace Azos.Sky.Log
{
  /// <summary>
  /// Thrown to indicate log archive related problems
  /// </summary>
  [Serializable]
  public class LogArchiveException : SkyException
  {
    public LogArchiveException() : base() {}
    public LogArchiveException(string message) : base(message) {}
    public LogArchiveException(string message, Exception inner) : base(message, inner) { }
    protected LogArchiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
