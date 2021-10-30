
using Azos.Data;
using Azos.Serialization.Arow;

namespace Azos.Security.Tokens
{
  /// <summary>
  /// SSO Session ID
  /// </summary>
  [Arow("3e74ffec-d9f5-4d26-8cee-3461b03ccd5b")]
  public sealed class SsoSessionToken : RingToken
  {
    public override int TokenByteStrength => 16;
    public override int TokenDefaultExpirationSeconds => 25/*hrs*/ * 60/*min*/ * 60;

    /// <summary>
    /// The internal SysAuthToken which represents the subject (user) who the public AccessToken impersonates (the subject/target).
    /// The token is always stored as a string:  {realm}://{content (depending on realm, binary is base-64 encoded)}
    /// </summary>
    [Field(backendName: "sat", isArow: true)]
    [Field(description: "The content of the internal system AuthenticationToken. WARNING: This should never ever be shared with any public party/given out")]
    public string SysAuthToken { get; set; }
  }
}


