/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;

namespace Azos.CodeAnalysis.Laconfig
{
    /// <summary>
    /// Represets laconfig parser result
    /// </summary>
    public sealed class LaconfigData : ObjectResultAnalysisContext<LaconicConfiguration>
    {

        public LaconfigData(LaconicConfiguration configuration) : base(null)
        {
            m_ResultObject = configuration;
        }


        public override Language Language
        {
            get { return LaconfigLanguage.Instance; }
        }

        public override string MessageCodeToString(int code)
        {
            return ((LaconfigMsgCode)code).ToString();
        }

    }
}
