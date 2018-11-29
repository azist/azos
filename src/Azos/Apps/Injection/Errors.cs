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
