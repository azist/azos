/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Web.Social
{
  /// <summary>
  /// General Azos social network specific exception
  /// </summary>
  [Serializable]
  public class SocialException: WebException
  {
    public SocialException() { }
    public SocialException(string message) : base(message) { }
    public SocialException(string message, Exception inner) : base(message, inner) { }
    protected SocialException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
