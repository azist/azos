
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.CodeAnalysis.JSON
{
    /// <summary>
    /// Message codes for JSON code processors
    /// </summary>
    public enum JSONMsgCode
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
            eSyntaxError,
            eUnterminatedArray,
            eUnterminatedObject,
            eObjectKeyExpected,
            eColonOperatorExpected,
            eNumericLiteralExpectedAfterSignOperator,
            eDuplicateObjectKey


    }
}
