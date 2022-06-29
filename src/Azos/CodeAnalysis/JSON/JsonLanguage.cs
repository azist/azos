/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using Azos.CodeAnalysis.Source;

namespace Azos.CodeAnalysis.JSON
{
  /// <summary>
  /// Represents JSON language
  /// </summary>
  public sealed class JsonLanguage : Language
  {
    public static readonly JsonLanguage Instance = new JsonLanguage();

    private JsonLanguage() : base() { }

    public override LanguageFamily Family => LanguageFamily.JavaScript;

    public override IEnumerable<string> FileExtensions
    {
      get
      {
        yield return "jsn";
        yield return "json";
      }
    }

    public override ILexer MakeLexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false)
      => new JsonLexer(context, srcRef, source, messages, throwErrors);

  }
}
