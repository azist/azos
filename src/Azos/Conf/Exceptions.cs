/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Conf
{
  /// <summary>
  /// Configuration-related exception
  /// </summary>
  [Serializable]
  public class ConfigException : AzosException
  {
    /// <summary>
    /// Initializes a new instance of a configuration-related exception
    /// </summary>
    public ConfigException() { }

    /// <summary>
    /// Initializes a new instance of a configuration-related exception with a specified error message
    /// </summary>
    public ConfigException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of a configuration-related exception with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    public ConfigException(string message, Exception inner) : base(message, inner) { }

    /// <summary>
    /// Initializes a new instance of a configuration-related exception with serialized data
    /// </summary>
    protected ConfigException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by Behavior class to indicate behavior apply error
  /// </summary>
  [Serializable]
  public sealed class BehaviorApplyException : ConfigException
  {
    /// <summary>
    /// Initializes a new instance of a behavior apply configuration-related exception
    /// </summary>
    public BehaviorApplyException() { }

    /// <summary>
    /// Initializes a new instance of a behavior apply configuration-related exception with a specified error message
    /// </summary>
    public BehaviorApplyException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of a behavior apply configuration-related exception with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    public BehaviorApplyException(string message, Exception inner) : base(message, inner) { }

    /// <summary>
    /// Initializes a new instance of a behavior apply configuration-related exception with serialized data
    /// </summary>
    internal BehaviorApplyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}