
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.CodeAnalysis.Source;

namespace Azos.CodeAnalysis
{

    /// <summary>
    /// Provides base implementation for common code processors
    /// </summary>
    public abstract class CommonCodeProcessor : ICodeProcessor, IConfigurable
    {

        protected CommonCodeProcessor(IAnalysisContext context, MessageList messages = null, bool throwErrors = false)
        {
              m_Context = context;
              m_Messages = messages ?? (context!=null? context.Messages : null);
              m_ThrowErrors = throwErrors;
        }

        private IAnalysisContext m_Context;
        private MessageList   m_Messages;
        private bool          m_ThrowErrors;


        /// <summary>
        /// Returns context that this processor operates under -
        /// an entity that retains state during analysis, such as: module compiler, project compiler, code unit translator etc.
        /// This entity may also contain compiler/parser/lexer options etc.
        /// </summary>
        public IAnalysisContext Context { get {return m_Context;} }



        /// <summary>
        /// References message list that lexer emits messages into. May be null
        /// </summary>
        public MessageList Messages{ get { return m_Messages; } }

        /// <summary>
        /// When true, throws an exception on the first error even when MessageList is set.
        /// When MessageList is not set any lexing error is always thrown regardless of this parameter
        /// </summary>
        public bool ThrowErrors{ get { return m_ThrowErrors; } }


        /// <summary>
        /// Returns language that this processor is capable of processing
        /// </summary>
        public abstract Language Language { get; }



        /// <summary>
        /// Returns string representation of message code which is output by this processor
        /// </summary>
        public abstract string MessageCodeToString(int code);


        protected void EmitMessage(MessageType type, int code, SourceCodeRef srcRef, SourcePosition? position = null, Token token = null, string text = null, Exception exception = null)
        {
            if (Messages==null && type!=MessageType.Error && type!=MessageType.InternalError) return;

            var msg = new Message(srcRef, type, code, this, position ?? SourcePosition.UNASSIGNED, token, text, exception);

            if (type==MessageType.Error || type==MessageType.InternalError)
             if (ThrowErrors || Messages==null)
             {
               throw new CodeProcessorException(this, StringConsts.CA_PROCESSOR_EXCEPTION_ERROR.Args(Language, GetType().Name, msg.ToString()));
             }


            Messages.Add( msg );
        }

        public virtual void Configure(IConfigSectionNode node)
        {
          ConfigAttribute.Apply(this, node);
        }
  }


    /// <summary>
    /// Provides base implementation for common code processors
    /// </summary>
    public abstract class SourceRefCommonCodeProcessor : CommonCodeProcessor
    {

        protected SourceRefCommonCodeProcessor(IAnalysisContext context, SourceCodeRef srcRef, MessageList messages = null, bool throwErrors = false) :
            base(context, messages, throwErrors)
        {
            m_SourceCodeReference = srcRef;
        }

        private SourceCodeRef m_SourceCodeReference;

        /// <summary>
        /// References source code that is processed
        /// </summary>
        public SourceCodeRef SourceCodeReference { get { return m_SourceCodeReference; } }


        protected void EmitMessage(MessageType type, int code, SourcePosition position, Token token = null, string text = null, Exception exception = null)
        {
            if (Messages==null && type!=MessageType.Error && type!=MessageType.InternalError) return;

            var msg = new Message(m_SourceCodeReference, type, code, this, position, token, text, exception);

            if (type==MessageType.Error || type==MessageType.InternalError)
             if (ThrowErrors || Messages==null)
             {
               throw new CodeProcessorException(this, StringConsts.CA_PROCESSOR_EXCEPTION_ERROR.Args(Language, GetType().Name, msg.ToString()));
             }


            Messages.Add( msg );
        }

    }

}
