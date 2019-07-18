
using Azos.Data;
using Azos.Serialization.Arow;

namespace Azos.Security.Tokens
{
  /// <summary>
  /// Represents a token which is supplied to API/service provider to impersonate a user
  /// </summary>
  [Arow]
  public sealed class AccessToken : RingToken
  {
    public override int TokenByteStrength => 64;
    public override int TokenDefaultExpirationSeconds => 10/*hrs*/ * 60/*min*/ * 60;

    /// <summary>
    /// The original ID of the client who this token was originally issued to
    /// </summary>
    [Field(backendName: "cid", isArow: true)]
    [Field(description: "The original Id of the client which this token was issued for")]
    public string ClientId{ get; set; }


    /// <summary>
    /// The internal SysAuthToken which represents the subject (user) who the public AccessToken impersonates (the subject/target).
    /// The token is always stored as a string:  {realm}://{content (depending on realm, binary is base-64 encoded)}
    /// </summary>
    [Field(backendName: "sat", isArow: true)]
    [Field(description: "The content of the internal system AuthenticationToken. WARNING: This should never ever be shared with any public party/given out")]
    public string SubjectSysAuthToken { get; set; }

  }
}


