/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Apps.Strategies
{
  /// <summary>
  /// Thrown to indicate errors relating to strategies
  /// </summary>
  [Serializable]
  public class StrategyException : AzosException
  {
    public StrategyException() { }
    public StrategyException(string message) : base(message) { }
    public StrategyException(string message, Exception inner) : base(message, inner) { }
    protected StrategyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
