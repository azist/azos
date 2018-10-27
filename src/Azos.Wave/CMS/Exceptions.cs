/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

using Azos.Web;

namespace Azos.Wave.CMS
{
  /// <summary>
  /// Base exception thrown by the WAVE.CMS framework
  /// </summary>
  [Serializable]
  public class CMSException : WaveException
  {
    public CMSException() { }
    public CMSException(string message) : base(message) { }
    public CMSException(string message, Exception inner) : base(message, inner) { }
    protected CMSException(SerializationInfo info, StreamingContext context): base(info, context) { }
  }

}