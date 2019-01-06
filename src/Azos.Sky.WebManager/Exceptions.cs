/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Sky.WebManager
{
  /// <summary>
  /// Base exception thrown by the WebManager site
  /// </summary>
  [Serializable]
  public class WebManagerException : SkyException
  {
    public WebManagerException(int code, string message) : this(message, null, code, null, null) { }
    public WebManagerException(string message) : this(message, null, 0, null, null) { }
    public WebManagerException(string message, Exception inner) : this(message, inner, 0, null, null) { }
    public WebManagerException(string message, Exception inner, int code, string sender, string topic) : base(message, inner, code, sender, topic) { }
    protected WebManagerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
