/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.IO.Streaming
{
  /// <summary>
  /// Base exception thrown by the archiving framework
  /// </summary>
  [Serializable]
  public class ArchivingException : AzosIOException
  {
    public ArchivingException() { }
    public ArchivingException(string message) : base(message) { }
    public ArchivingException(string message, Exception inner) : base(message, inner) { }
    protected ArchivingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}