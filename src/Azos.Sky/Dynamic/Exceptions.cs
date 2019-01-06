/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Dynamic
{
  /// <summary>
  /// Thrown to indicate workers related problems
  /// </summary>
  [Serializable]
  public class DynamicException : SkyException
  {
    public DynamicException() : base() {}
    public DynamicException(string message) : base(message) { }
    public DynamicException(string message, Exception inner) : base(message, inner) { }
    protected DynamicException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
