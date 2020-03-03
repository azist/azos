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
    public JazonDeserializationException() { }
    public JazonDeserializationException(JsonMsgCode code, string text) : base("Code {0}: {1}".Args(code, text)) { }
    public JazonDeserializationException(JsonMsgCode code, string text, SourcePosition pos) : base("Code {0} at {1}: {2}".Args(code, pos, text)) { }
    protected JazonDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}