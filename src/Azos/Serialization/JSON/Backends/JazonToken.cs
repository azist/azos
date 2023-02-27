/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.CodeAnalysis.JSON;

namespace Azos.Serialization.JSON.Backends
{
  public struct JazonToken
  {
    // Error
    internal JazonToken(JsonMsgCode code, string text)
    {
      Type = (JsonTokenType)(-1);
      Text = text;
      ULValue = (ulong)code;
      DValue = 0d;
    }

    // Identifier
    internal JazonToken(JsonTokenType type, string text)
    {
      Type = type;
      Text = text;
      ULValue = 0ul;
      DValue = 0d;
    }

    //Int, or Long value
    internal JazonToken(JsonTokenType type, string text, ulong ulValue)
    {
      Type = type;
      Text = text;
      ULValue = ulValue;
      DValue = 0d;
    }

    //Double value
    internal JazonToken(JsonTokenType type,  string text, double dValue)
    {
      Type = type;
      Text = text;
      ULValue = 0ul;
      DValue = dValue;
    }

    public readonly JsonTokenType Type;

    public readonly string Text;

    //To avoid boxing the primitives are stored in-place, StructLayout Explicit with overlap of long over dbl does not yield any benefits
    public readonly ulong ULValue;//bool is stored as 1
    public readonly double DValue;

    public bool IsError => Type < 0;
    public JsonMsgCode MsgCode => IsError ? (JsonMsgCode)ULValue : JsonMsgCode.INFOS;
    public bool IsPrimary => Type > JsonTokenType.SYMBOLS_START; //  !IsNonLanguage && Type != JsonTokenType.tComment;
    public bool IsNonLanguage => Type==JsonTokenType.tUnknown || (Type > JsonTokenType.NONLANG_START && Type < JsonTokenType.NONLANG_END);
  }

}
