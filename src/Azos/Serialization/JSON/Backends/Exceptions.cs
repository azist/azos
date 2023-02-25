/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

using Azos.CodeAnalysis.JSON;
using Azos.CodeAnalysis.Source;

namespace Azos.Serialization.JSON.Backends
{
  /// <summary>
  /// Base exception thrown by the JAZON when deserializing objects
  /// </summary>
  [Serializable]
  public class JazonDeserializationException : JSONDeserializationException
  {
    public readonly JsonMsgCode JazonCode;
    public readonly string JazonText;
    public readonly SourcePosition JazonPosition;


    public JazonDeserializationException() { }
    public JazonDeserializationException(JsonMsgCode code, string text) : base("Code {0}: {1}".Args(code, text))
    {
      JazonCode = code;
      JazonText = text;
    }
    public JazonDeserializationException(JsonMsgCode code, string text, SourcePosition pos) : base("Code {0} at {1}: {2}".Args(code, pos, text))
    {
      JazonCode = code;
      JazonText = text;
      JazonPosition = pos;
    }
    protected JazonDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public override JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = base.ProvideExternalStatus(includeDump);
      result["jz.code"] = JazonCode.ToString();
      result["jz.txt"] = JazonText;
      if (JazonPosition.IsAssigned)
      {
        result["jz.p.char"] = JazonPosition.CharNumber;
        result["jz.p.ln"] = JazonPosition.LineNumber;
        result["jz.p.col"] = JazonPosition.ColNumber;
      }
      return result;
    }
  }
}