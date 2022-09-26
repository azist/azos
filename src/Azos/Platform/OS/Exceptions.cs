/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Platform.OS
{
  /// <summary>
  /// Base exception thrown for OS-related issues
  /// </summary>
  [Serializable]
  public class OsException : AzosException
  {
    public OsException() {}
    public OsException(string message) : base(message) {}
    public OsException(string message, Exception inner) : base(message, inner) {}
    protected OsException(SerializationInfo info, StreamingContext context) : base(info, context) {}
  }

}