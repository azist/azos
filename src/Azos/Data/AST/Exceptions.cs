/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Data.AST
{
  /// <summary>
  /// Base exception thrown by the abstract syntax tree processing
  /// </summary>
  [Serializable]
  public class ASTException : DataException , IHttpStatusProvider
  {
    public ASTException() { }
    public ASTException(string message) : base(message) { }
    public ASTException(string message, Exception inner) : base(message, inner) { }
    protected ASTException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public int HttpStatusCode => WebConsts.STATUS_400;
    public string HttpStatusDescription => WebConsts.STATUS_400_DESCRIPTION + " / Bad Expression";
  }

}
