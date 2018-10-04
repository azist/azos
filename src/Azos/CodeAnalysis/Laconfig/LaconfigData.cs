
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
