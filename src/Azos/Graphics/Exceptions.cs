/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Graphics
{
  /// <summary>
  /// Base exception thrown for issues realted to Azos.Graphics
  /// </summary>
  [Serializable]
  public class GraphicsException : AzosException
  {
    public GraphicsException() {}

    public GraphicsException(string message) : base(message) {}

    public GraphicsException(string message, Exception inner) : base(message, inner) {}

    protected GraphicsException(SerializationInfo info, StreamingContext context) : base(info, context) {}

  }
}