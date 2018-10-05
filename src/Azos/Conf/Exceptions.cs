
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
    public ConfigException() { }
    public ConfigException(string message) : base(message) { }
    public ConfigException(string message, Exception inner) : base(message, inner) { }
    protected ConfigException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by Behavior class to indicate behavior apply error
  /// </summary>
  [Serializable]
  public sealed class BehaviorApplyException : ConfigException
  {
    public BehaviorApplyException() { }
    public BehaviorApplyException(string message) : base(message) { }
    public BehaviorApplyException(string message, Exception inner) : base(message, inner) { }
    internal BehaviorApplyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}