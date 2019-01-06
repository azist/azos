/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Apps.Injection
{
  /// <summary>
  /// Thrown to indicate errors relating to dependency injection
  /// </summary>
  [Serializable]
  public class DependencyInjectionException : AzosException
  {
    public DependencyInjectionException() { }
    public DependencyInjectionException(string message) : base(message) { }
    public DependencyInjectionException(string message, Exception inner) : base(message, inner) { }
    protected DependencyInjectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
