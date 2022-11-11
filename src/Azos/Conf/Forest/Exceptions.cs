/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Conf.Forest
{
  /// <summary>
  /// Configuration-related exception providing mandatory server 500 for errors like reading corrupt node config.
  /// This exception cloaks the inner exceptions which might be 400
  /// </summary>
  [Serializable]
  public class ConfigForestException : ConfigException, IHttpStatusProvider
  {
    /// <summary>
    /// Initializes a new instance of a configuration-related exception
    /// </summary>
    public ConfigForestException() { }

    /// <summary>
    /// Initializes a new instance of a configuration-related exception with a specified error message
    /// </summary>
    public ConfigForestException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of a configuration-related exception with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    public ConfigForestException(string message, Exception inner) : base(message, inner) { }

    /// <summary>
    /// Initializes a new instance of a configuration-related exception with serialized data
    /// </summary>
    protected ConfigForestException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public int HttpStatusCode => 500;

    public string HttpStatusDescription => "Tree error";
  }

}