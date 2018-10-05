/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
