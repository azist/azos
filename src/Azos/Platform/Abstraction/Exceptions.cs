/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Platform.Abstraction
{
  /// <summary>
  /// Base exception thrown for the PAL-related issues
  /// </summary>
  [Serializable]
  public class PALException : AzosException
  {
    public PALException() {}
    public PALException(string message) : base(message) {}
    public PALException(string message, Exception inner) : base(message, inner) {}
    protected PALException(SerializationInfo info, StreamingContext context) : base(info, context) {}
  }

}