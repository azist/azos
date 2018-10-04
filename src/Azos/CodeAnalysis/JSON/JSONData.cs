
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.CodeAnalysis.JSON
{
    /// <summary>
    /// Represents JSONDataParser result
    /// </summary>
    public sealed class JSONData : ObjectResultAnalysisContext<object>
    {

        public JSONData(IAnalysisContext context = null, MessageList messages = null, bool throwErrors = false):
                                               base(context, messages ?? new MessageList(), throwErrors)
        {

        }


        public override Language Language
        {
            get { return JSONLanguage.Instance; }
        }

        public override string MessageCodeToString(int code)
        {
            return ((JSONMsgCode)code).ToString();
        }

        internal void setData(object data)
        {
            m_ResultObject = data;
        }
    }
}
