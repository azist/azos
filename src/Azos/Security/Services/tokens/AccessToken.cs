using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;
using Azos.Serialization.Arow;
using Azos.Serialization.JSON;

namespace Azos.Security.Services
{
  /// <summary>
  /// Represents a token which is supplied to API/service provider to impersonate a user
  /// </summary>
  [Arow, Table(name: "accesstoken")]
  public sealed class AccessToken : RingToken
  {
    public override (int min, int max) TokenByteStrength => (64, 83);// 83 * 1.5 = 124.5 bytes; key length is (~100 .. ~128 base 64 chars)
    public override int TokenDefaultExpirationSeconds => 10/*hrs*/ * 60/*min*/ * 60;

    /// <summary>
    /// The original ID of the client who this token was originally issued to
    /// </summary>
    [Field(backendName: "cid", isArow: true)]
    [Field(description: "The original Id of the client which this token was issued for")]
    public string ClientId{ get; set; }


 //todo: Limit the permission grants for this token - how? Through realm???
    /// <summary>
    /// The internal AuthenticationToken which represents the user who the public AccessToken impersonates (the subject/target).
    /// The token is always stored as a string:  {realm}://{content (depending on realm, binary is base-64 encoded)}
    /// </summary>
    [Field(backendName: "sat", isArow: true)]
    [Field(description: "The content of the internal system AuthenticationToken. WARNING: This should never ever be shared with any public party/given out")]
    public string SubjectAuthenticationToken { get; set; }

  }
}


