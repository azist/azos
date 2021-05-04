/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;
using Azos.Serialization.JSON;

namespace Azos.CodeAnalysis
{
  /// <summary>
  /// Base exception thrown by the framework
  /// </summary>
  [Serializable]
  public class CodeAnalysisException : AzosException
  {
    public CodeAnalysisException() { }
    public CodeAnalysisException(string message) : base(message) { }
    public CodeAnalysisException(string message, Exception inner) : base(message, inner) { }
    protected CodeAnalysisException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }


  /// <summary>
  /// Thrown by code processors such as lexers, parsers ,  semantic analyzers, compilers etc...
  /// </summary>
  [Serializable]
  public class CodeProcessorException : CodeAnalysisException, IExternalStatusProvider
  {
    public CodeProcessorException(ICodeProcessor codeProcessor) { CodeProcessor = codeProcessor; }
    public CodeProcessorException(ICodeProcessor codeProcessor, string message) : base(message) { CodeProcessor = codeProcessor; }
    public CodeProcessorException(ICodeProcessor codeProcessor, string message, Exception inner) : base(message, inner) { CodeProcessor = codeProcessor; }

    protected CodeProcessorException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    [NonSerialized]
    public readonly ICodeProcessor CodeProcessor;

    public virtual JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var map = this.DefaultBuildErrorStatusProviderMap(includeDump, "code.analysis");
      if (CodeProcessor == null) return map;

      if (CodeProcessor.Language != null) map["code.lang"] = CodeProcessor.Language.ToString();
      if (CodeProcessor.Messages != null) map["code.msgs"] = CodeProcessor.Messages.ToString();

      if (includeDump)
      {
        map["code.processor"] = CodeProcessor.GetType().DisplayNameWithExpandedGenericArgs();
        if (CodeProcessor.Context != null) map["code.ctx"] = CodeProcessor.Context.GetType().Name;
      }

      return map;
    }
  }


  [Serializable]
  public class StringEscapeErrorException : CodeAnalysisException
  {
    public const string ERRORED_ESCAPE_FLD_NAME = "SEEE-EE";

    private StringEscapeErrorException() { }
    public StringEscapeErrorException(string escape) { ErroredEscape = escape; }
    protected StringEscapeErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      ErroredEscape = info.GetString(ERRORED_ESCAPE_FLD_NAME);
    }

    public readonly string ErroredEscape;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(ERRORED_ESCAPE_FLD_NAME, ErroredEscape);
      base.GetObjectData(info, context);
    }

  }
}
