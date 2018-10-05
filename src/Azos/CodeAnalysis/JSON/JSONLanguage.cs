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

namespace Azos.CodeAnalysis.JSON
{
    /// <summary>
    /// Represents JSON language
    /// </summary>
    public sealed class JSONLanguage : Language
    {
        public static readonly JSONLanguage Instance = new JSONLanguage();

        private JSONLanguage() : base() {}


        public override LanguageFamily Family
        {
            get { return LanguageFamily.JavaScript; }
        }

        public override IEnumerable<string> FileExtensions
        {
            get
            {
                yield return "jsn";
                yield return "json";
            }
        }

        public override ILexer MakeLexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false)
        {
            return new JSONLexer(context, srcRef, source, messages, throwErrors);
        }



    }
}
