/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Cms
{
  /// <summary>
  /// Base exception thrown by error conditions related to Content Management(CMS)
  /// </summary>
  [Serializable]
  public class CmsException : SkyException
  {
    public CmsException() { }
    public CmsException(string message) : base(message) { }
    public CmsException(string message, Exception inner) : base(message, inner) { }
    protected CmsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }


}
