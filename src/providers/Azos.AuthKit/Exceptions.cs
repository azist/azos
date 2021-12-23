/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.AuthKit
{
  /// <summary>
  /// Base exception thrown for AuthKit-related issues.
  /// </summary>
  [Serializable]
  public class AuthKitException : AzosException
  {
    public AuthKitException() { }
    public AuthKitException(string message) : base(message) { }
    public AuthKitException(string message, Exception inner) : base(message, inner) { }
    protected AuthKitException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
