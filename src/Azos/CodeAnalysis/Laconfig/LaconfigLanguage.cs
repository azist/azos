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
  /// Represents Laconic + Config = Laconfig terse configuration language
  /// </summary>
  public sealed class LaconfigLanguage : Language
  {
    public static readonly LaconfigLanguage Instance = new LaconfigLanguage();

    private LaconfigLanguage() : base() {}


    public override LanguageFamily Family
    {
        get { return LanguageFamily.StructuredConfig; }
    }

    public override IEnumerable<string> FileExtensions
    {
        get
        {
            yield return "lac";
            yield return "lacon";
            yield return "laconf";
            yield return "laconfig";
            yield return "rschema";
            yield return "amb";
        }
    }

    public override ILexer MakeLexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false)
    {
        return new LaconfigLexer(context, srcRef, source, messages, throwErrors);
    }
  }
}
