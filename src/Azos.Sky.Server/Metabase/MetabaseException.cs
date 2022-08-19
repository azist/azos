/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Metabase
{
  /// <summary>
  /// Thrown to indicate metabase-related problems
  /// </summary>
  [Serializable]
  public class MetabaseException : SkyException
  {
    public MetabaseException() : base() { }
    public MetabaseException(string message) : base(message) { }
    public MetabaseException(string message, Exception inner) : base(message, inner) { }
    protected MetabaseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
