
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
