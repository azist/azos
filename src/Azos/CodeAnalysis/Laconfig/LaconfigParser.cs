
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;

namespace Azos.CodeAnalysis.Laconfig
{
    /// <summary>
    /// Parses Laconfig lexer output into laconic configuration node graph
    /// </summary>
    public sealed partial class LaconfigParser : Parser<LaconfigLexer>
    {

        public LaconfigParser(LaconfigData context, LaconfigLexer input,  MessageList messages = null, bool throwErrors = false) :
            base(context, new LaconfigLexer[]{ input }, messages, throwErrors)
        {
            m_Lexer = Input.First();
        }

        private LaconfigLexer m_Lexer;

        public LaconfigLexer Lexer { get { return m_Lexer;} }

        public LaconfigData ResultContext { get{ return Context as LaconfigData;} }

        public override Language Language
        {
            get { return LaconfigLanguage.Instance; }
        }

        public override string MessageCodeToString(int code)
        {
            return ((LaconfigMsgCode)code).ToString();
        }




        protected override void DoParse()
        {
            try
            {
                var config = ResultContext.ResultObject;

                tokens = Lexer.GetEnumerator();
                fetchPrimary();
                doRoot( config );

                if (config.Root.Exists)
                   config.Root.ResetModified();

            }
            catch(abortException)
            {

            }
        }
    }
}
