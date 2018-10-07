
using System;
using System.Runtime.Serialization;

using Azos.Apps;
using Azos.Serialization.BSON;

namespace Azos.Scripting
{
  /// <summary>
  /// Base exception thrown scripting framework
  /// </summary>
  [Serializable]
  public class ScriptingException : AzosException
  {
    public ScriptingException() {}
    public ScriptingException(string message) : base(message) {}
    public ScriptingException(string message, Exception inner) : base(message, inner) {}
    protected ScriptingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }


  /// <summary>
  /// Base exception thrown Runner
  /// </summary>
  [Serializable]
  public class RunnerException : ScriptingException
  {
    public RunnerException() {}
    public RunnerException(string message) : base(message) {}
    public RunnerException(string message, Exception inner) : base(message, inner) {}
    protected RunnerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }


  /// <summary>
  /// Thrown when run method parameters can not be bound
  /// </summary>
  [Serializable]
  public class RunMethodBinderException : RunnerException
  {
    public RunMethodBinderException() {}
    public RunMethodBinderException(string message) : base(message) {}
    public RunMethodBinderException(string message, Exception inner) : base(message, inner) {}
    protected RunMethodBinderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

}