/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Apps.Hosting
{
  /// <summary>
  /// Thrown to indicate app hosting related problems
  /// </summary>
  [Serializable]
  public class AppHostingException : Sky.SkyException
  {
    public AppHostingException() : base() { }
    public AppHostingException(string message) : base(message) { }
    public AppHostingException(string message, Exception inner) : base(message, inner) { }
    protected AppHostingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

}
