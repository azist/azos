/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;
using Azos.Serialization.JSON;

namespace Azos.Sky.FileGateway
{
  /// <summary>
  /// Thrown to indicate file gateway related problems
  /// </summary>
  [Serializable]
  public class FileGatewayException : SkyException, IHttpStatusProvider, IExternalStatusProvider
  {
    public FileGatewayException() : base() {}
    public FileGatewayException(string message) : base(message) {}
    public FileGatewayException(string message, Exception inner) : base(message, inner) { }
    protected FileGatewayException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public int HttpStatusCode => 591;

    public string HttpStatusDescription => "File gateway error";

    public virtual JsonDataMap ProvideExternalStatus(bool includeDump)
     => this.DefaultBuildErrorStatusProviderMap(includeDump, "fs");
  }
}
