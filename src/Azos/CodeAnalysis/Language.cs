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

namespace Azos.CodeAnalysis
{

    /// <summary>
    /// Defines various languages
    /// </summary>
    public enum LanguageFamily
    {
        Other = 0,
        StructuredConfig,
        Aum,
        CSharp,
        C,
        CPP,
        Erlang,
        JavaScript,
        Java,
        INI,
        XML
    }


    /// <summary>
    /// Represents and abstraction of the language
    /// </summary>
    public abstract class Language
    {
        #region STATIC / .ctor
            private static List<Language> s_Languages = new List<Language>();

            /// <summary>
            /// Returns all languages registered in the system
            /// </summary>
            public static IEnumerable<Language> All
            {
                get
                {
                    IEnumerable<Language> result;
                    lock(s_Languages)
                        result = s_Languages.ToList();
                    return result;
                }
            }

            /// <summary>
            /// Tries to find a language by file extension or returns UnspecifiedLanguage
            /// </summary>
            public static Language TryFindLanguageByFileExtension(string fileExtension)
            {
                return All.FirstOrDefault(
                            lng => lng.FileExtensions.Contains( fileExtension, StringComparer.InvariantCultureIgnoreCase)
                           ) ?? UnspecifiedLanguage.Instance;
            }


            protected Language()
            {
                lock(s_Languages)
                 s_Languages.Add(this);
            }
        #endregion


        #region Properties

            /// <summary>
            /// Returns language family categorization
            /// </summary>
            public abstract LanguageFamily Family { get; }

            /// <summary>
            /// Returns file extensions without '.'
            /// </summary>
            public abstract IEnumerable<string> FileExtensions { get; }

        #endregion

        #region Public

            /// <summary>
            /// Makes lexer capable of this language analysis
            /// </summary>
            public abstract ILexer MakeLexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false);


            public override string ToString()
            {
                return "{0}::{1}".Args(Family, GetType().Name.Replace("Language", string.Empty));
            }
        #endregion
    }


    /// <summary>
    /// Represents Unspecified unknown language
    /// </summary>
    public sealed class UnspecifiedLanguage : Language
    {
        public static readonly UnspecifiedLanguage Instance = new UnspecifiedLanguage();

        private UnspecifiedLanguage() : base() {}


        public override LanguageFamily Family
        {
            get { return LanguageFamily.Other; }
        }

        public override IEnumerable<string> FileExtensions
        {
            get { yield break; }
        }


        public override ILexer MakeLexer(IAnalysisContext context, SourceCodeRef srcRef, ISourceText source, MessageList messages = null, bool throwErrors = false)
        {
            throw new NotSupportedException("UnspecifiedLanguage.MakeLexer()");
        }
    }


}
