/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Web.Cloud
{
  /// <summary>
  /// Thrown to indicate workers related problems
  /// </summary>
  [Serializable]
  public class CloudException : WebException
  {
    public CloudException() : base() {}
    public CloudException(string message) : base(message) { }
    public CloudException(string message, Exception inner) : base(message, inner) { }
    protected CloudException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
