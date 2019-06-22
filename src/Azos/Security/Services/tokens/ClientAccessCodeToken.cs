using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;
using Azos.Serialization.Arow;

namespace Azos.Security.Services
{
  /// <summary>
  /// Generated for OAuth clients on successful authorization, the client typically makes back-channel call to
  /// IDP service supplying this temp token.
  /// AccessCode tokens are typically short-lived (e.g. less than 5 minutes)
  /// </summary>
  [Arow]
  public sealed class ClientAccessCodeToken : RingToken
  {
    public override (int min, int max) TokenByteStrength => (4, 16);
    public override int TokenDefaultExpirationSeconds => 5/*min*/ * 60;

    public ClientAccessCodeToken(string issuer, int expireInSeconds) : base(issuer, expireInSeconds)
    {
    }

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
    public string SubjectAuthenticationToken { get; set; }



    /// <summary>
    /// The redirect URI which was requested at Authorization
    /// </summary>
    [Field(backendName: "ruri", isArow: true)]
    [Field(description: "The redirect URI which was requested at Authorization")]
    public string RedirectURI { get;set;}

  }
}
