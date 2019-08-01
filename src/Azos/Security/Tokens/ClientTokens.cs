/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using Azos.Serialization.Arow;

namespace Azos.Security.Tokens
{
  /// <summary>
  /// Generated for OAuth clients on successful authorization, the client typically makes back-channel call to
  /// IDP service supplying this temp token.
  /// This class stipulates general contract for Access and Refresh tokens, having the difference only in their intent
  /// </summary>
  [Arow]
  public abstract class ClientAccessTokenBase : RingToken
  {
    /// <summary>
    /// The original Id of the client which this access code was issued for
    /// </summary>
    [Field(backendName: "cid", isArow: true)]
    [Field(description: "The original Id of the client which this access code was issued for")]
    public string ClientId{ get; set;}

    /// <summary>
    /// The internal AuthenticationToken which represents the user who the public AccessToken impersonates (the subject/target).
    /// The token is always stored as a string:  {realm}://{content (depending on realm, binary is base-64 encoded)}
    /// </summary>
    [Field(backendName: "sat", isArow: true)]
    [Field(description: "The content of the internal system AuthenticationToken. WARNING: This should never ever be shared with any public party/given out")]
    public string SubjectSysAuthToken { get; set; }

  }


  /// <summary>
  /// Generated for OAuth clients on successful authorization, the client typically makes back-channel call to
  /// IDP service supplying this temp token.
  /// AccessCode tokens are typically short-lived (e.g. less than 3 minutes)
  /// </summary>
  [Arow]
  public sealed class ClientAccessCodeToken : ClientAccessTokenBase
  {
    public override int TokenByteStrength => 16;
    public override int TokenDefaultExpirationSeconds => 3/*min*/ * 60;

    /// <summary>
    /// The redirect URI which was requested at Authorization
    /// </summary>
    [Field(backendName: "ruri", isArow: true)]
    [Field(description: "The redirect URI which was requested at Authorization")]
    public string RedirectURI { get; set; }

    /// <summary>
    /// The client state supplied to Authorization call
    /// </summary>
    [Field(backendName: "state", isArow: true)]
    [Field(description: "The client state supplied to Authorization call")]
    public string State { get; set; }
  }

  /// <summary>
  /// Generated for OAuth clients on successful authorization, the client typically makes back-channel call to
  /// IDP service supplying this refresh token.
  /// RefreshTokens are typically long-lived (e.g. ~1day)
  /// </summary>
  [Arow]
  public sealed class ClientRefreshCodeToken : ClientAccessTokenBase
  {
    public override int TokenByteStrength => 32;
    public override int TokenDefaultExpirationSeconds => 1 /*days*/ *  24/*hrs*/ * 60/*min*/ * 60;
  }
}
