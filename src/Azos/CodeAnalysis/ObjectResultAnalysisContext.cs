
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.CodeAnalysis
{

    /// <summary>
    /// A context that can hold analysis result as TObject.
    /// This class is useful for cases like dynamic language parsers (i.e. JSON)
    /// </summary>
    public abstract class ObjectResultAnalysisContext<TObject> : CommonCodeProcessor, IAnalysisContext where TObject : class
    {
        protected ObjectResultAnalysisContext(IAnalysisContext context, MessageList messages = null, bool throwErrors = false):
                                               base(context, messages, throwErrors)
        {

        }

        protected TObject m_ResultObject;

        public TObject ResultObject { get { return m_ResultObject;} }

    }
}
