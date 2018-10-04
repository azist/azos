
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.CodeAnalysis.Laconfig
{
    /// <summary>
    /// Message codes for Laconfig code processors
    /// </summary>
    public enum LaconfigMsgCode
    {
        INFOS = 0,

        WARNING = 100,


        ERRORS = 1000,
            eUnterminatedString,
            ePrematureEOF,
            eUnterminatedComment,
            eInvalidStringEscape,
            eSectionNameExpected,
            eSectionOpenBraceExpected,
            eSectionOrAttributeNameExpected,
            eSectionOrAttributeValueExpected,
            eContentPastRootSection,
            eSyntaxError
    }
}
