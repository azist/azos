
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
