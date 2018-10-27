/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Templatization
{
  /// <summary>
  /// Defines an entity that a template can be rendered into.
  /// Templates are not necessarily text-based, consequently data is supplied as objects
  /// </summary>
  public interface IRenderingTarget
  {
    /// <summary>
    /// Encodes an object per underlying target specification. For example,
    /// a Http-related target may encode strings using HttpEncoder. If particular target
    /// does not support encoding then this method should just return the argument unmodified
    /// </summary>
    object Encode(object value);


    /// <summary>
    /// Writes a generic object into target. Templates are not necessarily text-based,
    /// consequently this method takes an object argument
    /// </summary>
    void Write(object value);

    /// <summary>
    /// Flushes writes into underlying target implementation. If target does not support
    /// buffering then this call does nothing
    /// </summary>
    void Flush();
  }
}
