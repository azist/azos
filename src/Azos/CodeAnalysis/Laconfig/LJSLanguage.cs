/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.CodeAnalysis.Source;

namespace Azos.CodeAnalysis.Laconfig
{
  /// <summary>
  /// Represents Laconic Java Script language which is based on Laconfig Lexer
  /// </summary>
  public sealed class LJSLanguage : Language
  {
    public static readonly LJSLanguage Instance = new LJSLanguage();

    private LJSLanguage() : base() {}


    public override LanguageFamily Family
    {
        get { return LanguageFamily.StructuredConfig; }
    }

    public override IEnumerable<string> FileExtensions
    {
        get
        {
            yield return "ljs";
        }
    }

    //LJS uses Laconfig Lexer but a different parser
    public override ILexer MakeLexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false)
    {
        return new LaconfigLexer(context, srcRef, source, messages, throwErrors);
    }
  }
}
