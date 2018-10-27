/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Serialization.BSON
{
  /// <summary>
  /// Base exception thrown by the BSON framework
  /// </summary>
  [Serializable]
  public class BSONException : AzosException
  {
    public BSONException() { }
    public BSONException(string message) : base(message) { }
    public BSONException(string message, Exception inner) : base(message, inner) { }
    protected BSONException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}