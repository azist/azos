
using System;
using System.Runtime.Serialization;

namespace Azos.Data.Modeling
{
  /// <summary>
  /// Base exception thrown by the Modeling-* framework
  /// </summary>
  [Serializable]
  public class ModelingException : AzosException
  {
    public ModelingException() { }
    public ModelingException(string message) : base(message) { }
    public ModelingException(string message, Exception inner) : base(message, inner) { }
    protected ModelingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by relational schema
  /// </summary>
  [Serializable]
  public class SchemaException : ModelingException
  {
    public SchemaException(string message) : base(message) { }
    public SchemaException(string message, Exception inner) : base(message, inner) { }
    protected SchemaException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by relational schema compiler
  /// </summary>
  [Serializable]
  public class CompilerException : ModelingException
  {
    public CompilerException(string message) : base(message) { }
    public CompilerException(string message, Exception inner) : base(message, inner) { }
    protected CompilerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by relational schema compiler while processing the source schema
  /// </summary>
  [Serializable]
  public class SchemaCompilationException : SchemaException
  {
    public const string NODE_PATH_FLD_NAME = "SCE-NP";

    public SchemaCompilationException(string nodePath, string message) : base(message) { NodePath = nodePath; }
    public SchemaCompilationException(string nodePath, string message, Exception inner) : base(message, inner) { NodePath = nodePath; }
    protected SchemaCompilationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      NodePath = info.GetString(NODE_PATH_FLD_NAME);
    }

    /// <summary>
    /// Returns node that issued compilation error
    /// </summary>
    public readonly string NodePath;

    public override string Message
    {
      get
      {
        return "Exception {0} at node '{1}'".Args(base.Message, NodePath ?? CoreConsts.UNKNOWN);
      }
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(NODE_PATH_FLD_NAME, NodePath);
      base.GetObjectData(info, context);
    }
  }
}