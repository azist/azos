/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.CodeAnalysis.Source;

namespace Azos.CodeAnalysis.XML
{
  /// <summary>
  /// Represents XML language
  /// </summary>
  public sealed class XMLLanguage : Language
  {
    public static readonly XMLLanguage Instance = new XMLLanguage();

    private XMLLanguage() : base() { }

    public override LanguageFamily Family => LanguageFamily.XML;

    public override IEnumerable<string> FileExtensions
    {
      get
      {
        yield return "xml";
        yield return "configuration";
        yield return "conf";
        yield return "config";

      }
    }

    public override ILexer MakeLexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false)
    {
      throw new NotImplementedException(GetType().Name + ".MakeLexer()");
    }

  }
}
