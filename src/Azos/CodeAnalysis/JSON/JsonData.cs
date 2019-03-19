/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.CodeAnalysis.JSON
{
    /// <summary>
    /// Represents JSONDataParser result
    /// </summary>
    public sealed class JsonData : ObjectResultAnalysisContext<object>
    {

        public JsonData(IAnalysisContext context = null, MessageList messages = null, bool throwErrors = false):
                                               base(context, messages ?? new MessageList(), throwErrors)
        {

        }


        public override Language Language
        {
            get { return JsonLanguage.Instance; }
        }

        public override string MessageCodeToString(int code)
        {
            return ((JsonMsgCode)code).ToString();
        }

        internal void setData(object data)
        {
            m_ResultObject = data;
        }
    }
}
