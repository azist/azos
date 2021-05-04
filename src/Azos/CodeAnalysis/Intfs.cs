/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using Azos.CodeAnalysis.Source;

namespace Azos.CodeAnalysis
{
  /// <summary>
  /// Describes an entity that may process source code in some language
  /// </summary>
  public interface ICodeProcessor
  {
    /// <summary>
    /// Returns context that this processor operates under
    /// </summary>
    IAnalysisContext Context { get; }

    /// <summary>
    /// References language that this entity supports
    /// </summary>
    Language Language { get; }

    /// <summary>
    /// References message list that processor emits messages into.
    /// May be null, in which case errors are always thrown because they can not get logged into message list
    /// </summary>
    MessageList Messages { get; }

    /// <summary>
    /// When true, throws an exception on the first error even when MessageList is set.
    /// When MessageList is not set any error is always thrown regardless of this parameter
    /// </summary>
    bool ThrowErrors { get; }

    /// <summary>
    /// Returns string representation of message code which is output by this processor
    /// </summary>
    string MessageCodeToString(int code);
  }


  /// <summary>
  /// Describes an entity that retains state during analysis, such as: module compiler, project compiler, code unit translator etc.
  /// This entity may also contain compiler/parser/lexer options etc.
  /// </summary>
  public interface IAnalysisContext : ICodeProcessor
  {

  }


  /// <summary>
  /// Describes general lexer interface.
  /// Lexer turn string source input which is obtained via ISourceText implementation (i.e. FileSource,WebSource...)
  ///  into tokenized stream. Some lexer implementations may support lazy analysis, that is - source text analysis which is done
  ///   in chunks when lexer is iterated over, others may lex the whole source text at once
  /// </summary>
  public interface ILexer : ICodeProcessor
  {
    /// <summary>
    /// References source code that was lexed
    /// </summary>
    SourceCodeRef SourceCodeReference { get; }

    /// <summary>
    /// References source code text that was lexed
    /// </summary>
    ISourceText Source { get; }

    /// <summary>
    /// Enumerates Token stream - depending on implementation enumeration may do
    /// lexical analysis token-by-token or in chunks with every enumerator advance
    /// </summary>
    IEnumerable<Token> TokenStream { get; }

    /// <summary>
    /// Returns true when the whole input source has been analyzed. This property is always true for lexers that do not support lazy analysis
    /// </summary>
    bool AllAnalyzed { get; }

    /// <summary>
    /// If lexer supports lazy analysis, forces analysis of the whole source
    /// </summary>
    void AnalyzeAll();
  }


  /// <summary>
  /// Describes general parser interface
  /// </summary>
  public interface IParser : ICodeProcessor
  {
    /// <summary>
    /// Lists source lexers that supply token stream for parsing
    /// </summary>
    IEnumerable<ILexer> SourceInput { get; }

    /// <summary>
    /// Indicates whether Parse() already happened
    /// </summary>
    bool HasParsed { get; }

    /// <summary>
    /// Performs parsing and sets HasParsed to true if it has not been performed yet
    /// </summary>
    void Parse();
  }

}
