/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Templatization
{
  /// <summary>
  /// Base exception thrown by the templatization-related functionality
  /// </summary>
  [Serializable]
  public class TemplatizationException : AzosException
  {
    public TemplatizationException() { }
    public TemplatizationException(string message) : base(message) { }
    public TemplatizationException(string message, Exception inner) : base(message, inner) { }
    protected TemplatizationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Base exception thrown by the template compilers
  /// </summary>
  [Serializable]
  public class TemplateCompilerException : TemplatizationException
  {
    public TemplateCompilerException() { }
    public TemplateCompilerException(string message) : base(message) { }
    public TemplateCompilerException(string message, Exception inner) : base(message, inner) { }
    protected TemplateCompilerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Indicates template source parsing exception
  /// </summary>
  [Serializable]
  public class TemplateParseException : TemplateCompilerException
  {
    public TemplateParseException() { }
    public TemplateParseException(string message) : base(message) { }
    public TemplateParseException(string message, Exception inner) : base(message, inner) { }
    protected TemplateParseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  #warning Implement using PAL
  ///// <summary>
  ///// Thrown by  template code compilers
  ///// </summary>
  //[Serializable]
  //public class TemplateCodeCompilerException : TemplateCompilerException, IWrappedExceptionDataSource
  //{
  //  public TemplateCodeCompilerException(CompilerError err) : base(err.ErrorText) { Error = err; }
  //  protected TemplateCodeCompilerException(SerializationInfo info, StreamingContext context) : base(info, context) { }

  //  [NonSerialized]
  //  public readonly CompilerError Error;

  //  public override string ToString()
  //  {
  //    return string.Format("#{0} {1} Warn: {2} Line: {3} Column: {4} File: \"{5}\"",
  //                         Error.ErrorNumber,
  //                         Error.ErrorText,
  //                         Error.IsWarning,
  //                         Error.Line,
  //                         Error.Column,
  //                         Error.FileName);
  //  }

  //  public string GetWrappedData()
  //  {
  //    return ToString();
  //  }
  //}
}