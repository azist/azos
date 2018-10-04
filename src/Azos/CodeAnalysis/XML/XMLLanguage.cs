
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.CodeAnalysis.Source;

namespace Azos.CodeAnalysis.XML
{
    /// <summary>
    /// Represents XML language
    /// </summary>
    public sealed class XMLLanguage : Language
    {
        public static readonly XMLLanguage Instance = new XMLLanguage();

        private XMLLanguage() : base() {}


        public override LanguageFamily Family
        {
            get { return LanguageFamily.XML; }
        }

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
            throw new NotImplementedException(GetType().Name+".MakeLexer()");
        }



    }
}
