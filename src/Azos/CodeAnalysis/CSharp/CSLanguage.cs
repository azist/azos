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

namespace Azos.CodeAnalysis.CSharp
{
    /// <summary>
    /// Represents C# language
    /// </summary>
    public sealed class CSLanguage : Language
    {
        public static readonly CSLanguage Instance = new CSLanguage();

        private CSLanguage() : base() {}


        public override LanguageFamily Family
        {
            get { return LanguageFamily.CSharp; }
        }

        public override IEnumerable<string> FileExtensions
        {
            get
            {
                yield return "cs";
            }
        }

        public override ILexer MakeLexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false)
        {
            return new CSLexer(context, srcRef, source, messages, throwErrors);
        }



    }
}
