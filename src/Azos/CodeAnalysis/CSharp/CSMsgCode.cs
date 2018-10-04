
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.CodeAnalysis.CSharp
{
    /// <summary>
    /// Message codes for CSharp code processors
    /// </summary>
    public enum CSMsgCode
    {
        INFOS = 0,

        WARNING = 100,


        ERRORS = 1000,
            eUnterminatedString,
            ePrematureEOF,
            eUnterminatedComment,
            eInvalidStringEscape,
            eValueTooBig,
            eInvalidIdentifier,
            eSyntaxError
    }
}
